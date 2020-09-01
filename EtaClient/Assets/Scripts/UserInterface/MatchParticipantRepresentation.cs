using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MatchParticipantRepresentation : MonoBehaviour
{
    [SerializeField]
    Text m_ButtonText;

    Account m_AccountRelative;

    //private void Start()
    //{
    //    gameObject.GetComponent<Button>().onClick.AddListener(() => OpenPrivateMessage());
    //}

    //void OpenPrivateMessage()
    //{
    //    if (m_AccountRelative.Identity == AccountManager.AccountInstance.Identity)
    //        return;

    //    if(m_AccountRelative != null && !PrivateMessageHandler.PrivateMessageInstanceExists(m_AccountRelative.Identity))
    //    {
    //        EventSink.InvokeStartPrivateMessageEvent
    //            (new StartPrivateMessageEventArgs(m_AccountRelative));
    //    }
    //}

    internal void SetAccountRelative(Account account)
    {
        m_AccountRelative = account;
        SetButtonText(account.Username);
    }

    internal void SetButtonText(string username)
    {
        if (m_ButtonText != null)
            m_ButtonText.text = username;
    }
}
