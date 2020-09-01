using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable
public class MatchChatHandler : MonoBehaviour
{
    [SerializeField]
    Text m_ChatText;
    [SerializeField]
    Scrollbar m_ScrollBar;
    [SerializeField]
    InputField m_ChatInput;
    [SerializeField]
    Toggle m_AutoscrollToggle;
    [SerializeField]
    Button m_SendButton;

    int m_MessageCount = 0;
    int m_MessageLimit = 128;

    string[] m_MessageSeparators = new string[] { "\n" };

    bool m_AutoScroll = true;
    bool m_RequiresScroll = false;

    Coroutine m_ScrollRoutine;

    public void RelayLobbyMessageFromInput()
    {
        if (!String.IsNullOrEmpty(m_ChatInput.text))
        {
            ClientManager.Instance.SendMatchMessage
                (m_ChatInput.text, MatchHandler.CurrentMatch.MatchIdentity);

            m_ChatInput.text = string.Empty;
        }
    }

    public void ToggleOutScroll()
    {
        if (m_AutoscrollToggle != null)
        {
            Debug.Log("Auto Scroll Toggle: " + m_AutoscrollToggle.isOn);
            m_AutoScroll = m_AutoscrollToggle.isOn;
        }
    }

	void Start ()
    {
        EventSink.MatchChatMessageEvent += EventSink_MatchChatMessageEvent;

        m_SendButton.onClick.AddListener(() => RelayLobbyMessageFromInput());

        m_AutoscrollToggle.onValueChanged.AddListener((val) => ToggleOutScroll());
	}

    private void EventSink_MatchChatMessageEvent(MatchChatMessageEventArgs args)
    {
        ClientManager.Post(() => AddMessageToChat(args.Time, args.Username, args.Content));
    }

    void SimChat()
    {
        string username = "Alpha";
        string time = DateTime.Now.ToShortTimeString();
        string content = string.Format
            ("This is a test message! ({0})", m_MessageCount++);

        AddMessageToChat(time, username, content);
    }

    void AddMessageToChat(string time, string username, string content)
    {
        if (m_ChatText != null)
        {
            string textCache = m_ChatText.text;
            string[] messages = textCache.Split
                (m_MessageSeparators, StringSplitOptions.None);

            if (messages.Length > m_MessageLimit)
            {
                m_ChatText.text = textCache.Remove(0, messages[0].Length).Trim(' ');

                Debug.Log(string.Format
                    ("Message Count: {0} # Message Zero Length: {1}", messages.Length, messages[0].Length));

                Debug.Log("Total String Length: " + m_ChatText.text.Length);
            }

            if (!string.IsNullOrEmpty(m_ChatText.text))
                m_ChatText.text += "\n";

            m_ChatText.text += string.Format
                ("[{0}] <b>{1}:</b> {2}", time, username, content);
        }

        if(m_ScrollBar != null && m_AutoScroll)
        {
            m_ScrollRoutine = StartCoroutine(ScrollDown());
        }
    }

    IEnumerator ScrollDown()
    {
        yield return new WaitForSeconds(0.25f);
        m_ScrollBar.value = 0;

        StopCoroutine(m_ScrollRoutine);
        m_ScrollRoutine = null;
    }
}
