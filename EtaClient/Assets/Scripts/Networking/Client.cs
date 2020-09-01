using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

public class ClientState
{
    public Socket Socket = null;
    public const int BufferSize = 1024;
    public byte[] Buffer = new byte[BufferSize];
    public StringBuilder MessageQueue = new StringBuilder();
}

public class Client : MonoBehaviour
{
    private const int m_Port = 11000;

    private static ManualResetEvent m_ConnectEvent = new ManualResetEvent(false);

    static ClientState m_ClientState;
    static ClientManager m_ClientManager;

    static Thread m_ClientThread;
    static Thread m_ReceiveThread;

    static int m_MaxConnectionAttempts = 20;
    static int m_ConnectAttemptsMade = 0;

    internal static void Disconnect()
    {
        try
        {
            m_ClientThread?.Abort();
            m_ReceiveThread?.Abort();

            EventSink.InvokeStandardLogEvent
                (new LogEventArgs("Client Thread Aborting.. Beginning Shutdown."));

            m_ClientState.Socket.Shutdown(SocketShutdown.Both);
            m_ClientState.Socket.BeginDisconnect
                (false, new AsyncCallback(DisconnectCallback), m_ClientState);
        }

        catch (Exception e)
        {
            EventSink.InvokeStandardLogEvent(new LogEventArgs(e.ToString()));
        }
    }

    internal static void DisconnectCallback(IAsyncResult result)
    {
        try
        {
            m_ClientState.Socket.EndDisconnect(result);
        }

        catch (Exception e)
        {
            EventSink.InvokeStandardLogEvent(new LogEventArgs(e.ToString()));
        }
    }

    internal static void StartClient(ClientManager manager)
    {
        try
        {
            m_ClientManager = manager;

            m_ClientState = new ClientState();

            ClientManager.MessageIsolator = new MessageIsolator(m_ClientState);

            Task.Run(() => Connect());
        }

        catch (Exception e)
        {
            EventSink.InvokeStandardLogEvent(new LogEventArgs(e.ToString()));
        }
    }

    private static void Connect()
    {
        try
        {
            m_ClientThread = Thread.CurrentThread;

            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];

            IPEndPoint remoteEP = new IPEndPoint(ipAddress, m_Port);

            m_ClientState.Socket = new Socket
                (ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            m_ClientState.Socket.BeginConnect
                (remoteEP, new AsyncCallback(ConnectCallback), m_ClientState.Socket);

            m_ConnectEvent.WaitOne();
        }

        catch (Exception e)
        {
            EventSink.InvokeStandardLogEvent(new LogEventArgs(e.ToString()));
        }
    }

    private static void ConnectCallback(IAsyncResult ar)
    {
        try
        {
            m_ClientState.Socket.EndConnect(ar);

            EventSink.InvokeStandardLogEvent(new LogEventArgs
                (string.Format("Socket connected to {0}", m_ClientState.Socket.RemoteEndPoint.ToString())));

            m_ConnectEvent.Set();

            EventSink.InvokeConnectionEvent
                (new ClientConnectedEventArgs());

            Task.Run(() => Receive());
        }

        catch(SocketException se)
        {
            EventSink.InvokeStandardLogEvent
                (new LogEventArgs("Socket Exception: " + se.ToString()));

            EventSink.InvokeConnectionFailedEvent
                (new ConnectionFailedEventArgs(m_ConnectAttemptsMade++));
        }

        catch (Exception ex)
        {
            EventSink.InvokeStandardLogEvent(new LogEventArgs(ex.ToString()));
        }
    }

    private static void Receive()
    {
        try
        {
            m_ReceiveThread = Thread.CurrentThread;
            m_ClientState.Socket.BeginReceive
                (m_ClientState.Buffer, 0, ClientState.BufferSize, 0,
                    new AsyncCallback(ReceiveCallback), m_ClientState);
        }

        catch (Exception e)
        {
            EventSink.InvokeStandardLogEvent(new LogEventArgs(e.ToString()));
        }
    }

    private static void ReceiveCallback(IAsyncResult ar)
    {
        try
        {
            int bytesRead = m_ClientState.Socket.EndReceive(ar);
            if (bytesRead > 0)
            {
                string msg = Encoding.ASCII.GetString(m_ClientState.Buffer, 0, bytesRead);

                if (m_ClientManager.PublicKeyReceived)
                {
                    m_ClientState.MessageQueue.Append(msg);
                    ClientManager.MessageIsolator.ProcessStream();
                }

                else if (AsymModule.IsPublicKey(msg))
                {
                    EventSink.InvokePublicKeySetEvent();
                }

                m_ClientState.Socket.BeginReceive
                    (m_ClientState.Buffer, 0, ClientState.BufferSize, 0,
                        new AsyncCallback(ReceiveCallback), m_ClientState);
            }
        }

        catch (Exception e)
        {
            EventSink.InvokeStandardLogEvent(new LogEventArgs(e.ToString()));
        }
    }

    internal static void Send(String data)
    {
        try
        {
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            m_ClientState.Socket.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), m_ClientState.Socket);
        }

        catch (Exception e)
        {
            EventSink.InvokeStandardLogEvent(new LogEventArgs(e.ToString()));
        }
    }

    private static void SendCallback(IAsyncResult ar)
    {
        try
        {
            m_ClientState.Socket.EndSend(ar);
        }

        catch (Exception e)
        {
            EventSink.InvokeStandardLogEvent(new LogEventArgs(e.ToString()));
        }
    }
}

//EventSink.InvokeStandardLogEvent(new LogEventArgs("Encrypted Message: " + msg));
//EventSink.InvokeStandardLogEvent(new LogEventArgs("Sending: " + data));