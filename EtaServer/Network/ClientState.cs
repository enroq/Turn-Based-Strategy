using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EtaServer
{
    public class ClientState
    {
        private Socket m_Socket = null;
        private const int m_BufferSize = 1024;
        private byte[] m_Buffer = new byte[m_BufferSize];

        private StringBuilder m_MessageQueue = new StringBuilder();
        private MessageIsolator m_MessageIsolator;
        private AsyncCallback m_AsyncCallback;

        private bool m_HasReceivedSessionKey = false;
        private bool m_IsSending = false;
        private bool m_IsAuthorized = false;

        private string m_ClientIdentifier = string.Empty;

        private AesModule m_AesModule;

        private Account m_AccountRelative;

        private DateTime m_LastSendTime;
        private DateTime m_LastHeartbeatTime;

        private TimeSpan m_HeartbeatInterval = TimeSpan.FromSeconds(20);
        private TimeSpan m_HeartbeatDecayTime = TimeSpan.FromSeconds(30);

        internal static int BufferSize { get { return m_BufferSize; } }
        internal string ClientId { get { return m_ClientIdentifier; } }
        internal Account AccountRelative { get { return m_AccountRelative; } }

        internal AesModule AesModule { get { return m_AesModule; } }

        internal bool IsSending { get { return m_IsSending; } }
        internal bool IsAuthorized { get { return m_IsAuthorized; } }

        internal DateTime LastSendTime { get { return m_LastSendTime; } }
        internal DateTime LastHeartbeatTime { get { return m_LastHeartbeatTime; } }

        internal Socket Socket
        {
            get { return m_Socket; }
            set { m_Socket = value; }
        }

        internal byte[] Buffer
        {
            get { return m_Buffer; }
            set { m_Buffer = value; }
        }

        internal StringBuilder MessageQueue
        {
            get { return m_MessageQueue; }
            set { m_MessageQueue = value; }
        }    

        public ClientState(Socket socket)
        {
            try
            {
                m_Socket = socket;

                m_ClientIdentifier = Guid.NewGuid().ToString();

                m_AesModule = new AesModule();
                m_MessageIsolator = new MessageIsolator(this, m_AesModule);

                m_AsyncCallback = new AsyncCallback(ReadCallback);

                NetworkEventDispatcher.
                    InvokeClientConnectionEvent(new ClientConnectionEventArgs(this));

                Socket.BeginReceive
                    (Buffer, 0, BufferSize, 0, m_AsyncCallback, this);
            }

            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        internal bool RequiresCirculation()
        {
            if(m_LastSendTime != null)
                if(DateTime.UtcNow.Subtract(m_LastSendTime) > m_HeartbeatInterval)
                {
                    m_LastHeartbeatTime = DateTime.UtcNow;
                    return true;
                }

            return false;
        }

        internal bool HeartbeatExpired()
        {
            return DateTime.UtcNow.Subtract(m_LastHeartbeatTime) > m_HeartbeatDecayTime
                && DateTime.UtcNow.Subtract(m_LastSendTime) > m_HeartbeatInterval;
        }

        internal void Authorize(Account account)
        {
            m_IsAuthorized = true;
            m_AccountRelative = account;
            ClientManager.AddClientAccountPair(this, account);
        }

        internal void InitializeAesModule(string key, string vector)
        {
            m_AesModule.InitializeProviderWithKey(key, vector);
        }

        internal bool ClientIsConnected()
        {
            if (!SocketConnected(Socket))
            {
                DisposeOfClientAndConnection();
                return false;
            }

            else
                return true;
        }

        bool SocketConnected(Socket s)
        {
            try
            {
                if (s.Connected)
                {
                    bool x = s.Poll(1000, SelectMode.SelectRead);
                    bool y = (s.Available == 0);
                    return
                        !(x && y);
                }

                else return false;
            }

            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }
        }

        internal void DisposeOfClientAndConnection()
        {
            try
            {
                ClientManager.RemoveClient(this);

                if (m_AccountRelative != null)
                    AccountHandler.RemoveOnlineAccount(m_AccountRelative);

                m_Socket.BeginDisconnect
                    (false, new AsyncCallback(DisconnectCallback), this);
            }

            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        internal void DisconnectCallback(IAsyncResult result)
        {
            try
            {
                m_Socket.EndDisconnect(result);
                ClientManager.NullifyClient(this);
            }

            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        internal void ReadCallback(IAsyncResult result)
        {
            try
            {
                int bytesRead = Socket.EndReceive(result);
                if (bytesRead > 0)
                {
                    string msg = Encoding.ASCII.GetString(Buffer, 0, bytesRead);

                    if (m_HasReceivedSessionKey)
                    {
                        MessageQueue.Append(msg);
                        m_MessageIsolator.ProcessStream();

                        if (ServerCore.DebugMode)
                            Console.WriteLine("Message Read On Thread: " + Thread.CurrentThread.ManagedThreadId);
                    }

                    else
                        m_HasReceivedSessionKey = IsSessionKey(msg);

                    Socket.BeginReceive(Buffer, 0, BufferSize, 0, m_AsyncCallback, this);
                }
            }

            catch (SocketException socEx)
            {
                Console.WriteLine("[Socket Exception]");
                Console.WriteLine(socEx.ToString());

                DisposeOfClientAndConnection();
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());

                DisposeOfClientAndConnection();
            }
            
        }

        internal void SendData(String data)
        {
            try
            {
                byte[] byteData; string encryptedMessage = string.Empty;

                if(!ClientIsConnected())
                    return;

                if (m_HasReceivedSessionKey)
                {
                    encryptedMessage = m_AesModule.EncryptStringToString(data);

                    byteData = Encoding.ASCII.GetBytes
                        (encryptedMessage + MessageIsolator.EncryptedMessageTerminator);

                    if (ServerCore.DebugMode)
                        OutputSendDebugData(encryptedMessage);
                }

                else
                    byteData = Encoding.ASCII.GetBytes(data);

                Socket.BeginSend
                    (byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), Socket);
            }

            catch(SocketException socEx)
            {
                Console.WriteLine("[Socket Exception]");
                Console.WriteLine(socEx.ToString());

                DisposeOfClientAndConnection();
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());

                DisposeOfClientAndConnection();
            }
        }

        internal void SendCallback(IAsyncResult ar)
        {
            try
            {
                int bytesSent = Socket.EndSend(ar);

                m_LastSendTime = DateTime.UtcNow;

                if (ServerCore.DebugMode)
                    Console.WriteLine("Sent {0} bytes to client.", bytesSent);
            }

            catch (SocketException socEx)
            {
                Console.WriteLine("[Socket Exception]");
                Console.WriteLine(socEx.ToString());

                DisposeOfClientAndConnection();
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());

                DisposeOfClientAndConnection();
            }
        }

        internal bool IsSessionKey(string msg)
        {
            try
            {
                string decryptedMessage = RSAModule.DecryptStringToString(msg);
                string[] segments = decryptedMessage.Split(':');

                if (ServerCore.DebugMode)
                    Console.WriteLine
                        ("Session Key Received From Client [{1}]", decryptedMessage, ClientId);

                InitializeAesModule(segments[0], segments[1]); return true;
            }

            catch(Exception e)
            {
                Console.WriteLine("[Error]: Unable To Handle Session Key!");

                Console.WriteLine(e.ToString());
                DisposeOfClientAndConnection();

                return false;
            }
        }

        internal void OutputSendDebugData(string data)
        {
            if (m_HasReceivedSessionKey)
                Console.WriteLine("Sending [{1}]: ({0}) ",
                    m_AesModule.DecryptStringToString(data).Replace("#|", " "), ClientId);
        }
    }
}
