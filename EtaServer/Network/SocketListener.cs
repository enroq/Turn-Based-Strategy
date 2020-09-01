using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EtaServer
{
    internal class SocketListener
    {
        internal static ManualResetEvent m_ManualResetEvent = new ManualResetEvent(false);

        int m_HostPort;
        IPAddress m_HostIp;
        IPEndPoint m_HostEndPoint;

        Socket m_Socket;
        bool m_IsListening = true;

        public SocketListener(int port)
        {
            Console.WriteLine(string.Format("Host Name: {0}", Dns.GetHostName()));

            m_HostIp = GetHostAddress();
            m_HostPort = port;
            m_HostEndPoint = new IPEndPoint(m_HostIp, m_HostPort);

            m_Socket = new Socket(m_HostIp.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        }

        internal void Start()
        {
            try
            { 
                m_Socket.Bind(m_HostEndPoint);
                m_Socket.Listen(1024);

                AsyncCallback asyncCallback = new AsyncCallback(AcceptCallback);
                while (m_IsListening)
                {
                    m_ManualResetEvent.Reset();
                    m_Socket.BeginAccept(asyncCallback, m_Socket);
                    m_ManualResetEvent.WaitOne();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        internal void AcceptCallback(IAsyncResult result)
        {
            m_ManualResetEvent.Set();

             ClientManager.AddClient
                ((new ClientState(((Socket)(result.AsyncState)).EndAccept(result))));

            if(ServerCore.DebugMode)
                Console.WriteLine
                    ("Client Accepted On Thread: " + Thread.CurrentThread.ManagedThreadId);
        }

        internal IPHostEntry GetHostInfo()
        {
            return Dns.GetHostEntry(Dns.GetHostName());
        }

        internal IPAddress GetHostAddress()
        {
            IPHostEntry entry = GetHostInfo();
            return entry.AddressList[0];
        }
    }
}

            //for (int i = 0; i<entry.AddressList.Length; i++)
            //    Console.WriteLine(entry.AddressList[i].ToString());
