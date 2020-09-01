using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Eta.Interdata;

namespace EtaServer
{
    class MatchHandler
    {
        private static Dictionary<string, MatchState> 
            m_Matches = new Dictionary<string, MatchState>();

        internal static Dictionary<string, MatchState> MatchesOnline { get { return m_Matches; } }

        internal static void EndMatch(string matchId)
        {
            if (m_Matches.ContainsKey(matchId))
                m_Matches.Remove(matchId);
        }

        internal static MatchState GetMatchStateById(string id)
        {
            if (m_Matches.ContainsKey(id))
                return m_Matches[id];
            else
                return null;
        }

        internal static void AttemptToJoinMatch(ClientState client, string matchId)
        {
            if(m_Matches.ContainsKey(matchId))
            {
                MatchState match = m_Matches[matchId];

                if (match.MatchIsFull())
                    MatchEventDispatcher.InvokeMatchJoinResultEvent(new MatchJoinEventArgs(client, match.MatchIdentity, false));
                else
                {
                    AddPlayerToMatch(match, client.AccountRelative.AccountId, client.AccountRelative.Username);

                    MatchEventDispatcher.InvokeMatchJoinResultEvent(new MatchJoinEventArgs(client, match.MatchIdentity, true));

                    MatchEventDispatcher.InvokeMatchSyncEvent(new MatchSyncEventArgs(match));
                }
            }
        }

        internal static void CreateNewMatch(string playerOneId, string playerOneName)
        {
            try
            {
                MatchState match = new MatchState();

                if (m_Matches.ContainsKey(match.MatchIdentity))
                {
                    Console.WriteLine("Created Match With Duplicate Identity..");
                    CreateNewMatch(playerOneId, playerOneName);
                }

                else
                {
                    AddPlayerToMatch(match, playerOneId, playerOneName);
                    m_Matches.Add(match.MatchIdentity, match);

                    if (ServerCore.DebugMode)
                        Console.WriteLine("New Match ({0}) Created: {1} : {2}", 
                            match.MatchIdentity.Substring(0, 8), match.PlayerOne.AccountIdentity, match.PlayerOne.Username);

                    MatchEventDispatcher.InvokeMatchCreatedEvent(new MatchCreatedEventArgs(match));
                }
            }

            catch (Exception e) { Console.WriteLine(e.ToString()); }
        }

        internal static void SendMessageToUsersInMatch(MatchState match, string msg)
        {
            if(match != null)
            {
                if(match.PlayerOne != null)
                    ClientManager.SendMessageToClientByAccount(match.PlayerOne.AccountIdentity, msg);
                if (match.PlayerTwo != null)
                    ClientManager.SendMessageToClientByAccount(match.PlayerTwo.AccountIdentity, msg);

                foreach(PlayerState player in match.Spectators.Values.ToList())
                {
                    ClientManager.SendMessageToClientByAccount(player.AccountIdentity, msg);
                }
            }
        }

        internal static void SendMessageToUsersInMatch(string matchId, string msg)
        {
            if (m_Matches.ContainsKey(matchId))
            {
                MatchState match = m_Matches[matchId];
                if (match.PlayerOne != null)
                    ClientManager.SendMessageToClientByAccount(match.PlayerOne.AccountIdentity, msg);
                if (match.PlayerTwo != null)
                    ClientManager.SendMessageToClientByAccount(match.PlayerTwo.AccountIdentity, msg);

                foreach (PlayerState player in match.Spectators.Values.ToList())
                {
                    ClientManager.SendMessageToClientByAccount(player.AccountIdentity, msg);
                }
            }
        }

        internal static void AddPlayerToMatch(MatchState match, string accountId, string username)
        {
            try
            {
                match.AddPlayer(accountId, username);

                AccountHandler.GetAccountById(accountId).SetCurrentMatch(match);
            }

            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        internal static void AddSpectatorToMatch(string matchId, string clientId, string accountId, string username)
        {
            if(m_Matches.ContainsKey(matchId))
            {
                MatchState match = m_Matches[matchId];
                AccountHandler.GetAccountById(accountId).SetCurrentMatch(match);

                if (match.CanAddSpectator())
                {
                    MatchEventDispatcher.InvokeSpectatorJoinEvent
                        (new SpectatorJoinEventArgs(matchId, true, clientId));

                    PlayerState spectator = new PlayerState(accountId, username);

                    match.AddSpectator(spectator);

                    MatchEventDispatcher.InvokeSpectatorSyncEvent
                        (new SpectatorSyncEventArgs(match, spectator));

                    match.SyncAllNetStatesWithSpec(spectator);
                }

                else
                {
                    MatchEventDispatcher.InvokeSpectatorJoinEvent
                        (new SpectatorJoinEventArgs(matchId, false, clientId));
                }
            }
        }

        internal static void RemoveUserFromMatch(string matchId, string accountId)
        {
            if (m_Matches.ContainsKey(matchId))
            {
                MatchState match = m_Matches[matchId];

                PlayerState userRemoved;
                DepartureType departureType = DepartureType.Unknown;

                match.RemoveUserFromMatch(accountId, out departureType, out userRemoved);

                if (match.PlayerOne == null & match.PlayerTwo == null)
                {
                    m_Matches.Remove(match.MatchIdentity);
                    MatchEventDispatcher.InvokeMatchEndEvent(new MatchEndEventArgs(matchId));
                }

                else
                    MatchEventDispatcher.InvokeUserDepartedEvent
                        (new UserDepartedEventArgs(match, departureType, userRemoved));
            }
        }

        internal static void RemoveUserFromMatch(MatchState match, string accountId)
        {
            PlayerState userRemoved;
            DepartureType departureType = DepartureType.Unknown;

            match.RemoveUserFromMatch(accountId, out departureType, out userRemoved);

            if (match.PlayerOne == null & match.PlayerTwo == null)
            {
                if (m_Matches.ContainsKey(match.MatchIdentity))
                {
                    m_Matches.Remove(match.MatchIdentity);
                    MatchEventDispatcher.InvokeMatchEndEvent
                        (new MatchEndEventArgs(match.MatchIdentity));
                }
            }

            else
                MatchEventDispatcher.InvokeUserDepartedEvent
                    (new UserDepartedEventArgs(match, departureType, userRemoved));
        }

        internal static void HandleDisconnectFromMatch(MatchState match, string accountId)
        {
            if (match.InProgress && (match.OpposingPlayerInMatch(accountId) || match.AwaitingPlayerTimeout()))
            {
                if (match.IsParticipantId(accountId))
                {
                    match.BeginDepartureTimer(accountId);
                    InterEventDispatcher.InvokeDCFromMatchInProgressEvent
                        (new DCFromMatchInProgressEventArgs(match, match.GetPlayerStateById(accountId)));
                }

                else RemoveUserFromMatch(match, accountId);
            }

            else RemoveUserFromMatch(match, accountId);
        }

        internal static void SyncGamePieceToMatch(string matchId, GamePieceNetworkState pieceNetworkState)
        {
            if(m_Matches.ContainsKey(matchId))
            {
                MatchState match = m_Matches[matchId];
                match.SyncGamePieceNetworkState(pieceNetworkState);
            }
        }

        internal static void HandlePieceSyncComplete(string matchId, string controllerId)
        {
            if (m_Matches.ContainsKey(matchId))
            {
                MatchState match = m_Matches[matchId];
                match.SendPiecesSyncToUsersInMatch(controllerId);
            }
        }

        internal static void HandleTurnStateUpdate(string matchId, int turnStep)
        {
            if(m_Matches.ContainsKey(matchId))
            {
                MatchState match = m_Matches[matchId];
                match.TurnState.SetCurrentStep((TurnStep)turnStep);

                InterEventDispatcher.InvokeTurnStateSyncEvent(new TurnStateSyncEventArgs(matchId, 
                    match.TurnState.CurrentPlayerId, (int)match.TurnState.CurrentTurnStep, (int)match.TurnState.LowestTurnStep));
            }
        }

        internal static void FlagFirstSyncForPlayer(string matchId, string controllerId)
        {
            if (m_Matches.ContainsKey(matchId))
            {
                MatchState match = m_Matches[matchId];
                if(!match.FirstSyncCompleted)
                    match.FlagFirstPieceSyncForPlayer(controllerId);
            }
        }

        internal static void SynchAttackTarget(string matchId, string attackerId, string targetId)
        {
            if (m_Matches.ContainsKey(matchId))
            {
                MatchState match = m_Matches[matchId];
                match.SyncAttackTarget(attackerId, targetId);
            }
        }

        internal static void ProcessAttackSyncComplete(string matchId, string attackerId, int x, int y)
        {
            if (m_Matches.ContainsKey(matchId))
            {
                MatchState match = m_Matches[matchId];
                match.ProcessAttackSyncComplete(attackerId, x, y);
            }
        }
    }
}
