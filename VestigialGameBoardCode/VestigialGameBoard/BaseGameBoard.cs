using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class BaseGameBoard 
{   
    private Vector3 m_InitialCentralVerticy;
    private Dictionary<Vector2, GameBoardComponent> m_CurrentBoardObjects;

    private Dictionary<Vector2, GameBoardComponent> 
        m_ActiveBoardObjects = new Dictionary<Vector2, GameBoardComponent>();

    private GameBoardComponent m_CurrentFocusComponent;

    private string m_BoardIdentifier = string.Empty;
    private static StringBuilder m_StringBuilder = new StringBuilder();

    public string IdentityString { get { return m_BoardIdentifier; } }
    public GameBoardComponent FocusComponent { get { return m_CurrentFocusComponent; } }

    public Dictionary<Vector2, GameBoardComponent> CurrentBoardComponents
    {
        get { return m_CurrentBoardObjects; }
    }

    public GameBoardComponent[] GetCurrentComponents()
    {
        List<GameBoardComponent> components = new List<GameBoardComponent>();
        foreach (KeyValuePair<Vector2, GameBoardComponent> kvp in CurrentBoardComponents)
            components.Add(kvp.Value);

        return components.ToArray();
    }

    public BaseGameBoard(Vector3 initialVector)
    {
        m_InitialCentralVerticy = initialVector;
        m_BoardIdentifier = GenerateId();
    }

    private string GenerateId()
    {
        try 
        { 
            return Guid.NewGuid().ToString()
                .Replace("-", string.Empty).Trim(); 
        }

        catch
            { Debug.Log("Error Generating New Board Indentifier.."); return "[ID:ERROR]"; }
    }

    internal GameBoardComponent QueryNextStep(Vector2 o, Vector2 d)
    {
        try
        {
            var component = GetBoardObjectByVector(o);
            if (component != null)
                return PathingHandler.DetermineNextNode(component, d);
            else
                return null;
        }

        catch (Exception e)
        {
            Debug.Log(e.ToString()); return null;
        }
    }

    internal GameBoardComponent[] DeterminePath(Vector2 origin, Vector2 destination)
    {
        List<GameBoardComponent> 
            nodes = new List<GameBoardComponent>();

        GameBoardComponent endNode = GetBoardObjectByVector(destination);
        GameBoardComponent currentNode = GetBoardObjectByVector(origin);

        while (currentNode != endNode)
        {
            currentNode = QueryNextStep(currentNode.Vector, destination);

            if (!nodes.Contains(currentNode)) 
                nodes.Add(currentNode);
        }

        nodes.Add(endNode); return nodes.ToArray();
    }

    internal void AttachBoardObject(GameObject o, Vector2 v)
    {
        try
        {
            var component = new GameBoardComponent(v, m_BoardIdentifier, o);
            AddGameBoardComponent(component);
        }

        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }
    }

    internal void AddGameBoardComponent(GameBoardComponent component)
    {
        if (m_CurrentBoardObjects == null)
            m_CurrentBoardObjects 
                = new Dictionary<Vector2, GameBoardComponent>();

        AddObjectToBoard(component.Vector, component);
    }

    private void AddObjectToBoard(Vector2 v, GameBoardComponent component)
    {
        if (!m_CurrentBoardObjects.ContainsKey(v))
            m_CurrentBoardObjects.Add(v, component);
    }

    internal GameBoardComponent GetBoardObjectByVector(Vector2 v)
    {
        GameBoardComponent root = null;

        if (m_CurrentBoardObjects.ContainsKey(v))
            root = m_CurrentBoardObjects[v];

        return root;
    }

    internal void AddActiveGameComponent(GameBoardComponent component)
    {
        try
        {
            if (m_ActiveBoardObjects == null)
                m_ActiveBoardObjects = new Dictionary<Vector2, GameBoardComponent>();

            if (!m_ActiveBoardObjects.ContainsKey(component.Vector))
                    m_ActiveBoardObjects.Add(component.Vector, component);
        }

        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }
    }

    internal void RemoveActiveGameComponent(GameBoardComponent component)
    {
        try
        {
            if (m_ActiveBoardObjects.ContainsKey(component.Vector))
                m_ActiveBoardObjects.Remove(component.Vector);
        }

        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }
    }

    internal GameBoardComponent[] GetNeighboringObjects(GameBoardComponent root)
    {
        int distanceInterval = 1; //Default Selection Length
        return
            GetHorizontalObjects(root, distanceInterval);
    }

    internal void SetFocusByVector(Vector2 v)
    {
        SetBoardFocus(GetBoardObjectByVector(v));
    }

    internal void SetBoardFocus(GameBoardComponent c)
    {
        if (c != null)
        {
            m_CurrentFocusComponent = c;
        }
    }

    internal GameBoardComponent[] GetAllObjectsInRange(GameBoardComponent root, int r)
    {
        int range = r;
        List<GameBoardComponent> 
            componentsInRange = new List<GameBoardComponent>();

        GameBoardComponent component = null;

        int x = (int)root.Vector.x;
        int y = (int)root.Vector.y;

        m_CurrentFocusComponent = component = root;

        if (component != null) 
        {
            componentsInRange.Add(component);
            component = null;
        }

        for (int i = 1; i <= range; i++)
        {
            component = GetBoardObjectByVector(new Vector2(x, y + i));
            if (component != null) 
            {
                componentsInRange.Add(component); 
                component = null;
            }

            component = GetBoardObjectByVector(new Vector2(x, y - i));
            if (component != null) 
            {
                componentsInRange.Add(component); 
                component = null;
            }

            component = GetBoardObjectByVector(new Vector2(x + i, y));
            if (component != null) 
            {
                componentsInRange.Add(component); 
                component = null;
            }

            component = GetBoardObjectByVector(new Vector2(x - i, y));
            if (component != null) 
            {
                componentsInRange.Add(component); 
                component = null;
            }
        }

        int rangeMod = range - 3;
        for (int h = 1; h <= range + rangeMod; h++)
        {
            range--;
            for (int j = 1; j <= range; j++)
            {
                component = GetBoardObjectByVector(new Vector2(x + h, y + j));
                if (component != null) 
                {
                    componentsInRange.Add(component); 
                    component = null;
                }

                component = GetBoardObjectByVector(new Vector2(x - h, y - j));
                if (component != null) 
                {
                    componentsInRange.Add(component); 
                    component = null;
                }

                component = GetBoardObjectByVector(new Vector2(x - h, y + j));
                if (component != null) 
                {
                    componentsInRange.Add(component); 
                    component = null;
                }

                component = GetBoardObjectByVector(new Vector2(x + h, y - j));
                if (component != null) 
                {
                    componentsInRange.Add(component); 
                    component = null;
                }
            }
        }       return componentsInRange.ToArray();
    }

    internal GameBoardComponent[] GetHorizontalObjects(GameBoardComponent root, int distance)
    {
        Vector2 rootVector = root.Vector; int distanceInterval = distance;
        List<GameBoardComponent> horizontalObjects = new List<GameBoardComponent>();

        var vbo = GetBoardObjectByVector
            (new Vector2(rootVector.x, rootVector.y + distanceInterval));
        if (vbo != null)
        {
            horizontalObjects.Add(vbo);
            vbo = null;
        }

        vbo = GetBoardObjectByVector
            (new Vector2(rootVector.x, rootVector.y - distanceInterval));
        if (vbo != null)
        {
            horizontalObjects.Add(vbo);
            vbo = null;
        }

        vbo = GetBoardObjectByVector
            (new Vector2(rootVector.x - distanceInterval, rootVector.y));
        if (vbo != null)
        {
            horizontalObjects.Add(vbo);
            vbo = null;
        }

        vbo = GetBoardObjectByVector
            (new Vector2(rootVector.x + distanceInterval, rootVector.y));
        if (vbo != null)
        {
            horizontalObjects.Add(vbo);
            vbo = null;
        }

        return horizontalObjects.ToArray();
    }

    bool ParseVectorEquality(Vector3 delta, Vector3 gamma)
    {
        return 
            delta.x == gamma.x && 
            delta.y == gamma.y && 
            delta.z == gamma.z;
    }

    internal void ClearActiveComponents()
    {
        List<GameBoardComponent> 
            activeComponents = new List<GameBoardComponent>();

        if (m_ActiveBoardObjects.Count > 0)
        {
            foreach (KeyValuePair<Vector2, GameBoardComponent> kvp in m_ActiveBoardObjects)
                activeComponents.Add(kvp.Value);

            foreach (GameBoardComponent component in activeComponents)
                component.ProcessSelectionState();

            activeComponents = null;
        }
    }

    internal void DebugBoardComponents()
    {
        int count = 0;
        m_StringBuilder.AppendLine(string.Format("# Game Board: {0}", m_BoardIdentifier));
        foreach (KeyValuePair<Vector2, GameBoardComponent> kvp in m_CurrentBoardObjects)      
            DebugBoardComponent(kvp.Value, count++);

        Debug.Log(m_StringBuilder.ToString()); m_StringBuilder.Length = 0;
    }

    internal static void DebugBoardComponent(GameBoardComponent component, int count)
    {
        m_StringBuilder.AppendLine
            (string.Format("Virtual Board Object [{0}]", component.ParentIdentifier));
        m_StringBuilder.AppendLine
            (string.Format("Vector [{0}]", component.Vector));
        m_StringBuilder.AppendLine
            (string.Format("GameObject [{0}]", component.ObjectRelative.name));
    }
}
