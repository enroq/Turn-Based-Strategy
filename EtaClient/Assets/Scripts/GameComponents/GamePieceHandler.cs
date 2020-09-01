using Eta.Interdata;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class GamePieceBoardState
{
    [SerializeField]
    Vector2 m_StartVector;

    [SerializeField]
    GameObject m_GamePiecePrefab;

    internal Vector2 StartVector { get { return m_StartVector; } }
    internal GameObject GamePiecePrefab { get { return m_GamePiecePrefab; } }

    public GamePiece GamePieceComponent { get; set; }

    public GamePieceBoardState(GameObject prefab, Vector2 startVector)
    {
        m_GamePiecePrefab = prefab;
        m_StartVector = startVector;

        GamePieceComponent = m_GamePiecePrefab.GetComponent<GamePiece>();

        if (GamePieceComponent == null)
            Debug.LogError("Game Piece Board State Prefab Missing Game Piece Component!");
    }
}

public class GamePieceHandler : MonoBehaviour
{
    [SerializeField]
    GameBoardInstance m_GameBoardRelative;

    [SerializeField]
    List<GameObject> m_GamePiecePrefabs;

    [SerializeField]
    List<GamePieceBoardState> m_DefaultPieceStates;

    static List<GamePieceBoardState> 
        m_StagedPieceStates = new List<GamePieceBoardState>();

    static Dictionary<string, GameObject> 
        m_GamePieceDictionary = new Dictionary<string, GameObject>();

    static Dictionary<string, GamePiece> 
        m_ActiveGamePieces = new Dictionary<string, GamePiece>();

    static Dictionary<string, Dictionary<string, GamePieceNetworkState>>
        m_PieceSyncCache = new Dictionary<string, Dictionary<string, GamePieceNetworkState>>();

    public static Dictionary<string, GameObject> 
        GamePieceDictionary { get { return m_GamePieceDictionary; } }

    public List<GamePieceBoardState> 
        DefaultPieceStates { get { return m_DefaultPieceStates; } }

    public static List<GamePieceBoardState> 
        StagedPieceStates { get { return m_StagedPieceStates; } }

    public static Dictionary<string, GamePiece> 
        ActiveGamePieces { get { return m_ActiveGamePieces; } }

    public static Dictionary<string, Dictionary<string, GamePieceNetworkState>> 
        PieceSyncCache { get { return m_PieceSyncCache; } }

    static GamePiece m_CurrentFocusPiece;
    
    Vector3 m_DeltaPosition = Vector3.zero;

    bool m_InitializedPieceMovement = false;

    public static GamePiece CurrentFocusPiece { get { return m_CurrentFocusPiece; } }

    internal static void RelayMultiPieceSyncsToClientManager(string matchId, string accountId, int playerSlot)
    {
        ClientManager.Instance.SendMultiplePieceSyncCommands
            (matchId, accountId, playerSlot, ActiveGamePieces.Values.ToArray());
    }

    internal static void RelayPieceSyncToClientManager(string accountId, int playerSlot, GamePiece piece)
    {
        if(MatchHandler.CurrentMatch != null)
            ClientManager.Instance.SendPieceSyncCommands
                (MatchHandler.CurrentMatch.MatchIdentity, accountId, playerSlot, piece);
    }

    private void Awake()
    {
        foreach (GameObject obj in m_GamePiecePrefabs)
        {
            GamePiece piece = obj.GetComponent<GamePiece>();

            if (!m_GamePieceDictionary.ContainsKey(piece.GamePieceName))
                m_GamePieceDictionary.Add(piece.GamePieceName, obj);
        }
    }

    void Start ()
    {
        EventSink.UnitMovementEvent += EventSink_UnitMovementEvent;
        EventSink.UnitAttackEvent += EventSink_UnitAttackEvent;

        InterEventDispatcher.GamePieceSyncReceivedEvent += InterEventDispatcher_GamePieceSyncReceivedEvent;
        InterEventDispatcher.GamePieceSyncCompleteEvent += InterEventDispatcher_GamePieceSyncCompleteEvent;

        InterEventDispatcher.NetworkGamePieceMoveEvent += InterEventDispatcher_NetworkGamePieceMoveEvent;
        InterEventDispatcher.AttackSyncCompleteEvent += InterEventDispatcher_AttackSyncCompleteEvent;

        InitializeCurrentPieces();
	}

    private void InterEventDispatcher_AttackSyncCompleteEvent(AttackSyncCompleteEventArgs args)
    {
        ClientManager.Post(() => HandleAttackSyncCompleteEvent
            (args.DefendingPlayerId, args.AttackingPieceId, new Vector2(args.BoardVectorX, args.BoardVectorY)));
    }

    private void EventSink_UnitAttackEvent(UnitAttackEventArgs args)
    {
        List<GamePiece> targets = new List<GamePiece>();

        foreach (GameBoardTile tile in m_GameBoardRelative.CurrentlySelectedTiles)
        {
            if (tile.OccupyingPiece != null)
            {
                targets.Add(tile.OccupyingPiece);
                Debug.LogFormat("Tile Occupant ({0}): {1}",
                    tile.BoardVector, tile.OccupyingPiece.gameObject.name);
            }
        }

        if (m_CurrentFocusPiece.TestMode)
        {
            m_GameBoardRelative.ClearCurrentSelections();
            m_CurrentFocusPiece.FinalizeAttack(targets.ToArray(), args.Tile);
        }

        else if(m_CurrentFocusPiece.UnderLocalControl)
        {
            ClientManager.Instance.SendAttackTargetSyncCommand
                (MatchHandler.CurrentMatch.MatchIdentity, m_CurrentFocusPiece.NetworkIdentity, 
                    m_CurrentFocusPiece.AttackRating, targets.ToArray(), args.Tile.BoardVector);
        }

        if(m_CurrentFocusPiece.TestMode)
            TurnStateHandler.FinishStep();
    }

    private void InterEventDispatcher_NetworkGamePieceMoveEvent(NetworkGamePieceMoveEventArgs args)
    {
        ClientManager.Post(() => HandleNetworkGamePieceMoveEvent(args));
    }

    private void HandleNetworkGamePieceMoveEvent(NetworkGamePieceMoveEventArgs args)
    {
        if (m_ActiveGamePieces.ContainsKey(args.PieceNetworkId))
        {
            GamePiece piece = m_ActiveGamePieces[args.PieceNetworkId];

            GameBoardTile destinationTile = m_GameBoardRelative
                .GetTileByVector(new Vector2(args.DestinationX, args.DestinationY));

            piece.InitializePathTravel
                (PathHandler.DeterminePath(m_GameBoardRelative, piece.BoardVector, destinationTile));
        }
    }

    private void InterEventDispatcher_GamePieceSyncCompleteEvent(GamePieceSyncCompleteEventArgs args)
    {
        ClientManager.Post(() => HandleSyncCompleteEvent(args.ControllerId));
    }

    private void HandleAttackSyncCompleteEvent(string defendingPlayerId, string attackingPieceId, Vector2 boardVector)
    {
        List<GamePiece> targets = new List<GamePiece>();
        if (MatchHandler.CurrentMatch != null)
        {
            if (m_PieceSyncCache.ContainsKey(defendingPlayerId))
            {
                foreach (KeyValuePair<string, GamePieceNetworkState> kvp in m_PieceSyncCache[defendingPlayerId])
                {
                    if (m_ActiveGamePieces.ContainsKey(kvp.Key))
                    {
                        UpdateGamePiece(kvp.Value);
                        targets.Add(m_ActiveGamePieces[kvp.Key]);
                    }
                }

                m_PieceSyncCache[defendingPlayerId].Clear();

                m_GameBoardRelative.ClearCurrentSelections();

                if (m_ActiveGamePieces.ContainsKey(attackingPieceId))
                {
                    m_ActiveGamePieces[attackingPieceId].FinalizeAttack
                            (targets.ToArray(), m_GameBoardRelative.GetTileByVector(boardVector));
                }

                TurnStateHandler.FinishStep();
            }
        }
    }

    private void HandleSyncCompleteEvent(string controllerId)
    {
        List<GamePieceNetworkState> 
            netStatesWithoutRelatives = new List<GamePieceNetworkState>();

        if (MatchHandler.CurrentMatch != null)
        {
            if (m_PieceSyncCache.ContainsKey(controllerId))
            {
                foreach (KeyValuePair<string, GamePieceNetworkState> kvp in m_PieceSyncCache[controllerId])
                {
                    if (m_ActiveGamePieces.ContainsKey(kvp.Key))
                        UpdateGamePiece(kvp.Value);

                    else
                        netStatesWithoutRelatives.Add(kvp.Value);
                }

                if (netStatesWithoutRelatives.Count > 0)
                    InstantiateGamePiecesFromNetState(netStatesWithoutRelatives.ToArray());

                m_PieceSyncCache[controllerId].Clear();
            }
        }
    }

    private void InterEventDispatcher_GamePieceSyncReceivedEvent(GamePieceSyncReceivedEventArgs args)
    {
        if (!m_PieceSyncCache.ContainsKey(args.ControllerId))
            m_PieceSyncCache.Add(args.ControllerId, new Dictionary<string, GamePieceNetworkState>());

        if (m_PieceSyncCache.ContainsKey(args.ControllerId))
        {
            if (!m_PieceSyncCache[args.ControllerId].ContainsKey(args.GamePieceNetworkState.NetworkIdentity))
                m_PieceSyncCache[args.ControllerId].Add
                    (args.GamePieceNetworkState.NetworkIdentity, args.GamePieceNetworkState);

            else if (m_PieceSyncCache[args.ControllerId].ContainsKey(args.GamePieceNetworkState.NetworkIdentity))
                m_PieceSyncCache[args.ControllerId]
                    [args.GamePieceNetworkState.NetworkIdentity] = args.GamePieceNetworkState;
        }
    }

    void InstantiateGamePiecesFromNetState(GamePieceNetworkState[] netStates)
    {
        foreach (GamePieceNetworkState netState in netStates)
        {
            if (GamePieceDictionary.ContainsKey(netState.GamePieceName))
            {
                GameObject obj = Instantiate(GamePieceDictionary[netState.GamePieceName]);
                GamePiece gamePiece = obj.GetComponent<GamePiece>();

                gamePiece.SetHandler(this);
                gamePiece.SetStartingVector(netState.PositionX, netState.PositionY);
                gamePiece.SetNetworkIdentity(netState.NetworkIdentity);
                gamePiece.SetControllerIdentity(netState.ControllerIdentity);
                gamePiece.SetParentBoard(m_GameBoardRelative);
                gamePiece.SetControllingPlayerPosition(netState.ControllerPosition);

                SetGamePieceStartingPosition(obj, gamePiece, netState.ControllerPosition);

                m_ActiveGamePieces.Add(gamePiece.NetworkIdentity, gamePiece);
            }
        }
    }

    void UpdateGamePiece(GamePieceNetworkState netState)
    {
        if(ActiveGamePieces.ContainsKey(netState.NetworkIdentity))
        {
            Debug.LogFormat("Updating Netstate: {0}", netState.GamePieceName);

            GamePiece piece = ActiveGamePieces[netState.NetworkIdentity];

            piece.CurrentHitPoints = netState.Hitpoints;

            piece.SetCurrentVector(netState.PositionX, netState.PositionY);

            UpdatePiecePosition(piece.gameObject, piece);

            Quaternion deltaRot = piece.gameObject.transform.rotation;
            deltaRot.eulerAngles = new Vector3(0, netState.Rotation, 0);
            piece.gameObject.transform.rotation = deltaRot;
        }
    }

    void InitializeCurrentPieces()
    {
        if (StageLoadHandler.LoadedGamePieceStates != null)
            m_StagedPieceStates = StageLoadHandler.LoadedGamePieceStates;
        else
        {
            m_StagedPieceStates = m_DefaultPieceStates;
            foreach (GamePieceBoardState gpbs in m_StagedPieceStates)
                gpbs.GamePieceComponent = gpbs.GamePiecePrefab.GetComponent<GamePiece>();
        }
    }

    private void EventSink_UnitMovementEvent(UnitMovementEventArgs args)
    {
        if(m_CurrentFocusPiece)
            SendFocusUnitToTile(args.Tile);
    }

    internal void InitializeTestPieces()
    {
        InstantiateGamePieces(1, "nullOne");
        InstantiateGamePieces(2, "nullTwo");
    }

    internal void InstantiateGamePieces(int playerSlot, string controllerId)
    {
        InstantiateGamePiecesFromBoardStates(m_StagedPieceStates.ToArray(), playerSlot, controllerId);
    }

    internal void InstantiateGamePiecesFromBoardStates(GamePieceBoardState[] pieceStates, int playerSlot, string controllerId)
    {
        foreach(GamePieceBoardState pState in pieceStates)
        {
            GameObject obj = Instantiate(pState.GamePiecePrefab);
            GamePiece gamePiece = obj.GetComponent<GamePiece>();

            gamePiece.CurrentHitPoints = gamePiece.MaxHitpoints;

            gamePiece.SetHandler(this);
            gamePiece.SetStartingVector(pState.StartVector);
            gamePiece.SetNetworkIdentity(Guid.NewGuid().ToString());
            gamePiece.SetControllerIdentity(controllerId);
            gamePiece.SetParentBoard(m_GameBoardRelative);
            gamePiece.SetControllingPlayerPosition(playerSlot);

            obj.name += string.Format(" <{0}>", gamePiece.NetworkIdentity);

            SetGamePieceStartingPosition(obj, gamePiece, playerSlot);

            m_ActiveGamePieces.Add(gamePiece.NetworkIdentity, gamePiece);
        }

        if(controllerId != "nullOne" && controllerId != "nullTwo")
            RelayMultiPieceSyncsToClientManager
                (MatchHandler.CurrentMatch.MatchIdentity, controllerId, playerSlot);
    }

    void SetGamePieceStartingPosition(GameObject obj, GamePiece piece, int playerSlot)
    {
        ProcessStartingPosition(piece, playerSlot);
        obj.transform.position = m_DeltaPosition;

        if (playerSlot == 2)
            obj.transform.Rotate(0, 180, 0);
    }

    internal void ProcessStartingPosition(GamePiece piece, int playerSlot)
    {
        switch(playerSlot)
        {
            case 1:
                {
                    m_DeltaPosition.x = piece.StartingVector.x;
                    m_DeltaPosition.y = m_GameBoardRelative.BoardHeight;
                    m_DeltaPosition.z = piece.StartingVector.y;

                    piece.SetCurrentVector(piece.StartingVector);
                    break;
                }
            case 2:
                {
                    m_DeltaPosition.x = piece.StartingVector.x;
                    m_DeltaPosition.y = m_GameBoardRelative.BoardHeight;
                    m_DeltaPosition.z = Mathf.Abs(piece.StartingVector.y);

                    piece.SetCurrentVector
                        (piece.StartingVector.x, Mathf.Abs(piece.StartingVector.y));
                    break;
                }
            default:
                {
                    Debug.LogFormat("[Error] Invalid Player Slot ({0}) Usage In Game Piece Initialization..", playerSlot);
                    break;
                }
        }
    }

    internal void UpdatePiecePosition(GameObject obj, GamePiece piece)
    {
        m_DeltaPosition = new Vector3
            (piece.BoardVector.x, m_GameBoardRelative.BoardHeight, piece.BoardVector.y);

        Debug.LogFormat("Board Vector [{0}] To Delta Position [{1}]", 
            piece.BoardVector.ToString("F5"), m_DeltaPosition.ToString("F5"));

        obj.transform.position = m_DeltaPosition;

        Debug.LogFormat("Delta Position [{0}] To Transform Position [{1}]",
             m_DeltaPosition.ToString("F5"), obj.transform.position.ToString("F5"));
    }

    internal void ProcessCurrentPosition(GamePiece piece)
    {
        m_DeltaPosition.x = piece.BoardVector.x;
        m_DeltaPosition.y = m_GameBoardRelative.BoardHeight;
        m_DeltaPosition.z = piece.BoardVector.y;
    }

    internal void ClearFocusPiece()
    {
        m_CurrentFocusPiece = null;
    }

    internal void SetFocusPiece(GamePiece piece)
    {
        if(m_CurrentFocusPiece == piece)
        {
            m_CurrentFocusPiece = null;
            m_GameBoardRelative.ClearCurrentSelections();

            piece.DeactivateRotationCanvas();
        }

        if(m_CurrentFocusPiece != null && m_CurrentFocusPiece != piece 
                && (m_GameBoardRelative.SelectionType == SelectionType.Target || m_GameBoardRelative.SelectionType == SelectionType.Attack))
        {
            GameBoardTile tile = m_GameBoardRelative.GetTileByVector(piece.BoardVector);

            if (m_GameBoardRelative.CurrentlySelectedTiles.Contains(tile))
            {
                m_CurrentFocusPiece.HandleAttackSelection(tile);
                return;
            }
        }

        if (m_CurrentFocusPiece == null || (m_CurrentFocusPiece != null && !m_CurrentFocusPiece.IsActivelyEngaged))
        {
            if (m_CurrentFocusPiece != null)
            {
                m_CurrentFocusPiece.DeactivateRotationCanvas();
                m_GameBoardRelative.ClearCurrentSelections();
            }

            m_CurrentFocusPiece = piece;
        }

        if (!m_CurrentFocusPiece.IsActivelyEngaged && 
            (AccountManager.AccountInstance == null || m_CurrentFocusPiece.UnderLocalControl))
        {
            if (m_GameBoardRelative.GetHorizontalTiles
                (m_GameBoardRelative.GetTileByVector(piece.BoardVector), 1, true).Length > 0)
            {
                m_GameBoardRelative.SelectTilesInRange
                    (piece.BoardVector, piece.MovementRange, SelectionType.Movement, true, false);
            }
        }
    }

    void SendFocusUnitToTile(GameBoardTile destinationTile)
    {
        if (!m_CurrentFocusPiece.IsActivelyEngaged)
        {
            m_GameBoardRelative.ClearCurrentSelections();

            if (MatchHandler.CurrentMatch != null)
            {
                m_InitializedPieceMovement = true;
                ClientManager.Instance.SendNetworkPieceMovementCommand
                    (MatchHandler.CurrentMatch.MatchIdentity, m_CurrentFocusPiece.NetworkIdentity, destinationTile.BoardVector);
            }

            else
                m_CurrentFocusPiece.InitializePathTravel
                    (PathHandler.DeterminePath(m_GameBoardRelative, m_CurrentFocusPiece.BoardVector, destinationTile));
        }
    }

    internal void HandlePathComplete(GamePiece piece)
    {
        if (m_InitializedPieceMovement || m_GameBoardRelative.TestMode)
        {
            m_InitializedPieceMovement = false;
            RelayPieceSyncToClientManager
                (piece.ControllingPlayerId, piece.ControllingPlayerPosition, piece);

            if(piece.UnderLocalControl || m_GameBoardRelative.TestMode)
                TurnStateHandler.FinishStep();
        }
    }

    internal void HandleRotationChange(GamePiece piece)
    {
        RelayPieceSyncToClientManager
            (piece.ControllingPlayerId, piece.ControllingPlayerPosition, piece);
    }
}
