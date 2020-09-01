using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour
{
    [SerializeField]
    GameObject m_LobbyPanel;

    [SerializeField]
    Text m_UsernameLabel;

    [SerializeField]
    Button m_QuitGameButton;

    [SerializeField]
    GameObject m_UserDisplayArea;

    [SerializeField]
    GameObject m_UserRepresenationPrefab;

    [SerializeField]
    GameObject m_FriendsListDisplayArea;

    [SerializeField]
    GameObject m_FriendsListScrollView;

    [SerializeField]
    Button m_FriendsListButton;

    [SerializeField]
    Button m_OnlineUserListButton;

    [SerializeField]
    GameObject m_FriendRepresentationPrefab;

    [SerializeField]
    GameObject m_FriendRemoveConfirmationPanel;

    Dictionary<string, GameObject>
        m_UserRepCache = new Dictionary<string, GameObject>();

    Dictionary<string, GameObject>
        m_FriendRepCache = new Dictionary<string, GameObject>();

    private void Start()
    {
        EventSink.LoginSuccessEvent += EventSink_LoginAcceptedEvent;
        EventSink.ForeignAccountSyncEvent += EventSink_ForeignAccountSyncEvent;
        EventSink.AddFriendToListEvent += EventSink_AddFriendToListEvent;
        EventSink.FriendRemovedEvent += EventSink_FriendRemovedEvent;
        EventSink.MatchJoinedEvent += EventSink_MatchJoinedEvent;
        EventSink.MatchCancelledEvent += EventSink_MatchCancelledEvent;

        m_OnlineUserListButton.interactable = false;

        m_QuitGameButton.onClick.AddListener(() => QuitGame());
    }

    private void QuitGame()
    {
        ClientManager.Instance.OnApplicationQuit();
    }

    private void EventSink_MatchCancelledEvent(MatchCancelledEventArgs args)
    {
        if (m_LobbyPanel)
            m_LobbyPanel.SetActive(true);
    }

    private void EventSink_MatchJoinedEvent(MatchJoinedEventArgs args)
    {
        if(m_LobbyPanel)
            m_LobbyPanel.SetActive(false);
    }

    private void EventSink_FriendRemovedEvent(FriendRemovedEventArgs args)
    {
        ClientManager.Post(() => RemoveFriendFromCache(args.Account));
    }

    private void EventSink_AddFriendToListEvent(FriendAddedToListEventArgs args)
    {
        ClientManager.Post(() => AddUserToFriendList(args.Account));

        Debug.Log("Add Friend Event Has Been Triggered..");
    }

    private void EventSink_ForeignAccountSyncEvent(ForeignAccountSyncEventArgs args)
    {
        if (args.SyncType == 0)
        {
            ClientManager.Post(() => AddUserButtonToDisplay(args.Index, args.Account));
            if (m_FriendRepCache.ContainsKey(args.Account.Username))
            {
                ClientManager.Post(() => EnableFriendButton(args.Account.Username));
            }
        }

        else if (args.SyncType == 1)
        {
            ClientManager.Post(() => RemoveUserButtonFromDisplay(args.Account));
            if (m_FriendRepCache.ContainsKey(args.Account.Username))
            {
                ClientManager.Post(() => DisableFriendButton(args.Account.Username));
            }
        }
    }

    private void EventSink_LoginAcceptedEvent(LoginSuccessEventArgs args)
    {
        ClientManager.Post(() => DisplayLobby());

        ClientManager.Post(() => SetUsernameLabel(args.Account.Username));
    }

    private void DisableFriendButton(string username)
    {
        m_FriendRepCache[username].GetComponent<Button>().interactable = false;
    }

    private void EnableFriendButton(string username)
    {
        m_FriendRepCache[username].GetComponent<Button>().interactable = true;
    }

    public void SwitchToFriendsList()
    {
        m_FriendsListButton.interactable = false;
        m_OnlineUserListButton.interactable = true;

        HideUserList();
        DisplayFriendsList();
    }

    public void SwitchToUserList()
    {
        m_FriendsListButton.interactable = true;
        m_OnlineUserListButton.interactable = false;

        DisplayUserList();
        HideFriendsList();
    }

    private void HideUserList()
    {
        if (m_UserDisplayArea != null)
            m_UserDisplayArea.SetActive(false);
    }

    private void DisplayUserList()
    {
        if (m_UserDisplayArea != null)
            m_UserDisplayArea.SetActive(true);
    }

    private void HideFriendsList()
    {
        if (m_FriendsListDisplayArea != null)
            m_FriendsListDisplayArea.SetActive(false);

        if (m_FriendsListScrollView != null)
            m_FriendsListScrollView.SetActive(false);
    }

    private void DisplayFriendsList()
    {
        if (m_FriendsListDisplayArea != null)
            m_FriendsListDisplayArea.SetActive(true);

        if (m_FriendsListScrollView != null)
            m_FriendsListScrollView.SetActive(true);
    }

    private void DisplayLobby()
    {
        if(m_LobbyPanel != null)
            m_LobbyPanel.SetActive(true);
    }

    private void SetUsernameLabel(string username)
    {
        if(m_UsernameLabel != null)
            m_UsernameLabel.text = username;
    }

    private void RemoveUserButtonFromDisplay(Account account)
    {
        if(m_UserRepCache.ContainsKey(account.Username))
        {
            Destroy(m_UserRepCache[account.Username]);
            m_UserRepCache.Remove(account.Username);
        }
    }

    private void AddUserButtonToDisplay(int index, Account account)
    {
        GameObject button = Instantiate(m_UserRepresenationPrefab);
        UserRepresenation userrep = button.GetComponent<UserRepresenation>();

        m_UserRepCache.Add(account.Username, button);

        button.SetActive(false);

        userrep.SetAccountRelative(account);
        userrep.transform.SetParent(m_UserDisplayArea.transform, false);
        userrep.transform.SetSiblingIndex(index);

        button.SetActive(true);
    }

    private void AddUserToFriendList(Account account)
    {
        GameObject button = Instantiate(m_FriendRepresentationPrefab);
        FriendRepresentation friendrep = button.GetComponent<FriendRepresentation>();

        m_FriendRepCache.Add(account.Username, button);

        button.SetActive(false);

        friendrep.SetAccountRelative(account, this);
        friendrep.transform.SetParent(m_FriendsListDisplayArea.transform, false);

        button.SetActive(true);

        if (AccountManager.GetAccountById(account.Identity) == null)
            button.GetComponent<Button>().interactable = false;
    }

    internal void ConfirmRemovalOfFriend(Account account)
    {
        m_FriendRemoveConfirmationPanel.GetComponent
            <FriendRemovalInstance>().AttachAccountToInstance(account, this);

        m_FriendRemoveConfirmationPanel.SetActive(true);
    }

    internal void RemoveFriendFromCache(Account account)
    {
        if (m_FriendRepCache.ContainsKey(account.Username))
        {
            Destroy(m_FriendRepCache[account.Username]);
            m_FriendRepCache.Remove(account.Username);
        }
    }
}
