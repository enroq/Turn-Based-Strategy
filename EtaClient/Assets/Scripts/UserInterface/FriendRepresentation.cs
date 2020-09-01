using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FriendRepresentation : MonoBehaviour {

    [SerializeField]
    Button m_RemoveFriendButton;
    [SerializeField]
    Text m_UsernameLabel;

    Account m_AccountRelative;
    LobbyManager m_Manager;

    // Use this for initialization
    void Start ()
    {
        m_RemoveFriendButton.onClick.AddListener(() => ConfirmFriendRemoval());
	}
	
    private void ConfirmFriendRemoval()
    {
        m_Manager.ConfirmRemovalOfFriend(m_AccountRelative);
    }

    internal void SetAccountRelative(Account account, LobbyManager manager)
    {
        m_AccountRelative = account;
        m_Manager = manager;
        m_UsernameLabel.text = m_AccountRelative.Username;
    }
}
