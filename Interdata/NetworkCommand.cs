using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eta.Interdata
{
    public enum AccountSyncType
    {
        Connect,
        Disconnect
    }

    public enum NetworkCommandType
    {
        Login,
        CreateAccount,
        LoginSuccess,
        LoginFail,
        AccountCreated,
        AccountCreationFailed,
        LobbyMessage,
        SyncForeignAccount,
        LogOut,
        Heartbeat,
        PrivateMessage,
        AddFriend,
        SyncFriend,
        FriendRequest,
        RemoveFriend,
        CreateMatch,
        MatchCreated,
        AttemptJoinMatch,
        JoinMatchResult,
        FindRandomMatch,
        MatchMessage,
        SyncMatchParticipants,
        EndMatch,
        SyncMatchSpectator,
        ReadyMatch,
        StartMatch,
        SpectateMatch,
        AddSpectator,
        RemoveSpectator,
        SpectateMatchResult,
        UserLeftMatchInProg,
        DepartureTimerExpired,
        SyncGamePiece,
        EndGamePieceSync,
        NetworkPieceMoveEvent,
        TurnStateSync,
        TurnStateUpdate,
        SyncAttackTarget,
        ProcessAttack
    }

    public class NetworkCommand
    {
        internal static string[]
            m_NetworkCommands = new string[]
            {
                "login",
                "crtacct",
                "lgnsccs",
                "lgnfail",
                "acctcrtd",
                "acctcrtfl",
                "lbymsg",
                "syncfacct",
                "lgout",
                "hrtbt",
                "pvtmsg",
                "adfrnd",
                "sncfrnd",
                "frndrq",
                "rmfrnd",
                "crtmtch",
                "mtchcrtd",
                "jnmtch",
                "jnmtchres",
                "fndrdmmtch",
                "mtchmsg",
                "syncmtchps",
                "endmtch",
                "synmtcspc",
                "rdymtch",
                "strtmtch",
                "spcmtch",
                "addspc",
                "rmvspc",
                "spcmtchres",
                "usrlftmtch",
                "dprttmrexp",
                "syncgmepce",
                "endpcesync",
                "piecemove",
                "trnsttsyn",
                "trnsttup",
                "synatktgt",
                "procattk"
            };

        internal static Dictionary<string, int>
            m_NetworkCommandDictionary = new Dictionary<string, int>()
        {
            { "login",      (int)NetworkCommandType.Login },
            { "crtacct",    (int)NetworkCommandType.CreateAccount},
            { "lgnsccs",    (int)NetworkCommandType.LoginSuccess },
            { "lgnfail",    (int)NetworkCommandType.LoginFail },
            { "acctcrtd",   (int)NetworkCommandType.AccountCreated },
            { "acctcrtfl",  (int)NetworkCommandType.AccountCreationFailed },
            { "lbymsg",     (int)NetworkCommandType.LobbyMessage },
            { "syncfacct",  (int)NetworkCommandType.SyncForeignAccount },
            { "lgout",      (int)NetworkCommandType.LogOut },
            { "hrtbt",      (int)NetworkCommandType.Heartbeat },
            { "pvtmsg",     (int)NetworkCommandType.PrivateMessage },
            { "adfrnd",     (int)NetworkCommandType.AddFriend },
            { "sncfrnd",    (int)NetworkCommandType.SyncFriend },
            { "frndrq",     (int)NetworkCommandType.FriendRequest },
            { "rmfrnd",     (int)NetworkCommandType.RemoveFriend },
            { "crtmtch",    (int)NetworkCommandType.CreateMatch },
            { "mtchcrtd",   (int)NetworkCommandType.MatchCreated },
            { "jnmtch",     (int)NetworkCommandType.AttemptJoinMatch },
            { "jnmtchres",  (int)NetworkCommandType.JoinMatchResult },
            { "fndrdmmtch", (int)NetworkCommandType.FindRandomMatch },
            { "mtchmsg",    (int)NetworkCommandType.MatchMessage },
            { "syncmtchps", (int)NetworkCommandType.SyncMatchParticipants },
            { "endmtch",    (int)NetworkCommandType.EndMatch },
            { "synmtcspc",  (int)NetworkCommandType.SyncMatchSpectator },
            { "rdymtch",    (int)NetworkCommandType.ReadyMatch },
            { "strtmtch",   (int)NetworkCommandType.StartMatch },
            { "spcmtch",    (int)NetworkCommandType.SpectateMatch },
            { "addspc",     (int)NetworkCommandType.AddSpectator },
            { "rmvspc",     (int)NetworkCommandType.RemoveSpectator },
            { "spcmtchres", (int)NetworkCommandType.SpectateMatchResult },
            { "usrlftmtch", (int)NetworkCommandType.UserLeftMatchInProg },
            { "dprttmrexp", (int)NetworkCommandType.DepartureTimerExpired },
            { "syncgmepce", (int)NetworkCommandType.SyncGamePiece },
            { "endpcesync", (int)NetworkCommandType.EndGamePieceSync },
            { "piecemove",  (int)NetworkCommandType.NetworkPieceMoveEvent },
            { "trnsttsyn",  (int)NetworkCommandType.TurnStateSync },
            { "trnsttup",   (int)NetworkCommandType.TurnStateUpdate },
            { "synatktgt",  (int)NetworkCommandType.SyncAttackTarget },
            { "procattk",   (int)NetworkCommandType.ProcessAttack }
        };

        public static int GetCommandIndex(string cmd)
        {
            if (m_NetworkCommandDictionary.ContainsKey(cmd))
                return m_NetworkCommandDictionary[cmd];

            else return -1;
        }

        public static string GetCommand(NetworkCommandType type)
        {
            if (m_NetworkCommands.Length >= (int)type && (int)type >= 0)
                return m_NetworkCommands[(int)type];

            else return null;
        }
    }
}
