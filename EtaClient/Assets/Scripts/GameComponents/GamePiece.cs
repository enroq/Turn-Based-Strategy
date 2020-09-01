using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Eta.Interdata;

public class GamePiece : MonoBehaviour, IBoardComponent
{
    [SerializeField]
    bool m_TestMode;

    [SerializeField]
    string m_GamePieceName;

    [SerializeField]
    int m_MovementRange;

    [SerializeField]
    int m_MaxHitpoints;

    [SerializeField]
    int m_DefenseRating;

    [SerializeField]
    float m_MovementSpeed = 1.0f;

    [SerializeField]
    float m_StopThreshold = 0.2f;

    [SerializeField]
    AttackState m_AttackState;

    [SerializeField]
    GameObject m_RotationSelectionCanvas;

    private Animator m_Animator;

    private int m_ControllingPlayerPosition = 0;
    private string m_NetworkIdentity = string.Empty;
    private string m_ControllingPlayerId = string.Empty;

    private GameBoardInstance m_ParentBoard;
    private GamePieceHandler m_Handler;

    private int m_CurrentPathIndex;

    private Vector2 m_DefaultStartingVector;
    private Vector3 m_DeltaVector = Vector3.zero;
    private Quaternion m_DeltaRotation = Quaternion.identity;

    private bool m_IsMoving;
    private Vector2 m_CurrentVector;
    private Vector2 m_CurrentStartingVector = Vector2.zero;

    internal Vector2 StartingVector
    {
        get
        {
            if (m_CurrentStartingVector == Vector2.zero)
                return m_DefaultStartingVector;
            else
                return m_CurrentStartingVector;
        }
    }

    internal GamePieceHandler Handler { get { return m_Handler; } }

    internal string NetworkIdentity { get { return m_NetworkIdentity; } }
    internal string ControllingPlayerId { get { return m_ControllingPlayerId; } }
    internal int ControllingPlayerPosition { get { return m_ControllingPlayerPosition; } }

    internal bool IsMoving { get { return m_IsMoving; } }
    internal int MovementRange { get { return m_MovementRange; } }
    internal string GamePieceName { get { return m_GamePieceName; } }

    internal bool IsActivelyEngaged { get { return IsMoving; } }

    internal bool UnderLocalControl
    {
        get
        {   if (AccountManager.AccountInstance != null)
                return (ControllingPlayerId == AccountManager.AccountInstance.Identity);
            else
                return false;
        }
    }

    internal bool TestMode { get { return m_ParentBoard.TestMode; } }

    public Vector2 BoardVector { get { return m_CurrentVector; } }

    public int MaxHitpoints { get { return m_MaxHitpoints; } }
    public int DefenseRating { get { return m_DefenseRating; } }

    public int AttackRating { get { return m_AttackState.AttackRating; } }
    public int TurnDelay { get { return m_AttackState.TurnDelay; } }

    public int CurrentHitPoints { get; set; }

    public GameBoardTile CurrentTileOccupied { get; set; }

    private void Start()
    {
        if ((m_Animator = GetComponent<Animator>()) == null)
            Debug.LogErrorFormat("[{0}] Is Missing Animator Component..", gameObject.name);
    }

    Vector3 m_SyncVector = Vector3.zero;
    private void Update()
    {
        if (!m_IsMoving)
        {
            m_SyncVector.x = BoardVector.x;
            m_SyncVector.y = m_ParentBoard.BoardHeight;
            m_SyncVector.z = BoardVector.y;

            gameObject.transform.position = m_SyncVector;
        }
    }

    internal void SetParentBoard(GameBoardInstance board)
    {
        m_ParentBoard = board;
    }

    internal void SetHandler(GamePieceHandler handler)
    {
        m_Handler = handler;
    }

    internal void SetNetworkIdentity(string networkId)
    {
        m_NetworkIdentity = networkId;
    }

    internal void SetControllerIdentity(string controllerId)
    {
        m_ControllingPlayerId = controllerId;
    }

    internal void SetCurrentVector(Vector2 v)
    {
        SetCurrentVector(v.x, v.y);
    }

    internal void SetCurrentVector(float x, float y)
    {
        m_ParentBoard.SetTilePassableState(m_CurrentVector, true);
        m_ParentBoard.ClearTileOccupant(m_CurrentVector);

        m_CurrentVector.x = x;
        m_CurrentVector.y = y;

        m_ParentBoard.SetTilePassableState(m_CurrentVector, false);
        m_ParentBoard.SetTileOccupant(m_CurrentVector, this);
    }

    internal void SetStartingVector(Vector2 v)
    {
        m_CurrentStartingVector = v;
    }

    internal void SetStartingVector(float x, float y)
    {
        m_CurrentStartingVector.x = x;
        m_CurrentStartingVector.y = y;
    }

    internal void SetDefaultStartingVector(Vector2 v)
    {
        m_CurrentStartingVector = v;
    }

    internal void SelectGamePiece()
    {
        if (TurnStateHandler.IsControllersTurn(this))
        {
            if(TurnStateHandler.CurrentTurnState.CurrentTurnStep == TurnStep.Move)
                m_Handler.SetFocusPiece(this);

            else if (GamePieceHandler.CurrentFocusPiece == this)
            {
                if (TurnStateHandler.CurrentTurnState.CurrentTurnStep == TurnStep.Attack)
                    BeginAttack();

                else 
                    if (TurnStateHandler.CurrentTurnState.CurrentTurnStep == TurnStep.Direction)
                        ActivateRotationCanvas();
            }
        }
    }

    internal void HandleTurnStepChange(TurnStep turnStep)
    {
        switch(turnStep)
        {
            case TurnStep.End:
                {
                    if (!IsActivelyEngaged)
                    {
                        m_ParentBoard.ClearCurrentSelections();
                        m_Handler.ClearFocusPiece();
                    }
                    break;
                }
            default:
                {
                    Debug.LogFormat("Turn Step Case Invalid: {0}", turnStep.ToString());
                    break;
                }
        }
    }

    internal void ProcessDamage(int dmg)
    {
        int rawDmg = (dmg - DefenseRating);

        if (rawDmg >= 1)
            CurrentHitPoints -= rawDmg;
        else
            CurrentHitPoints--;

        if (CurrentHitPoints <= 0)
            StartCoroutine(HandleDeath());
    }

    internal void ProcessDamage()
    {
        if (CurrentHitPoints <= 0)
            StartCoroutine(HandleDeath());
    }

    IEnumerator HandleDeath()
    {
        m_Animator.Play("Death");

        yield return new WaitForSeconds(2.0f);

        m_ParentBoard.SetTilePassableState(BoardVector, true);
        CurrentTileOccupied.ClearOccupant();

        gameObject.SetActive(false);
    }

    internal void HandleDoubleClick()
    {
        if(m_ParentBoard.SelectionType == SelectionType.Attack)
        {
            GameBoardTile tile = m_ParentBoard.GetTileByVector(BoardVector);
            if (m_ParentBoard.CurrentlySelectedTiles.Contains(tile))
            {
                EventSink.InvokeUnitAttackEvent
                    (new UnitAttackEventArgs(tile));
            }
        }
    }

    internal void SetControllingPlayerPosition(int i)
    {
        m_ControllingPlayerPosition = i;
    }

    Coroutine m_TravelRoutine;
    internal void InitializePathTravel(GameBoardTile[] tilePath)
    {
        if (tilePath == null)
            return;

        m_IsMoving = true;
        m_CurrentPathIndex = 0;

        m_TravelRoutine = StartCoroutine(MoveAlongPath(tilePath)); 
    }

    IEnumerator MoveAlongPath(GameBoardTile[] tilePath)
    {
        while (m_CurrentPathIndex < tilePath.Length - 1)
        {
            yield return new WaitForEndOfFrame();

            if (m_CurrentPathIndex == 0)
            {
                m_DeltaVector.x = tilePath[m_CurrentPathIndex].BoardVector.x;
                m_DeltaVector.y = m_ParentBoard.BoardHeight;
                m_DeltaVector.z = tilePath[m_CurrentPathIndex].BoardVector.y;

                m_DeltaRotation = Quaternion.LookRotation(m_DeltaVector - transform.position);
                m_DeltaRotation.z = 0;
                transform.rotation = m_DeltaRotation;
            }

            if(!m_Animator.GetBool("IsWalking"))
                m_Animator.SetBool("IsWalking", true);

            if (Vector3.Distance(transform.position, m_DeltaVector) > m_StopThreshold)
                transform.position = Vector3.MoveTowards
                    (transform.position, m_DeltaVector, m_MovementSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, m_DeltaVector) <= m_StopThreshold)
            {
                m_CurrentPathIndex++;

                if (m_CurrentPathIndex != tilePath.Length - 1)
                {
                    m_DeltaVector.x = tilePath[m_CurrentPathIndex].BoardVector.x;
                    m_DeltaVector.y = m_ParentBoard.BoardHeight;
                    m_DeltaVector.z = tilePath[m_CurrentPathIndex].BoardVector.y;

                    m_DeltaRotation = Quaternion.LookRotation(m_DeltaVector - transform.position);
                    m_DeltaRotation.z = 0;
                    transform.rotation = m_DeltaRotation;
                }
            }
        }

        EndPath(tilePath[tilePath.Length - 1].BoardVector);
    }

    void EndPath(Vector2 v)
    {
        if (m_TravelRoutine != null)
            StopCoroutine(m_TravelRoutine);

        m_IsMoving = false;
        SetCurrentVector(v);

        transform.rotation = Quaternion.Euler
            (Utility.GetRotationToNearestRightAngle(transform.rotation.eulerAngles));

        m_Animator.SetBool("IsWalking", false);

        Debug.Log("Position After Travel: " + transform.position.ToString("F5"));

        gameObject.transform.position = new Vector3(v.x, m_ParentBoard.BoardHeight, v.y);

        Debug.Log("Position After Set: " + transform.position.ToString("F5"));

        m_Handler.HandlePathComplete(this);

        if(UnderLocalControl || m_ParentBoard.TestMode)
            BeginAttack();
    }

    internal void DeactivateRotationCanvas()
    {
        if (m_RotationSelectionCanvas)
            m_RotationSelectionCanvas.SetActive(false);
    }

    internal void ActivateRotationCanvas()
    {
        if (AccountManager.AccountInstance == null || UnderLocalControl)
        {
            if (m_RotationSelectionCanvas)
                m_RotationSelectionCanvas.SetActive(true);
        }
    }

    internal void BeginAttack()
    {
        switch(m_AttackState.TargetType)
        {
            case TargetType.Single:
                {
                    PrepareAttack_SingleTarget();
                    break;
                }
            case TargetType.Area:
                {
                    PrepareAttack_AreaTarget();
                    break;
                }
            case TargetType.Linear:
                {
                    PrepareAttack_LinearTarget();
                    break;
                }
            case TargetType.Radius:
                {
                    SelectAttackTargetTiles_RadiusTarget();
                    break;
                }
            default:
                {
                    Debug.LogWarning("Game Piece Attempting To Attack Without Attack Type.");
                    break;
                }
        }
    }

    void PrepareAttack_SingleTarget()
    {
        m_ParentBoard.SelectTilesInRange
            (BoardVector, m_AttackState.TargetRange, SelectionType.Target, false, false);
    }

    void PrepareAttack_AreaTarget()
    {
        m_ParentBoard.SelectTilesInRange
            (BoardVector, m_AttackState.TargetRange, SelectionType.Target, false, false);
    }

    void PrepareAttack_LinearTarget()
    {
        m_ParentBoard.SelectHorizontalTiles
            (BoardVector, m_AttackState.TargetRange, SelectionType.Target, false);
    }

    internal void HandleAttackSelection(GameBoardTile tile)
    {
        switch(m_AttackState.TargetType)
        {
            case TargetType.Single:
                {
                    SelectAttackTargetTiles_SingleTarget(tile);
                    break;
                }
            case TargetType.Area:
                {
                    SelectAttackTargetTiles_AreaTarget(tile);
                    break;
                }
            case TargetType.Linear:
                {
                    SelectAttackTargetTiles_LinearTarget(tile);
                    break;
                }
            default:
                {
                    Debug.LogWarning("Game Piece Attempting To Attack Without Attack Type.");
                    break;
                }
        }
    }

    void SelectAttackTargetTiles_SingleTarget(GameBoardTile tile)
    {
        m_ParentBoard.SelectTilesInRange
            (tile, 0, SelectionType.Attack, false);
    }

    void SelectAttackTargetTiles_AreaTarget(GameBoardTile tile)
    {
        m_ParentBoard.SelectTilesInRange
            (tile, m_AttackState.AttackRange, SelectionType.Attack, false);
    }

    void SelectAttackTargetTiles_LinearTarget(GameBoardTile tile)
    {
        m_ParentBoard.SelectRowFromHorizontalSelection
            (BoardVector, tile, m_AttackState.AttackRange, SelectionType.Attack);
    }

    void SelectAttackTargetTiles_RadiusTarget()
    {
        m_ParentBoard.SelectTilesInRange
            (BoardVector, m_AttackState.AttackRange, SelectionType.Attack, false, false);
    }

    internal void FinalizeAttack(GamePiece[] targets, GameBoardTile targetTile)
    {
        switch (m_AttackState.AttackType)
        {
            case AttackType.Melee:
                {
                    if(targets.Length > 0 && targets[0] != null)
                        HandleMeleeAttackResults(targets[0], m_AttackState);
                    break;
                }
            case AttackType.Magic:
                {
                    if (targets.Length > 0 && targets[0] != null)
                    {
                        if (m_AttackState.TargetType != TargetType.Radius)
                            TurnToFaceTarget(targetTile);

                        for (int i = 0; i < targets.Length; i++)
                            HandleMagicAttackResults(targets[i], m_AttackState);
                    }
                    break;
                }
            case AttackType.Projectile:
                {
                    goto default;
                }
            default:
                {
                    Debug.LogWarning("Game Piece Attempting To Attack Without Attack Type.");
                    break;
                }
        }

        if(targets.Length > 0 && targets[0] != null)
            m_Animator.Play("CrouchIdle");

        StartCoroutine(DelayedRotationCanvasDisplay());
    }

    void TurnToFaceTarget(GameBoardTile targetTile)
    {
        m_DeltaRotation = Quaternion.LookRotation
            (targetTile.transform.position - transform.position);

        m_DeltaRotation.z = 0;

        transform.rotation = Quaternion.Euler
            (Utility.GetRotationToNearestRightAngle(m_DeltaRotation.eulerAngles));

        GamePieceHandler.RelayPieceSyncToClientManager
            (ControllingPlayerId, ControllingPlayerPosition, this);
    }

    void TurnToFaceTarget(GamePiece targetPiece)
    {
        m_DeltaRotation = Quaternion.LookRotation
        (targetPiece.transform.position - transform.position);

        m_DeltaRotation.z = 0;

        transform.rotation = Quaternion.Euler
            (Utility.GetRotationToNearestRightAngle(m_DeltaRotation.eulerAngles));

        GamePieceHandler.RelayPieceSyncToClientManager
            (ControllingPlayerId, ControllingPlayerPosition, this);
    }

    void HandleMeleeAttackResults(GamePiece target, AttackState attacker)
    {
        TurnToFaceTarget(target);

        if (TestMode)
            target.ProcessDamage(attacker.AttackRating);
        else
            target.ProcessDamage();

        if (attacker.EffectPrefab != null)
            StartCoroutine
                (InstantiateTemporarily(attacker.EffectPrefab, target.transform));
    }

    void HandleMagicAttackResults(GamePiece target, AttackState attacker)
    {
        if (TestMode)
            target.ProcessDamage(attacker.AttackRating);
        else
            target.ProcessDamage();

        if (attacker.EffectPrefab != null)
            StartCoroutine
                (InstantiateTemporarily(attacker.EffectPrefab, target.transform));
    }

    IEnumerator DelayedRotationCanvasDisplay()
    {
        yield return 
            new WaitForSeconds(1.0f);

        ActivateRotationCanvas();
    }

    IEnumerator InstantiateTemporarily(GameObject prefab, Transform transform)
    {
        GameObject obj = Instantiate(prefab, transform.position, prefab.transform.rotation);

        yield return new WaitForSeconds(2.0f);

        Destroy(obj);
    }
}
