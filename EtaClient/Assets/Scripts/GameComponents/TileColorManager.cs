using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <remarks>
/// Be aware that all material types must share the same forward rendering options 
/// and alpha source (metalic/albedo) as the base cube/tile material. Otherwise,
/// when deselecting a tile and selecting another, the tile will appear a different color.
/// </remarks>
public class TileColorManager : MonoBehaviour
{
    enum FlashSequence
    {
        Increase,
        Decrease
    }

    [SerializeField]
    Material m_MoveSelectMaterial;

    [SerializeField]
    Material m_AttackSelectMaterial;

    GameBoardInstance m_ParentBoard;

    Color m_DeltaMovementColor;
    Color m_CurrentMoveColor;
    Color m_DefaultMoveSelectColor;

    Color m_DeltaAttackColor;
    Color m_CurrentAttackColor;
    Color m_DefaultAttackSelectColor;

    FlashSequence m_CurrentMoveFlashSequence = FlashSequence.Decrease;
    FlashSequence m_CurrentAttackFlashSequence = FlashSequence.Decrease;

    void Start ()
    {
		if((m_ParentBoard = GetComponent<GameBoardInstance>()) == null)
        {
            Debug.LogError("TileColorManager Attached To Object Without GameBoardInstance Component!");
            return;
        }

        m_DeltaMovementColor = m_MoveSelectMaterial.color;
        m_DefaultMoveSelectColor = m_MoveSelectMaterial.color;

        m_DeltaAttackColor = m_AttackSelectMaterial.color;
        m_DefaultAttackSelectColor = m_AttackSelectMaterial.color;
    }
	
	void Update ()
    {
		if(m_ParentBoard.CurrentlySelectedTiles.Count > 0)
        {
            if(m_ParentBoard.SelectionType == SelectionType.Movement)
            {
                ProcessMovementFlash();

                for (int i = m_ParentBoard.CurrentlySelectedTiles.Count - 1; i >= 0; i--)
                {
                    if (!m_ParentBoard.CurrentlySelectedTiles[i].IsFocused)
                    {
                        m_ParentBoard.CurrentlySelectedTiles[i].ModifyColor(m_CurrentMoveColor);
                    }
                }
            }

            else if (m_ParentBoard.SelectionType == SelectionType.Attack ||
                        m_ParentBoard.SelectionType == SelectionType.Target)
            {
                ProcessAttackFlash();

                for (int i = m_ParentBoard.CurrentlySelectedTiles.Count - 1; i >= 0; i--)
                {
                    if (!m_ParentBoard.CurrentlySelectedTiles[i].IsFocused)
                    {
                        m_ParentBoard.CurrentlySelectedTiles[i].ModifyColor(m_CurrentAttackColor);
                    }
                }
            }
        }
	}

    void ProcessMovementFlash()
    {
        if (m_CurrentMoveFlashSequence == FlashSequence.Decrease)
        {
            m_DeltaMovementColor.g -= Time.deltaTime / 1.5f;
            if (m_DeltaMovementColor.g > 0.55f)
                m_CurrentMoveColor = m_DeltaMovementColor;
            else
                m_CurrentMoveFlashSequence = FlashSequence.Increase;
        }

        else if (m_CurrentMoveFlashSequence == FlashSequence.Increase)
        {
            m_DeltaMovementColor.g += Time.deltaTime / 1.5f;
            if (m_DeltaMovementColor.g < 1)
                m_CurrentMoveColor = m_DeltaMovementColor;
            else
                m_CurrentMoveFlashSequence = FlashSequence.Decrease;
        }
    }

    void ProcessAttackFlash()
    {
        if (m_CurrentAttackFlashSequence == FlashSequence.Decrease)
        {
            m_DeltaAttackColor.g -= Time.deltaTime / 1.5f;
            if (m_DeltaAttackColor.g > 0.55f)
                m_CurrentAttackColor = m_DeltaAttackColor;
            else
                m_CurrentAttackFlashSequence = FlashSequence.Increase;
        }

        else if (m_CurrentAttackFlashSequence == FlashSequence.Increase)
        {
            m_DeltaAttackColor.g += Time.deltaTime / 1.5f;
            if (m_DeltaAttackColor.g < 0.85)
                m_CurrentAttackColor = m_DeltaAttackColor;
            else
                m_CurrentAttackFlashSequence = FlashSequence.Decrease;
        }
    }
}
