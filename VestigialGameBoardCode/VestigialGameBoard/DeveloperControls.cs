using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DeveloperControls : MonoBehaviour, IDragHandler
{
    public GameObject m_DropDownMenu;
    public GameObject m_XInputField_Alpha;
    public GameObject m_YInputField_Alpha;
    public GameObject m_XInputField_Delta;
    public GameObject m_YInputField_Delta;
    public GameObject m_RangeSelectionInput;
    public GameObject m_LabelToggleSwitch;
    
    private Vector2 m_LastOriginVectorParsed = BoardGenerationEngine.EmptyVector;
    private Vector2 m_LastDestinationVectorParsed = BoardGenerationEngine.EmptyVector;

    void Start()
    {
        InitializeDropdown();
        RefreshDropdown();
    }

    public void OnDrag(PointerEventData data)
    {
        transform.position = Input.mousePosition;
    }

    void InitializeDropdown()
    {
        ClearDropdown();
        GatherCurrentGameBoardIds();
        InitializeSelectedGameBoard();
    }

    void ClearDropdown()
    {
        var dropdown = m_DropDownMenu.GetComponent<Dropdown>();
        if (dropdown != null)
            dropdown.ClearOptions();
    }

    void RefreshDropdown()
    {
        var dropdown = m_DropDownMenu.GetComponent<Dropdown>();
        if (dropdown != null)
            dropdown.RefreshShownValue();
    }

    void UpdateDropDownMenu(string id)
    {
        try
        {
            var option = new Dropdown.OptionData(id);
            var dropdown = m_DropDownMenu.GetComponent<Dropdown>();
            if (dropdown != null)
                dropdown.options.Add(option);
        }

        catch (Exception e) { Debug.Log(e.ToString()); }
    }

    public void InitializeSelectedGameBoard()
    {
        var dropDown = m_DropDownMenu.GetComponent<Dropdown>();
        if (dropDown != null)
        {
            GameEngine.SelectGameboardByIndex(0);
        }
    }

    void GatherCurrentGameBoardIds()
    {
        foreach (KeyValuePair<string, BaseGameBoard> kvp in GameEngine.ActiveGameBoards)
            UpdateDropDownMenu(kvp.Key);
    }

    internal Vector2 GetOriginVectorFromInput()
    {
        try
        {
            string x = m_XInputField_Alpha.GetComponent<InputField>().text;
            string y = m_YInputField_Alpha.GetComponent<InputField>().text;

            m_LastOriginVectorParsed = new Vector2(Int32.Parse(x), Int32.Parse(y));

            return m_LastOriginVectorParsed;
        }

        catch (Exception e)
        {
            Debug.Log(e.ToString());
            return m_LastOriginVectorParsed;
        }
    }
    internal Vector2 GetDestinationVectorFromInput()
    {
        try
        {
            string x = m_XInputField_Delta.GetComponent<InputField>().text;
            string y = m_YInputField_Delta.GetComponent<InputField>().text;

            m_LastDestinationVectorParsed = new Vector2(Int32.Parse(x), Int32.Parse(y));

            return m_LastDestinationVectorParsed;
        }

        catch (Exception e)
        {
            Debug.Log(e.ToString());
            return m_LastDestinationVectorParsed;
        }
    }

    public void OnDrawPath()
    {
        PathingHandler.ParseVectorDistance
            (GetOriginVectorFromInput(), GetDestinationVectorFromInput());
    }

    public void SelectGameCubeFromCurrent()
    {
        SelectComponentFromBoard
            (GetCurrentBoardIdentity());
    }

    public void SelectGameCubeFromIndexes()
    {
        if(GameEngine.ActiveGameBoards != null && GameEngine.ActiveGameBoards.Count > 0)
            foreach (KeyValuePair<string, BaseGameBoard> kvp in GameEngine.ActiveGameBoards)
                SelectComponentFromBoard(kvp.Key);
    }

    public void SelectComponentFromBoard(string id)
    {
        try
        {
            var currentBoard = GameEngine.GetGameBoardById(id);
            var origin = GetOriginVectorFromInput();
            var component = currentBoard.GetBoardObjectByVector(origin);

            component.ToggleObjectSelection(true);
        }

        catch (Exception e)
        {
            Debug.Log("[Error]: Last Vector Parsed: " + m_LastOriginVectorParsed);
            Debug.Log(e.ToString());
        }
    }

    public void OnSelectRange()
    {
        int rng = GetCurrentRange();
        string id = GetCurrentBoardIdentity();

        if(!(id.Contains("ID:ERROR")))
            SelectComponentsFromBoard
                (id, rng == -1 ? 0 : rng);
    }

    public void OnClearSelection()
    {
        var board = GameEngine.GetGameBoardById(GetCurrentBoardIdentity());
        board.ClearActiveComponents();
        board = null;
    }

    public void DrawPath()
    {
        try
        {
            var board = GameEngine.GetGameBoardById(GetCurrentBoardIdentity());

            Vector2 origin = GetOriginVectorFromInput();
            Vector2 destination = GetDestinationVectorFromInput();

            GameBoardComponent[] pathComponents = board.DeterminePath(origin, destination);

            for (int i = 0; i < pathComponents.Length; i++)
            {
                pathComponents[i].ToggleObjectSelection(false);
            }
        }

        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }
    }


    public void OnToggleLabels()
    {
        var board = GameEngine.GetGameBoardById(GetCurrentBoardIdentity());
        var components = board.GetCurrentComponents();
        for (int i = 0; i < components.Length; i++)
        {
            components[i].ToggleOverheadDisplay();
        }
    }

    internal string GetCurrentBoardIdentity()
    {
        return GameEngine.CurrentlySelectedBoard.IdentityString;
    }

    internal int GetCurrentRange()
    {
        try
        {
            var text = m_RangeSelectionInput.GetComponent<InputField>().text;
            if (text != null)
                return Int32.Parse(text.ToString()); else return -1;
        }

        catch (Exception e)
        {
            Debug.Log(e.ToString());
            return -1;
        }
    }

    public void SelectComponentsFromBoard(string id, int range)
    {
        try
        {
            var currentBoard = GameEngine.GetGameBoardById(id);
            var component = currentBoard.GetBoardObjectByVector(GetOriginVectorFromInput());

            GameBoardComponent[] components = 
                currentBoard.GetAllObjectsInRange(component, range);

            for (int i = 0; i < components.Length; i++)
                components[i].ToggleObjectSelection(false);
        }

        catch (Exception e)
        {
            Debug.Log("[Error]: Last Vector Parsed: " + m_LastOriginVectorParsed);
            Debug.Log(e.ToString());
        }
    }

    public void UpdateCurrentBoardSelection()
    {
        var dropDown = m_DropDownMenu.GetComponent<Dropdown>();
        if (dropDown != null)
        {
            int i = dropDown.value;         
            GameEngine.SelectGameboardByIndex(i);
                Debug.Log("Selecting Game Board By Index: " + i);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)
            && Input.GetKeyDown(KeyCode.Q))
        {

        }
    }
}
