using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FriendRemovalInstance : MonoBehaviour
{
    [SerializeField]
    GameObject m_ConfirmationPanel;

    [SerializeField]
    Button m_ConfirmationButton;

    [SerializeField]
    Button m_CancelButton;

    [SerializeField]
    Text m_ConfirmationText;

    Account m_AccountRelative;
    LobbyManager m_Manager;

	void Start ()
    {
        m_ConfirmationButton.onClick.AddListener(() => RemoveFriend());
        m_CancelButton.onClick.AddListener(() => CancelRemoval());
	}

    private void RemoveFriend()
    {
        ClientManager.Instance.SendFriendRemovalNotification(m_AccountRelative);
        AccountManager.RemoveFriendFromList(m_AccountRelative.Identity);

        m_Manager.RemoveFriendFromCache(m_AccountRelative);

        m_AccountRelative = null;
        m_ConfirmationPanel.SetActive(false);
    }

    private void CancelRemoval()
    {
        m_AccountRelative = null;
        m_ConfirmationPanel.SetActive(false);
    }

    internal void AttachAccountToInstance(Account account, LobbyManager manager)
    {
        if (account != null)
        {
            m_AccountRelative = account;
            m_Manager = manager;
            m_ConfirmationText.text = GetConfirmationText();
        }

        else
            Debug.LogError("Attempting To Set Friend Removal Instance Account Relative To Null..");
    }

    private string GetConfirmationText()
    {
        return string.Format("Are You Sure You Want To Remove <b>{0}</b> From Your Friends List?", m_AccountRelative.Username);
    }
}
