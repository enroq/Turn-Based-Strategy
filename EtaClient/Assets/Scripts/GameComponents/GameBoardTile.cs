using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SelectionType
{
    None,
    Movement,
    Target,
    Attack
}

public class GameBoardTile : MonoBehaviour, IBoardComponent
{
    [SerializeField]
    Material m_DefaultMaterial;

    [SerializeField]
    Material m_MoveSelectMaterial;

    [SerializeField]
    Material m_AttackSelectMaterial;

    [SerializeField]
    Material m_FocusedMaterial;

    bool m_IsSelected;
    bool m_IsPassable = true;
    bool m_IsFocused;

    Vector2 m_BoardVector;

    SelectionType m_SelectionType;

    GameBoardInstance m_ParentBoard;
    GamePiece m_OccupyingPiece;

    MeshRenderer m_MeshRenderer;

    public Vector2 BoardVector { get { return m_BoardVector; } }

    public SelectionType SelectionType { get { return m_SelectionType; } }

    internal bool IsSelected { get { return m_IsSelected; } }
    internal bool IsPassable { get { return m_IsPassable; } }
    internal bool IsFocused { get { return m_IsFocused; } }

    internal GamePiece OccupyingPiece { get { return m_OccupyingPiece; } }

    private void Start()
    {
        m_MeshRenderer = GetComponent<MeshRenderer>();
    }

    internal void InitializeTile(Vector2 v, GameBoardInstance board)
    {
        m_BoardVector = v;
        m_ParentBoard = board;
    }

    internal void InitializeTileForStaging(Vector2 v)
    {
        m_BoardVector = v;
    }

    internal void SelectNeighbors(int length, SelectionType selectionType, bool queryPassable)
    {
        m_ParentBoard.SelectHorizontalTiles(this, length, selectionType, queryPassable);
    }

    internal void SelectTilesInRange(int range, bool queryPassable, SelectionType selectionType)
    {
        m_ParentBoard.SelectTilesInRange(this, range, selectionType, queryPassable);
    }

    internal void HandleSingleClickEvent()
    {
        EventSink.InvokeTileSelectedEvent
            (new TileSelectedEventArgs(this));
    }

    internal void HandleDoubleClick()
    {
        if (IsFocused && SelectionType == SelectionType.Movement)
        {
            EventSink.InvokeUnitMovementEvent
                (new UnitMovementEventArgs(this));
        }

        else if (IsSelected && SelectionType == SelectionType.Attack)
        {
            EventSink.InvokeUnitAttackEvent
                (new UnitAttackEventArgs(this));
        }
    }

    internal void FocusTile()
    {
        m_IsFocused = true;
        EnableFocusMaterial();
    }

    internal void RemoveFocus()
    {
        m_IsFocused = false;
        DisableFocusMaterial();
    }

    internal void SetPassbleState(bool passable)
    {
        m_IsPassable = passable;
    }

    internal void SetOccupyingPiece(GamePiece piece)
    {
        m_OccupyingPiece = piece;
        piece.CurrentTileOccupied = this;
    }

    internal void ClearOccupant()
    {
        if(m_OccupyingPiece != null)
            m_OccupyingPiece.CurrentTileOccupied = null;

        m_OccupyingPiece = null;
    }

    internal void Deselect()
    {
        m_IsSelected = false;
        m_IsFocused = false;

        m_SelectionType = SelectionType.None;

        EnabledDefaultMaterial();
    }

    internal void Select(SelectionType selectionType)
    {
        m_IsSelected = true;
        m_SelectionType = selectionType;
    }

    internal void ModifyColor(Color color)
    {
        m_MeshRenderer.material.color = color;
    }

    internal void EnableActiveMaterial()
    {
        m_MeshRenderer.material = m_MoveSelectMaterial;
    }

    internal void EnabledDefaultMaterial()
    {
        m_MeshRenderer.material = m_DefaultMaterial;
    }

    internal void EnableFocusMaterial()
    {
        m_MeshRenderer.material = m_FocusedMaterial;
    }

    internal void DisableFocusMaterial()
    {
        if (IsSelected)
            EnableActiveMaterial();
        else
            EnabledDefaultMaterial();
    }
}