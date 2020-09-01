using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtaServer
{
    public enum AccountCreationFailType
    {
        Unknown,
        EmailExists,
        UsernameExists
    }

    class NetworkEventDispatcher
    {
        public delegate void UserLoginEventHandler(UserLoginEventArgs args);
        public delegate void ClientMessageEventHandler(ClientMessageEventArgs args);
        public delegate void AccountCreationFailedEventHandler(AccountCreationFailedArgs args);
        public delegate void AccountCreationSuccessEventHandler(AccountCreationSuccessArgs args);
        public delegate void ClientConnectionEventHandler(ClientConnectionEventArgs args);
        public delegate void ClientDisconnectEventHandler(ClientDisconnectEventArgs args);
        public delegate void ClientCirculationEventHandler(ClientCirculationEventArgs args);

        public static event UserLoginEventHandler UserLoginEvent;
        public static event ClientMessageEventHandler ClientMessageEvent;
        public static event AccountCreationFailedEventHandler AccountCreationFailedEvent;
        public static event AccountCreationSuccessEventHandler AccountCreationSucessEvent;
        public static event ClientConnectionEventHandler ClientConnectionEvent;
        public static event ClientDisconnectEventHandler ClientDisconnectEvent;
        public static event ClientCirculationEventHandler ClientCirculationEvent;

        public static void InvokeUserLoginEvent(UserLoginEventArgs args)
        {
            UserLoginEvent?.Invoke(args);
        }

        public static void InvokeClientMessageEvent(ClientMessageEventArgs args)
        {
            ClientMessageEvent?.Invoke(args);
        }

        public static void InvokeAccountCreationFailedEvent(AccountCreationFailedArgs args)
        {
            AccountCreationFailedEvent?.Invoke(args);
        }

        public static void InvokeAccountCreationSuccessEvent(AccountCreationSuccessArgs args)
        {
            AccountCreationSucessEvent?.Invoke(args);
        }

        public static void InvokeClientConnectionEvent(ClientConnectionEventArgs args)
        {
            ClientConnectionEvent?.Invoke(args);
        }

        public static void InvokeClientDisconnectEvent(ClientDisconnectEventArgs args)
        {
            ClientDisconnectEvent?.Invoke(args);
        }

        public static void InvokeClientCirculationEvent(ClientCirculationEventArgs args)
        {
            ClientCirculationEvent?.Invoke(args);
        }
    }

    public class ClientCirculationEventArgs
    {
        ClientState m_ClientState;

        internal ClientState ClientState { get { return m_ClientState; } }

        public ClientCirculationEventArgs(ClientState state)
        {
            m_ClientState = state;
        }
    }

    public class ClientDisconnectEventArgs
    {
        ClientState m_ClientState;

        internal ClientState ClientState { get { return m_ClientState; } }

        public ClientDisconnectEventArgs(ClientState state)
        {
            m_ClientState = state;
        }
    }

    public class ClientConnectionEventArgs
    {
        ClientState m_ClientState;

        internal ClientState ClientState { get { return m_ClientState; } }

        public ClientConnectionEventArgs(ClientState state)
        {
            m_ClientState = state;
        }
    }

    public class UserLoginEventArgs
    {
        string m_ClientId;
        string m_Username;

        bool m_LoginSuccess;
        bool m_LoggedIn;

        internal bool Success { get { return m_LoginSuccess; } }
        internal bool LoggedIn { get { return m_LoggedIn; } }

        internal string ClientId { get { return m_ClientId; } }
        internal string Username { get { return m_Username; } }

        public UserLoginEventArgs(string clientId, string username, bool success, bool loggedIn)
        {
            m_ClientId = clientId;
            m_LoginSuccess = success;
            m_Username = username;
            m_LoggedIn = loggedIn;
        }
    }

    public class AccountCreationSuccessArgs
    {
        string m_ClientId;

        internal string ClientId { get { return m_ClientId; } }

        public AccountCreationSuccessArgs(string clientId)
        {
            m_ClientId = clientId;
        }
    }

    public class AccountCreationFailedArgs
    {
        string m_ClientId;
        AccountCreationFailType m_FailureType;
        
        internal string ClientId { get { return m_ClientId; } }
        internal AccountCreationFailType FailureType { get { return m_FailureType; } }

        public AccountCreationFailedArgs(string clientId, AccountCreationFailType type)
        {
            m_FailureType = type;
            m_ClientId = clientId;
        }
    }

    internal class ClientMessageEventArgs
    {
        string m_Message;
        string m_ClientId;

        internal string Message { get { return m_Message; } }
        internal string ClientId { get { return m_ClientId; } }

        internal void UpdateMessage(string clientId, string message)
        {
            m_Message = message;
            m_ClientId = clientId;
        }
    }
}
