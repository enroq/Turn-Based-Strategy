using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Eta.Interdata
{
    public enum DepartureType
    {
        Participant,
        Spectator,
        Unknown
    }

    public class PlayerState
    {
        string m_AccountId;
        string m_Username;

        public List<GamePieceNetworkState>
            m_ModifiedNetworkStates = new List<GamePieceNetworkState>();

        Dictionary<string, GamePieceNetworkState> 
            m_GamePieceNetworkStates = new Dictionary<string, GamePieceNetworkState>();

        public string AccountIdentity { get { return m_AccountId; } }
        public string Username { get { return m_Username; } }

        public Dictionary<string, GamePieceNetworkState> 
            GamePieceNetworkStates { get { return m_GamePieceNetworkStates; } }

        public List<GamePieceNetworkState> 
            ModifiedNetworkStates { get { return m_ModifiedNetworkStates; } }

        public PlayerState(string id, string username)
        {
            m_AccountId = id;
            m_Username = username;
        }

        public void SyncGamePieceNetworkState(GamePieceNetworkState gpns)
        {
            if (!m_GamePieceNetworkStates.ContainsKey(gpns.NetworkIdentity))
            {
                m_GamePieceNetworkStates.Add(gpns.NetworkIdentity, gpns);
                m_ModifiedNetworkStates.Add
                    (m_GamePieceNetworkStates[gpns.NetworkIdentity]);
            }

            else
                UpdateGamePieceNetState(gpns);
        }

        public void UpdateGamePieceNetState(GamePieceNetworkState gpns, bool forceUpdate = false)
        {
            if(m_GamePieceNetworkStates.ContainsKey(gpns.NetworkIdentity))
            {
                GamePieceNetworkState currentNetState = m_GamePieceNetworkStates[gpns.NetworkIdentity];
                if (!currentNetState.Equals(gpns))
                {
                    m_GamePieceNetworkStates[gpns.NetworkIdentity] = gpns;
                    m_ModifiedNetworkStates.Add
                        (m_GamePieceNetworkStates[gpns.NetworkIdentity]);
                }

                else if (forceUpdate)
                {
                    m_GamePieceNetworkStates[gpns.NetworkIdentity] = gpns;
                    m_ModifiedNetworkStates.Add
                        (m_GamePieceNetworkStates[gpns.NetworkIdentity]);
                }
            }
        }

        public void ClearGamePieceNetworkStates()
        {
            m_GamePieceNetworkStates.Clear();
        }

        public void SendModifiedPieceSync(string matchId, bool endSync = true)
        {
            InterEventDispatcher.InvokeSendGamePiecesSyncEvent(new SendGamePiecesSyncEventArgs
                (matchId, AccountIdentity, ModifiedNetworkStates.ToArray(), endSync));

            m_ModifiedNetworkStates.Clear();
        }
    }

    public class MatchDepartureTimer
    {
        int m_Seconds;
        int m_CurrentIntervalCount;
        Timer m_DepartureTimer;
        PlayerState m_PlayerState;
        MatchState m_Match;

        public MatchDepartureTimer(MatchState match, string playerId, int seconds)
        {
            m_Match = match;
            m_Seconds = seconds;
            m_PlayerState = match.GetPlayerStateById(playerId);

            m_DepartureTimer = new Timer(1000);
            m_DepartureTimer.Elapsed += DepartureTimer_Elapsed;
            m_DepartureTimer.Enabled = true;
        }

        private void DepartureTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            m_CurrentIntervalCount++;

            InterEventDispatcher.InvokeMatchDepartureTimerTickEvent
                (new MatchDepartureTimerTickEventArgs(m_Match, m_CurrentIntervalCount));

            if(m_CurrentIntervalCount >= m_Seconds)
            {
                m_DepartureTimer.Enabled = false;
                m_DepartureTimer.Dispose();

                InterEventDispatcher.InvokeMatchDepartureTimerExpiredEvent
                    (new MatchDepartureTimerEventArgs(m_Match, m_PlayerState));

                m_Match.DisposeOfDepartureTimer(this);
            }
        }
    }

    public class MatchState
    {
        string m_MatchId;
        bool m_InProgress;

        PlayerState m_PlayerOne;
        PlayerState m_PlayerTwo;

        Dictionary<string, PlayerState> m_Spectators = new Dictionary<string, PlayerState>();

        bool m_PlayerOneHasSyncedPieces = false;
        bool m_PlayerTwoHasSyncedPieces = false;

        static int m_MaxSpectators = 3;

        public string MatchIdentity { get { return m_MatchId; } }
        public bool InProgress { get { return m_InProgress; } set { m_InProgress = value; } }

        public bool FirstSyncCompleted { get { return m_PlayerOneHasSyncedPieces && m_PlayerTwoHasSyncedPieces; } }

        public PlayerState PlayerOne { get { return m_PlayerOne; } }
        public PlayerState PlayerTwo { get { return m_PlayerTwo; } }

        public Dictionary<string, PlayerState> 
            Spectators { get { return m_Spectators; } }

        public List<MatchDepartureTimer> 
            m_DepartureTimers = new List<MatchDepartureTimer>();

        public TurnState TurnState;

        public MatchState()
        {
            m_MatchId = Guid.NewGuid().ToString();
        }

        public MatchState(string matchId, PlayerState playerOne, PlayerState playerTwo)
        {
            m_MatchId = matchId;
            m_PlayerOne = playerOne;
            m_PlayerTwo = playerTwo;
        }

        public MatchState(string matchId, PlayerState player, int playerSlot)
        {
            m_MatchId = matchId;

            if (playerSlot == 1)
                m_PlayerOne = player;

            else 
                if (playerSlot == 2)
                    m_PlayerTwo = player;
        }

        public MatchState(string matchId, PlayerState playerOne)
        {
            m_MatchId = matchId;
            m_PlayerOne = playerOne;
        }

        public bool MatchIsFull()
        {
            return (m_PlayerOne != null && m_PlayerTwo != null);
        }

        public void GenerateTurnState()
        {
            TurnState = new TurnState
                (m_PlayerOne.AccountIdentity, m_PlayerTwo.AccountIdentity);
        }

        public int GetPlayerSlotFromId(string playerId)
        {
            if (m_PlayerOne != null && m_PlayerOne.AccountIdentity == playerId)
                return 1;

            else if (m_PlayerTwo != null && m_PlayerTwo.AccountIdentity == playerId)
                return 2;

            else
                return 0;
        }

        public bool AwaitingPlayerTimeout()
        {
            return m_DepartureTimers.Count > 0;
        }

        public bool IsParticipantId(string playerId)
        {
            return (m_PlayerOne != null && m_PlayerOne.AccountIdentity == playerId)
                || (m_PlayerTwo != null && m_PlayerTwo.AccountIdentity == playerId);
        }

        public void AddPlayer(string id, string username)
        {
            if (m_PlayerOne == null)
                m_PlayerOne = new PlayerState(id, username);

            else

            if (m_PlayerTwo == null)
                m_PlayerTwo = new PlayerState(id, username);

            if (MatchIsFull())
            {
                m_InProgress = true;
                GenerateTurnState();
            }
        }

        public bool CanAddSpectator()
        {
            return m_Spectators.Count < m_MaxSpectators;
        }

        public void AddSpectator(PlayerState state)
        {
            if (!m_Spectators.ContainsKey(state.AccountIdentity))
            {
                m_Spectators.Add(state.AccountIdentity, state);
            }
        }

        public void SyncAllNetStatesWithSpec(PlayerState state)
        {
            if (PlayerOne != null)
            {
                InterEventDispatcher.InvokeSpectatorJoinSyncEvent(new SpectatorJoinSyncEventArgs
                    (MatchIdentity, state.AccountIdentity, 
                        PlayerOne.AccountIdentity, PlayerOne.GamePieceNetworkStates.Values.ToArray()));
            }

            if (PlayerTwo != null)
            {
                InterEventDispatcher.InvokeSpectatorJoinSyncEvent(new SpectatorJoinSyncEventArgs
                    (MatchIdentity, state.AccountIdentity, 
                        PlayerTwo.AccountIdentity, PlayerTwo.GamePieceNetworkStates.Values.ToArray()));
            }
        }

        public void RemoveUserFromMatch(string id, out DepartureType departureType, out PlayerState userRemoved)
        {
            departureType = DepartureType.Unknown;
            userRemoved = null;

            if (PlayerOne != null && PlayerOne.AccountIdentity == id)
            {
                userRemoved = PlayerOne;
                m_PlayerOne = null;
                departureType = DepartureType.Participant;
            }

            else if (PlayerTwo != null && PlayerTwo.AccountIdentity == id)
            {
                userRemoved = PlayerTwo;
                m_PlayerTwo = null;
                departureType = DepartureType.Participant;
            }

            else if (m_Spectators.ContainsKey(id))
            {
                userRemoved = m_Spectators[id];
                m_Spectators.Remove(id);
                departureType = DepartureType.Spectator;
            }
        }

        public void RemoveUserFromMatch(string id)
        {
            if (PlayerOne != null && PlayerOne.AccountIdentity == id)
                m_PlayerOne = null;

            else if (PlayerTwo != null && PlayerTwo.AccountIdentity == id)
                m_PlayerTwo = null;

            else if (m_Spectators.ContainsKey(id))
                m_Spectators.Remove(id);
        }

        public PlayerState GetPlayerStateById(string id)
        {
            if (m_PlayerOne.AccountIdentity == id)
                return m_PlayerOne;
            else
                if (m_PlayerTwo.AccountIdentity == id)
                return m_PlayerTwo;
            else
                return null;
        }

        public void BeginDepartureTimer(string accountId)
        {
            m_DepartureTimers.Add(new MatchDepartureTimer(this, accountId, 60));
        }

        public void DisposeOfDepartureTimer(MatchDepartureTimer timer)
        {
            if (m_DepartureTimers.Contains(timer))
                m_DepartureTimers.Remove(timer);

            timer = null;
        }

        public string[] GetAccountIdentities()
        {
            List<string> temp = new List<string>();

            if (m_PlayerOne != null)
                temp.Add(m_PlayerOne.AccountIdentity);

            if (m_PlayerTwo != null)
                temp.Add(m_PlayerTwo.AccountIdentity);

            foreach (PlayerState player in m_Spectators.Values)
                temp.Add(player.AccountIdentity);

            return temp.ToArray();
        }

        public bool OpposingPlayerInMatch(string accountId)
        {
            if (m_PlayerOne != null && m_PlayerOne.AccountIdentity == accountId)
                return m_PlayerTwo != null;
            if (m_PlayerTwo != null && m_PlayerTwo.AccountIdentity == accountId)
                return m_PlayerOne != null;

            else
                return false;
        }

        public void SyncGamePieceNetworkState(GamePieceNetworkState pieceNetworkState)
        {
            PlayerState player = GetPlayerStateById
                (pieceNetworkState.ControllerIdentity);
            
            if (player == null)
                return;

            player.SyncGamePieceNetworkState(pieceNetworkState);
        }

        public void SendPiecesSyncToUsersInMatch(string controllerId)
        {
            PlayerState player = GetPlayerStateById(controllerId);

            if (player == null)
                return;

            player.SendModifiedPieceSync(MatchIdentity);
        }

        public void FlagFirstPieceSyncForPlayer(string controllerId)
        {
            if (PlayerOne.AccountIdentity == controllerId)
                m_PlayerOneHasSyncedPieces = true;
            else
                if (PlayerTwo.AccountIdentity == controllerId)
                    m_PlayerTwoHasSyncedPieces = true;
            
            if(m_PlayerOneHasSyncedPieces && m_PlayerTwoHasSyncedPieces)
                InterEventDispatcher.InvokeMatchStartEvent
                    (new MatchStartEventArgs(MatchIdentity, TurnState.CurrentPlayerId));
        }

        public void SyncAttackTarget(string attackerId, string targetId)
        {
            if(PlayerOne.GamePieceNetworkStates.ContainsKey(attackerId))
            {
                GamePieceNetworkState attacker = PlayerOne.GamePieceNetworkStates[attackerId];
                if(PlayerTwo.GamePieceNetworkStates.ContainsKey(targetId))
                {
                    GamePieceNetworkState target = PlayerTwo.GamePieceNetworkStates[targetId];
                    target.ProcessDamage(attacker.AttackRating);
                    PlayerTwo.UpdateGamePieceNetState(target, true);
                }
            }

            else if (PlayerTwo.GamePieceNetworkStates.ContainsKey(attackerId))
            {
                GamePieceNetworkState attacker = PlayerTwo.GamePieceNetworkStates[attackerId];
                if (PlayerOne.GamePieceNetworkStates.ContainsKey(targetId))
                {
                    GamePieceNetworkState target = PlayerOne.GamePieceNetworkStates[targetId];
                    target.ProcessDamage(attacker.AttackRating);
                    PlayerOne.UpdateGamePieceNetState(target, true);
                }
            }
        }

        public void ProcessAttackSyncComplete(string attackerId, int x, int y)
        {
            if (PlayerOne.GamePieceNetworkStates.ContainsKey(attackerId))
            {
                PlayerTwo.SendModifiedPieceSync(MatchIdentity, false);
                InterEventDispatcher.InvokeAttackSyncCompleteEvent
                    (new AttackSyncCompleteEventArgs(MatchIdentity, attackerId, PlayerTwo.AccountIdentity, x, y));
                return;
            }

            if (PlayerTwo.GamePieceNetworkStates.ContainsKey(attackerId))
            {
                PlayerOne.SendModifiedPieceSync(MatchIdentity, false);
                InterEventDispatcher.InvokeAttackSyncCompleteEvent
                    (new AttackSyncCompleteEventArgs(MatchIdentity, attackerId, PlayerOne.AccountIdentity, x, y));
                return;
            }
        }
    }
}
