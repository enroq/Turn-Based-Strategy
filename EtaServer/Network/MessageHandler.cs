using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Eta.Interdata;

namespace EtaServer
{
    /// <summary>
    /// Determines How To Handle Messages Recieved Based On Message Content And Relays Information Back To Client(s).
    /// </summary>
    class MessageHandler
    {
        private static string m_SegmentTerminator = "#|";
        private static string[] 
            m_SegmentTerminatorArray = new string[] { m_SegmentTerminator };

        public MessageHandler()
        {
            NetworkEventDispatcher.ClientMessageEvent += EventDispatcher_MessageIsolated;
            NetworkEventDispatcher.AccountCreationFailedEvent += NetworkEventDispatcher_AccountCreationFailedEvent;
            NetworkEventDispatcher.AccountCreationSucessEvent += NetworkEventDispatcher_AccountCreationSucessEvent;
            NetworkEventDispatcher.UserLoginEvent += NetworkEventDispatcher_UserLoginEvent;
            NetworkEventDispatcher.ClientConnectionEvent += NetworkEventDispatcher_ClientConnectionEvent;
            NetworkEventDispatcher.ClientDisconnectEvent += NetworkEventDispatcher_ClientDisconnectEvent;
            NetworkEventDispatcher.ClientCirculationEvent += NetworkEventDispatcher_ClientCirculationEvent;

            MatchEventDispatcher.MatchCreatedEvent += MatchEventDispatcher_MatchCreatedEvent;
            MatchEventDispatcher.MatchJoinResultEvent += MatchEventDispatcher_MatchJoinResultEvent;
            MatchEventDispatcher.MatchSyncEvent += MatchEventDispatcher_MatchSyncEvent;
            MatchEventDispatcher.MatchEndEvent += MatchEventDispatcher_MatchEndEvent;
            MatchEventDispatcher.SpectatorJoinedEvent += MatchEventDispatcher_SpectatorJoinedEvent;
            MatchEventDispatcher.UserDepartedEvent += MatchEventDispatcher_UserDepartedEvent;
            MatchEventDispatcher.SpectatorSyncEvent += MatchEventDispatcher_SpectatorSyncEvent;

            InterEventDispatcher.DCFromMatchInProgressEvent += InterEventDispatcher_DCFromMatchInProgressEvent;
            InterEventDispatcher.MatchDepartureTimerExpiredEvent += InterEventDispatcher_MatchDepartureTimerExpiredEvent;
            InterEventDispatcher.SendGamePiecesSyncEvent += InterEventDispatcher_SendGamePiecesSyncEvent;
            InterEventDispatcher.SpectatorJoinSyncEvent += InterEventDispatcher_SpectatorJoinSyncEvent;
            InterEventDispatcher.MatchStartEvent += InterEventDispatcher_MatchStartEvent;
            InterEventDispatcher.TurnStateSyncEvent += InterEventDispatcher_TurnStateSyncEvent;
            InterEventDispatcher.AttackSyncCompleteEvent += InterEventDispatcher_AttackSyncCompleteEvent;
        }

        private void InterEventDispatcher_AttackSyncCompleteEvent(AttackSyncCompleteEventArgs args)
        {
            string cmd = string.Format("{0}{1}{2}{1}{3}{1}{4}{1}{5}{1}{6}{1}{7}",
                (int)ReadProtocol.GetVersion(),
                m_SegmentTerminator,
                NetworkCommand.GetCommand(NetworkCommandType.ProcessAttack),
                args.MatchId,
                args.AttackingPieceId,
                args.DefendingPlayerId,
                args.BoardVectorX.ToString(),
                args.BoardVectorY.ToString());

            MatchHandler.SendMessageToUsersInMatch(args.MatchId, cmd);
        }

        private void InterEventDispatcher_TurnStateSyncEvent(TurnStateSyncEventArgs args)
        {
            string cmd = string.Format("{0}{1}{2}{1}{3}{1}{4}{1}{5}{1}{6}",
                (int)ReadProtocol.GetVersion(),
                m_SegmentTerminator,
                NetworkCommand.GetCommand(NetworkCommandType.TurnStateSync),
                args.MatchId,
                args.CurrentPlayerId,
                args.TurnStep.ToString(),
                args.LowestTurnStep.ToString());

            MatchHandler.SendMessageToUsersInMatch(args.MatchId, cmd);
        }

        private void InterEventDispatcher_MatchStartEvent(MatchStartEventArgs args)
        {
            string cmd = string.Format("{0}{1}{2}{1}{3}{1}{4}",
                (int)ReadProtocol.GetVersion(),
                m_SegmentTerminator,
                NetworkCommand.GetCommand(NetworkCommandType.StartMatch),
                args.MatchId,
                args.PlayerIdOfFirstToMove);

            MatchHandler.SendMessageToUsersInMatch(args.MatchId, cmd);
        }

        private void MatchEventDispatcher_SpectatorSyncEvent(SpectatorSyncEventArgs args)
        {
            string cmd = string.Format("{0}{1}{2}{1}{3}{1}{4}{1}{5}",
                (int)ReadProtocol.GetVersion(),
                m_SegmentTerminator,
                NetworkCommand.GetCommand(NetworkCommandType.SyncMatchSpectator),
                args.Match.MatchIdentity,
                args.PlayerState.AccountIdentity,
                args.PlayerState.Username);

            MatchHandler.SendMessageToUsersInMatch(args.Match, cmd);
        }

        private void InterEventDispatcher_SpectatorJoinSyncEvent(SpectatorJoinSyncEventArgs args)
        {
            SendGamePieceSyncToSpectator
                (args.MatchId, args.SpectatorId, args.ControllerId, args.GamePieceNetworkStates);
        }

        private void InterEventDispatcher_SendGamePiecesSyncEvent(SendGamePiecesSyncEventArgs args)
        {
            SendGamePieceSyncToMatch
                (args.MatchId, args.ControllerId, args.GamePieceNetworkStates, args.EndSync);
        }

        private void SendGamePieceSyncToSpectator
            (string matchId,string spectatorId, string controllerId, GamePieceNetworkState[] pieces)
        {
            for (int i = 0; i < pieces.Length; i++)
            {
                MatchHandler.SendMessageToUsersInMatch
                    (matchId, GetSyncCommandFromPieceNetworkState(matchId, pieces[i]));
            }

            string cmd = string.Format("{0}{1}{2}{1}{3}{1}{4}",
                (int)ReadProtocol.GetVersion(),
                m_SegmentTerminator,
                NetworkCommand.GetCommand(NetworkCommandType.EndGamePieceSync),
                matchId,
                controllerId);

            ClientManager.SendMessageToClientByAccount(spectatorId, cmd);
        }

        private void SendGamePieceSyncToMatch(string matchId, string controllerId, GamePieceNetworkState[] pieces, bool endSync)
        {
            for(int i = 0; i < pieces.Length; i++)
            {
                MatchHandler.SendMessageToUsersInMatch
                    (matchId, GetSyncCommandFromPieceNetworkState(matchId, pieces[i]));
            }

            if (endSync)
            {
                string cmd = string.Format("{0}{1}{2}{1}{3}{1}{4}",
                    (int)ReadProtocol.GetVersion(),
                    m_SegmentTerminator,
                    NetworkCommand.GetCommand(NetworkCommandType.EndGamePieceSync),
                    matchId,
                    controllerId);

                MatchHandler.FlagFirstSyncForPlayer(matchId, controllerId);
                MatchHandler.SendMessageToUsersInMatch(matchId, cmd);
            }
        }

        private string GetSyncCommandFromPieceNetworkState(string matchId, GamePieceNetworkState piece)
        {
            return string.Format("{0}{1}{2}{1}{3}{1}{4}{1}{5}{1}{6}{1}{7}{1}{8}{1}{9}{1}{10}{1}{11}{1}{12}{1}{13}{1}{14}",
                (int)ReadProtocol.GetVersion(),
                m_SegmentTerminator,
                NetworkCommand.GetCommand(NetworkCommandType.SyncGamePiece),
                matchId,
                piece.ControllerIdentity,
                piece.ControllerPosition,
                piece.NetworkIdentity,
                piece.GamePieceName,
                piece.PositionX,
                piece.PositionY,
                piece.Hitpoints,
                piece.DefenseRating,
                piece.AttackRating,
                piece.TurnDelay,
                piece.Rotation);
        }

        private void InterEventDispatcher_MatchDepartureTimerExpiredEvent(MatchDepartureTimerEventArgs args)
        {
            MatchHandler.RemoveUserFromMatch(args.Match, args.PlayerState.AccountIdentity);
        }

        private void InterEventDispatcher_DCFromMatchInProgressEvent(DCFromMatchInProgressEventArgs args)
        {
            string cmd = string.Format("{0}{1}{2}{1}{3}{1}{4}",
                (int)ReadProtocol.GetVersion(),
                m_SegmentTerminator,
                NetworkCommand.GetCommand(NetworkCommandType.UserLeftMatchInProg),
                args.Match.MatchIdentity,
                args.PlayerState.AccountIdentity);

            string[] Ids = args.Match.GetAccountIdentities();

            for (int i = Ids.Length - 1; i >= 0; i--)
                ClientManager.SendMessageToClientByAccount(Ids[i], cmd);
        }

        private void MatchEventDispatcher_UserDepartedEvent(UserDepartedEventArgs args)
        {
            if(args.DepartureType == DepartureType.Participant)
            {
                ClientManager.SendMessageToAllClients(GetMatchSyncCommand(args.Match));
            }

            else if(args.DepartureType == DepartureType.Spectator)
            {
                SendSpectatorDepartureNotification(args.Match.MatchIdentity, args.PlayerState);
            }

            else if(args.DepartureType == DepartureType.Unknown)
            {
                Console.WriteLine("[{0}] User Departure Returned Unknown..", args.Match.MatchIdentity);
            }
        }

        private void SendSpectatorDepartureNotification(string matchId, PlayerState spectator)
        {
            string cmd = string.Format("{0}{1}{2}{1}{3}{1}{4}{1}{5}",
                (int)ReadProtocol.GetVersion(),
                m_SegmentTerminator,
                NetworkCommand.GetCommand(NetworkCommandType.RemoveSpectator),
                matchId,
                spectator.AccountIdentity,
                spectator.Username);

            ClientManager.SendMessageToAllClients(cmd);
        }

        private void MatchEventDispatcher_SpectatorJoinedEvent(SpectatorJoinEventArgs args)
        {
            HandleSpectatorJoinEvent(args.MatchId, args.ClientId, args.Succeeded);
        }

        private void MatchEventDispatcher_MatchEndEvent(MatchEndEventArgs args)
        {
            HandleMatchEndEvent(args.MatchId);
        }

        private void MatchEventDispatcher_MatchSyncEvent(MatchSyncEventArgs args)
        {
            ClientManager.SendMessageToAllClients(GetMatchSyncCommand(args.Match));
        }

        private void MatchEventDispatcher_MatchJoinResultEvent(MatchJoinEventArgs args)
        {
            HandleMatchJoinResult(args.Client, args.MatchId, args.JoinSucceeded);
        }

        private void HandleSpectatorJoinEvent(string matchId, string clientId, bool succeeded)
        {
            MatchState match = MatchHandler.GetMatchStateById(matchId);

            string cmd = string.Format("{0}{1}{2}{1}{3}{1}{4}",
                (int)ReadProtocol.GetVersion(),
                m_SegmentTerminator,
                NetworkCommand.GetCommand(NetworkCommandType.SpectateMatchResult),
                matchId,
                succeeded.ToString().ToLowerInvariant());

            ClientManager.GetClientById(clientId).SendData(cmd);

            SyncMatchSpectators(match);
        }

        private void HandleMatchEndEvent(string matchId)
        {
            string cmd = string.Format("{0}{1}{2}{1}{3}",
                (int)ReadProtocol.GetVersion(),
                m_SegmentTerminator,
                NetworkCommand.GetCommand(NetworkCommandType.EndMatch),
                matchId);

            ClientManager.SendMessageToAllClients(cmd);
        }

        private void HandleMatchJoinResult(ClientState client, string matchId, bool succeeeded)
        {
            string cmd = string.Format("{0}{1}{2}{1}{3}{1}{4}",
                (int)ReadProtocol.GetVersion(),
                m_SegmentTerminator,
                NetworkCommand.GetCommand(NetworkCommandType.JoinMatchResult),
                succeeeded.ToString(),
                matchId);

            client.SendData(cmd);
        }

        private void MatchEventDispatcher_MatchCreatedEvent(MatchCreatedEventArgs args)
        {
            HandleMatchCreationEvent(args.Match);
        }

        private void NetworkEventDispatcher_ClientCirculationEvent(ClientCirculationEventArgs args)
        {
            string cmd = string.Format("{0}{1}{2}",
                (int)ReadProtocol.GetVersion(),
                 m_SegmentTerminator,
                 NetworkCommand.GetCommand(NetworkCommandType.Heartbeat));

            if (args.ClientState != null)
                args.ClientState.SendData(cmd);
        }

        private void NetworkEventDispatcher_ClientDisconnectEvent(ClientDisconnectEventArgs args)
        {
            if(args.ClientState.AccountRelative != null)
                SendAccountSyncDataToAllClients
                    (args.ClientState.AccountRelative, args.ClientState, AccountSyncType.Disconnect);
        }

        private void NetworkEventDispatcher_ClientConnectionEvent(ClientConnectionEventArgs args)
        {
            ClientState client = args.ClientState;

            if(client != null)
            {
                string s = string.Format("{0}", RSAModule.GetPublicRsa);

                client.SendData(s);
            }
        }

        private void NetworkEventDispatcher_UserLoginEvent(UserLoginEventArgs args)
        {
            try
            {
                ClientState client = ClientManager.GetClientById(args.ClientId);
                if (client != null)
                {
                    if (args.Success)
                    {
                        Account account = new Account(args.ClientId, args.Username);
                        AccountHandler.AddOnlineAccount(account);

                        client.Authorize(account);

                        //0#|lgnsccs#|username#|email#|rating#|wins#|losses#|id
                        string s = string.Format("{0}{1}{2}{1}{3}{1}{4}{1}{5}{1}{6}{1}{7}{1}{8}",
                            (int)ReadProtocol.GetVersion(),
                            m_SegmentTerminator,   
                            NetworkCommand.GetCommand(NetworkCommandType.LoginSuccess),
                            account.Username, 
                            account.Email, 
                            account.Rating, 
                            account.Wins, 
                            account.Losses,
                            account.AccountId);

                        client.SendData(s);

                        SendAllAccountSyncDataToClient(client, AccountSyncType.Connect);
                        SendAccountSyncDataToAllClients(account, client, AccountSyncType.Connect);

                        SyncUserFriends(account);
                        SyncAllAvailableMatchesWithClient(client);
                    }

                    else
                    {
                        string s = string.Format("{0}{1}{2}{1}{3}",
                            (int)ReadProtocol.GetVersion(), 
                            m_SegmentTerminator,
                            NetworkCommand.GetCommand(NetworkCommandType.LoginFail),
                            args.LoggedIn.ToString());

                        client.SendData(s);
                    }
                }
            }

            catch(Exception e) { Console.WriteLine(e.ToString()); }
        }

        private void NetworkEventDispatcher_AccountCreationSucessEvent(AccountCreationSuccessArgs args)
        {
            ClientState client = ClientManager.GetClientById(args.ClientId);

            if(client != null)
            {
                string s = (string.Format("{0}{1}{2}", 
                    (int)ReadProtocol.GetVersion(),
                    m_SegmentTerminator, 
                    NetworkCommand.GetCommand(NetworkCommandType.AccountCreated)));

                client.SendData(s);
            }
        }

        private void NetworkEventDispatcher_AccountCreationFailedEvent(AccountCreationFailedArgs args)
        {
            ClientState client = ClientManager.GetClientById(args.ClientId);

            if (client != null)
            {
                string s = string.Format("{0}{1}{2}{1}{3}",
                    (int)ReadProtocol.GetVersion(), 
                    m_SegmentTerminator, 
                    NetworkCommand.GetCommand(NetworkCommandType.AccountCreationFailed), 
                    (int)args.FailureType);

                client.SendData(s);
            }
        }

        private void EventDispatcher_MessageIsolated(ClientMessageEventArgs args)
        {
            if (ServerCore.DebugMode)
            {
                Console.WriteLine("Message Dispatched On Thread: " + Thread.CurrentThread.ManagedThreadId);
                Console.WriteLine(String.Format("Message From: [{0}] ({1})", args.ClientId, args.Message.Replace("#|", " ")));
            }

            ParseMessageByProtocol(args.ClientId, args.Message);
        }

        private void ParseMessageByProtocol(string id, string message)
        {
            string[] messageSegments = message.Split(m_SegmentTerminatorArray, StringSplitOptions.None);
            try
            {
                //Protocol Versions Used For Backwards Compatability With Older Clients.
                ProtocolVersion version = (ProtocolVersion)(Int32.Parse(messageSegments[0]));
                switch (version)
                {
                    case ProtocolVersion.One:
                        {
                            ParseMessage_ProtocolOne(id, messageSegments);
                            break;
                        }
                    default: break;
                }
            }

            catch (Exception e) { Console.WriteLine(e.ToString()); }
        }

        private void ParseMessage_ProtocolOne(string clientId, string[] segments)
        {
            if (ServerCore.DebugMode)
                Console.WriteLine("Processing Command: " + segments[1]);

            //Protocol One Dictates Command Type Comes From Segment One.
            int commandIndex = NetworkCommand.GetCommandIndex(segments[1]);
            switch(commandIndex)
            {
                case (int)NetworkCommandType.Login:
                    {
                        HandleAccountLoginRequest_One(clientId, segments);
                        break;
                    }
                case (int)NetworkCommandType.CreateAccount:
                    {
                        HandleAccountCreationRequest_One(clientId, segments);
                        break;
                    }
                case (int)NetworkCommandType.LobbyMessage:
                    {
                        HandleLobbyMessage_One(clientId, segments);
                        break;
                    }
                case (int)NetworkCommandType.LogOut:
                    {
                        HandleLogout_One(clientId);
                        break;
                    }
                case (int)NetworkCommandType.Heartbeat:
                    {
                        HandleCirculation_One(clientId);
                        break;
                    }

                case (int)NetworkCommandType.PrivateMessage:
                    {
                        HandlePrivateMessage_One(segments);
                        break;
                    }
                case (int)NetworkCommandType.AddFriend:
                    {
                        HandleFriendAddition_One(clientId, segments);
                        break;
                    }
                case (int)NetworkCommandType.FriendRequest:
                    {
                        HandleFriendRequest_One(clientId, segments);
                        break;
                    }
                case (int)NetworkCommandType.RemoveFriend:
                    {
                        HandleFriendRemoval_One(clientId, segments);
                        break;
                    }
                case (int)NetworkCommandType.CreateMatch:
                    {
                        AttemptMatchCreation_One(clientId, segments);
                        break;
                    }
                case (int)NetworkCommandType.EndMatch:
                    {
                        EndMatch_One(segments);
                        break;
                    }
                case (int)NetworkCommandType.AttemptJoinMatch:
                    {
                        ProcessJoinMatchAttempt_One(clientId, segments);
                        break;
                    }
                case (int)NetworkCommandType.SpectateMatch:
                    {
                        ProcessSpectateMatchRequest_One(clientId, segments);
                        break;
                    }
                case (int)NetworkCommandType.MatchMessage:
                    {
                        HandleMatchMessage_One(segments);
                        break;
                    }
                case (int)NetworkCommandType.SyncGamePiece:
                    {
                        HandleGamePieceSync_One(segments);
                        break;
                    }
                case (int)NetworkCommandType.EndGamePieceSync:
                    {
                        HandleEndPieceSync_One(segments);
                        break;
                    }
                case (int)NetworkCommandType.NetworkPieceMoveEvent:
                    {
                        HandleNetworkPieceMove_One(segments);
                        break;
                    }
                case (int)NetworkCommandType.TurnStateUpdate:
                    {
                        HandleTurnStateUpdate_One(segments);
                        break;
                    }
                case (int)NetworkCommandType.SyncAttackTarget:
                    {
                        HandleSyncAttackTarget_One(segments);
                        break;
                    }
                case (int)NetworkCommandType.ProcessAttack:
                    {
                        ProcessAttackSyncComplete_One(segments);
                        break;
                    }
                default:
                    {
                        Console.WriteLine
                            ("Unknown Network Command: {0}", segments[1]);
                        break;
                    }
            }
        }

        private void ProcessAttackSyncComplete_One(string[] segments)
        {
            try
            {
                MatchHandler.ProcessAttackSyncComplete
                    (segments[2], segments[3], Int32.Parse(segments[4]), Int32.Parse(segments[5]));
            }
            
            catch(Exception e) { Console.WriteLine(e.ToString()); }
        }

        private void HandleSyncAttackTarget_One(string[] segments)
        {   //protocol#|cmd#|matchId#|attackerId#|dmg#|targetId
            try
            {
                MatchHandler.SynchAttackTarget
                    (segments[2], segments[3], segments[4]);
            }

            catch(Exception e) { Console.WriteLine(e.ToString()); }
        }

        private void HandleTurnStateUpdate_One(string[] segments)
        {
            try
            {
                MatchHandler.HandleTurnStateUpdate(segments[2], Int32.Parse(segments[3]));
            }

            catch(Exception e) { Console.WriteLine(e.ToString()); }
        }

        private void HandleNetworkPieceMove_One(string[] segments)
        {
            //protocol#|command#|matchId#|pieceNetId#|x#|y

            string cmd = string.Format("{0}{1}{2}{1}{3}{1}{4}{1}{5}{1}{6}",
                (int)ReadProtocol.GetVersion(),
                m_SegmentTerminator,
                NetworkCommand.GetCommand(NetworkCommandType.NetworkPieceMoveEvent),
                segments[2],
                segments[3],
                segments[4],
                segments[5]);

            MatchHandler.SendMessageToUsersInMatch(segments[2], cmd);
        }

        private void HandleEndPieceSync_One(string[] segments)
        {
            //protocol#|cmd#|matchId#|accountId
            MatchHandler.HandlePieceSyncComplete(segments[2], segments[3]);
        }

        private void HandleGamePieceSync_One(string[] segments)
        {
            //0#|syncgmepce#|matchId#|playerAccountId#|playerSlot#|pieceNetworkId#|pieceName
            // #|positionX#|positionY#|hitpoints#|defense#|attack#|turnDelay#|rotation

            try
            {
                GamePieceNetworkState pieceNetworkState = new GamePieceNetworkState
                (
                    segments[3],
                    Int32.Parse(segments[4]),
                    segments[5],
                    segments[6],
                    Int32.Parse(segments[7]),
                    Int32.Parse(segments[8]),
                    Int32.Parse(segments[9]),
                    Int32.Parse(segments[10]),
                    Int32.Parse(segments[11]),
                    Int32.Parse(segments[12]),
                    float.Parse(segments[13])
                );

                MatchHandler.SyncGamePieceToMatch(segments[2], pieceNetworkState);
            }

            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void HandleMatchMessage_One(string[] segments)
        {   
            //protocol#|lbymsg#|matchId#|username#|time#|message<#>
            string cmd = string.Format("{0}{1}{2}{1}{3}{1}{4}{1}{5}{1}{6}",
                (int)ReadProtocol.GetVersion(),
                m_SegmentTerminator,
                NetworkCommand.GetCommand(NetworkCommandType.MatchMessage),
                segments[2],
                segments[3],
                segments[4],
                segments[5]);

            MatchHandler.SendMessageToUsersInMatch(segments[2], cmd);
        }

        private void ProcessSpectateMatchRequest_One(string clientId, string[] segments)
        {
            Account account = ClientManager.GetClientById(clientId).AccountRelative;
            MatchHandler.AddSpectatorToMatch
                (segments[2], clientId, account.AccountId, account.Username);
        }

        private void ProcessJoinMatchAttempt_One(string clientId, string[] segments)
        {
            MatchHandler.AttemptToJoinMatch
                (ClientManager.GetClientById(clientId), segments[2]);
        }

        private void EndMatch_One(string[] segments)
        {
            string matchId = segments[2];

            MatchHandler.EndMatch(matchId);

            MatchEventDispatcher.InvokeMatchEndEvent(new MatchEndEventArgs(matchId));
        }

        private void AttemptMatchCreation_One(string clientId, string[] segments)
        {
            try
            {   //protocol#|crtmtch#|userId#|username
                string accountId = segments[2];
                string username = segments[3];

                MatchHandler.CreateNewMatch(accountId, username);
            }

            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void HandleFriendRemoval_One(string clientId, string[] segments)
        {
            //protocol#|rmfrnd#|id
            string idTo = segments[2];
            string idFrom = ClientManager.GetClientById(clientId).AccountRelative.AccountId;

            AccountDatabaseHandler.RemoveUserRelationships(idFrom, idTo);
            AccountDatabaseHandler.RemoveUserRelationships(idTo, idFrom);

            string cmd = string.Format("{0}{1}{2}{1}{3}",
            (int)ReadProtocol.GetVersion(),
            m_SegmentTerminator,
            NetworkCommand.GetCommand(NetworkCommandType.RemoveFriend),
            idFrom);

            if (AccountHandler.AccountOnline(AccountDatabaseHandler.GetAccountNameFromId(idTo)))
                ClientManager.SendMessageToClientByAccount(AccountHandler.GetAccountById(idTo), cmd);
        }

        private void HandleFriendRequest_One(string clientId, string[] segments)
        {
            //protocol#|frndrq#|id
            ClientState client = ClientManager.GetClientById(clientId);
            string idTo = segments[2];

            string cmd = string.Format("{0}{1}{2}{1}{3}",
                (int)ReadProtocol.GetVersion(),
                m_SegmentTerminator,
                NetworkCommand.GetCommand(NetworkCommandType.FriendRequest),
                client.AccountRelative.AccountId);

            if (AccountHandler.AccountOnline(AccountDatabaseHandler.GetAccountNameFromId(idTo)))
                ClientManager.SendMessageToClientByAccount
                    (AccountHandler.GetAccountById(idTo), cmd);
        }

        private void HandleFriendAddition_One(string clientId, string[] segments)
        {
            try //protocol#|adfrnd#|id
            {
                string idFrom = ClientManager.GetClientById(clientId).AccountRelative.AccountId;
                string idTo = segments[2];

                AccountDatabaseHandler.AddFriendRelationship(idFrom, idTo);
                AccountDatabaseHandler.AddFriendRelationship(idTo, idFrom);

                if (AccountHandler.AccountOnline(AccountDatabaseHandler.GetAccountNameFromId(idFrom)))
                    AccountHandler.GetAccountById(idFrom).AddFriend(idTo);

                if (AccountHandler.AccountOnline(AccountDatabaseHandler.GetAccountNameFromId(idTo)))
                    AccountHandler.GetAccountById(idTo).AddFriend(idFrom);

                string cmd = string.Format("{0}{1}{2}{1}{3}",
                    (int)ReadProtocol.GetVersion(),
                    m_SegmentTerminator,
                    NetworkCommand.GetCommand(NetworkCommandType.AddFriend),
                    idTo);

                ClientManager.SendMessageToClientByAccount
                    (AccountHandler.GetAccountById(idFrom), cmd);

                cmd = string.Format("{0}{1}{2}{1}{3}",
                   (int)ReadProtocol.GetVersion(),
                   m_SegmentTerminator,
                   NetworkCommand.GetCommand(NetworkCommandType.AddFriend),
                   idFrom);

                ClientManager.SendMessageToClientByAccount
                    (AccountHandler.GetAccountById(idTo), cmd);
            }

            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void HandlePrivateMessage_One(string[] segments)
        {
            //protocol#|pvtmsg#|from#|to#|time#|message
            string cmd = string.Format("{0}{1}{2}{1}{3}{1}{4}{1}{5}{1}{6}",
                (int)ReadProtocol.GetVersion(),
                m_SegmentTerminator,
                NetworkCommand.GetCommand(NetworkCommandType.PrivateMessage),
                segments[2],
                segments[3],
                segments[4],
                segments[5]);

            ClientManager.SendMessageToClientByAccount
                (AccountHandler.GetAccountById(segments[3]), cmd);
        }

        private void HandleCirculation_One(string clientId)
        {
            ClientManager.RemoveClientFromHeartbeatCache(clientId);
        }

        private void HandleLogout_One(string clientId)
        {
            ClientManager.LogClientOut(clientId);
        }

        private void HandleAccountCreationRequest_One(string id, string[] segments)
        {
            AccountDatabaseHandler.AttemptAccountCreation(id, segments[2], segments[3], segments[4]);
        }

        private void HandleAccountLoginRequest_One(string id, string[] segments)
        {
            if(!AccountHandler.AccountOnline(segments[2]))
                AccountDatabaseHandler.AttemptLogin(id, segments[2], segments[3]);
            else
                NetworkEventDispatcher.InvokeUserLoginEvent
                    (new UserLoginEventArgs(id, segments[2], false, true));
        }

        private void HandleLobbyMessage_One(string id, string[] segments)
        {
            string cmd = string.Format("{0}{1}{2}{1}{3}{1}{4}{1}{5}",
                (int)ReadProtocol.GetVersion(),
                m_SegmentTerminator,
                NetworkCommand.GetCommand(NetworkCommandType.LobbyMessage),
                segments[2], segments[3], segments[4]);

            ClientManager.SendMessageToAllClients(cmd);
        }

        private void SyncUserFriends(Account account)
        {
            string[] friendIds = account.FriendIdentities.ToArray();

            for(int i = friendIds.Length -1; i >= 0; i--)
            {
                Account friendAccount = new Account
                    (AccountDatabaseHandler.GetAccountNameFromId(friendIds[i]));

                ClientManager.SendMessageToClientByAccount
                    (account, GetFriendSyncCommand(friendAccount));
            }
        }

        private void SendAllAccountSyncDataToClient(ClientState client, AccountSyncType type)
        {
            foreach (KeyValuePair<string, Account> account in AccountHandler.AccountsOnline)
            {
                SendAccountSyncDataToClient(client, account.Value, type);
            }
        }

        private void SendAccountSyncDataToClient(ClientState client, Account account, AccountSyncType type)
        {
            client.SendData(GetAccountSyncCommand(account, type));
        }

        private void SendAccountSyncDataToAllClients(Account account, ClientState client, AccountSyncType type)
        {
            ClientManager.SendMessageToAllClients(GetAccountSyncCommand(account, type), client);
        }

        private string GetAccountSyncCommand(Account account, AccountSyncType syncType)
        {
            //protocol#|syncfacct#|syncType#|username#|rating#|wins#|losses<#>
            string cmd = string.Format("{0}{1}{2}{1}{3}{1}{4}{1}{5}{1}{6}{1}{7}{1}{8}",
                (int)ReadProtocol.GetVersion(),
                m_SegmentTerminator,
                NetworkCommand.GetCommand(NetworkCommandType.SyncForeignAccount),
                (int)syncType,
                account.Username,
                account.Rating,
                account.Wins,
                account.Losses,
                account.AccountId);

            return cmd;
        }

        private string GetFriendSyncCommand(Account account)
        {
            //protocol#|sncfrnd#|username#|rating#|wins#|losses#|id
            string cmd = string.Format("{0}{1}{2}{1}{3}{1}{4}{1}{5}{1}{6}{1}{7}",
                (int)ReadProtocol.GetVersion(),
                m_SegmentTerminator,
                NetworkCommand.GetCommand(NetworkCommandType.SyncFriend),
                account.Username,
                account.Rating,
                account.Wins,
                account.Losses,
                account.AccountId);

            return cmd;
        }

        private void HandleMatchCreationEvent(MatchState match)
        {
            //protocol#|mtchcrtd#|matchId#|playerOneId#|playerOneUsername
            string cmd = string.Format("{0}{1}{2}{1}{3}{1}{4}{1}{5}",
                (int)ReadProtocol.GetVersion(),
                m_SegmentTerminator,
                NetworkCommand.GetCommand(NetworkCommandType.MatchCreated),
                match.MatchIdentity,
                match.PlayerOne.AccountIdentity,
                match.PlayerOne.Username);

            MatchHandler.SendMessageToUsersInMatch(match, cmd);

            ClientState client = ClientManager.GetClientByAccount
                (AccountHandler.GetAccountById(match.PlayerOne.AccountIdentity));

            ClientManager.SendMessageToAllClients(cmd, client);
        }

        private string GetMatchSyncCommand(MatchState match)
        {      
            //protocol#|mtchcrtd#|matchId#|playerOneId#|playerOneUsername#|playerTwoId#|playerTwoUsername
            string cmd = string.Format("{0}{1}{2}{1}{3}{1}{4}{1}{5}{1}{6}{1}{7}{1}{8}",
                (int)ReadProtocol.GetVersion(),
                m_SegmentTerminator,
                NetworkCommand.GetCommand(NetworkCommandType.SyncMatchParticipants),
                match.MatchIdentity,
                match.PlayerOne != null ? match.PlayerOne.AccountIdentity : "-1",
                match.PlayerOne != null ? match.PlayerOne.Username : "none",
                match.PlayerTwo != null ? match.PlayerTwo.AccountIdentity : "-1",
                match.PlayerTwo != null ? match.PlayerTwo.Username : "none",
                match.InProgress.ToString());

            return cmd;
        }

        private void SyncMatchSpectators(MatchState match)
        {
            foreach(PlayerState state in match.Spectators.Values.ToList())
            {
                string cmd = string.Format("{0}{1}{2}{1}{3}{1}{4}{1}{5}",
                    (int)ReadProtocol.GetVersion(),
                    m_SegmentTerminator,
                    NetworkCommand.GetCommand(NetworkCommandType.SyncMatchSpectator),
                    match.MatchIdentity,
                    state.AccountIdentity,
                    state.Username);

                ClientManager.SendMessageToAllClients(cmd);
            }
        }

        private void SyncAllAvailableMatchesWithClient(ClientState client)
        {
            foreach(MatchState match in MatchHandler.MatchesOnline.Values.ToList())
            {
                client.SendData(GetMatchSyncCommand(match));
            }
        }
    }
}
