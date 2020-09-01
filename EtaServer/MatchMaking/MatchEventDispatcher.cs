using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Eta.Interdata;

namespace EtaServer
{
    class MatchEventDispatcher
    {
        public delegate void MatchCreatedEventHandler(MatchCreatedEventArgs args);
        public delegate void MatchJoinResultEventHandler(MatchJoinEventArgs args);
        public delegate void MatchSyncEventHandler(MatchSyncEventArgs args);
        public delegate void MatchEndEventHandler(MatchEndEventArgs args);
        public delegate void SpectatorJoinEventHandler(SpectatorJoinEventArgs args);
        public delegate void UserDepartedEventHandler(UserDepartedEventArgs args);
        public delegate void SpectatorSyncEventHandler(SpectatorSyncEventArgs args);

        public static event MatchCreatedEventHandler MatchCreatedEvent;
        public static event MatchJoinResultEventHandler MatchJoinResultEvent;
        public static event MatchSyncEventHandler MatchSyncEvent;
        public static event MatchEndEventHandler MatchEndEvent;
        public static event SpectatorJoinEventHandler SpectatorJoinedEvent;
        public static event UserDepartedEventHandler UserDepartedEvent;
        public static event SpectatorSyncEventHandler SpectatorSyncEvent;

        public static void InvokeMatchCreatedEvent(MatchCreatedEventArgs args)
        {
            MatchCreatedEvent?.Invoke(args);
        }

        public static void InvokeMatchJoinResultEvent(MatchJoinEventArgs args)
        {
            MatchJoinResultEvent?.Invoke(args);
        }

        public static void InvokeMatchSyncEvent(MatchSyncEventArgs args)
        {
            MatchSyncEvent?.Invoke(args);
        }

        public static void InvokeMatchEndEvent(MatchEndEventArgs args)
        {
            MatchEndEvent?.Invoke(args);
        }

        public static void InvokeSpectatorJoinEvent(SpectatorJoinEventArgs args)
        {
            SpectatorJoinedEvent?.Invoke(args);
        }

        public static void InvokeUserDepartedEvent(UserDepartedEventArgs args)
        {
            UserDepartedEvent?.Invoke(args);
        }

        public static void InvokeSpectatorSyncEvent(SpectatorSyncEventArgs args)
        {
            SpectatorSyncEvent?.Invoke(args);
        }
    }

    public class SpectatorSyncEventArgs
    {
        MatchState m_Match;
        PlayerState m_PlayerState;

        internal MatchState Match { get { return m_Match; } }
        internal PlayerState PlayerState { get { return m_PlayerState; } }

        public SpectatorSyncEventArgs(MatchState match, PlayerState playerState)
        {
            m_Match = match;
            m_PlayerState = playerState;
        }
    }

    public class UserDepartedEventArgs
    {
        MatchState m_Match;
        DepartureType m_DepartureType;
        PlayerState m_PlayerState;

        internal MatchState Match { get { return m_Match; } }
        internal DepartureType DepartureType { get { return m_DepartureType; } }
        internal PlayerState PlayerState { get { return m_PlayerState; } }

        public UserDepartedEventArgs(MatchState match, DepartureType departureType, PlayerState playerState)
        {
            m_Match = match;
            m_DepartureType = departureType;
            m_PlayerState = playerState;
        }
    }

    public class SpectatorJoinEventArgs
    {
        string m_MatchId;
        bool m_Succeeded;
        string m_ClientId;

        internal string MatchId { get { return m_MatchId; } }
        internal bool Succeeded { get { return m_Succeeded; } }
        internal string ClientId { get { return m_ClientId; } }

        public SpectatorJoinEventArgs(string matchId, bool succeeded, string clientId)
        {
            m_MatchId = matchId;
            m_Succeeded = succeeded;
            m_ClientId = clientId;
        }
    }

    public class MatchEndEventArgs
    {
        string m_MatchId;

        internal string MatchId { get { return m_MatchId; } }

        public MatchEndEventArgs(string matchId)
        {
            m_MatchId = matchId;
        }
    }

    public class MatchSyncEventArgs
    {
        MatchState m_Match;

        internal MatchState Match { get { return m_Match; } }

        public MatchSyncEventArgs(MatchState match)
        {
            m_Match = match;
        }
    }

    public class MatchJoinEventArgs
    {
        bool m_JoinSucceeded;
        string m_MatchId;
        ClientState m_Client;

        internal bool JoinSucceeded { get { return m_JoinSucceeded; } }
        internal string MatchId { get { return m_MatchId; } }
        internal ClientState Client { get { return m_Client; } }

        public MatchJoinEventArgs(ClientState client, string matchId, bool success)
        {
            m_Client = client;
            m_JoinSucceeded = success;
            m_MatchId = matchId;
        }
    }

    public class MatchCreatedEventArgs
    {
        MatchState m_Match;

        internal MatchState Match { get { return m_Match; } }

        public MatchCreatedEventArgs(MatchState match)
        {
            m_Match = match;
        }
    }
}
