using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PrivateMessageInstance : MonoBehaviour
{
    [SerializeField]
    Button m_SendButton;
    [SerializeField]
    InputField m_MessageInput;
    [SerializeField]
    Scrollbar m_Scrollbar;
    [SerializeField]
    Text m_MessageText;
    [SerializeField]
    Text m_UsernameLabel;
    [SerializeField]
    Button m_ExitButton;
    [SerializeField]
    Button m_MinimizeButton;
    [SerializeField]
    Button m_AddFriendButton;
    [SerializeField]
    GameObject m_MessagePanel;

    Account m_AccountToFrom;
    Coroutine m_ScrollRoutine;

    internal Account AccountToFrom { get { return m_AccountToFrom; } }

	void Start ()
    {
        m_SendButton.onClick.AddListener(() => SendMessage());
        m_ExitButton.onClick.AddListener(() => ExitPrivateMessage());
        m_MinimizeButton.onClick.AddListener(() => MinimizeMessage());
        m_AddFriendButton.onClick.AddListener(() => AddFriend());
	}

    void AddFriend()
    {
        if (m_AccountToFrom.Identity != AccountManager.AccountInstance.Identity)
        {
            ClientManager.Instance.SendFriendRequest(m_AccountToFrom);
            AddTextToContent("<b>Friend Request Sent!</b>");
        }
    }

    void ExitPrivateMessage()
    {
        PrivateMessageHandler.RemovePrivateMessage(m_AccountToFrom.Identity);
    }

    void MinimizeMessage()
    {
        PrivateMessageHandler.MinimizeMessage(m_AccountToFrom.Identity);
    }

    void SendMessage()
    {
        if(!string.IsNullOrEmpty(m_MessageInput.text))
        {
            ClientManager.Instance.SendPrivateMessage
                (m_MessageInput.text, m_AccountToFrom, AccountManager.AccountInstance);

            AddTextToContent
                (
                    m_MessageInput.text, 
                    DateTime.UtcNow.ToShortTimeString(), 
                    AccountManager.AccountInstance
                );

            m_MessageInput.text = string.Empty;
        }
    }

    internal void InitializePrivateMessage(Account accountFrom)
    {
        m_AccountToFrom = accountFrom;
        m_UsernameLabel.text = m_AccountToFrom.Username;
    }

    internal void InitializePrivateMessage(Account accountFrom, string firstMessage, string timeStamp)
    {
        InitializePrivateMessage(accountFrom);

        AddTextToContent(firstMessage, timeStamp, accountFrom);
    }

    internal void AddTextToContent(string text, string timeStamp, Account account)
    {
        if (!string.IsNullOrEmpty(m_MessageText.text))
            m_MessageText.text += "\n";

        m_MessageText.text += string.Format
            ("[{0}] <b>{1}:</b> {2}", timeStamp, account.Username, text);

        m_ScrollRoutine = StartCoroutine(ScrollDown());
    }

    internal void AddTextToContent(string text)
    {
        if (!string.IsNullOrEmpty(m_MessageText.text))
            m_MessageText.text += "\n";

        m_MessageText.text += string.Format
            ("{0}", text);
    }

    IEnumerator ScrollDown()
    {
        yield return new WaitForSeconds(0.5f);
        m_Scrollbar.value = 0f;

        StopCoroutine(m_ScrollRoutine);
        m_ScrollRoutine = null;
    }
}
