using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EtaServer
{
    class ClientManager
    {
        private static Dictionary<string, ClientState> 
            m_Clients = new Dictionary<string, ClientState>();

        private static Dictionary<Account, ClientState>
            m_ClientAccountPairs = new Dictionary<Account, ClientState>();

        internal static Dictionary<Account, ClientState> 
            ClientAccountPairs { get { return m_ClientAccountPairs; } }

        internal static List<ClientState> 
            m_ClientHeartbeatCache = new List<ClientState>();

        internal static int ClientCount { get { return m_Clients.Count; } }

        internal static void CycleClientHeartbeats()
        {
            try
            {
                while (true)
                {
                    Thread.Sleep(1500 * 10);

                    ClientState[] clientCache = m_Clients.Values.ToArray();
                    for (int i = clientCache.Length - 1; i >= 0; i--)
                    {
                        Thread.Sleep(50);
                        if (clientCache[i].RequiresCirculation())
                        {
                            if (!m_ClientHeartbeatCache.Contains(clientCache[i]))
                                m_ClientHeartbeatCache.Add(clientCache[i]);

                            NetworkEventDispatcher.InvokeClientCirculationEvent
                                (new ClientCirculationEventArgs(clientCache[i]));
                        }
                    }

                    clientCache = m_ClientHeartbeatCache.ToArray();
                    for (int i = clientCache.Length - 1; i >= 0; i--)
                    {
                        Thread.Sleep(50);
                        if (clientCache[i].HeartbeatExpired() && m_ClientHeartbeatCache.Contains(clientCache[i]))
                            clientCache[i].DisposeOfClientAndConnection();
                    }
                }
            }

            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        internal static void AddClient(ClientState client)
        {
            if(!m_Clients.ContainsKey(client.ClientId))
                m_Clients.Add(client.ClientId, client);
        }

        internal static void AddClientAccountPair(ClientState client, Account account)
        {
            if (!m_ClientAccountPairs.ContainsKey(account))
                m_ClientAccountPairs.Add(account, client);
        }

        internal static void NullifyClient(ClientState client)
        {
            client = null;
        }

        internal static void RemoveClient(ClientState client)
        {
            if (m_Clients.ContainsKey(client.ClientId))
            {
                m_Clients.Remove(client.ClientId);

                if (client.AccountRelative != null && 
                    m_ClientAccountPairs.ContainsKey(client.AccountRelative))
                        m_ClientAccountPairs.Remove(client.AccountRelative);

                NetworkEventDispatcher.InvokeClientDisconnectEvent(new ClientDisconnectEventArgs(client));

                if (ServerCore.DebugMode)
                    Console.WriteLine("Removing [{0}] From Client List.", client.ClientId);
            }
        }

        internal static ClientState GetClientById(string id)
        {
            if (m_Clients.ContainsKey(id))
                return m_Clients[id];

            else
                return null;
        }

        internal static ClientState GetClientByAccount(Account account)
        {
            if (m_ClientAccountPairs.ContainsKey(account))
                return m_ClientAccountPairs[account];
            else
                return null;
        }

        internal static void SendMessageToAllClients(string message)
        {
            try
            {
                foreach (ClientState cState in m_Clients.Values.ToList())
                {
                    if(cState.IsAuthorized)
                        cState.SendData(message);
                }
            }

            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        /// <summary>
        /// Sends Message To All Clients Excluding The One Provided Via Parameter
        /// </summary>
        internal static void SendMessageToAllClients(string message, ClientState client)
        {
            try
            {
                foreach (ClientState cState in m_Clients.Values.ToList())
                {
                    if(cState != client && cState.IsAuthorized)
                        cState.SendData(message);
                }
            }

            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        /// <summary>
        /// Sends Message To All Clients Excluding Those Contained In List Of Clients Provided Via Parameter.
        /// </summary>
        internal static void SendMessageToAllClients(string message, List<ClientState> clients)
        {
            try
            {
                foreach (ClientState cState in m_Clients.Values.ToList())
                {
                    if (!clients.Contains(cState) && cState.IsAuthorized)
                        cState.SendData(message);
                }
            }

            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        internal static bool ClientIsConnected(string clientId)
        {
            if (m_Clients.ContainsKey(clientId))
                return ClientIsConnected(m_Clients[clientId]);
            else
                return false;
        }

        internal static bool ClientIsConnected(ClientState client)
        {
            return client.ClientIsConnected();
        }

        internal static void LogClientOut(string clientId)
        {
            if(m_Clients.ContainsKey(clientId))
            {
                m_Clients[clientId].DisposeOfClientAndConnection();
            }
        }

        internal static void RemoveClientFromHeartbeatCache(string clientId)
        {
            if(m_Clients.ContainsKey(clientId))
            {
                ClientState client = m_Clients[clientId];

                if (m_ClientHeartbeatCache.Contains(client))
                    m_ClientHeartbeatCache.Remove(client);
            }
        }

        internal static void SendMessageToClientByAccount(Account account, string message)
        {
            if(m_ClientAccountPairs.ContainsKey(account))
            {
                m_ClientAccountPairs[account].SendData(message);
            }
        }

        internal static void SendMessageToClientByAccount(string id, string message)
        {
            Account account = AccountHandler.GetAccountById(id);

            if (account == null)
                return;

            if (m_ClientAccountPairs.ContainsKey(account))
            {
                m_ClientAccountPairs[account].SendData(message);
            }
        }
    }
}
