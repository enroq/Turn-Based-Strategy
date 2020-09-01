using System;
using System.Collections.Generic;
using UnityEngine;
using Eta.Interdata;

public class ServerMessageHandler
{
    public ServerMessageHandler()
    {
        EventSink.ServerMessageEvent += EventSink_ServerMessageEvent;
    }

    private void EventSink_ServerMessageEvent(ServerMessageEventArgs args)
    {
        ParseServerMessageByProtocol(args.Message);
        EventSink.InvokeStandardLogEvent
            (new LogEventArgs("Message Received: " + args.Message));
    }

    void ParseServerMessageByProtocol(string message)
    {
        string[] segments = message.Split
            (ClientManager.SegmentTerminatorArray, StringSplitOptions.None);

        try
        {
            //Protocol Versions Used For Backwards Compatability With Older Clients.
            int version;

            if (Int32.TryParse(segments[0], out version))
            {
                switch ((ProtocolVersion)version)
                {
                    case ProtocolVersion.One:
                        {
                            ParseMessage_ProtocolOne(segments);
                            break;
                        }
                    default: break;
                }
            }
        }

        catch (Exception e)
        {
            EventSink.InvokeStandardLogEvent(new LogEventArgs(e.ToString()));
        }
    }

    void ParseMessage_ProtocolOne(string[] segments)
    {
        int commandIndex = NetworkCommand.GetCommandIndex(segments[1]);
        switch (commandIndex)
        {
            case (int)NetworkCommandType.AccountCreated:
                {
                    NotifyAccountCreated_One();
                    break;
                }
            case (int)NetworkCommandType.AccountCreationFailed:
                {
                    NotifyAccountCreationFailed_One(segments);
                    break;
                }
            case (int)NetworkCommandType.LoginSuccess:
                {
                    HandleLoginSuccess_One(segments);
                    break;
                }
            case (int)NetworkCommandType.LoginFail:
                {
                    HandleLoginFail_One(segments);
                    break;
                }
            case (int)NetworkCommandType.LobbyMessage:
                {
                    HandleLobbyMessage_One(segments);
                    break;
                }
            case (int)NetworkCommandType.SyncForeignAccount:
                {
                    SyncForeignAccount_One(segments);
                    break;
                }
            case (int)NetworkCommandType.Heartbeat:
                {
                    ReturnCirculation_One();
                    break;
                }
            case (int)NetworkCommandType.PrivateMessage:
                {
                    ProcessPrivateMessage_One(segments);
                    break;
                }
            case (int)NetworkCommandType.SyncFriend:
                {
                    SyncFriend_One(segments);
                    break;
                }
            case (int)NetworkCommandType.FriendRequest:
                {
                    HandleFriendRequest_One(segments);
                    break;
                }
            case (int)NetworkCommandType.AddFriend:
                {
                    AddFriend_One(segments);
                    break;
                }
            case (int)NetworkCommandType.RemoveFriend:
                {
                    RemoveFriend_One(segments);
                    break;
                }
            case (int)NetworkCommandType.MatchCreated:
                {
                    HandleMatchCreatedMessage_One(segments);
                    break;
                }
            case (int)NetworkCommandType.EndMatch:
                {
                    HandleMatchEnded_One(segments);
                    break;
                }
            case (int)NetworkCommandType.SyncMatchParticipants:
                {
                    SyncMatchState_One(segments);
                    break;
                }
            case (int)NetworkCommandType.JoinMatchResult:
                {
                    HandleJoinResult_One(segments);
                    break;
                }
            case (int)NetworkCommandType.SyncMatchSpectator:
                {
                    HandleSpectatorSync_One(segments);
                    break;
                }
            case (int)NetworkCommandType.SpectateMatchResult:
                {
                    HandleSpectateResult_One(segments);
                    break;
                }
            case (int)NetworkCommandType.RemoveSpectator:
                {
                    HandleSpectatorDeparture_One(segments);
                    break;
                }
            case (int)NetworkCommandType.MatchMessage:
                {
                    HandleMatchChatMessage_One(segments);
                    break;
                }
            case (int)NetworkCommandType.UserLeftMatchInProg:
                {
                    HandleMatchInProgLeft_One(segments);
                    break;
                }
            case (int)NetworkCommandType.SyncGamePiece:
                {
                    HandleGamePieceSync_One(segments);
                    break;
                }
            case (int)NetworkCommandType.EndGamePieceSync:
                {
                    HandlePieceSyncComplete_One(segments);
                    break;
                }
            case (int)NetworkCommandType.NetworkPieceMoveEvent:
                {
                    HandleNetworkPieceMoveEvent_One(segments);
                    break;
                }
            case (int)NetworkCommandType.StartMatch:
                {
                    HandleStartMatch_One(segments);
                    break;
                }
            case (int)NetworkCommandType.TurnStateSync:
                {
                    HandleTurnSync_One(segments);
                    break;
                }
            case (int)NetworkCommandType.ProcessAttack:
                {
                    HandleProcessAttack_One(segments);
                    break;
                }
            default:
                {
                    EventSink.InvokeStandardLogEvent
                        (new LogEventArgs("Unknown Message: " + segments[1]));
                    break;
                }
        }
    }

    private void HandleProcessAttack_One(string[] segments)
    {
        try
        {
            InterEventDispatcher.InvokeAttackSyncCompleteEvent
                (new AttackSyncCompleteEventArgs
                    (segments[2], segments[3], segments[4], Int32.Parse(segments[5]), Int32.Parse(segments[6])));
        }
        catch(Exception e) { Debug.Log(e.ToString()); }
    }

    private void HandleTurnSync_One(string[] segments)
    { //0#|cmd#|matchId#|controllerId#|stepVal#|lowestStep
        try
        {
            InterEventDispatcher.InvokeTurnStateSyncEvent
                (new TurnStateSyncEventArgs(segments[2], segments[3], Int32.Parse(segments[4]), Int32.Parse(segments[5])));
        }

        catch(Exception e ) { Debug.Log(e.ToString()); }
    }

    private void HandleStartMatch_One(string[] segments)
    {
        try
        {
            InterEventDispatcher.InvokeMatchStartEvent
                (new MatchStartEventArgs(segments[2], segments[3]));

            Debug.LogFormat("Match Start Message Received For Match [{0}]", segments[2]);
        }

        catch(Exception e) { Debug.Log(e.ToString()); }
    }

    private void HandleNetworkPieceMoveEvent_One(string[] segments)
    {
        try
        {
            InterEventDispatcher.InvokeNetworkGamePieceMoveEvent
                (new NetworkGamePieceMoveEventArgs(segments[2], segments[3], Int32.Parse(segments[4]), Int32.Parse(segments[5])));
        }

        catch(Exception e) { Debug.Log(e.ToString()); }
    }

    private void HandlePieceSyncComplete_One(string[] segments)
    {
        InterEventDispatcher.InvokeGamePieceSyncCompleteEvent
            (new GamePieceSyncCompleteEventArgs(segments[2], segments[3]));
    }

    private void HandleGamePieceSync_One(string[] segments)
    {
        if (MatchHandler.CurrentMatch == null)
            return;

        try
        {
            GamePieceNetworkState netState = new GamePieceNetworkState
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

            InterEventDispatcher.InvokeGamePieceSyncReceivedEvent
                (new GamePieceSyncReceivedEventArgs(segments[2], netState.ControllerIdentity, netState));
        }

        catch (Exception e) { Debug.Log(e.ToString()); }
    }

    private void HandleMatchInProgLeft_One(string[] segments)
    {
        MatchState match = MatchHandler.GetMatchById(segments[2]);

        if(match != null)
        {
            PlayerState playerState = match.GetPlayerStateById(segments[3]);
            if (playerState != null)
                InterEventDispatcher.InvokeDCFromMatchInProgressEvent
                    (new DCFromMatchInProgressEventArgs(match, playerState));
        }
    }

    private void HandleMatchChatMessage_One(string[] segments)
    {
        EventSink.InvokeMatchChatMessageEvent
            (new MatchChatMessageEventArgs(segments[3], segments[4], segments[5]));
    }

    private void HandleSpectatorDeparture_One(string[] segments)
    {
        EventSink.InvokeSpectatorDepartureEvent(new SpectatorDepartureEventArgs
            (segments[2], 
                new PlayerState(segments[3], segments[4])));
    }

    private void HandleSpectateResult_One(string[] segments)
    {
        if (segments[3].ToLowerInvariant().Equals("true"))
            EventSink.InvokeSpectateResultEvent(new SpectateResultEventArgs(true, segments[2]));
        else
            if (segments[3].ToLowerInvariant().Equals("false"))
                EventSink.InvokeSpectateResultEvent(new SpectateResultEventArgs(false, segments[2]));
    }

    private void HandleSpectatorSync_One(string[] segments)
    {
        EventSink.InvokeSpectatorSyncEvent(new SpectatorSyncEventArgs
            (segments[2], new PlayerState(segments[3], segments[4])));
    }

    private void HandleJoinResult_One(string[] segments)
    {
        if (segments[2].ToLowerInvariant().Equals("false"))
            EventSink.InvokeMatchJoinResultEvent(new MatchJoinResultEventArgs(false, segments[3]));
        else
            if (segments[2].ToLowerInvariant().Equals("true"))
                EventSink.InvokeMatchJoinResultEvent(new MatchJoinResultEventArgs(true, segments[3]));
    }

    private void SyncMatchState_One(string[] segments)
    {
        //protocol#|syncmtchps#|matchId#|playerOneId#|playerOneUsername#|playerTwoId#|playerTwoUsername
        MatchState match = null;
        if (segments[4] != "none" && segments[6] != "none")
        {
            match = new MatchState
                (segments[2],
                    new PlayerState(segments[3], segments[4]),
                    new PlayerState(segments[5], segments[6]));
        }

        else if (segments[4] != "none")
        {
           match = new MatchState
                (segments[2], new PlayerState(segments[3], segments[4]), 1);
        }

        else if(segments[6] != "none")
        {
            match = new MatchState
                (segments[2], new PlayerState(segments[5], segments[6]), 2);
        }

        if (segments[7].ToLowerInvariant() == "true")
            match.InProgress = true;

        if(match != null)
            EventSink.InvokeParticipantSyncEvent(new MatchSyncEventArgs(match));
    }

    private void HandleMatchEnded_One(string[] segments)
    {
        EventSink.InvokeMatchEndedEvent(new MatchEndedEventArgs(segments[2]));
    }

    private void HandleMatchCreatedMessage_One(string[] segments)
    {
        //protocol#|mtchcrtd#|matchId#|playerOneId#|playerOneUsername
        MatchState match = new MatchState
            (segments[2], new PlayerState(segments[3], segments[4]));

        EventSink.InvokeMatchCreatedEvent(new MatchCreatedEventArgs(match));
    }

    private void RemoveFriend_One(string[] segments)
    {
        string idFrom = segments[2];
        EventSink.InvokeFriendRemovedEvent
            (new FriendRemovedEventArgs(AccountManager.GetAccountById(idFrom)));

        AccountManager.RemoveFriendFromList(idFrom);
    }

    private void AddFriend_One(string[] segments)
    {
        string idFrom = segments[2];

        AccountManager.SyncFriendToList(idFrom, AccountManager.GetAccountById(idFrom));
        ClientManager.Post
            (() => PrivateMessageHandler.NotifyFriendRequestAccepted(idFrom));
    }

    private void HandleFriendRequest_One(string[] segments)
    {
        try
        {
            EventSink.InvokeFriendRequestEvent(new FriendRequestEventArgs
                (AccountManager.GetAccountById(segments[2])));
        }

        catch(Exception e)
        {
            Debug.Log(e.ToString());
        }
    }

    private void SyncFriend_One(string[] segments)
    {
        try //protocol#|sncfrnd#|username#|rating#|wins#|losses#|id
        {
            AccountManager.SyncFriendToList(segments[6],
                new Account(segments[6], segments[2], string.Empty, segments[3], segments[4], segments[5]));
        }

        catch(Exception e)
        {
            Debug.Log(e.ToString());
        }
    }

    private void ProcessPrivateMessage_One(string[] segments)
    {
        try //protocol#|pvtmsg#|from#|to#|time#|message
        {
            EventSink.InvokePrivateMessageEvent
                (new PrivateMessageEventArgs(segments[2], segments[5], segments[4]));
        }

        catch(Exception e)
        {
            Debug.Log(e.ToString());
        }
    }

    private void ReturnCirculation_One()
    {
        ClientManager.Instance.SendCirculationCommand();
    }

    private void SyncForeignAccount_One(string[] segments)
    {
        //protocol#|syncfacct#|syncType#|username#|rating#|wins#|losses#|id
        if (segments[2] == ((int)AccountSyncType.Connect).ToString())
        {
            AccountManager.AddForeignAccount //Empty String Is E-mail, Don't Think I'll Put It On Client
                (new Account(segments[7], segments[3], string.Empty, segments[4], segments[5], segments[6]));
        }

        else if(segments[2] == ((int)AccountSyncType.Disconnect).ToString())
        {
            try { AccountManager.RemoveForeignAccount(segments[7]); }

            catch (Exception e) { Debug.Log(e.ToString()); }
        }
    }

    private void HandleLobbyMessage_One(string[] segments)
    {
        //protocol#|lbymsg#|username#|time#|message<#>
        EventSink.InvokeLobbyMessageEvent
            (new LobbyMessageEventArgs(segments[2], segments[3], segments[4]));
    }

    private void HandleLoginFail_One(string[] segments)
    {
        if(segments[2].ToLowerInvariant().Equals("false"))
            EventSink.InvokeMessageBoxEvent
                (new MessageBoxEventArgs("Username Or Password Is Incorrect."));

        if (segments[2].ToLowerInvariant().Equals("true"))
            EventSink.InvokeMessageBoxEvent
                (new MessageBoxEventArgs("Account Is Currently Logged In."));
    }

    private void HandleLoginSuccess_One(string[] segments)
    {
        //0#|lgnsccs#|username#|email#|rating#|wins#|losses|#id
        AccountManager.SetCurrentAccount
            (new Account(segments[7], segments[2], segments[3], segments[4], segments[5], segments[6]));

        EventSink.InvokeLoginSuccessEvent(new LoginSuccessEventArgs(AccountManager.AccountInstance));
    }

    private void NotifyAccountCreated_One()
    {
        EventSink.InvokeMessageBoxEvent
            (new MessageBoxEventArgs("Your Account Has Been Sucessfully Created."));

        EventSink.InvokeAccountCreatedEvent();
    }

    private void NotifyAccountCreationFailed_One(string[] segments)
    {
        try
        {
            int failType = Int32.Parse(segments[2]);
            switch (failType)
            {
                case 0:
                    {
                        EventSink.InvokeMessageBoxEvent
                            (new MessageBoxEventArgs("[Error]: Failed To Create New Account. Please Try Again Later."));
                        break;
                    }
                case 1:
                    {
                        EventSink.InvokeMessageBoxEvent
                            (new MessageBoxEventArgs("[Notice]: Can Not Create Account. E-mail Already In Use. Please Try Another."));
                        break;
                    }
                case 2:
                    {
                        EventSink.InvokeMessageBoxEvent
                            (new MessageBoxEventArgs("[Notice]: Can Not Create Account. Username Already Exists. Please Try Another."));
                        break;
                    }
                default:
                    {
                        EventSink.InvokeMessageBoxEvent
                            (new MessageBoxEventArgs("[Error]: Failed To Create New Account. Please Try Again Later."));
                        break;
                    }
            }
        }

        catch (Exception e)
        {
            EventSink.InvokeStandardLogEvent(new LogEventArgs(e.ToString()));
        }
    }
}
