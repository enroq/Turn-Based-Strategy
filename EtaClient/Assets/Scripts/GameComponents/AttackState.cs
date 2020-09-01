using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TargetType
{
    Single, //Single Tile Target
    Area,   //Multiple Tiles Around Target Tile
    Radius, //Multiple Tiles Around Game Piece
    Linear, //Line Of Tiles Horizontal From Game Piece
    Allies  //All Game Pieces On Team
}

public enum AttackType
{
    None,
    Melee,
    Projectile,
    Magic
}

[Serializable]
public class AttackState
{
    [SerializeField]
    TargetType m_TargetType;

    [SerializeField]
    AttackType m_AttackType;

    [SerializeField]
    int m_TargetRange;

    [SerializeField]
    int m_AttackRange;

    [SerializeField]
    int m_AttackRating;

    [SerializeField]
    int m_TurnDelay;

    [SerializeField]
    GameObject m_EffectPrefab;

    [SerializeField]
    GameObject m_ProjectilePrefab;

    public TargetType TargetType { get { return m_TargetType; } }
    public AttackType AttackType { get { return m_AttackType; } }

    public int TargetRange { get { return m_TargetRange; } }
    public int AttackRange { get { return m_AttackRange; } }
    public int AttackRating { get { return m_AttackRating; } }
    public int TurnDelay { get { return m_TurnDelay; } }

    public GameObject EffectPrefab { get { return m_EffectPrefab; } }
    public GameObject ProjectilePrefab { get { return m_ProjectilePrefab; } }

    internal int CurrentTurnDelay { get; set; }
}
