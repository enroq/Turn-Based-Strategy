using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eta.Interdata
{
    public enum TurnStep
    {
        None,
        Move,
        Attack,
        Direction,
        End
    }

    public class TurnState
    {
        private string m_PlayerOneId;
        private string m_PlayerTwoId;

        private string m_CurrentPlayerId;

        private TurnStep m_CurrentTurnStep;

        private TurnStep m_LowestTurnStep;

        public string CurrentPlayerId { get { return m_CurrentPlayerId; } }

        public TurnStep CurrentTurnStep { get { return m_CurrentTurnStep; } }
        public TurnStep LowestTurnStep { get { return m_LowestTurnStep; } }

        public string PlayerOneId { get { return m_PlayerOneId; } }
        public string PlayerTwoId { get { return m_PlayerTwoId; } }

        static Random m_Random = new Random();
        public static bool RandomBoolean()
        {
            return m_Random.Next(0, 2) == 0;
        }

        public TurnState(string playerOneId, string playerTwoId)
        {
            m_PlayerOneId = playerOneId;
            m_PlayerTwoId = playerTwoId;

            if (RandomBoolean())
                SetCurrentPlayer(m_PlayerOneId);
            else
                SetCurrentPlayer(m_PlayerTwoId);

            m_CurrentTurnStep = TurnStep.Move;
        }

        public TurnState(MatchState match, string firstToMoveId)
        {
            m_PlayerOneId = match.PlayerOne.AccountIdentity;
            m_PlayerTwoId = match.PlayerTwo.AccountIdentity;

            SetCurrentPlayer(firstToMoveId);

            m_CurrentTurnStep = TurnStep.Move;
        }

        public void SetCurrentPlayer(string id)
        {
            m_CurrentPlayerId = id;
        }

        public void EndTurn()
        {
            m_CurrentTurnStep = TurnStep.Move;
            m_LowestTurnStep = TurnStep.None;

            SwitchCurrentPlayer();
        }

        public void SwitchCurrentPlayer()
        {
            if (m_CurrentPlayerId == m_PlayerOneId)
                SetCurrentPlayer(m_PlayerTwoId);

            else if (m_CurrentPlayerId == m_PlayerTwoId)
                SetCurrentPlayer(m_PlayerOneId);
        }

        public void SetCurrentStep(TurnStep turnStep)
        {
            if (m_LowestTurnStep < turnStep)
            {
                m_LowestTurnStep = m_CurrentTurnStep;
                m_CurrentTurnStep = turnStep;
            }

            if (turnStep == TurnStep.End)
                EndTurn();
        }

        public void SetLowestTurnStep(TurnStep turnStep)
        {
            m_LowestTurnStep = turnStep;
        }
    }
}
