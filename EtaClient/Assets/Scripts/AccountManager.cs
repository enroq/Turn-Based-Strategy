using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Account
{
    private string m_Username;
    private string m_Email;

    private string m_Identity;

    private int m_Rating;
    private int m_Wins;
    private int m_Losses;

    public string Username { get { return m_Username; } }
    public string Email { get { return m_Email; } }

    public string Identity { get { return m_Identity; } }

    public int Rating { get { return m_Rating; } }
    public int Wins { get { return m_Wins; } }
    public int Losses { get { return m_Losses; } }

    public Account
        (string id, string username, string email, string rating, string wins, string losses)
    {
        m_Username = username;
        m_Email = email;
        m_Identity = id;

        try
        {
            Int32.TryParse(rating, out m_Rating);
            Int32.TryParse(wins, out m_Wins);
            Int32.TryParse(losses, out m_Losses);
        }

        catch(Exception e)
        {
            Debug.Log(e.ToString());
        }
    }
}

public class AccountManager : MonoBehaviour
{
    private static Account m_AccountInstance;

    public static Account AccountInstance { get { return m_AccountInstance; } }

    private static List<Account> m_ForeignAccounts = new List<Account>();

    private static Dictionary<string, Account> 
        m_AccountsDictionary = new Dictionary<string, Account>();

    private static Dictionary<string, Account>
        m_FriendsDictionary = new Dictionary<string, Account>();

    public static void SetCurrentAccount(Account account)
    {
        m_AccountInstance = account;
    }

    public static Account GetAccountById(string id)
    {
        if (m_AccountsDictionary.ContainsKey(id))
            return m_AccountsDictionary[id];

        else
            return null;
    }

    public static void SyncFriendToList(string accountId, Account account)
    {
        if(!m_FriendsDictionary.ContainsKey(accountId))
        {
            m_FriendsDictionary.Add(accountId, account);

            Debug.LogFormat("Adding [{0}] To Friends List..", account.Username);

            EventSink.InvokeAddFriendToListEvent
                (new FriendAddedToListEventArgs(account));
        }
    }

    public static void RemoveFriendFromList(string id)
    {
        if (m_FriendsDictionary.ContainsKey(id))
        {
            m_FriendsDictionary.Remove(id);
        }
    }

    public static void RemoveForeignAccount(string accountId)
    {
        if(m_AccountsDictionary.ContainsKey(accountId))
        {
            Account account = m_AccountsDictionary[accountId];

            if (m_ForeignAccounts.Contains(account))
                m_ForeignAccounts.Remove(account);

            m_AccountsDictionary.Remove(accountId);

            EventSink.InvokeForeignAccountEvent
                (new ForeignAccountSyncEventArgs(m_ForeignAccounts.IndexOf(account), account, 1));
        }
    }

    public static void AddForeignAccount(Account account)
    {
        if (!m_ForeignAccounts.Contains(account))
        {
            int placementIndex = -1;
            for (int i = 0; i < m_ForeignAccounts.Count; i++)
            {
                if (m_ForeignAccounts[i].Username.CompareTo(account.Username) == 1)
                {
                    placementIndex = i;
                    break;
                }
            }

            if (placementIndex != -1)
            {
                m_ForeignAccounts.Insert(placementIndex, account);
                EventSink.InvokeForeignAccountEvent
                    (new ForeignAccountSyncEventArgs(placementIndex, account, 0));
            }
            else
            {
                m_ForeignAccounts.Add(account);
                EventSink.InvokeForeignAccountEvent
                    (new ForeignAccountSyncEventArgs(m_ForeignAccounts.IndexOf(account), account, 0));
            }

            m_AccountsDictionary.Add(account.Identity, account);
        }
    }
}
