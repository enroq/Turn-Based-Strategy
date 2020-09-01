using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameBoardInstance : MonoBehaviour
{
    [SerializeField]
    GamePieceHandler m_GamePieceHandler;
    [SerializeField]
    GameObject m_BoardTilePrefab;
    [SerializeField]
    int m_BoardSize = 7;
    [SerializeField]
    int m_EdgeCropping = 2;
    [SerializeField]
    int m_GapIndex = 3;
    [SerializeField]
    int m_BoardHeight = 1;
    [SerializeField]
    bool m_TestMode;

    List<Vector2> m_BoardVectors = new List<Vector2>();
    List<GameBoardTile> m_CurrentlySelectedTiles = new List<GameBoardTile>();

    Dictionary<Vector2, GameBoardTile>
        m_GameBoardTiles = new Dictionary<Vector2, GameBoardTile>();

    GameBoardTile m_CurrentFocusTile;
    SelectionType m_SelectionType;

    internal bool TestMode { get { return m_TestMode; } }

    internal int BoardSize { get { return m_BoardSize; } }
    internal int BoardHeight { get { return m_BoardHeight; } }

    internal SelectionType SelectionType { get { return m_SelectionType; } }

    internal List<GameBoardTile> 
        CurrentlySelectedTiles { get { return m_CurrentlySelectedTiles; } }

    private void Start()
    {
        GenerateBoard();

        EventSink.TileSelectedEvent += EventSink_TileSelectedEvent;

        if (m_GamePieceHandler != null && m_TestMode)
            m_GamePieceHandler.InitializeTestPieces();

        else if (m_GamePieceHandler != null && MatchHandler.PlayerPosition != 0)
            m_GamePieceHandler.InstantiateGamePieces
                (MatchHandler.PlayerPosition, AccountManager.AccountInstance.Identity);
    }

    private void EventSink_TileSelectedEvent(TileSelectedEventArgs args)
    {
        QueryTileFocus(args.Tile);
    }

    void GenerateBoard()
    {
        GenerateBoardVectors(m_BoardSize);
        GenerateBoardTiles
            (m_BoardVectors.ToArray());
    }

    internal void QueryTileFocus(GameBoardTile tile)
    {
        if (m_CurrentlySelectedTiles.Contains(tile) 
            && m_SelectionType == SelectionType.Movement)
        {
            if (m_CurrentFocusTile != null)
                m_CurrentFocusTile.RemoveFocus();

            m_CurrentFocusTile = tile;
            m_CurrentFocusTile.FocusTile();
        }

        else if (m_CurrentlySelectedTiles.Contains(tile)
            && m_SelectionType == SelectionType.Target)
        {
            if(GamePieceHandler.CurrentFocusPiece != null)
            {
                GamePieceHandler.CurrentFocusPiece.HandleAttackSelection(tile);
            }
        }

        else 
            if(!m_CurrentlySelectedTiles.Contains(tile))
                ClearCurrentSelections();
    }

    internal GameBoardTile GetTileByVector(Vector2 v)
    {
        if (m_GameBoardTiles.ContainsKey(v))
            return m_GameBoardTiles[v];
        else
            return null;
    }

    internal void SetTilePassableState(Vector2 v, bool passable)
    {
        if (m_GameBoardTiles.ContainsKey(v))
            m_GameBoardTiles[v].SetPassbleState(passable);
    }

    internal void SetTileOccupant(Vector2 v, GamePiece piece)
    {
        if (m_GameBoardTiles.ContainsKey(v))
            m_GameBoardTiles[v].SetOccupyingPiece(piece);
    }

    internal void ClearTileOccupant(Vector2 v)
    {
        if (m_GameBoardTiles.ContainsKey(v))
            m_GameBoardTiles[v].ClearOccupant();
    }

    void GenerateBoardTiles(Vector2[] boardVectors)
    {
        Vector3 deltaVector = Vector3.zero;
        Vector2 gammaVector = Vector2.zero;

        for (int i = boardVectors.Length - 1; i >= 0; i--)
        {
            deltaVector.x = boardVectors[i].x;
            deltaVector.z = boardVectors[i].y;

            gammaVector.x = boardVectors[i].x;
            gammaVector.y = boardVectors[i].y;

            if (!m_GameBoardTiles.ContainsKey(gammaVector))
            {
                GameObject go = Instantiate
                    (m_BoardTilePrefab, deltaVector, m_BoardTilePrefab.transform.rotation);

                go.GetComponent<GameBoardTile>().InitializeTile(gammaVector, this);
                go.name = string.Format("{0} [{1}] ({2})", go.name, go.GetInstanceID(), gammaVector.ToString());
                m_GameBoardTiles.Add
                    (gammaVector, go.GetComponent<GameBoardTile>());
            }
        }
    }

    void GenerateBoardVectors(int length)
    {
        Vector2 deltaVector = Vector2.zero;

        m_BoardVectors.Add(deltaVector);

        for (int i = 1; i <= length; i++)
            GenerateAxisVectors(deltaVector, i);

        for (int x = 1; x <= length; x++)
            for (int y = 1; y <= length; y++)
                GenerateQuadrantVectors(deltaVector, x, y);
    }

    void GenerateAxisVectors(Vector2 deltaVector, int i)
    {
        if (IsExcludedAxisVector(i))
        {
            Debug.LogFormat("Exluding Axis Vector: {0}", i);
            return;
        }

        deltaVector.x = i;
        deltaVector.y = 0;

        m_BoardVectors.Add(deltaVector);

        deltaVector.x = 0;
        deltaVector.y = i;

        m_BoardVectors.Add(deltaVector);

        deltaVector.x = -i;
        deltaVector.y = 0;

        m_BoardVectors.Add(deltaVector);

        deltaVector.x = 0;
        deltaVector.y = -i;

        m_BoardVectors.Add(deltaVector);
    }

    void GenerateQuadrantVectors(Vector2 deltaVector, int x, int y)
    {
        if (IsExcludedQuadVector(x, y))
        {
            Debug.LogFormat("Exluding Vector: {0}, {1}", x, y);
            return;
        }

        deltaVector.x = x;
        deltaVector.y = y;

        m_BoardVectors.Add(deltaVector);

        deltaVector.x = -x;
        deltaVector.y = -y;

        m_BoardVectors.Add(deltaVector);

        deltaVector.x = -x;
        deltaVector.y = y;

        m_BoardVectors.Add(deltaVector);

        deltaVector.x = x;
        deltaVector.y = -y;

        m_BoardVectors.Add(deltaVector);
    }

    bool IsExcludedQuadVector(int x, int y)
    {
        if (Mathf.Abs(x) >= m_BoardSize - m_EdgeCropping && Mathf.Abs(y) == m_BoardSize
            || Mathf.Abs(x) == m_BoardSize && Mathf.Abs(y) >= m_BoardSize - m_EdgeCropping)
            return true;

        if (Mathf.Abs(x) == m_BoardSize - Mathf.RoundToInt(m_EdgeCropping / 2)
            && Mathf.Abs(y) == m_BoardSize - (m_EdgeCropping - 1))
            return true;

        if (Mathf.Abs(x) == m_BoardSize - (m_EdgeCropping - 1)
             && Mathf.Abs(y) >= m_BoardSize - Mathf.RoundToInt(m_EdgeCropping / 2))
            return true;

        return
            Mathf.Abs(x) >= m_BoardSize - Mathf.RoundToInt(m_EdgeCropping / 2)
                && Mathf.Abs(y) >= m_BoardSize - Mathf.RoundToInt(m_EdgeCropping / 2);
    }

    bool IsExcludedAxisVector(int i)
    {
        return Mathf.Abs(i) == m_GapIndex;
    }

    internal void SelectTile(GameBoardTile tile, SelectionType selectionType)
    {
        if(!m_CurrentlySelectedTiles.Contains(tile))
        {
            tile.Select(selectionType);
            m_CurrentlySelectedTiles.Add(tile);
        }
    }

    internal void DeselectTile(GameBoardTile tile)
    {
        if (m_CurrentlySelectedTiles.Contains(tile))
        {
            tile.Deselect();
            m_CurrentlySelectedTiles.Remove(tile);
        }
    }

    internal void SelectRowFromHorizontalSelection(Vector2 relativeVector, GameBoardTile tile, int range, SelectionType selectionType)
    {
        GameBoardTile relative = GetTileByVector(relativeVector);

        if(relative != null)
        {
            SelectRowFromHorizontalSelection(relative, tile, range, selectionType);
        }
    }

    internal void SelectRowFromHorizontalSelection(GameBoardTile relative, GameBoardTile tile, int range, SelectionType selectionType)
    {
        var tiles = GetRowFromHorizontalSelection(relative, tile, range);

        ClearCurrentSelections();
        m_SelectionType = selectionType;

        SelectTile(tile, selectionType);
        SelectTiles(tiles, selectionType);
    }

    internal GameBoardTile[] GetRowFromHorizontalSelection(GameBoardTile relative, GameBoardTile targetTile, int range)
    {
        GameBoardTile[] tiles = m_CurrentlySelectedTiles.ToArray();

        //North Row
        if(targetTile.BoardVector.x >= relative.BoardVector.x && targetTile.BoardVector.y > relative.BoardVector.y)
        {
            tiles = tiles.Where
                (t => t.BoardVector.x >= relative.BoardVector.x && t.BoardVector.y > relative.BoardVector.y).ToArray();
        }

        //West Row
        else if (targetTile.BoardVector.x < relative.BoardVector.x && targetTile.BoardVector.y <= relative.BoardVector.y)
        {
            tiles = tiles.Where
                (t => t.BoardVector.x < relative.BoardVector.x && t.BoardVector.y <= relative.BoardVector.y).ToArray();
        }

        //South Row
        else if (targetTile.BoardVector.x >= relative.BoardVector.x && targetTile.BoardVector.y < relative.BoardVector.y)
        {
            tiles = tiles.Where
                (t => t.BoardVector.x >= relative.BoardVector.x && t.BoardVector.y < relative.BoardVector.y).ToArray();
        }

        //East Row
        else if (targetTile.BoardVector.x > relative.BoardVector.x && targetTile.BoardVector.y >= relative.BoardVector.y)
        {
            tiles = tiles.Where
                (t => t.BoardVector.x > relative.BoardVector.x && t.BoardVector.y >= relative.BoardVector.y).ToArray();
        }

        return tiles;
    }

    internal void SelectHorizontalTiles(Vector2 vector, int length, SelectionType selectionType, bool queryPassable)
    {
        GameBoardTile tile = GetTileByVector(vector);

        if(tile != null)
        {
            SelectHorizontalTiles(tile, length, selectionType, queryPassable);
        }
    }

    internal void SelectHorizontalTiles(GameBoardTile tile, int length, SelectionType selectionType, bool queryPassable)
    {
        ClearCurrentSelections();

        m_SelectionType = selectionType;

        SelectTiles(GetHorizontalTiles(tile, length, queryPassable), selectionType);
    }

    internal void SelectTilesInRange(GameBoardTile tile, int range, SelectionType selectionType, bool queryPassable, bool selectOrigin = true)
    {
        ClearCurrentSelections();

        m_SelectionType = selectionType;

        if(selectOrigin) SelectTile(tile, selectionType);

        SelectTiles
            (GetTilesInRange(tile, range, queryPassable), selectionType);
    }

    internal void SelectTilesInRange(Vector2 vector, int range, SelectionType selectionType, bool queryPassable, bool selectOrigin = true)
    {
        GameBoardTile tile = GetTileByVector(vector);

        if (tile != null)
        {
            ClearCurrentSelections();

            m_SelectionType = selectionType;

            if (selectOrigin) SelectTile(tile, selectionType);

            SelectTiles
                (GetTilesInRange(tile, range, queryPassable), selectionType);
        }
    }

    internal void SelectTiles(GameBoardTile[] tiles, SelectionType selectionType)
    {
        for (int i = 0; i < tiles.Length; i++)
            if (tiles[i] != null)
            {
                SelectTile(tiles[i], selectionType);
            }
    }

    internal void SelectTilesWithDelay(GameBoardTile[] tiles, SelectionType selectionType)
    {
        StartCoroutine(DelayedSelectTile(tiles, selectionType));
    }

    IEnumerator DelayedSelectTile(GameBoardTile[] tiles, SelectionType selectionType)
    {
        for (int i = 0; i < tiles.Length; i++)
            if (tiles[i] != null)
            {
                yield return new WaitForSeconds(0.5f);

                SelectTile(tiles[i], selectionType);
                m_CurrentlySelectedTiles.Add(tiles[i]);
            }
    }

    public void ClearCurrentSelections()
    {
        m_SelectionType = SelectionType.None;
        for(int i = m_CurrentlySelectedTiles.Count -1; i >= 0; i--)
            DeselectTile(m_CurrentlySelectedTiles[i]);
    }

    internal GameBoardTile[] GetHorizontalTiles(GameBoardTile tile, int length, bool queryPassable)
    {
        List<GameBoardTile> horizontalTiles = new List<GameBoardTile>();

        for (int i = 1; i <= length; i++)
            PopulateHorizontalTiles(horizontalTiles, tile, i, queryPassable);

        return horizontalTiles.ToArray();
    }

    internal void PopulateHorizontalTiles
        (List<GameBoardTile> horizontalTiles, GameBoardTile tile, int offset, bool queryPassable)
    {
        Vector2 deltaVector = Vector2.zero;
        GameBoardTile neighboringTile;

        deltaVector.x = tile.BoardVector.x + offset;
        deltaVector.y = tile.BoardVector.y;

        neighboringTile = GetTileByVector(deltaVector);

        if (neighboringTile != null)
        {
            if (queryPassable && neighboringTile.IsPassable)
                horizontalTiles.Add(neighboringTile);

            else if (!queryPassable)
                horizontalTiles.Add(neighboringTile);

            neighboringTile = null;
        }

        deltaVector.x = tile.BoardVector.x - offset;
        deltaVector.y = tile.BoardVector.y;

        neighboringTile = GetTileByVector(deltaVector);

        if (neighboringTile != null)
        {
            if (queryPassable && neighboringTile.IsPassable)
                horizontalTiles.Add(neighboringTile);

            else if (!queryPassable)
                horizontalTiles.Add(neighboringTile);

            neighboringTile = null;
        }

        deltaVector.x = tile.BoardVector.x;
        deltaVector.y = tile.BoardVector.y + offset;

        neighboringTile = GetTileByVector(deltaVector);

        if (neighboringTile != null)
        {
            if (queryPassable && neighboringTile.IsPassable)
                horizontalTiles.Add(neighboringTile);

            else if (!queryPassable)
                horizontalTiles.Add(neighboringTile);

            neighboringTile = null;
        }

        deltaVector.x = tile.BoardVector.x;
        deltaVector.y = tile.BoardVector.y - offset;

        neighboringTile = GetTileByVector(deltaVector);

        if (neighboringTile != null)
        {
            if (queryPassable && neighboringTile.IsPassable)
                horizontalTiles.Add(neighboringTile);

            else if (!queryPassable)
                horizontalTiles.Add(neighboringTile);

            neighboringTile = null;
        }
    }

    GameBoardTile[] GetTilesInRange(GameBoardTile root, int r, bool queryPassable)
    {
        int range = r;
        List<GameBoardTile>
            componentsInRange = new List<GameBoardTile>();

        GameBoardTile component = null;

        int x = (int)root.BoardVector.x;
        int y = (int)root.BoardVector.y;

        for (int i = 1; i <= range; i++)
        {
            component = GetTileByVector(new Vector2(x, y + i));
            if (component != null)
            {
                if (queryPassable && component.IsPassable)
                    componentsInRange.Add(component);

                else if (!queryPassable)
                    componentsInRange.Add(component);

                component = null;
            }

            component = GetTileByVector(new Vector2(x, y - i));
            if (component != null)
            {
                if (queryPassable && component.IsPassable)
                    componentsInRange.Add(component);

                else if (!queryPassable)
                    componentsInRange.Add(component);

                component = null;
            }

            component = GetTileByVector(new Vector2(x + i, y));
            if (component != null)
            {
                if (queryPassable && component.IsPassable)
                    componentsInRange.Add(component);

                else if (!queryPassable)
                    componentsInRange.Add(component);

                component = null;
            }

            component = GetTileByVector(new Vector2(x - i, y));
            if (component != null)
            {
                if (queryPassable && component.IsPassable)
                    componentsInRange.Add(component);

                else if (!queryPassable)
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
                component = GetTileByVector(new Vector2(x + h, y + j));
                if (component != null)
                {
                    if (queryPassable && component.IsPassable)
                        componentsInRange.Add(component);

                    else if(!queryPassable)
                        componentsInRange.Add(component);

                    component = null;
                }

                component = GetTileByVector(new Vector2(x - h, y - j));
                if (component != null)
                {
                    if (queryPassable && component.IsPassable)
                        componentsInRange.Add(component);

                    else if (!queryPassable)
                        componentsInRange.Add(component);

                    component = null;
                }

                component = GetTileByVector(new Vector2(x - h, y + j));
                if (component != null)
                {
                    if (queryPassable && component.IsPassable)
                        componentsInRange.Add(component);

                    else if (!queryPassable)
                        componentsInRange.Add(component);

                    component = null;
                }

                component = GetTileByVector(new Vector2(x + h, y - j));
                if (component != null)
                {
                    if (queryPassable && component.IsPassable)
                        componentsInRange.Add(component);

                    else if (!queryPassable)
                        componentsInRange.Add(component);

                    component = null;
                }
            }
        }
        return componentsInRange.ToArray();
    }

    void ProcessTileForAddition(GameBoardTile component, bool queryPassable, List<GameBoardTile> componentsInRange)
    {
        if (component != null)
        {
            if (queryPassable && component.IsPassable)
                componentsInRange.Add(component);

            else if (!queryPassable)
                componentsInRange.Add(component);

        }
    }
}

//else if (targetTile.BoardVector.x <= relative.BoardVector.x && targetTile.BoardVector.y > relative.BoardVector.y)
//{
//    tiles = tiles.Where
//        (t => t.BoardVector.x <= relative.BoardVector.x && t.BoardVector.y > relative.BoardVector.y).ToArray();
//}

//else if (targetTile.BoardVector.x > relative.BoardVector.x && targetTile.BoardVector.y <= relative.BoardVector.y)
//{
//    tiles = tiles.Where
//        (t => t.BoardVector.x > relative.BoardVector.x && t.BoardVector.y <= relative.BoardVector.y).ToArray();
//}

//else if (targetTile.BoardVector.x < relative.BoardVector.x && targetTile.BoardVector.y >= relative.BoardVector.y)
//{
//    tiles = tiles.Where
//        (t => t.BoardVector.x < relative.BoardVector.x && t.BoardVector.y >= relative.BoardVector.y).ToArray();
//}

//else if (targetTile.BoardVector.x <= relative.BoardVector.x && targetTile.BoardVector.y < relative.BoardVector.y)
//{
//    tiles = tiles.Where
//        (t => t.BoardVector.x <= relative.BoardVector.x && t.BoardVector.y < relative.BoardVector.y).ToArray();
//}