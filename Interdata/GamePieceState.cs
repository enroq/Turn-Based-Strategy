using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eta.Interdata
{
    public class GamePieceNetworkState
    {
        string m_GamePieceName;
        string m_NetworkIdentity;

        string m_ControllerIdentity;

        int m_ControllerPosition;

        int m_PositionX;
        int m_PositionY;

        float m_Rotation;

        int m_Hitpoints;
        int m_DefenseRating;
        int m_AttackRating;
        int m_TurnDelay;
        int m_CurrentTurnDelay;

        bool m_IsDestroyed;

        public string GamePieceName { get { return m_GamePieceName; } }
        public string NetworkIdentity { get { return m_NetworkIdentity; } }

        public string ControllerIdentity { get { return m_ControllerIdentity; } }

        public int ControllerPosition { get { return m_ControllerPosition; } }

        public int PositionX { get { return m_PositionX; } }
        public int PositionY { get { return m_PositionY; } }

        public float Rotation { get { return m_Rotation; } }

        public int Hitpoints { get { return m_Hitpoints; } set { m_Hitpoints = value; } }
        public int DefenseRating { get { return m_DefenseRating; } set { m_DefenseRating = value; } }
        public int AttackRating { get { return m_AttackRating; } set { m_AttackRating = value; } }
        public int TurnDelay { get { return m_TurnDelay; } set { m_TurnDelay = value; } }
        public int CurrentTurnDelay { get { return m_CurrentTurnDelay; } set { m_CurrentTurnDelay = value; } }

        public bool IsDestroyed { get { return m_IsDestroyed; } }

        public GamePieceNetworkState
            (string controllerId, int controllerPos, string networkId, string pieceId, 
                int posX, int posY, int hp, int def, int atk, int delay, float rotation)
        {
            m_ControllerIdentity = controllerId;
            m_NetworkIdentity = networkId;
            m_GamePieceName = pieceId;

            m_ControllerPosition = controllerPos;

            m_PositionX = posX;
            m_PositionY = posY;

            m_Hitpoints = hp;
            m_DefenseRating = def;
            m_AttackRating = atk;

            m_TurnDelay = delay;

            m_Rotation = rotation;
        }

        public bool Equals(GamePieceNetworkState netState)
        {
            return
                m_ControllerIdentity == netState.ControllerIdentity &&
                m_NetworkIdentity == netState.NetworkIdentity &&
                m_GamePieceName == netState.GamePieceName &&
                m_ControllerPosition == netState.ControllerPosition &&
                m_PositionX == netState.PositionX &&
                m_PositionY == netState.PositionY &&
                m_Hitpoints == netState.Hitpoints &&
                m_DefenseRating == netState.DefenseRating &&
                m_AttackRating == netState.AttackRating &&
                m_TurnDelay == netState.TurnDelay &&
                m_Rotation == netState.Rotation;
        }

        internal void ProcessDamage(int dmg)
        {
            int rawDmg = (dmg - DefenseRating);

            if (rawDmg >= 1)
                Hitpoints -= rawDmg;
            else
                Hitpoints--;

            if (Hitpoints <= 0)
                m_IsDestroyed = true;
        }
    }
}
