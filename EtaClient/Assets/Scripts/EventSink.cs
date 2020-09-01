using Eta.Interdata;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventSink
{
    public delegate void ClientConnectedEventHandler(ClientConnectedEventArgs args);
    public delegate void StandardLogEventHandler(LogEventArgs args);
    public delegate void MessageBoxEventHandler(MessageBoxEventArgs args);
    public delegate void ServerMessageEventHandler(ServerMessageEventArgs args);
    public delegate void PublicKeyReceivedEventHandler(PublicKeyReceivedEventArgs args);
    public delegate void LoginSuccessEventHandler(LoginSuccessEventArgs args);
    public delegate void LobbyMessageEventHandler(LobbyMessageEventArgs args);
    public delegate void ForeignAccountSyncEventHandler(ForeignAccountSyncEventArgs args);
    public delegate void ConnectionFailedEventHandler(ConnectionFailedEventArgs args);
    public delegate void PrivateMessageEventHandler(PrivateMessageEventArgs args);
    public delegate void StartPrivateMessageEventHandler(StartPrivateMessageEventArgs args);
    public delegate void AddFriendToListEventHandler(FriendAddedToListEventArgs args);
    public delegate void FriendRequestEventHandler(FriendRequestEventArgs args);
    public delegate void FriendRemovedEventHandler(FriendRemovedEventArgs args);
    public delegate void MatchCreatedEventHandler(MatchCreatedEventArgs args);
    public delegate void MatchJoinedEventHandler(MatchJoinedEventArgs args);
    public delegate void MatchCancelledEventHandler(MatchCancelledEventArgs args);
    public delegate void MatchEndedEventHandler(MatchEndedEventArgs args);
    public delegate void ParticipantSyncEventHandler(MatchSyncEventArgs args);
    public delegate void MatchJoinResultEventHandler(MatchJoinResultEventArgs args);
    public delegate void MatchStartedEventHandler(MatchStartedEventArgs args);
    public delegate void SpectatorSyncEventHandler(SpectatorSyncEventArgs args);
    public delegate void SpectateResultEventHandler(SpectateResultEventArgs args);
    public delegate void SpectatorDepartureEventHandler(SpectatorDepartureEventArgs args);
    public delegate void MatchChatMessageEventHandler(MatchChatMessageEventArgs args);
    public delegate void SingleClickEventHandler(MouseClickEventArgs args);
    public delegate void DoubleClickEventHandler(MouseClickEventArgs args);
    public delegate void TileSelectedEventHandler(TileSelectedEventArgs args);
    public delegate void UnitMovementEventHandler(UnitMovementEventArgs args);
    public delegate void CameraTransitionEventHandler(CameraTransitionEventArgs args);
    public delegate void UnitAttackEventHandler(UnitAttackEventArgs args);

    public delegate void PublicKeySetEventHandler();
    public delegate void AccountCreatedEventHandler();

    public static event ClientConnectedEventHandler ClientConnectedEvent;
    public static event StandardLogEventHandler StandardLogEvent;
    public static event MessageBoxEventHandler MessageBoxEvent;
    public static event ServerMessageEventHandler ServerMessageEvent;
    public static event LoginSuccessEventHandler LoginSuccessEvent;
    public static event PublicKeyReceivedEventHandler PublicKeyReceivedEvent;
    public static event PublicKeySetEventHandler PublicKeySetEvent;
    public static event AccountCreatedEventHandler AccountCreatedEvent;
    public static event ConnectionFailedEventHandler ConnectionFailedEvent;
    public static event LobbyMessageEventHandler LobbyMessageEvent;
    public static event ForeignAccountSyncEventHandler ForeignAccountSyncEvent;
    public static event PrivateMessageEventHandler PrivateMessageEvent;
    public static event StartPrivateMessageEventHandler StartPrivateMessageEvent;
    public static event AddFriendToListEventHandler AddFriendToListEvent;
    public static event FriendRequestEventHandler FriendRequestEvent;
    public static event FriendRemovedEventHandler FriendRemovedEvent;
    public static event MatchCreatedEventHandler MatchCreatedEvent;
    public static event MatchJoinedEventHandler MatchJoinedEvent;
    public static event MatchCancelledEventHandler MatchCancelledEvent;
    public static event MatchEndedEventHandler MatchEndedEvent;
    public static event ParticipantSyncEventHandler ParticipantSyncEvent;
    public static event MatchJoinResultEventHandler MatchJoinResultEvent;
    public static event MatchStartedEventHandler MatchStartedEvent;
    public static event SpectatorSyncEventHandler SpectatorSyncEvent;
    public static event SpectateResultEventHandler SpectateResultEvent;
    public static event SpectatorDepartureEventHandler SpectatorDepartureEvent;
    public static event MatchChatMessageEventHandler MatchChatMessageEvent;
    public static event SingleClickEventHandler SingleClickEvent;
    public static event DoubleClickEventHandler DoubleClickEvent;
    public static event TileSelectedEventHandler TileSelectedEvent;
    public static event UnitMovementEventHandler UnitMovementEvent;
    public static event CameraTransitionEventHandler CameraTransitionEvent;
    public static event UnitAttackEventHandler UnitAttackEvent;

    public static void InvokeServerMessageEvent(ServerMessageEventArgs args)
    {
        ServerMessageEvent?.Invoke(args);
    }

    public static void InvokeMessageBoxEvent(MessageBoxEventArgs args)
    {
        MessageBoxEvent?.Invoke(args);
    }

    public static void InvokeStandardLogEvent(LogEventArgs args)
    {
        StandardLogEvent?.Invoke(args);
    }

    public static void InvokeConnectionEvent(ClientConnectedEventArgs args)
    {
        ClientConnectedEvent?.Invoke(args);
    }

    public static void InvokePublicKeyReceivedEvent(PublicKeyReceivedEventArgs args)
    {
        PublicKeyReceivedEvent?.Invoke(args);
    }

    public static void InvokeLoginSuccessEvent(LoginSuccessEventArgs args)
    {
        LoginSuccessEvent?.Invoke(args);
    }

    public static void InvokeLobbyMessageEvent(LobbyMessageEventArgs args)
    {
        LobbyMessageEvent?.Invoke(args);
    }

    public static void InvokeForeignAccountEvent(ForeignAccountSyncEventArgs args)
    {
        ForeignAccountSyncEvent?.Invoke(args);
    }

    public static void InvokePublicKeySetEvent()
    {
        PublicKeySetEvent?.Invoke();
    }

    public static void InvokeAccountCreatedEvent()
    {
        AccountCreatedEvent?.Invoke();
    }

    public static void InvokeConnectionFailedEvent(ConnectionFailedEventArgs args)
    {
        ConnectionFailedEvent?.Invoke(args);
    }

    public static void InvokePrivateMessageEvent(PrivateMessageEventArgs args)
    {
        PrivateMessageEvent?.Invoke(args);
    }

    public static void InvokeStartPrivateMessageEvent(StartPrivateMessageEventArgs args)
    {
        StartPrivateMessageEvent?.Invoke(args);
    }

    public static void InvokeAddFriendToListEvent(FriendAddedToListEventArgs args)
    {
        AddFriendToListEvent?.Invoke(args);
    }

    public static void InvokeFriendRequestEvent(FriendRequestEventArgs args)
    {
        FriendRequestEvent?.Invoke(args);
    }

    public static void InvokeFriendRemovedEvent(FriendRemovedEventArgs args)
    {
        FriendRemovedEvent?.Invoke(args);
    }

    public static void InvokeMatchCreatedEvent(MatchCreatedEventArgs args)
    {
        MatchCreatedEvent?.Invoke(args);
    }

    public static void InvokeMatchJoinedEvent(MatchJoinedEventArgs args)
    {
        MatchJoinedEvent?.Invoke(args);
    }

    public static void InvokeMatchCancelledEvent(MatchCancelledEventArgs args)
    {
        MatchCancelledEvent?.Invoke(args);
    }

    public static void InvokeMatchEndedEvent(MatchEndedEventArgs args)
    {
        MatchEndedEvent?.Invoke(args);
    }

    public static void InvokeParticipantSyncEvent(MatchSyncEventArgs args)
    {
        ParticipantSyncEvent?.Invoke(args);
    }

    public static void InvokeMatchJoinResultEvent(MatchJoinResultEventArgs args)
    {
        MatchJoinResultEvent?.Invoke(args);
    }

    public static void InvokeSpectatorSyncEvent(SpectatorSyncEventArgs args)
    {
        SpectatorSyncEvent?.Invoke(args);
    }

    public static void InvokeSpectateResultEvent(SpectateResultEventArgs args)
    {
        SpectateResultEvent?.Invoke(args);
    }

    public static void InvokeSpectatorDepartureEvent(SpectatorDepartureEventArgs args)
    {
        SpectatorDepartureEvent?.Invoke(args);
    }

    public static void InvokeMatchChatMessageEvent(MatchChatMessageEventArgs args)
    {
        MatchChatMessageEvent?.Invoke(args);
    }

    public static void InvokeMatchStartedEvent(MatchStartedEventArgs args)
    {
        MatchStartedEvent?.Invoke(args);
    }

    public static void InvokeSingleClickEvent(MouseClickEventArgs args)
    {
        SingleClickEvent?.Invoke(args);
    }

    public static void InvokeDoubleClickEvent(MouseClickEventArgs args)
    {
        DoubleClickEvent?.Invoke(args);
    }

    public static void InvokeTileSelectedEvent(TileSelectedEventArgs args)
    {
        TileSelectedEvent?.Invoke(args);
    }

    public static void InvokeUnitMovementEvent(UnitMovementEventArgs args)
    {
        UnitMovementEvent?.Invoke(args);
    }

    public static void InvokeCameraTransitionEvent(CameraTransitionEventArgs args)
    {
        CameraTransitionEvent?.Invoke(args);
    }

    public static void InvokeUnitAttackEvent(UnitAttackEventArgs args)
    {
        UnitAttackEvent?.Invoke(args);
    }
}

public class UnitAttackEventArgs
{
    GameBoardTile m_Tile;

    internal GameBoardTile Tile { get { return m_Tile; } }

    public UnitAttackEventArgs(GameBoardTile tile)
    {
        m_Tile = tile;
    }
}

public class CameraTransitionEventArgs
{
    Camera m_TargetCamera;

    internal Camera TargetCamera { get { return m_TargetCamera; } }

    public CameraTransitionEventArgs(Camera camera)
    {
        m_TargetCamera = camera;
    }
}

public class MatchStartedEventArgs
{
}

public class UnitMovementEventArgs
{
    GameBoardTile m_Tile;

    internal GameBoardTile Tile { get { return m_Tile; } }

    public UnitMovementEventArgs(GameBoardTile tile)
    {
        m_Tile = tile;
    }
}

public class TileSelectedEventArgs
{
    GameBoardTile m_Tile;

    internal GameBoardTile Tile { get { return m_Tile; } }

    public TileSelectedEventArgs(GameBoardTile tile)
    {
        m_Tile = tile;
    }
}

public class MouseClickEventArgs
{
    Transform m_TransformHit;
    int m_MouseIndexClicked;

    internal Transform TransformHit { get { return m_TransformHit; } }
    internal int MouseIndexClicked { get { return m_MouseIndexClicked; } }

    internal void UpdateMouseClickEventArgs(Transform transform, int mouseIndex)
    {
        m_TransformHit = transform;
        m_MouseIndexClicked = mouseIndex;
    }
}

public class MatchChatMessageEventArgs
{
    private string m_Username;
    private string m_Time;
    private string m_Content;

    public string Username { get { return m_Username; } }
    public string Time { get { return m_Time; } }
    public string Content { get { return m_Content; } }

    public MatchChatMessageEventArgs(string username, string time, string content)
    {
        m_Username = username;
        m_Time = time;
        m_Content = content;
    }
}

public class SpectatorDepartureEventArgs
{
    string m_MatchId;
    PlayerState m_Spectator;

    internal string MatchId { get { return m_MatchId; } }
    internal PlayerState Spectator { get { return m_Spectator; } }

    public SpectatorDepartureEventArgs(string matchId, PlayerState spectator)
    {
        m_MatchId = matchId;
        m_Spectator = spectator;
    }
}

public class SpectateResultEventArgs
{
    bool m_Succeeded;
    string m_MatchId;

    internal bool Succeeded { get { return m_Succeeded; } }
    internal string MatchId { get { return m_MatchId; } }

    public SpectateResultEventArgs(bool succeeded, string matchId)
    {
        m_Succeeded = succeeded;
        m_MatchId = matchId;
    }
}

public class SpectatorSyncEventArgs
{
    string m_MatchId;
    PlayerState m_Spectator;

    internal string MatchId { get { return m_MatchId; } }
    internal PlayerState Spectator { get { return m_Spectator; } }

    public SpectatorSyncEventArgs(string matchId, PlayerState spectator)
    {
        m_MatchId = matchId;
        m_Spectator = spectator;
    }
}

public class MatchJoinResultEventArgs
{
    bool m_JoinSucceeded;
    string m_MatchId;

    internal bool JoinSucceeded { get { return m_JoinSucceeded; } }
    internal string MatchId { get { return m_MatchId; } }

    public MatchJoinResultEventArgs(bool succeeded, string matchId)
    {
        m_JoinSucceeded = succeeded;
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

public class MatchEndedEventArgs
{
    private string m_MatchId;

    internal string MatchId { get { return m_MatchId; } }

    public MatchEndedEventArgs(string matchId)
    {
        m_MatchId = matchId;
    }
}

public class MatchCancelledEventArgs
{
    MatchState m_Match;

    internal MatchState Match { get { return m_Match; } }

    public MatchCancelledEventArgs(MatchState match)
    {
        m_Match = match;
    }
}

public class MatchJoinedEventArgs
{
    MatchState m_Match;

    internal MatchState Match { get { return m_Match; } }

    public MatchJoinedEventArgs(MatchState match)
    {
        m_Match = match;
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

public class FriendRemovedEventArgs
{
    Account m_Account;

    internal Account Account { get { return m_Account; } }
    public FriendRemovedEventArgs(Account account)
    {
        m_Account = account;
    }
}

public class FriendRequestEventArgs
{
    Account m_Account;

    internal Account Account { get { return m_Account; } }
    public FriendRequestEventArgs(Account account)
    {
        m_Account = account;
    }
}

public class FriendAddedToListEventArgs
{
    Account m_Account;

    internal Account Account { get { return m_Account; } }
    public FriendAddedToListEventArgs(Account account)
    {
        m_Account = account;
    }
}

public class StartPrivateMessageEventArgs
{
    Account m_Account;

    internal Account Account { get { return m_Account; } }
    public StartPrivateMessageEventArgs(Account account)
    {
        m_Account = account;
    }
}

public class PrivateMessageEventArgs
{
    string m_AccountId;
    string m_Content;
    string m_TimeStamp;

    internal string AccountId { get { return m_AccountId; } }
    internal string Content { get { return m_Content; } }
    internal string TimeStamp { get { return m_TimeStamp; } }

    public PrivateMessageEventArgs(string accountId, string content, string timeStamp)
    {
        m_AccountId = accountId;
        m_Content = content;
        m_TimeStamp = timeStamp;
    }
}

public class ConnectionFailedEventArgs
{
    int m_Attempts;

    internal int ConnectionAttempts { get { return m_Attempts; } }

    public ConnectionFailedEventArgs(int attempts)
    {
        m_Attempts = attempts;
    }
}

public class ForeignAccountSyncEventArgs
{
    Account m_Account;
    int m_Index;
    int m_SyncType;

    internal Account Account { get { return m_Account; } }
    internal int Index { get { return m_Index; } }
    internal int SyncType { get { return m_SyncType; } }

    public ForeignAccountSyncEventArgs(int index, Account account, int type)
    {
        m_Account = account;
        m_Index = index;
        m_SyncType = type;
    }
}

public class LobbyMessageEventArgs
{
    private string m_Username;
    private string m_Time;
    private string m_Content;

    public string Username { get { return m_Username; } }
    public string Time { get { return m_Time; } }
    public string Content { get { return m_Content; } }

    public LobbyMessageEventArgs(string username, string time, string content)
    {
        m_Username = username;
        m_Time = time;
        m_Content = content;
    }
}

public class LoginSuccessEventArgs
{
    Account m_Account;

    public Account Account { get { return m_Account; } }

    public LoginSuccessEventArgs(Account account)
    {
        m_Account = account;
    }
}

public class PublicKeyReceivedEventArgs
{
    string m_Key;

    public string Key { get { return m_Key; } }

    public PublicKeyReceivedEventArgs(string key)
    {
        m_Key = key;
    }
}

public class ServerMessageEventArgs
{
    internal string Message { get; set; }
    public ServerMessageEventArgs(string message)
    {
        Message = message;
    }
}

public class MessageBoxEventArgs
{
    internal string Message { get; set; }
    public MessageBoxEventArgs(string message)
    {
        Message = message;
    }
}

public class LogEventArgs
{
    internal string Message { get; set; }
    public LogEventArgs(string message)
    {
        Message = message;
    }
}

public class ClientConnectedEventArgs
{
    public ClientConnectedEventArgs() { }
}
