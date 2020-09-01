using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Eta.Interdata;

public class MatchPanelBehavior : MonoBehaviour
{
    [SerializeField]
    Text m_PlayerOneUsernameLabel;

    [SerializeField]
    Text m_PlayerTwoUsernameLabel;

    [SerializeField]
    Button m_SpectateButton;

    [SerializeField]
    Button m_JoinButton;

    [SerializeField]
    Color m_EmptyUserColor;

    [SerializeField]
    Color m_FilledUserColor;

    [SerializeField]
    Text m_SpectatorCountLabel;

    MatchState m_MatchRelative;

    string m_EmptySlotText = "";

    private void Start()
    {
        m_JoinButton.onClick.AddListener(() => AttemptJoinMatch());
        m_SpectateButton.onClick.AddListener(() => AttemptSpectateMatch());
    }

    void AttemptJoinMatch()
    {
        ClientManager.Instance.SendJoinMatchCommand(m_MatchRelative.MatchIdentity);
    }

    void AttemptSpectateMatch()
    {
        ClientManager.Instance.SendSpectateMatchCommand(m_MatchRelative.MatchIdentity);
    }

    internal void Initialize(MatchState state)
    {
        if (state != null)
        {
            m_MatchRelative = state;

            if (state.PlayerOne != null)
                SetPlayerOneUsernameLabel(state.PlayerOne.Username);

            ClearPlayerTwoLabel();
        }
    }

    internal void UpdateMatch(MatchState state)
    {
        if (state != null)
        {
            m_MatchRelative = state;

            if (state.PlayerOne != null)
                SetPlayerOneUsernameLabel(state.PlayerOne.Username);
            else
                ClearPlayerOneLabel();

            if (state.PlayerTwo != null)
                SetPlayerTwoUsernameLabel(state.PlayerTwo.Username);
            else
                ClearPlayerTwoLabel();

            if (state.MatchIsFull() || state.InProgress)
                m_JoinButton.interactable = false;
            else
                m_JoinButton.interactable = true;

            if (state.CanAddSpectator())
                m_SpectateButton.interactable = true;
            else
                m_SpectateButton.interactable = false;

            if (m_SpectatorCountLabel != null)
                m_SpectatorCountLabel.text = 
                    "Spectators: " + m_MatchRelative.Spectators.Count.ToString();
        }
    }

    internal void SetPlayerOneUsernameLabel(string username)
    {
        m_PlayerOneUsernameLabel.text = username;
        m_PlayerOneUsernameLabel.color = m_FilledUserColor;
    }

    internal void SetPlayerTwoUsernameLabel(string username)
    {
        m_PlayerTwoUsernameLabel.text = username;
        m_PlayerTwoUsernameLabel.color = m_FilledUserColor;
    }

    internal void ClearPlayerOneLabel()
    {
        m_PlayerOneUsernameLabel.text = m_EmptySlotText;
        m_PlayerOneUsernameLabel.color = m_EmptyUserColor;
    }

    internal void ClearPlayerTwoLabel()
    {
        m_PlayerTwoUsernameLabel.text = m_EmptySlotText;
        m_PlayerTwoUsernameLabel.color = m_EmptyUserColor;
    }
}
