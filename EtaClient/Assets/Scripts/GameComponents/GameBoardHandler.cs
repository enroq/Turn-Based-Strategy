using Eta.Interdata;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameBoardHandler : MonoBehaviour
{
    [SerializeField]
    GameBoardInstance m_TargetGameBoard;

    private void Start()
    {
        EventSink.ParticipantSyncEvent += EventSink_ParticipantSyncEvent;
        EventSink.MatchEndedEvent += EventSink_MatchEndedEvent;
        EventSink.SpectatorSyncEvent += EventSink_SpectatorSyncEvent;
    }

    private void EventSink_SpectatorSyncEvent(SpectatorSyncEventArgs args)
    {
        ClientManager.Post(() => StartCoroutine(QueryBoardDisplay(args)));
    }

    private void EventSink_ParticipantSyncEvent(MatchSyncEventArgs args)
    {
        ClientManager.Post(() => StartCoroutine(QueryBoardDisplay(args)));
    }

    private void EventSink_MatchEndedEvent(MatchEndedEventArgs args)
    {
        if(MatchHandler.CurrentMatch != null && MatchHandler.CurrentMatch.MatchIdentity == args.MatchId)
            ClientManager.Post(() => m_TargetGameBoard.gameObject.SetActive(false));
    }

    private IEnumerator QueryBoardDisplay(MatchSyncEventArgs args)
    {
        yield return new WaitForSeconds(1.0f);

        Debug.LogFormat("Match In Progress: {0} | Current Match Not Null: {1}",
            MatchHandler.CurrentMatch != null ? args.Match.InProgress.ToString() : "NULL",
            MatchHandler.CurrentMatch != null);

        Debug.LogFormat("Match Identities: {0} | {1}",
            MatchHandler.CurrentMatch != null ? MatchHandler.CurrentMatch.MatchIdentity : "NULL",
            args.Match.MatchIdentity);

        if (args.Match.InProgress && MatchHandler.CurrentMatch != null &&
            MatchHandler.CurrentMatch.MatchIdentity == args.Match.MatchIdentity)
        {
           m_TargetGameBoard.gameObject.SetActive(true);
        }
    }

    private IEnumerator QueryBoardDisplay(SpectatorSyncEventArgs args)
    {
        yield return new WaitForSeconds(1.0f);

        MatchState match = MatchHandler.GetMatchById(args.MatchId);

        Debug.LogFormat("Match In Progress: {0} | Current Match Not Null: {1}",
            MatchHandler.CurrentMatch != null ? match.InProgress.ToString() : "NULL", MatchHandler.CurrentMatch != null);

        Debug.LogFormat("Match Identities: {0} | {1}",
            MatchHandler.CurrentMatch != null ? MatchHandler.CurrentMatch.MatchIdentity : "NULL", match.MatchIdentity);

        if (match.InProgress && MatchHandler.CurrentMatch != null &&
            MatchHandler.CurrentMatch.MatchIdentity == match.MatchIdentity)
        {
            m_TargetGameBoard.gameObject.SetActive(true);
        }
    }
}