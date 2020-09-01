using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEngine : MonoBehaviour
{
    private static Dictionary<string, BaseGameBoard> 
        m_ActiveGameBoards = new Dictionary<string, BaseGameBoard>();
    private static BaseGameBoard m_CurrentlySelectedBoard { get; set; }

    public static Dictionary<string, BaseGameBoard> ActiveGameBoards{ get { return m_ActiveGameBoards; }}
    public static BaseGameBoard CurrentlySelectedBoard { get { return m_CurrentlySelectedBoard; } }

    internal static void SetSelectedGameboard(string id)
    {
        try
        {
            if (m_ActiveGameBoards.ContainsKey(id))
                m_CurrentlySelectedBoard = m_ActiveGameBoards[id];
        }

        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }
    }

    internal static void SelectGameboardByIndex(Int32 i)
    {
        try
        {
            BaseGameBoard[] boards = CurrentGameBoards();
            SetSelectedGameboard(boards[i].IdentityString);
        }

        catch (Exception e) { Debug.Log(e.ToString()); }
    }

    internal static BaseGameBoard[] CurrentGameBoards()
    {
        List<BaseGameBoard> boards = new List<BaseGameBoard>();
        foreach(KeyValuePair<string, BaseGameBoard> kvp in ActiveGameBoards)
            boards.Add(kvp.Value);  
        
        return boards.ToArray();
    }

    internal static BaseGameBoard GetGameBoardById(string id)
    {
        try
        {
            if (ActiveGameBoards.ContainsKey(id))
                return ActiveGameBoards[id];
            else
                return null;
        }

        catch (Exception e) { Debug.Log(e.ToString()); return null; }
    }

    internal static void AddGameBoardToCache(BaseGameBoard o)
    {
        try 
        { 
            m_ActiveGameBoards.Add(o.IdentityString, o);
        }

        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }
    }

    internal void DebugComponents()
    {
        foreach (KeyValuePair<string, BaseGameBoard> kvp in m_ActiveGameBoards)
        {
            GetGameBoardById(kvp.Key).DebugBoardComponents();
        }
    }
}
