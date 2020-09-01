using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Eta.Interdata;

namespace EtaServer
{
    internal class Account
    {
        private string m_ClientId;
        private string m_Username;
        private string m_Email;

        private string m_AccountId;

        private int m_Rating;
        private int m_Wins;
        private int m_Losses;

        private List<string> m_FriendIdentities = new List<string>();

        private MatchState m_CurrentMatch = null;

        public string Username { get { return m_Username; } }
        public string Email { get { return m_Email; } }
        public string AccountId { get { return m_AccountId; } }

        public int Rating { get { return m_Rating; } }
        public int Wins { get { return m_Wins; } }
        public int Losses { get { return m_Losses; } }

        public List<string> FriendIdentities { get { return m_FriendIdentities; } }

        public MatchState CurrentMatch { get { return m_CurrentMatch; } }

        public Account(string clientId, string username)
        {
            m_ClientId = clientId;
            m_Username = username;

            PopulateDetails();
            PopulateFriends();
        }

        public Account(string username)
        {
            if(string.IsNullOrEmpty(username))
            {
                Console.WriteLine("[Error]: Attempting To Create Account Object With Empty Username..");
                return;
            }

            m_Username = username;
            PopulateDetails();
        }

        private void PopulateDetails()
        {
            try
            {
                string[] detailCache =
                    AccountDatabaseHandler.GetAccountDetails(m_Username);

                m_Email = detailCache[0];
                m_AccountId = detailCache[4];

                Int32.TryParse(detailCache[1], out m_Rating);
                Int32.TryParse(detailCache[2], out m_Wins);
                Int32.TryParse(detailCache[3], out m_Losses);
            }

            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void PopulateFriends()
        {
            try
            {
                string[] friendIdCache =
                    AccountDatabaseHandler.GetUserFriends(m_AccountId);

                for (int i = friendIdCache.Length - 1; i >= 0; i--)
                    AddFriend(friendIdCache[i]);
            }

            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        internal void AddFriend(string id)
        {
            if (!m_FriendIdentities.Contains(id))
                m_FriendIdentities.Add(id);
        }

        internal void RemoveFriend(string id)
        {
            if (m_FriendIdentities.Contains(id))
                m_FriendIdentities.Remove(id);
        }

        internal void SetCurrentMatch(MatchState match)
        {
            if (match != null)
                m_CurrentMatch = match;
        }
    }

    internal class AccountHandler
    {
        static Dictionary<string, Account> 
            m_AccountsOnline = new Dictionary<string, Account>();

        static Dictionary<string, Account> 
            m_AccountIdentities = new Dictionary<string, Account>();

        public static Dictionary<string, Account> 
            AccountsOnline { get { return m_AccountsOnline; } }

        internal static void AddOnlineAccount(Account account)
        {
            if (!m_AccountsOnline.ContainsKey(account.Username))
                m_AccountsOnline.Add(account.Username, account);

            if (!m_AccountIdentities.ContainsKey(account.AccountId))
                m_AccountIdentities.Add(account.AccountId, account);
        }

        internal static void RemoveOnlineAccount(Account account)
        {
            if (m_AccountsOnline.ContainsKey(account.Username))
                m_AccountsOnline.Remove(account.Username);

            if (m_AccountIdentities.ContainsKey(account.AccountId))
                m_AccountIdentities.Remove(account.AccountId);

            if(account.CurrentMatch != null)
            {
                MatchHandler.HandleDisconnectFromMatch
                    (account.CurrentMatch, account.AccountId);
            }
        }

        internal static Account GetAccountById(string id)
        {
            if (m_AccountIdentities.ContainsKey(id))
                return m_AccountIdentities[id];
            else
                return null;
        }

        internal static bool AccountOnline(string username)
        {
            if(m_AccountsOnline.ContainsKey(username))
            {
                Account account = m_AccountsOnline[username];

                if (ClientManager.ClientAccountPairs.ContainsKey(account))
                    if (ClientManager.ClientAccountPairs[account].ClientIsConnected())
                        return true;
            }

            return false;
        }

        /// <summary>
        /// Generates An Array Of The Account Names Of Users Online In Reverse Alphabetical Order.
        /// </summary>
        /// <remarks>
        /// Returns Reverse Alphabetical Order So It Can Be Looped In Reverse.
        /// </remarks>
        internal static string[] OnlineAccountNames()
        {
            List<string> accounts
                = m_AccountsOnline.Keys.OrderByDescending(x => x).ToList();

            return accounts.ToArray();
        }

        /// <summary>
        /// Used To Concatenate Names Of Online Accounts To Be Sent To Clients
        /// </summary>
        internal static string OnlineAccountNameCluster(string[] names)
        {
            StringBuilder cluster = new StringBuilder();

            for(int i = names.Length -1; i >= 0; i--)
                cluster.Append(names[i] + ",");

            if (ServerCore.DebugMode)
                Console.WriteLine
                    ("Account Name Cluster Generated: \n" + cluster.ToString());

            return cluster.ToString();
        }
    }
}
