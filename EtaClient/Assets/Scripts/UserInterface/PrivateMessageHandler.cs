using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrivateMessageHandler : MonoBehaviour
{
    [SerializeField]
    GameObject m_MessageNotificationPanel;
    [SerializeField]
    GameObject m_MessageDisplayPanel;
    [SerializeField]
    GameObject m_PrivateMessageBoxPrefab;
    [SerializeField]
    GameObject m_MessageRepresenationPrefab;
    [SerializeField]
    GameObject m_FriendRequestPanelPrefab;

    private static Dictionary<string, GameObject> 
        m_PrivateMessages = new Dictionary<string, GameObject>();

    private static Dictionary<string, GameObject>
        m_MessageNotifications = new Dictionary<string, GameObject>();

    private static Dictionary<string, GameObject>
        m_FriendRequests = new Dictionary<string, GameObject>();

    internal static void NotifyFriendRequestAccepted(string accountId)
    {
        if (m_PrivateMessages.ContainsKey(accountId))
        {
            m_PrivateMessages[accountId].GetComponent
                <PrivateMessageInstance>().AddTextToContent("<b>Friend Request Accepted!</b>");
        }
    }

    internal static bool PrivateMessageInstanceExists(string accountId)
    {
        return m_PrivateMessages.ContainsKey(accountId);
    }

    internal static void RemovePrivateMessage(string accountId)
    {
        if (m_PrivateMessages.ContainsKey(accountId))
        {
            Destroy(m_PrivateMessages[accountId]);
            m_PrivateMessages.Remove(accountId);
        }

        if (m_MessageNotifications.ContainsKey(accountId))
        {
            Destroy(m_MessageNotifications[accountId]);
            m_MessageNotifications.Remove(accountId);
        }
    }

    internal static void MinimizeMessage(string accountId)
    {
        if (m_PrivateMessages.ContainsKey(accountId))
            m_PrivateMessages[accountId].SetActive(false);
    }

    internal static void MaximizeMessage(string accountId)
    {
        if (m_PrivateMessages.ContainsKey(accountId))
            m_PrivateMessages[accountId].SetActive(true);
    }

    void Start ()
    {
        EventSink.PrivateMessageEvent += EventSink_PrivateMessageEvent;
        EventSink.StartPrivateMessageEvent += EventSink_StartPrivateMessageEvent;
        EventSink.FriendRequestEvent += EventSink_FriendRequestEvent;
	}

    private void EventSink_PrivateMessageEvent(PrivateMessageEventArgs args)
    {
        ClientManager.Post(() => HandlePrivateMessage(args));
    }

    private void EventSink_FriendRequestEvent(FriendRequestEventArgs args)
    {
        ClientManager.Post(() => DisplayFriendRequest(args.Account));
    }

    private void EventSink_StartPrivateMessageEvent(StartPrivateMessageEventArgs args)
    {
        HandleStartPrivateMessage(args);
    }

    private void HandleStartPrivateMessage(StartPrivateMessageEventArgs args)
    {
        string accountId = args.Account.Identity;
        if (!m_PrivateMessages.ContainsKey(accountId))
        {
            m_PrivateMessages.Add
                (accountId, Instantiate(m_PrivateMessageBoxPrefab));

            m_PrivateMessages[accountId].transform.SetParent(m_MessageDisplayPanel.transform, false);

            m_PrivateMessages[accountId].GetComponent
                <PrivateMessageInstance>().InitializePrivateMessage(args.Account);

            if (!m_MessageNotifications.ContainsKey(accountId))
            {
                m_MessageNotifications.Add(accountId, Instantiate(m_MessageRepresenationPrefab));
                m_MessageNotifications[accountId].transform.SetParent(m_MessageNotificationPanel.transform, false);
                m_MessageNotifications[accountId].GetComponent
                    <PrivateMessageButtonRepresenation>().SetPrivateMessageObject(m_PrivateMessages[accountId]);
            }
        }
    }

    private void DisplayFriendRequest(Account account)
    {
        string accountId = account.Identity;
        if(!m_FriendRequests.ContainsKey(accountId))
        {
            m_FriendRequests.Add
                (accountId, Instantiate(m_FriendRequestPanelPrefab));
            m_FriendRequests[accountId].GetComponent<FriendRequestInstance>().SetAccountRelative(account, this);
            m_FriendRequests[accountId].transform.SetParent(m_MessageDisplayPanel.transform, false);
        }
    }

    internal void RemoveFriendRequest(string id)
    {
        if (m_FriendRequests.ContainsKey(id))
        {
            m_FriendRequests.Remove(id);
        }
    }

    private void HandlePrivateMessage(PrivateMessageEventArgs args)
    {
        string accountId = args.AccountId;
        if (!m_PrivateMessages.ContainsKey(accountId))
        {
            m_PrivateMessages.Add
                (accountId, Instantiate(m_PrivateMessageBoxPrefab));

            m_PrivateMessages[accountId].transform.SetParent(m_MessageDisplayPanel.transform, false);

            m_PrivateMessages[accountId].GetComponent<PrivateMessageInstance>().
                InitializePrivateMessage(AccountManager.GetAccountById(accountId), args.Content, args.TimeStamp);

            if (!m_MessageNotifications.ContainsKey(accountId))
            {
                m_MessageNotifications.Add(accountId, Instantiate(m_MessageRepresenationPrefab));
                m_MessageNotifications[accountId].transform.SetParent(m_MessageNotificationPanel.transform, false);
                m_MessageNotifications[accountId].GetComponent
                    <PrivateMessageButtonRepresenation>().SetPrivateMessageObject(m_PrivateMessages[accountId]);
            }
        }

        else if (m_PrivateMessages.ContainsKey(accountId))
        {
            m_PrivateMessages[accountId].GetComponent
                <PrivateMessageInstance>().AddTextToContent
                    (args.Content, args.TimeStamp, AccountManager.GetAccountById(accountId));
        }
    }
}
