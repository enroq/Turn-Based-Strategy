using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MessageDisplayHandler : MonoBehaviour
{
    [SerializeField]
    GameObject m_MessagePanel;

    [SerializeField]
    Text m_MessageText;

    [SerializeField]
    Button m_ConfirmButton;

    EventSystem m_EventSystem;

    private void Start()
    {
        m_EventSystem = EventSystem.current;
        EventSink.MessageBoxEvent += EventSink_MessageBoxEvent;
    }

    private void EventSink_MessageBoxEvent(MessageBoxEventArgs args)
    {
        ClientManager.Post(() => ActivatePanelWithMessage(args.Message));
    }

    void ActivatePanelWithMessage(string msg)
    {
        m_MessageText.text = msg;
        m_MessagePanel.SetActive(true);

        TabInputHandler.InvokeTransitionEvent();

        m_EventSystem.SetSelectedGameObject
            (m_ConfirmButton.gameObject, new BaseEventData(m_EventSystem));
    }

    public void DisposeOfMessageBox()
    {
        m_MessageText.text = string.Empty;
        m_MessagePanel.SetActive(false);
    }
}
