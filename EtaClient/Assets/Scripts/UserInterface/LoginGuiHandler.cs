using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoginGuiHandler : MonoBehaviour {

    [SerializeField]
    GameObject m_LoginPanel;
    [SerializeField]
    GameObject m_SignUpPanel;

    [SerializeField]
    private Button m_LoginButton;
    [SerializeField]
    private Button m_CreateAccountButton;

    [SerializeField]
    private InputField m_UsernameText;
    [SerializeField]
    private InputField m_PasswordText;

    [SerializeField]
    GameObject m_ConnectionFailedPanel;
    [SerializeField]
    Text m_ConnectionFailedText;

    bool m_IsConnected;
    string m_FailedConnectionString = "Attempting To Connect... ({0})";

	private void Start()
    {
        EventSink.ClientConnectedEvent += ClientManager_ClientConnectedEvent;
        EventSink.LoginSuccessEvent += EventSink_LoginAcceptedEvent;
        EventSink.ConnectionFailedEvent += EventSink_ConnectionFailedEvent;

        InvokeRepeating
            ("CheckConnection", 1.0f, 1.0f);

        Debug.Log("Login Gui Handler Started..");
    }

    private void EventSink_ConnectionFailedEvent(ConnectionFailedEventArgs args)
    {
        ClientManager.Post(() =>
        {
            if (!m_ConnectionFailedPanel.activeInHierarchy)
                m_ConnectionFailedPanel.SetActive(true);
        });

        ClientManager.Post(() =>
        {
            if(m_ConnectionFailedText != null && m_ConnectionFailedText.IsActive())
                m_ConnectionFailedText.text =
                    string.Format(m_FailedConnectionString, args.ConnectionAttempts);
        });
    }

    private void EventSink_LoginAcceptedEvent(LoginSuccessEventArgs args)
    {
        ClientManager.Post(() => DismissLoginPanel());
    }

    private void ClientManager_ClientConnectedEvent(ClientConnectedEventArgs args)
    {
        m_IsConnected = true;

        Debug.Log("Connection Event Received..");

        ClientManager.Post(() =>
        {
            if (m_ConnectionFailedPanel.activeInHierarchy)
                m_ConnectionFailedPanel.SetActive(false);
        });
    }

    private void CheckConnection()
    {
        if(m_IsConnected)
        {
            if (m_CreateAccountButton != null)
                m_CreateAccountButton.interactable = true;

            if (m_LoginButton != null)
                m_LoginButton.interactable = true;

            CancelInvoke();
        }
    }

    public void AttemptLogin()
    {
        if(string.IsNullOrEmpty(m_UsernameText.text))
        {
            EventSink.InvokeMessageBoxEvent(new MessageBoxEventArgs("Username Field Must Not Be Empty."));
            return;
        }

        if(string.IsNullOrEmpty(m_PasswordText.text))
        {
            EventSink.InvokeMessageBoxEvent(new MessageBoxEventArgs("Password Field Must Not Be Empty."));
            return;
        }

        ClientManager.Instance.AttemptLogin
            (m_UsernameText.text, m_PasswordText.text);
    }

    internal void DismissLoginPanel()
    {
        m_LoginPanel.SetActive(false);
    }

    internal void DisplaySignUpForm()
    {
        m_SignUpPanel.SetActive(true);
    }

    public void TransitionToSignUp()
    {
        DismissLoginPanel();
        DisplaySignUpForm();
    }
}
