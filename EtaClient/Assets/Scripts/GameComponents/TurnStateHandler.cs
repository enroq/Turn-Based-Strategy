using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Eta.Interdata;
using UnityEngine;

class TurnStateHandler : MonoBehaviour
{
    [SerializeField]
    bool m_TestMode;

    static bool TestMode { get; set; }

    private void Start()
    {
        TestMode = m_TestMode;
    }

    internal static TurnState TestTurnState = new TurnState("nullOne", "nullTwo");

    internal static MatchState CurrentMatch { get { return MatchHandler.CurrentMatch; } }
    internal static TurnState CurrentTurnState
    {
        get
        {
            if (TestMode) 
                return TestTurnState;

            if (CurrentMatch != null && CurrentMatch.TurnState != null)
            {
                return CurrentMatch.TurnState;
            }

            return null;
        }
    }

    internal static bool IsLocalPlayersTurn()
    {
        return CurrentTurnState.CurrentPlayerId == AccountManager.AccountInstance.Identity;
    }

    internal static void UpdateTurnState(TurnStep step)
    {
        if (TestMode)
        {
            CurrentTurnState.SetCurrentStep(step);
            return;
        }

        if (CurrentTurnState == null)
        {
            Debug.LogFormat("Attempting To Update Null Turn State For Match ({0})", CurrentMatch.MatchIdentity);
            return;
        }

        if (CurrentTurnState.LowestTurnStep < step)
        {
            ClientManager.Instance.SendTurnUpdateCommand(CurrentMatch.MatchIdentity, (int)step);
        }
    }

    internal static void FinishStep()
    {
        if(TestMode)
        {
            FinishStepTest();
            return;
        }

        if (CurrentTurnState == null)
        {
            Debug.LogFormat("Attempting To Update Null Turn State For Match ({0})", CurrentMatch.MatchIdentity);
            return;
        }

        ClientManager.Instance.SendTurnUpdateCommand
            (CurrentMatch.MatchIdentity, (int)(CurrentTurnState.CurrentTurnStep +1));
    }

    internal static void FinishStepTest()
    {
        Debug.LogFormat("Turn State Step: {0}", TestTurnState.CurrentTurnStep.ToString());
        TestTurnState.SetCurrentStep(TestTurnState.CurrentTurnStep + 1);
        Debug.LogFormat("Turn State Step: {0} | Current Player: {1}",
            TestTurnState.CurrentTurnStep.ToString(), TestTurnState.CurrentPlayerId);
    }

    internal static bool IsControllersTurn(GamePiece piece)
    {
        return CurrentTurnState.CurrentPlayerId == piece.ControllingPlayerId;
    }

    public void EndTurnViaButton()
    {
        if (IsLocalPlayersTurn())
        {
            UpdateTurnState(TurnStep.End);
            GamePieceHandler.CurrentFocusPiece.HandleTurnStepChange(TurnStep.End);
        }
    }
}

