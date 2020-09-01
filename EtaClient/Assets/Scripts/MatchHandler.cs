using Eta.Interdata;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MatchHandler : MonoBehaviour
{
    [SerializeField]
    Button m_StartMatchButton;

    [SerializeField]
    Button m_FindMatchButton;

    [SerializeField]
    GameObject m_MatchGuiPanel;

    [SerializeField]
    GameObject m_MatchWaitPanel;

    [SerializeField]
    Image m_WaitSpinner;

    [SerializeField]
    Button m_CancelMatchMakingButton;

    [SerializeField]
    GameObject m_MatchUsersDisplayArea;

    [SerializeField]
    GameObject m_MatchListDisplayPanel;

    [SerializeField]
    GameObject m_MatchInstancePanelPrefab;

    [SerializeField]
    GameObject m_ParticipantRepButton;

    [SerializeField]
    GameObject m_MatchExpirationPanel;

    [SerializeField]
    Text m_ExpirationTimeLabel;

    //-------------------------------------//

    static MatchState m_CurrentMatch;

    static bool m_AwaitingMatchCreated;
    static bool m_AwaitingChallenger;
    static bool m_MatchIsDisplayed;
    static bool m_MatchAwaitingExpiration;

    static int m_PlayerPosition = 0;

    static Dictionary<string, MatchState> 
        m_AvailableMatches = new Dictionary<string, MatchState>();

    static Dictionary<string, GameObject> 
        m_MatchPanelRelationships = new Dictionary<string, GameObject>();

    static Dictionary<string, GameObject> 
        m_ParticipantRepCache = new Dictionary<string, GameObject>();

    static Dictionary<string, GameObject>
        m_SpectatorRepCache = new Dictionary<string, GameObject>();

    internal static MatchState CurrentMatch
    {
        get { return m_CurrentMatch; }
    }

    internal static int PlayerPosition { get { return m_PlayerPosition; } }

    internal static void SetCurrentMatch(MatchState match)
    {
        if (match != null)
            m_CurrentMatch = match;
    }

    internal static MatchState GetMatchById(string matchId)
    {
        if (m_AvailableMatches.ContainsKey(matchId))
            return m_AvailableMatches[matchId];
        else
            return null;
    }

    internal void ClearMatchState()
    {
        m_CurrentMatch = null;
        m_MatchIsDisplayed = false;
        m_AwaitingChallenger = false;
        m_AwaitingMatchCreated = false;
        m_MatchGuiPanel.SetActive(false);
    }

    private void Start()
    {
        m_StartMatchButton.onClick.AddListener(() => CreateMatch());
        m_CancelMatchMakingButton.onClick.AddListener(() => CancelMatchMaking());

        EventSink.MatchCreatedEvent += EventSink_MatchCreatedEvent;
        EventSink.MatchEndedEvent += EventSink_MatchEndedEvent;
        EventSink.ParticipantSyncEvent += EventSink_ParticipantSyncEvent;
        EventSink.MatchJoinResultEvent += EventSink_MatchJoinResultEvent;
        EventSink.SpectatorSyncEvent += EventSink_SpectatorSyncEvent;
        EventSink.SpectateResultEvent += EventSink_SpectateResultEvent;
        EventSink.SpectatorDepartureEvent += EventSink_SpectatorDepartureEvent;

        InterEventDispatcher.DCFromMatchInProgressEvent += InterEventDispatcher_DCFromMatchInProgressEvent;
        InterEventDispatcher.MatchDepartureTimerTickEvent += InterEventDispatcher_MatchDepartureTimerTickEvent;
        InterEventDispatcher.MatchStartEvent += InterEventDispatcher_MatchStartEvent;
        InterEventDispatcher.TurnStateSyncEvent += InterEventDispatcher_TurnStateSyncEvent;
    }

    private void InterEventDispatcher_TurnStateSyncEvent(TurnStateSyncEventArgs args)
    {
        ClientManager.Post(() => HandleTurnStateSync(args));
    }

    void HandleTurnStateSync(TurnStateSyncEventArgs args)
    {
        if (args.MatchId == m_CurrentMatch.MatchIdentity)
        {
            m_CurrentMatch.TurnState.SetCurrentPlayer(args.CurrentPlayerId);
            m_CurrentMatch.TurnState.SetLowestTurnStep((TurnStep)args.LowestTurnStep);
            m_CurrentMatch.TurnState.SetCurrentStep((TurnStep)args.TurnStep);
        }
    }

    private void InterEventDispatcher_MatchStartEvent(MatchStartEventArgs args)
    {
        ClientManager.Post(() => HandleMatchStart(args));
    }

    private void HandleMatchStart(MatchStartEventArgs args)
    {
        m_CurrentMatch.TurnState = new TurnState
            (m_CurrentMatch, args.PlayerIdOfFirstToMove);

        EventSink.InvokeMatchChatMessageEvent
            (new MatchChatMessageEventArgs("System", DateTime.Now.ToShortTimeString(),
                string.Format("{0} Has been randomly selected to move first.", 
                    m_CurrentMatch.GetPlayerStateById(m_CurrentMatch.TurnState.CurrentPlayerId).Username)));
    }

    private void InterEventDispatcher_MatchDepartureTimerTickEvent(MatchDepartureTimerTickEventArgs args)
    {
        ClientManager.Post(() => UpdateExpirationTime(args.SecondsExpired));
    }

    private void InterEventDispatcher_DCFromMatchInProgressEvent(DCFromMatchInProgressEventArgs args)
    {
        ClientManager.Post(() => HandleMatchAbandonedEvent(args.Match, args.PlayerState));
    }

    private void EventSink_SpectatorDepartureEvent(SpectatorDepartureEventArgs args)
    {
        ClientManager.Post(() => SyncMatchSpectators(args.MatchId, args.Spectator, false));
    }

    private void EventSink_SpectateResultEvent(SpectateResultEventArgs args)
    {
        ClientManager.Post(() => HandleJoinResults(args.Succeeded, args.MatchId));
    }

    private void EventSink_SpectatorSyncEvent(SpectatorSyncEventArgs args)
    {
        ClientManager.Post(() => SyncMatchSpectators(args.MatchId, args.Spectator, true));
    }

    private void EventSink_MatchJoinResultEvent(MatchJoinResultEventArgs args)
    {
        ClientManager.Post(() => HandleJoinResults(args.JoinSucceeded, args.MatchId));
    }

    private void EventSink_ParticipantSyncEvent(MatchSyncEventArgs args)
    {
        ClientManager.Post(() => SyncMatchParticipants(args.Match));
    }

    private void EventSink_MatchEndedEvent(MatchEndedEventArgs args)
    {
        ClientManager.Post(() => HandleEndMatchEvent(args.MatchId));
    }

    private void EventSink_MatchCreatedEvent(MatchCreatedEventArgs args)
    {
        ClientManager.Post(() => HandleMatchCreatedEvent(args.Match));
    }

    private void Update()
    {
        if(m_AwaitingChallenger)
        {
            if (m_WaitSpinner.fillAmount >= 1)
                m_WaitSpinner.fillAmount = 0;

            m_WaitSpinner.fillAmount += Time.deltaTime / 3.14f;
        }
    }

    private void HandleMatchAbandonedEvent(MatchState match, PlayerState player)
    {
        if (match.MatchIdentity == m_CurrentMatch.MatchIdentity)
        {
            m_MatchAwaitingExpiration = true;
            m_MatchExpirationPanel.SetActive(true);
            match.BeginDepartureTimer(player.AccountIdentity);

            EventSink.InvokeMatchChatMessageEvent
                (new MatchChatMessageEventArgs("System", DateTime.Now.ToShortTimeString(),
                    string.Format("{0} Has disconnected from the match..", player.Username)));
        }
    }

    private void UpdateExpirationTime(int seconds)
    {
        TimeSpan expirationTime = new TimeSpan(0, 0, 60 - seconds);
        if(m_ExpirationTimeLabel != null)
            m_ExpirationTimeLabel.text = "Match Ends (" + expirationTime.ToString(@"m\:ss") + ")";
    }

    private void HandleEndMatchEvent(string matchId)
    {
        RemoveAvailableMatch(matchId);

        if(m_CurrentMatch != null && m_CurrentMatch.MatchIdentity == matchId)
        {
            ClearMatchState();
            CameraHandler.ActivateCamera(1);
        }
    }

    private void HandleJoinResults(bool successful, string matchId)
    {
        if (!m_AvailableMatches.ContainsKey(matchId))
            return;

        if(successful)
        {
            Debug.Log("Match Join Successful: " + matchId);

            m_CurrentMatch = m_AvailableMatches[matchId];

            string playerOneId = m_CurrentMatch.PlayerOne == null ? "Null" : m_CurrentMatch.PlayerOne.Username;
            string playerTwoId = m_CurrentMatch.PlayerTwo == null ? "Null" : m_CurrentMatch.PlayerTwo.Username;

            Debug.LogFormat("Match Info: {0} vs {1}", playerOneId, playerTwoId);

            EventSink.InvokeMatchJoinedEvent
                (new MatchJoinedEventArgs(m_CurrentMatch));

            DisplayMatch();
        }

        else
        {
            ClearMatchState();
            EventSink.InvokeMessageBoxEvent
                (new MessageBoxEventArgs("Unable To Join Match."));
        }
    }

    private void SyncMatchParticipants(MatchState match)
    {
        if(m_CurrentMatch != null && 
            match.MatchIdentity == m_CurrentMatch.MatchIdentity)
        {
            SyncParticipantList(match);

            SetCurrentMatch(match);

            if(match.MatchIsFull() && !m_MatchIsDisplayed)
                DisplayMatch();

            m_PlayerPosition = m_CurrentMatch.
                GetPlayerSlotFromId(AccountManager.AccountInstance.Identity);

            if(m_PlayerPosition != 0)
                CameraHandler.ActivateCamera(m_PlayerPosition);

            Debug.Log("Match Sync Successful: " + match.MatchIdentity);

            string playerOneId = m_CurrentMatch.PlayerOne == null ? "Null" : m_CurrentMatch.PlayerOne.Username;
            string playerTwoId = m_CurrentMatch.PlayerTwo == null ? "Null" : m_CurrentMatch.PlayerTwo.Username;

            Debug.LogFormat("Match Info: {0} vs {1}", playerOneId, playerTwoId);
        }

        if (m_AvailableMatches.ContainsKey(match.MatchIdentity))
        {
            m_AvailableMatches[match.MatchIdentity] = match;

            if (m_MatchPanelRelationships.ContainsKey(match.MatchIdentity))
                m_MatchPanelRelationships[match.MatchIdentity].
                    GetComponent<MatchPanelBehavior>().UpdateMatch(match);
        }

        else
            AddAvailableMatch(match);
    }

    private void SyncMatchSpectators(string matchId, PlayerState spectator, bool addition)
    {
        if (m_AvailableMatches.ContainsKey(matchId))
        {
            MatchState match = m_AvailableMatches[matchId];

            if (addition)
                match.AddSpectator(spectator);
            else
                match.RemoveUserFromMatch(spectator.AccountIdentity);

            if (m_MatchPanelRelationships.ContainsKey(match.MatchIdentity))
                m_MatchPanelRelationships[match.MatchIdentity].
                    GetComponent<MatchPanelBehavior>().UpdateMatch(match);

            if (m_CurrentMatch != null &&
                match.MatchIdentity == m_CurrentMatch.MatchIdentity)
            {
                SyncSpectatorList(match);
            }
        }
    }

    private void SyncParticipantList(MatchState match)
    {
        foreach (GameObject go in m_ParticipantRepCache.Values)
            Destroy(go);

        m_ParticipantRepCache.Clear();

        if (match.PlayerOne != null)
        {
            GameObject participantRep = Instantiate(m_ParticipantRepButton);

            participantRep.GetComponent<MatchParticipantRepresentation>()
                .SetAccountRelative(AccountManager.GetAccountById(match.PlayerOne.AccountIdentity));

            m_ParticipantRepCache.Add(match.PlayerOne.AccountIdentity, participantRep);
            participantRep.transform.SetParent(m_MatchUsersDisplayArea.transform, false);
        }

        if (match.PlayerTwo != null)
        {
            GameObject participantRep = Instantiate(m_ParticipantRepButton);

            participantRep.GetComponent<MatchParticipantRepresentation>()
                .SetAccountRelative(AccountManager.GetAccountById(match.PlayerTwo.AccountIdentity));

            m_ParticipantRepCache.Add(match.PlayerTwo.AccountIdentity, participantRep);
            participantRep.transform.SetParent(m_MatchUsersDisplayArea.transform, false);
        }
    }

    private void SyncSpectatorList(MatchState match)
    {
        foreach (GameObject go in m_SpectatorRepCache.Values)
            Destroy(go);

        m_SpectatorRepCache.Clear();

        foreach (PlayerState playerState in match.Spectators.Values)
        {
            GameObject spectatorRep = Instantiate(m_ParticipantRepButton);

            spectatorRep.GetComponent<MatchParticipantRepresentation>()
                .SetAccountRelative(AccountManager.GetAccountById(playerState.AccountIdentity));

            m_SpectatorRepCache.Add(playerState.AccountIdentity, spectatorRep);
            spectatorRep.transform.SetParent(m_MatchUsersDisplayArea.transform, false);
        }
    }

    private void CancelMatchMaking()
    {
        m_AwaitingChallenger = false;
        m_MatchWaitPanel.SetActive(false);

        ClientManager.Instance.SendEndMatchCommand(m_CurrentMatch.MatchIdentity);

        EventSink.InvokeMatchCancelledEvent
            (new MatchCancelledEventArgs(m_CurrentMatch));

        ClearMatchState();
    }

    internal void HandleMatchCreatedEvent(MatchState match)
    {
        if (m_AwaitingMatchCreated &&
            match.PlayerOne.AccountIdentity == AccountManager.AccountInstance.Identity)
        {
            JoinMatchCreated(match);
        }

        AddAvailableMatch(match);
    }

    internal void AddAvailableMatch(MatchState match)
    {
        if (!m_AvailableMatches.ContainsKey(match.MatchIdentity))
        {
            m_AvailableMatches.Add(match.MatchIdentity, match);

            m_MatchPanelRelationships.Add
                (match.MatchIdentity, Instantiate(m_MatchInstancePanelPrefab));

            m_MatchPanelRelationships[match.MatchIdentity].
                transform.SetParent(m_MatchListDisplayPanel.transform, false);

            m_MatchPanelRelationships[match.MatchIdentity].
                GetComponent<MatchPanelBehavior>().UpdateMatch(match);
        }
    }

    internal void RemoveAvailableMatch(string id)
    {
        if (m_AvailableMatches.ContainsKey(id))
            m_AvailableMatches.Remove(id);

        if (m_MatchPanelRelationships.ContainsKey(id))
        {
            Destroy(m_MatchPanelRelationships[id]);
            m_MatchPanelRelationships.Remove(id);
        }
    }

    internal void CreateMatch()
    {
        m_AwaitingMatchCreated = true;

        ClientManager.Instance.
            SendStartMatchCommand(AccountManager.AccountInstance);
    }

    internal void JoinMatchCreated(MatchState match)
    {
        if(m_AwaitingMatchCreated)
            m_AwaitingMatchCreated = false;

        SetCurrentMatch(match);

        if (m_MatchWaitPanel != null)
            m_MatchWaitPanel.SetActive(true);

        m_AwaitingChallenger = true;

        EventSink.InvokeMatchJoinedEvent
            (new MatchJoinedEventArgs(match));
    }

    internal void DisplayMatch()
    {
        m_AwaitingChallenger = false;

        m_MatchGuiPanel.SetActive(true);
        m_MatchWaitPanel.SetActive(false);

        SyncParticipantList(m_CurrentMatch);
        m_MatchIsDisplayed = true;
    }
}
