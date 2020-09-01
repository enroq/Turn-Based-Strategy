using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eta.Interdata
{
    public class InterEventDispatcher
    {
        public delegate void DCFromMatchInProgressEventHandler(DCFromMatchInProgressEventArgs args);
        public delegate void MatchDepartureTimerExpiredEventHandler(MatchDepartureTimerEventArgs args);
        public delegate void MatchDepartureTimerTickEventHandler(MatchDepartureTimerTickEventArgs args);
        public delegate void SendGamePiecesSyncEventHandler(SendGamePiecesSyncEventArgs args);
        public delegate void GamePieceSyncReceivedEventHandler(GamePieceSyncReceivedEventArgs args);
        public delegate void GamePieceSyncCompleteEventHandler(GamePieceSyncCompleteEventArgs args);
        public delegate void NetworkGamePieceMoveEventHandler(NetworkGamePieceMoveEventArgs args);
        public delegate void SpectatorJoinSyncEventHandler(SpectatorJoinSyncEventArgs args);
        public delegate void MatchStartEventHandler(MatchStartEventArgs args);
        public delegate void TurnStateSyncEventHandler(TurnStateSyncEventArgs args);
        public delegate void TurnStateUpdateEventHandler(TurnStateUpdateEventArgs args);
        public delegate void AttackSyncCompleteEventHandler(AttackSyncCompleteEventArgs args);

        public static event DCFromMatchInProgressEventHandler DCFromMatchInProgressEvent;
        public static event MatchDepartureTimerExpiredEventHandler MatchDepartureTimerExpiredEvent;
        public static event MatchDepartureTimerTickEventHandler MatchDepartureTimerTickEvent;
        public static event SendGamePiecesSyncEventHandler SendGamePiecesSyncEvent;
        public static event GamePieceSyncReceivedEventHandler GamePieceSyncReceivedEvent;
        public static event GamePieceSyncCompleteEventHandler GamePieceSyncCompleteEvent;
        public static event NetworkGamePieceMoveEventHandler NetworkGamePieceMoveEvent;
        public static event SpectatorJoinSyncEventHandler SpectatorJoinSyncEvent;
        public static event MatchStartEventHandler MatchStartEvent;
        public static event TurnStateSyncEventHandler TurnStateSyncEvent;
        public static event TurnStateUpdateEventHandler TurnStateUpdateEvent;
        public static event AttackSyncCompleteEventHandler AttackSyncCompleteEvent;

        public static void InvokeDCFromMatchInProgressEvent(DCFromMatchInProgressEventArgs args)
        {
            DCFromMatchInProgressEvent?.Invoke(args);
        }

        public static void InvokeMatchDepartureTimerExpiredEvent(MatchDepartureTimerEventArgs args)
        {
            MatchDepartureTimerExpiredEvent?.Invoke(args);
        }

        public static void InvokeMatchDepartureTimerTickEvent(MatchDepartureTimerTickEventArgs args)
        {
            MatchDepartureTimerTickEvent?.Invoke(args);
        }

        public static void InvokeSendGamePiecesSyncEvent(SendGamePiecesSyncEventArgs args)
        {
            SendGamePiecesSyncEvent?.Invoke(args);
        }

        public static void InvokeGamePieceSyncReceivedEvent(GamePieceSyncReceivedEventArgs args)
        {
            GamePieceSyncReceivedEvent?.Invoke(args);
        }

        public static void InvokeGamePieceSyncCompleteEvent(GamePieceSyncCompleteEventArgs args)
        {
            GamePieceSyncCompleteEvent?.Invoke(args);
        }

        public static void InvokeNetworkGamePieceMoveEvent(NetworkGamePieceMoveEventArgs args)
        {
            NetworkGamePieceMoveEvent?.Invoke(args);
        }

        public static void InvokeSpectatorJoinSyncEvent(SpectatorJoinSyncEventArgs args)
        {
            SpectatorJoinSyncEvent?.Invoke(args);
        }

        public static void InvokeMatchStartEvent(MatchStartEventArgs args)
        {
            MatchStartEvent?.Invoke(args);
        }

        public static void InvokeTurnStateSyncEvent(TurnStateSyncEventArgs args)
        {
            TurnStateSyncEvent?.Invoke(args);
        }

        public static void InvokeTurnStateUpdateEvent(TurnStateUpdateEventArgs args)
        {
            TurnStateUpdateEvent?.Invoke(args);
        }

        public static void InvokeAttackSyncCompleteEvent(AttackSyncCompleteEventArgs args)
        {
            AttackSyncCompleteEvent.Invoke(args);
        }
    }

    public class AttackSyncCompleteEventArgs
    {
        string m_MatchId;
        string m_AttackingPieceId;
        string m_DefendingPlayerId;
        int m_BoardVectorX;
        int m_BoardVectorY;

        public string MatchId { get { return m_MatchId; } }
        public string AttackingPieceId { get { return m_AttackingPieceId; } }
        public string DefendingPlayerId { get { return m_DefendingPlayerId; } }
        public int BoardVectorX { get { return m_BoardVectorX; } }
        public int BoardVectorY { get { return m_BoardVectorY; } }

        public AttackSyncCompleteEventArgs(string matchId, string attackerId, string defendingPlayerId, int x, int y)
        {
            m_MatchId = matchId;
            m_AttackingPieceId = attackerId;
            m_DefendingPlayerId = defendingPlayerId;
            m_BoardVectorX = x;
            m_BoardVectorY = y;
        }
    }

    public class TurnStateUpdateEventArgs
    {
        string m_MatchId;
        int m_TurnStep;

        public string MatchId { get { return m_MatchId; } }
        public int TurnStep { get { return m_TurnStep; } }

        public TurnStateUpdateEventArgs(string matchId, int turnStep)
        {
            m_MatchId = matchId;
            m_TurnStep = turnStep;
        }
    }

    public class TurnStateSyncEventArgs
    {
        string m_CurrentPlayerId;
        string m_MatchId;
        int m_TurnStep;
        int m_LowestTurnStep;

        public string CurrentPlayerId { get { return m_CurrentPlayerId; } }
        public string MatchId { get { return m_MatchId; } }
        public int TurnStep { get { return m_TurnStep; } }
        public int LowestTurnStep { get { return m_LowestTurnStep; } }

        public TurnStateSyncEventArgs(string matchId, string currentPlayerId, int turnStep, int lowestStep)
        {
            m_MatchId = matchId;
            m_CurrentPlayerId = currentPlayerId;
            m_TurnStep = turnStep;
            m_LowestTurnStep = lowestStep;
        }
    }

    public class MatchStartEventArgs
    {
        string m_MatchId;
        string m_PlayerIdOfFirstToMove;

        public string MatchId { get { return m_MatchId; } }
        public string PlayerIdOfFirstToMove { get { return m_PlayerIdOfFirstToMove; } }

        public MatchStartEventArgs(string matchId, string firstMoveId)
        {
            m_MatchId = matchId;
            m_PlayerIdOfFirstToMove = firstMoveId;
        }
    }

    public class SpectatorJoinSyncEventArgs
    {
        string m_MatchId;
        string m_SpectatorId;
        string m_ControllerId;

        GamePieceNetworkState[] m_GamePieceNetworkStates;

        public string MatchId { get { return m_MatchId; } }
        public string SpectatorId { get { return m_SpectatorId; } }
        public string ControllerId { get { return m_ControllerId; } }
        public GamePieceNetworkState[]
            GamePieceNetworkStates { get { return m_GamePieceNetworkStates; } }

        public SpectatorJoinSyncEventArgs
            (string matchId, string spectatorId, string controllerId, GamePieceNetworkState[] pieceNetworkStates)
        {
            m_MatchId = matchId;
            m_SpectatorId = spectatorId;
            m_ControllerId = controllerId;
            m_GamePieceNetworkStates = pieceNetworkStates;
        }
    }

    public class NetworkGamePieceMoveEventArgs
    {
        float m_DestinationX;
        float m_DestinationY;

        string m_MatchId;
        string m_PieceNetworkId;

        public float DestinationX { get { return m_DestinationX; } }
        public float DestinationY { get { return m_DestinationY; } }

        public string MatchId { get { return m_MatchId; } }
        public string PieceNetworkId { get { return m_PieceNetworkId; } }

        public NetworkGamePieceMoveEventArgs(string matchId, string pieceNetworkId, float x, float y)
        {
            m_MatchId = matchId;
            m_PieceNetworkId = pieceNetworkId;
            m_DestinationX = x;
            m_DestinationY = y;
        }
    }

    public class GamePieceSyncCompleteEventArgs
    {
        string m_MatchId;
        string m_ControllerId;

        public string MatchId { get { return m_MatchId; } }
        public string ControllerId { get { return m_ControllerId; } }

        public GamePieceSyncCompleteEventArgs(string matchId, string controllerId)
        {
            m_MatchId = matchId;
            m_ControllerId = controllerId;
        }
    }

    public class GamePieceSyncReceivedEventArgs
    {
        string m_MatchId;
        string m_ControllerId;
        GamePieceNetworkState m_GamePieceNetworkState;

        public string MatchId { get { return m_MatchId; } }
        public string ControllerId { get { return m_ControllerId; } }
        public GamePieceNetworkState 
            GamePieceNetworkState { get { return m_GamePieceNetworkState; } }

        public GamePieceSyncReceivedEventArgs(string matchId, string controllerId, GamePieceNetworkState pieceNetworkState)
        {
            m_MatchId = matchId;
            m_ControllerId = controllerId;
            m_GamePieceNetworkState = pieceNetworkState;
        }
    }

    public class SendGamePiecesSyncEventArgs
    {
        bool m_EndSync;
        string m_MatchId;
        string m_ControllerId;
        GamePieceNetworkState[] m_GamePieceNetworkStates;

        public bool EndSync { get { return m_EndSync; } }
        public string MatchId { get { return m_MatchId; } }
        public string ControllerId { get { return m_ControllerId; } }
        public GamePieceNetworkState[] 
            GamePieceNetworkStates { get { return m_GamePieceNetworkStates; } }

        public SendGamePiecesSyncEventArgs
            (string matchId, string controllerId, GamePieceNetworkState[] pieceNetworkStates, bool endSync)
        {
            m_EndSync = endSync;
            m_MatchId = matchId;
            m_ControllerId = controllerId;
            m_GamePieceNetworkStates = pieceNetworkStates;
        }
    }

    public class MatchDepartureTimerTickEventArgs
    {
        int m_SecondsExpired;
        MatchState m_Match;

        public int SecondsExpired { get { return m_SecondsExpired; } }
        public MatchState Match { get { return m_Match; } }

        public MatchDepartureTimerTickEventArgs(MatchState match, int secondsExpired)
        {
            m_Match = match;
            m_SecondsExpired = secondsExpired;
        }
    }

    public class MatchDepartureTimerEventArgs
    {
        MatchState m_Match;
        PlayerState m_PlayerState;

        public MatchState Match { get { return m_Match; } }
        public PlayerState PlayerState { get { return m_PlayerState; } }

        public MatchDepartureTimerEventArgs(MatchState match, PlayerState player)
        {
            m_Match = match;
            m_PlayerState = player;
        }
    }

    public class DCFromMatchInProgressEventArgs
    {
        MatchState m_Match;
        PlayerState m_PlayerState;

        public MatchState Match { get { return m_Match; } }
        public PlayerState PlayerState { get { return m_PlayerState; } }

        public DCFromMatchInProgressEventArgs(MatchState match, PlayerState player)
        {
            m_Match = match;
            m_PlayerState = player;
        }
    }
}
