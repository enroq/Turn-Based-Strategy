using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GameBoardComponent
{
    private Vector2 m_Vector;
    private string m_ParentId;
    private GameObject m_ObjectRelative;
    private BaseGameBoard m_ParentBoard;
    private bool m_IsSelected = false;
    private bool m_Passable = true;

    public Vector2 Vector { get { return m_Vector; } }
   
    public string ParentIdentifier { get { return m_ParentId; } }
    public GameObject ObjectRelative { get { return m_ObjectRelative; } }
    public bool IsCurrentlySelected { get { return m_IsSelected; } }
    public bool IsPassable { get { return m_Passable; } }
    public BaseGameBoard ParentBoard { get { return m_ParentBoard; } }

    public GameBoardComponent
        (Vector2 v, string id, GameObject relative)
    {
        SetVector(v);
        SetParent(id);
        SetObjectRelative(relative);
    }

    public void ToggleObjectSelection(bool singular) 
    {
        if (singular && !IsCurrentlySelected)
        {
            ParentBoard.ClearActiveComponents();
            ParentBoard.SetBoardFocus(this);
        }

        ProcessSelectionState(); 
    }

    public void ToggleOverheadDisplay()
    {
        try
        {
            var behaviour = m_ObjectRelative.GetComponent<BoardComponentBehaviour>();
            behaviour.ToggleOverheadDisplay();
        }

        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }
    }

    public GameBoardComponent[] GetNeighbors()
    {
        try
        {
            return ParentBoard.GetNeighboringObjects(this);
        }
        catch
        {
            return null;
        }
    }

    internal void ProcessSelectionState()
    {
        if (m_IsSelected)
        {
            m_IsSelected = false;
            ClearSelectionMaterial();
            ParentBoard.RemoveActiveGameComponent
                (ParentBoard.GetBoardObjectByVector(Vector));
        }

        else
        {
            m_IsSelected = true;
            SetSelectionMaterial();
            ParentBoard.AddActiveGameComponent
                (ParentBoard.GetBoardObjectByVector(Vector));
        }
    }

    private void ClearSelectionMaterial()
    {
        ObjectRelative.
            GetComponent<BoardComponentBehaviour>().SetInactiveMaterial();
    }

    private void SetSelectionMaterial()
    {
        ObjectRelative.
                GetComponent<BoardComponentBehaviour>().SetActiveMaterial();
    }

    internal void SetVector(Vector2 v)
    {
        if(v != BoardGenerationEngine.EmptyVector)
            m_Vector = v;
    }

    internal void SetParent(string id)
    {
        if (id != null)
        {
            m_ParentId = id;
            var board = GameEngine.GetGameBoardById(id);
            if (board != null)
                m_ParentBoard = board;
        }
    }

    internal void SetObjectRelative(GameObject o)
    {
        if (o != null)
        {
            m_ObjectRelative = o;
            var behavior = m_ObjectRelative.GetComponent<BoardComponentBehaviour>();
            if (behavior != null)
                behavior.SetComponent(this);
        }
    }

}


