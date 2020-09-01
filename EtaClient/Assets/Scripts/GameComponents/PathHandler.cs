using System;
using System.Collections.Generic;
using UnityEngine;


public class PathHandler
{
    static List<GameBoardTile>
        m_Tiles = new List<GameBoardTile>();

    internal static GameBoardTile[] DeterminePath
        (GameBoardInstance gameBoard, Vector2 origin, GameBoardTile endTile)
    {
        m_Tiles.Clear();

        GameBoardTile currentTile = gameBoard.GetTileByVector(origin);

        int i = 0;
        while (currentTile.BoardVector != endTile.BoardVector && i < 32)
        {
            i++;

            currentTile = GetNextTileInPath
                (gameBoard, currentTile, endTile.BoardVector);

            if (currentTile == null)
            {
                Debug.LogWarning("Pathing Error: Next Tile In Path Returned Null!");
                return null;
            }

            if (!m_Tiles.Contains(currentTile))
                m_Tiles.Add(currentTile);
        }

        m_Tiles.Add(endTile); return m_Tiles.ToArray();
    }

    internal static GameBoardTile GetNextTileInPath(GameBoardInstance gameBoard, GameBoardTile tile, Vector2 destination)
    {
        try
        {
            if (tile != null)
                return DetermineNextNode(gameBoard, tile, destination);
            else
                return null;
        }

        catch (Exception e)
        {
            Debug.Log(e.ToString()); return null;
        }
    }

    public static GameBoardTile DetermineNextNode(GameBoardInstance gameBoard, GameBoardTile current, Vector2 destination)
    {
        float currentLowestDistance = Int16.MaxValue;
        float currentLowestHeuristic = Int16.MaxValue;

        GameBoardTile closestComponent = null; Vector2 origin = current.BoardVector;

        GameBoardTile[] neighboringTiles = gameBoard.GetHorizontalTiles(current, 1, true);

        for (int i = 0; i < neighboringTiles.Length; i++)
        {
            if (m_Tiles.Contains(neighboringTiles[i]))
                continue;

            float h = (neighboringTiles[i].BoardVector - destination).sqrMagnitude;
            float g = (neighboringTiles[i].BoardVector - origin).sqrMagnitude;
            float f = g + h;

            if (f < currentLowestDistance)
            {
                closestComponent = neighboringTiles[i];
                currentLowestDistance = f;
                currentLowestHeuristic = h;
            }

            else if (f == currentLowestDistance)
            {
                if (h < currentLowestHeuristic)
                {
                    closestComponent = neighboringTiles[i];
                    currentLowestDistance = f;
                    currentLowestHeuristic = h;
                }
            }
        }

        if (closestComponent != null)
            return closestComponent;

        else
            return null;
    }
}