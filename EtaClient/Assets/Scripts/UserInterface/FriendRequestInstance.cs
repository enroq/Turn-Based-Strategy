using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FriendRequestInstance : MonoBehaviour
{
    [SerializeField]
    GameObject m_RequestPanel;

    [SerializeField]
    Text m_NotificationText;

    [SerializeField]
    Button m_AcceptButton;

    [SerializeField]
    Button m_DeclineButton;

    private Account m_AccountToFrom;
    private PrivateMessageHandler m_Handler;

    private void Start()
    {
        m_DeclineButton.onClick.AddListener(() => RemovePanel());
        m_AcceptButton.onClick.AddListener(() => AcceptRequest());
    }

    internal void SetAccountRelative(Account account, PrivateMessageHandler handler)
    {
        if (account != null)
        {
            m_Handler = handler;
            m_AccountToFrom = account;
            m_NotificationText.text = GetRequestContentString();
        }

        else Debug.LogError
                ("Attempting To Set Account Relative Of Friend Request To Null!");
    }

    private string GetRequestContentString()
    {
        return string.Format("{0} Would Like To Be Your Friend!", m_AccountToFrom.Username);
    }

    void RemovePanel()
    {
        m_Handler.RemoveFriendRequest(m_AccountToFrom.Identity);

        if (m_RequestPanel != null)
            Destroy(m_RequestPanel);
    }

    void AcceptRequest()
    {
        ClientManager.Instance.AcceptFriendRequest(m_AccountToFrom);
        m_Handler.RemoveFriendRequest(m_AccountToFrom.Identity);
        RemovePanel();
    }
}
