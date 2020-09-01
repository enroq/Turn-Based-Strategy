using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using Eta.Interdata;
using System.Threading.Tasks;

/// <summary>
/// Handles Sending Messages To Server And Relaying Information From Other Modules To The Actual Client
/// </summary>
public class ClientManager : MonoBehaviour
{
    private static ClientManager m_Instance;
    private static MessageIsolator m_MessageIsolator;
    private static ServerMessageHandler m_Smh;
    private static AesModule m_AesModule;
    private static SynchronizationContext m_SyncContext;

    private static string m_SegmentTerminator = "#|";
    private static string m_EncryptedMessageTerminator = ",";

    internal static string SegmentTerminator { get { return m_SegmentTerminator; } }
    internal static string EncryptedMessageTerminator { get { return m_EncryptedMessageTerminator; } }

    internal static string[] SegmentTerminatorArray { get { return new string[] { m_SegmentTerminator }; } }

    private bool m_LoginAuthenticated = false;
    private bool m_SessionKeySent = false;
    private bool m_PublicKeyReceived = false;
    private bool m_IsQuitting = false;

    internal static MessageIsolator MessageIsolator
    {
        get { return m_MessageIsolator; }
        set { m_MessageIsolator = value; }
    }

    public static ClientManager Instance
    {
        get { return m_Instance; }
        set { m_Instance = value; }
    }

    public ServerMessageHandler MessageHandler
    {
        get { return m_Smh; }
    }

    public bool LoginAuthenticated
    {
        get { return m_LoginAuthenticated; }
        set { m_LoginAuthenticated = value; }
    }

    public bool HasSentSessionKey
    {
        get { return m_SessionKeySent; }
    }

    public bool PublicKeyReceived
    {
        get { return m_PublicKeyReceived; }
        set { m_PublicKeyReceived = value; }
    }

    private void Start()
    {
        if(m_Instance == null)
            m_Instance = this;

        Application.runInBackground = true;

        m_SyncContext = SynchronizationContext.Current;

        m_Smh = new ServerMessageHandler();
        m_AesModule = new AesModule();

        m_AesModule.InitializeProvider();

        EventSink.LoginSuccessEvent += EventSink_LoginAcceptedEvent;
        EventSink.PublicKeySetEvent += EventSink_PublicKeySetEvent;
        EventSink.ConnectionFailedEvent += EventSink_ConnectionFailedEvent;

        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

        StartCoroutine(StartClient());
    }

    IEnumerator StartClient()
    {
        //Half Second Delay Prevents Race Conditions
        yield return
            new WaitForSeconds(0.5f);

        Client.StartClient(this);

        StopCoroutine("StartClient");
    }

    private void EventSink_ConnectionFailedEvent(ConnectionFailedEventArgs args)
    {
        Client.Disconnect();
        Client.StartClient(this);
    }

    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        try
        {
            string filename = "error.txt";

            if (File.Exists(filename))
                File.AppendAllText(filename, e.ToString());
            else
                File.WriteAllText(filename, e.ToString());
        }   catch { }
    }

    internal void OnApplicationQuit()
    {
        if (!m_IsQuitting)
        {
            Application.CancelQuit();

            SendLogoutNotification();

            m_IsQuitting = true;

            if (Application.isEditor)
            {
                Thread.Sleep(750);
                ApplicationQuit();
            }

            else
            {
                StartCoroutine(ApplicationQuitRoutine());
            }
        }
    }

    IEnumerator ApplicationQuitRoutine()
    {
        yield return new WaitForSeconds(0.75f);

        Client.Disconnect();

        Application.Quit();
    }

    void ApplicationQuit()
    {
        Client.Disconnect();

        Application.Quit();
    }

    internal static void Post(Action action)
    {
        if(action != null)
            m_SyncContext.Post((o) => { action(); }, null);
    }

    private void EventSink_PublicKeySetEvent()
    {
        SendSessionKey();
        m_SessionKeySent = true;
        m_PublicKeyReceived = true;
    }

    private void EventSink_LoginAcceptedEvent(LoginSuccessEventArgs args)
    {
        m_LoginAuthenticated = true;
    }

    private void SendData(string data)
    {
        if (!m_SessionKeySent)
            Client.Send(data);
        else
            Client.Send
                (m_AesModule.EncryptStringToString(data) + EncryptedMessageTerminator);
    }

    private void SendSessionKey()
    {
        Client.Send
            (AsymModule.EncryptStringToString(GetAesKeyVector()));
    }

    internal string GetAesKeyVector()
    {
        return m_AesModule.GetKeyVector();
    }

    internal static string EncryptToString(string data)
    {
        return m_AesModule.EncryptStringToString(data);
    }

    internal static string DecryptToString(string data)
    {
        return m_AesModule.DecryptStringToString(data);
    }

    internal void AttemptLogin(string username, string password)
    {
        string cmd = string.Format("{0}{1}{2}{1}{3}{1}{4}",
            (int)ReadProtocol.GetVersion(), m_SegmentTerminator,
                NetworkCommand.GetCommand(NetworkCommandType.Login), username, password);

        Debug.Log("Sending: " + cmd);

        SendData(cmd);
    }

    //protocol#|crtacct#|username#|password#|email<#>
    internal void RequestNewAccount(string username, string password, string email)
    {
        string cmd = string.Format("{0}{1}{2}{1}{3}{1}{4}{1}{5}",
            (int)ReadProtocol.GetVersion(),

             m_SegmentTerminator,

             NetworkCommand.GetCommand(NetworkCommandType.CreateAccount),

             username, password, email);

        Debug.Log("Sending: " + cmd);

        SendData(cmd);
    }

    //protocol#|lbymsg#|username#|time#|message<#>
    internal void SendLobbyMessage(string message)
    {
        string cmd = string.Format("{0}{1}{2}{1}{3}{1}{4}{1}{5}",
            (int)ReadProtocol.GetVersion(),

            m_SegmentTerminator,

            NetworkCommand.GetCommand(NetworkCommandType.LobbyMessage),

            AccountManager.AccountInstance.Username,

            DateTime.UtcNow.ToShortTimeString(),

            message);

        Debug.Log("Sending: " + cmd);

        SendData(cmd);
    }

    internal void SendLogoutNotification()
    {
        string cmd = string.Format("{0}{1}{2}", 
            (int)ReadProtocol.GetVersion(), 
            m_SegmentTerminator, 
            NetworkCommand.GetCommand(NetworkCommandType.LogOut));

        Debug.Log("Sending: " + cmd);

        SendData(cmd);
    }

    internal void SendCirculationCommand()
    {
        string cmd = string.Format("{0}{1}{2}",
            (int)ReadProtocol.GetVersion(),
            m_SegmentTerminator,
            NetworkCommand.GetCommand(NetworkCommandType.Heartbeat));

        Debug.Log("Sending: " + cmd);

        SendData(cmd);
    }

    //protocol#|pvtmsg#|from#|to#|time#|message
    internal void SendPrivateMessage(string message, Account accountTo, Account accountFrom)
    {
        string cmd = string.Format("{0}{1}{2}{1}{3}{1}{4}{1}{5}{1}{6}",
            (int)ReadProtocol.GetVersion(),
            m_SegmentTerminator,
            NetworkCommand.GetCommand(NetworkCommandType.PrivateMessage),
            accountFrom.Identity,
            accountTo.Identity,
            DateTime.UtcNow.ToShortTimeString(),
            message);

        Debug.Log("Sending: " + cmd);

        SendData(cmd);
    }

    //protocol#|adfrnd#|friendId
    internal void AcceptFriendRequest(Account account)
    {
        string cmd = string.Format("{0}{1}{2}{1}{3}",
            (int)ReadProtocol.GetVersion(),
            m_SegmentTerminator,
            NetworkCommand.GetCommand(NetworkCommandType.AddFriend),
            account.Identity);

        Debug.Log("Sending: " + cmd);

        SendData(cmd);
    }

    internal void SendFriendRequest(Account account)
    {
        string cmd = string.Format("{0}{1}{2}{1}{3}",
            (int)ReadProtocol.GetVersion(),
            m_SegmentTerminator,
            NetworkCommand.GetCommand(NetworkCommandType.FriendRequest),
            account.Identity);

        Debug.Log("Sending: " + cmd);

        SendData(cmd);
    }

    internal void SendFriendRemovalNotification(Account account)
    {
        string cmd = string.Format("{0}{1}{2}{1}{3}",
            (int)ReadProtocol.GetVersion(),
            m_SegmentTerminator,
            NetworkCommand.GetCommand(NetworkCommandType.RemoveFriend),
            account.Identity);

        Debug.Log("Sending: " + cmd);

        SendData(cmd);
    }

    internal void SendStartMatchCommand(Account account)
    {
        //protocol#|crtmtch#|userId#|username
        string cmd = string.Format("{0}{1}{2}{1}{3}{1}{4}",
            (int)ReadProtocol.GetVersion(),
            m_SegmentTerminator,
            NetworkCommand.GetCommand(NetworkCommandType.CreateMatch),
            account.Identity,
            account.Username);

        Debug.Log("Sending: " + cmd);

        SendData(cmd);
    }

    //protocol#|lbymsg#|matchId#|username#|time#|message<#>
    internal void SendMatchMessage(string message, string matchId)
    {
        string cmd = string.Format("{0}{1}{2}{1}{3}{1}{4}{1}{5}{1}{6}",
            (int)ReadProtocol.GetVersion(),
            m_SegmentTerminator,
            NetworkCommand.GetCommand(NetworkCommandType.MatchMessage),
            matchId,
            AccountManager.AccountInstance.Username,
            DateTime.UtcNow.ToShortTimeString(),
            message);

        Debug.Log("Sending: " + cmd);

        SendData(cmd);
    }

    internal void SendEndMatchCommand(string matchId)
    {
        //Protocol#|endmtch#|matchId
        string cmd = string.Format("{0}{1}{2}{1}{3}",
            (int)ReadProtocol.GetVersion(),
            m_SegmentTerminator,
            NetworkCommand.GetCommand(NetworkCommandType.EndMatch),
            matchId);

        Debug.Log("Sending: " + cmd);

        SendData(cmd);
    }

    internal void SendJoinMatchCommand(string matchId)
    {
        //Protocol#|jnmtch#|matchId
        string cmd = string.Format("{0}{1}{2}{1}{3}",
            (int)ReadProtocol.GetVersion(),
            m_SegmentTerminator,
            NetworkCommand.GetCommand(NetworkCommandType.AttemptJoinMatch),
            matchId);

        Debug.Log("Sending: " + cmd);

        SendData(cmd);
    }

    internal void SendSpectateMatchCommand(string matchId)
    {
        //Protocol#|spcmtch#|matchId
        string cmd = string.Format("{0}{1}{2}{1}{3}",
            (int)ReadProtocol.GetVersion(),
            m_SegmentTerminator,
            NetworkCommand.GetCommand(NetworkCommandType.SpectateMatch),
            matchId);

        Debug.Log("Sending: " + cmd);

        SendData(cmd);
    }

    internal void SendMultiplePieceSyncCommands
        (string matchId, string accountId, int playerSlot, GamePiece[] pieces)
    {
        //0#|syncgmepce#|matchId#|playerAccountId#|playerSlot#|pieceNetworkId#|pieceName#|
        //   positionX#|positionY#|hitpoints#|defense#|attack#|turnDelay
        string cmd;
        for(int i = 0; i < pieces.Length; i++)
        {
            GamePiece piece = pieces[i];

            cmd = string.Format("{0}{1}{2}{1}{3}{1}{4}{1}{5}{1}{6}{1}{7}{1}{8}{1}{9}{1}{10}{1}{11}{1}{12}{1}{13}{1}{14}",
                (int)ReadProtocol.GetVersion(),
                m_SegmentTerminator,
                NetworkCommand.GetCommand(NetworkCommandType.SyncGamePiece),
                matchId,
                accountId,
                playerSlot,
                piece.NetworkIdentity,
                piece.GamePieceName,
                piece.BoardVector.x,
                piece.BoardVector.y,
                piece.CurrentHitPoints,
                piece.DefenseRating,
                piece.AttackRating,
                piece.TurnDelay,
                (int)piece.gameObject.transform.rotation.eulerAngles.y);

            Debug.Log("Sending: " + cmd);

            SendData(cmd);
        }

        cmd = string.Format("{0}{1}{2}{1}{3}{1}{4}", 
            (int)ReadProtocol.GetVersion(), 
            m_SegmentTerminator, 
            NetworkCommand.GetCommand(NetworkCommandType.EndGamePieceSync),
            matchId,
            accountId);

        Debug.Log("Sending: " + cmd);

        SendData(cmd);
    }

    internal void SendPieceSyncCommands
        (string matchId, string accountId, int playerSlot, GamePiece piece)
    {
        string cmd = string.Format("{0}{1}{2}{1}{3}{1}{4}{1}{5}{1}{6}{1}{7}{1}{8}{1}{9}{1}{10}{1}{11}{1}{12}{1}{13}{1}{14}",
               (int)ReadProtocol.GetVersion(),
               m_SegmentTerminator,
               NetworkCommand.GetCommand(NetworkCommandType.SyncGamePiece),
               matchId,
               accountId,
               playerSlot,
               piece.NetworkIdentity,
               piece.GamePieceName,
               piece.BoardVector.x,
               piece.BoardVector.y,
               piece.CurrentHitPoints,
               piece.DefenseRating,
               piece.AttackRating,
               piece.TurnDelay,
               (int)piece.gameObject.transform.rotation.eulerAngles.y);

        Debug.Log("Sending: " + cmd);

        SendData(cmd);

        cmd = string.Format("{0}{1}{2}{1}{3}{1}{4}",
            (int)ReadProtocol.GetVersion(),
            m_SegmentTerminator,
            NetworkCommand.GetCommand(NetworkCommandType.EndGamePieceSync),
            matchId,
            accountId);

        Debug.Log("Sending: " + cmd);

        SendData(cmd);
    }

    internal void SendNetworkPieceMovementCommand(string matchId, string pieceNetworkId, Vector2 v)
    {
        //protocol#|command#|matchId#|pieceNetId#|x#|y
        string cmd = string.Format("{0}{1}{2}{1}{3}{1}{4}{1}{5}{1}{6}",
            (int)ReadProtocol.GetVersion(),
            m_SegmentTerminator,
            NetworkCommand.GetCommand(NetworkCommandType.NetworkPieceMoveEvent),
            matchId,
            pieceNetworkId,
            v.x,
            v.y);

        Debug.Log("Sending: " + cmd);

        SendData(cmd);
    }

    internal void SendTurnUpdateCommand(string matchId, int turnStep)
    {
        //protocol#|cmd#|matchId#|step
        string cmd = string.Format("{0}{1}{2}{1}{3}{1}{4}",
            (int)ReadProtocol.GetVersion(),
            m_SegmentTerminator,
            NetworkCommand.GetCommand(NetworkCommandType.TurnStateUpdate),
            matchId,
            turnStep.ToString());

        Debug.Log("Sending: " + cmd);

        SendData(cmd);
    }

    Coroutine m_DelayProcessAttackRoutine;
    internal void SendAttackTargetSyncCommand
        (string matchId, string attackerId, int dmg, GamePiece[] targets, Vector2 tileVector)
    {   //protocol#|cmd#|matchId#|attackerId#|dmg#|targetId#|tilevector
        for (int i = 0; i < targets.Length; i++)
        {
            string cmd = string.Format("{0}{1}{2}{1}{3}{1}{4}{1}{5}",
                (int)ReadProtocol.GetVersion(),
                m_SegmentTerminator,
                NetworkCommand.GetCommand(NetworkCommandType.SyncAttackTarget),
                matchId,
                attackerId,
                targets[i].NetworkIdentity);

            SendData(cmd);

            m_DelayProcessAttackRoutine = StartCoroutine
                (DelaySendProcessAttackCommand(matchId, attackerId, tileVector));
        }
    }

    internal IEnumerator DelaySendProcessAttackCommand(string matchId, string attackerId, Vector2 tileVector)
    {
        yield return new WaitForSecondsRealtime(0.5f);

        string cmd = string.Format("{0}{1}{2}{1}{3}{1}{4}{1}{5}{1}{6}",
            (int)ReadProtocol.GetVersion(),
            m_SegmentTerminator,
            NetworkCommand.GetCommand(NetworkCommandType.ProcessAttack),
            matchId,
            attackerId, 
            ((int)tileVector.x).ToString(),
            ((int)tileVector.y).ToString());
        
        SendData(cmd);

        StopCoroutine(m_DelayProcessAttackRoutine); m_DelayProcessAttackRoutine = null;
    }
}