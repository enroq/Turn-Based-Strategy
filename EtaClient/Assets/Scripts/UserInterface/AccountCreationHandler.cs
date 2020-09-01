using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AccountCreationHandler : MonoBehaviour
{
    [SerializeField]
    private InputField m_UsernameText;
    [SerializeField]
    private InputField m_PasswordText;
    [SerializeField]
    private InputField m_PasswordConfirmText;
    [SerializeField]
    private InputField m_EmailText;

    [SerializeField]
    GameObject m_LoginPanel;
    [SerializeField]
    GameObject m_SignUpPanel;

    private int m_MinUsernameLength = 4;
    private int m_MaxUsernameLength = 32;

    private void Start()
    {
        EventSink.AccountCreatedEvent += EventSink_AccountCreatedEvent;
    }

    private void EventSink_AccountCreatedEvent()
    {
        ClientManager.Post(() => TransitionToLogin());
    }

    public void AttemptAccountCreation()
    {
        if(string.IsNullOrEmpty(m_UsernameText.text) ||
           string.IsNullOrEmpty(m_PasswordText.text) ||
           string.IsNullOrEmpty(m_PasswordConfirmText.text) ||
           string.IsNullOrEmpty(m_EmailText.text))
        {
            EventSink.InvokeMessageBoxEvent
                (new MessageBoxEventArgs("One Or More Of The Required Fields Is Empty."));
            return;
        }

        if(m_UsernameText.text.Length < m_MinUsernameLength 
            || m_UsernameText.text.Length > m_MaxUsernameLength)
        {
            EventSink.InvokeMessageBoxEvent
                (new MessageBoxEventArgs("Username Must Be Between 4-32 Characters."));
            return;
        }

        if(!System.Text.RegularExpressions.Regex.IsMatch(m_UsernameText.text, @"^\w+$"))
        {
            EventSink.InvokeMessageBoxEvent
                (new MessageBoxEventArgs("Username May Only Contain Letters, Numbers, And Underscores."));
            return;
        }

        if(m_PasswordText.text != m_PasswordConfirmText.text)
        {
            EventSink.InvokeMessageBoxEvent
                (new MessageBoxEventArgs("Passwords Supplied Do Not Match."));
            return;
        }

        if(m_EmailText.text.IndexOf('@') <= -1 || m_EmailText.text.IndexOf('.') <= -1)
        {
            EventSink.InvokeMessageBoxEvent
                (new MessageBoxEventArgs("Email Address Supplied Is Not Properly Formatted."));
            return;
        }

        ClientManager.Instance.RequestNewAccount
            (m_UsernameText.text, m_PasswordText.text, m_EmailText.text);
    }

    internal void DisplayLoginPanel()
    {
        m_LoginPanel.SetActive(true);
    }

    internal void DismissSignUpForm()
    {
        m_SignUpPanel.SetActive(false);
    }

    public void TransitionToLogin()
    {
        DisplayLoginPanel();
        DismissSignUpForm();
    }
}
