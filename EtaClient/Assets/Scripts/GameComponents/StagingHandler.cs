using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StagingHandler : MonoBehaviour
{
    [SerializeField]
    Camera m_TargetCamera;

    [SerializeField]
    int m_StageWidth;

    [SerializeField]
    int m_StageLength;

    [SerializeField]
    int m_StageHeight;

    [SerializeField]
    Vector2 m_ReferenceVector;

    [SerializeField]
    GameObject m_TilePrefab;

    [SerializeField]
    GamePieceHandler m_GamePieceHandler;

    List<GamePieceBoardState> m_StagingPieces;
    List<GameObject> m_StagedPieces = new List<GameObject>();

    Vector2 m_DeltaVector;
    Vector3 m_GammaVector;

    Vector3 m_StageOrigin;

    Dictionary<Vector2, GameBoardTile> 
        m_StagingTiles = new Dictionary<Vector2, GameBoardTile>();

    bool m_IsDraggingPiece;

    int m_LeftMouseButtonIndex = 0;
    float m_MaxRayDistance = 1000f;

    Ray m_Ray;

    RaycastHit[] m_RaycastHits = new RaycastHit[64];

    GamePiece m_PieceCache;
    GameBoardTile m_TileCache;

    private void Start()
    {
        m_StageOrigin = transform.position;
        StartCoroutine(DelayedInitialization());
    }

    IEnumerator DelayedInitialization()
    {
        yield return new WaitForSeconds(0.25f);

        if (StageLoadHandler.LoadedGamePieceStates != null)
            m_StagingPieces = StageLoadHandler.LoadedGamePieceStates;
        else
            m_StagingPieces = m_GamePieceHandler.DefaultPieceStates;

        GenerateStage();
        GeneratePieces();
    }

    private void Update()
    {
        if (Input.GetMouseButton(m_LeftMouseButtonIndex))
        {
            m_Ray = m_TargetCamera.ScreenPointToRay(Input.mousePosition);
            Physics.RaycastNonAlloc(m_Ray, m_RaycastHits, m_MaxRayDistance);

            for (int i = 0; i < m_RaycastHits.Length; i++)
            {
                if (m_RaycastHits[i].transform == null)
                    continue;

                if (!m_IsDraggingPiece && 
                    (m_PieceCache = m_RaycastHits[i].transform.GetComponent<GamePiece>()) != null)
                {
                    m_IsDraggingPiece = true;
                    break;
                }

                else if (m_IsDraggingPiece)
                {
                    m_TileCache = m_RaycastHits[i].transform.GetComponent<GameBoardTile>();
                    if (m_TileCache != null)
                    {
                        SyncPieceToTile(m_PieceCache, m_TileCache);
                        break;
                    }
                }
            }
        }

        else if(Input.GetMouseButtonUp(m_LeftMouseButtonIndex))
        {
            m_IsDraggingPiece = false;
            m_PieceCache = null;
            m_TileCache = null;
        }
    }

    public void RelayUpdateToLoadHandler()
    {
        StageLoadHandler.UpdateCurrentStageState(m_StagedPieces);
    }

    void SyncPieceToTile(GamePiece piece, GameBoardTile tile)
    {
        if (tile.OccupyingPiece != null)
            return;

        if (piece.CurrentTileOccupied != null)
            piece.CurrentTileOccupied.ClearOccupant();

        piece.SetStartingVector(tile.BoardVector);

        Vector3 deltaPostion = tile.transform.position;
        deltaPostion.y = m_StageHeight;

        piece.transform.position = deltaPostion;

        tile.SetOccupyingPiece(piece);
    }

    void GenerateStage()
    {
        for(int x = 0; x < m_StageLength; x++)
        {
            for(int z = 0; z < m_StageWidth; z++)
            {
                ResetVectors();

                m_DeltaVector.x += x;
                m_DeltaVector.y += z;

                m_GammaVector.x += x;
                m_GammaVector.z += z;

                GameObject tile = Instantiate
                    (m_TilePrefab, m_GammaVector, m_TilePrefab.transform.rotation);

                tile.GetComponent<GameBoardTile>().
                    InitializeTileForStaging(m_DeltaVector);

                m_StagingTiles.Add(m_DeltaVector, tile.GetComponent<GameBoardTile>());
            }
        }
    }

    void GeneratePieces()
    {
        foreach(GamePieceBoardState pState in m_StagingPieces)
        {
            GameObject pieceObject = Instantiate(pState.GamePiecePrefab);
            GamePiece piece = pieceObject.GetComponent<GamePiece>();
            piece.SetStartingVector(pState.StartVector);

            if (m_StagingTiles.ContainsKey(piece.StartingVector))
            {
                SyncPieceToTile
                    (piece, m_StagingTiles[piece.StartingVector]);

                m_StagedPieces.Add(pieceObject);
            }

            else Debug.LogWarningFormat
                    ("Attempting To Stage Gamepiece ({0}) At Invalid Vector [{1}]", piece.GamePieceName, piece.StartingVector);
        }
    }

    void ResetVectors()
    {
        m_DeltaVector.x = m_ReferenceVector.x;
        m_DeltaVector.y = m_ReferenceVector.y;

        m_GammaVector = m_StageOrigin;
    }
}
