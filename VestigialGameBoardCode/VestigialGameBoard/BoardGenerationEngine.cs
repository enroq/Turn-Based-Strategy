using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardGenerationEngine : MonoBehaviour 
{
    public int m_BoardDiameter = 7;
    public GameObject m_GameCubePrefab;

    private List<Vector2> m_CurrentActiveVectors;
    private BaseGameBoard m_BoardObject;

    private Vector3 m_DeltaVector;
    private Vector3 m_AlphaVector;

    public static Vector2 EmptyVector { get { return new Vector2(0, 0); } }

    void Awake()
    {      
        GenerateGameBoard();
        m_AlphaVector = GetComponent<Transform>().position;      
    }

    void GenerateGameBoard()
    {
        m_BoardObject = new BaseGameBoard(m_AlphaVector);
        PopulateActiveVectors(m_BoardDiameter);

        GameEngine.AddGameBoardToCache(m_BoardObject);
        GenerateComponentRelatives();
    }

    void GenerateComponentRelatives()
    {
        GenerateComponentsFromVector(m_CurrentActiveVectors);
    }

    void PopulateActiveVectors(int l)
    {
        AddActiveVector(0, 0);

        for (int x = 1; x <= l; x++)
        {
            AddActiveVector(0, x); AddActiveVector(x, 0);
            AddActiveVector(0, -x); AddActiveVector(-x, 0);
        }

        for (int i = 1; i <= l; i++)
            for (int j = 1; j <= l; j++)

                if (!IsExcludedVector(i, j, l) && !IsCenterVector(i, j))
                {
                    AddActiveVector(i, j); AddActiveVector(-i, j);
                    AddActiveVector(i, -j); AddActiveVector(-i, -j);
                }
    }

    /// <param name="l">Length Of Board</param>
    bool IsExcludedVector(int x, int y, int l)
    {
        if ((ChallengeAbsoluteValue(x, (l -1)) && ChallengeAbsoluteValue(y, l)
            || (ChallengeAbsoluteValue(y, (l -1)) && ChallengeAbsoluteValue(x, l))))
        {
            return true;
        }

        else return 
            (ChallengeAbsoluteValue(x, l) && ChallengeAbsoluteValue(y, l));
    }

    bool IsCenterVector(int x, int y)
    {
        return (x == 0 && y == 0);
    }

    bool ChallengeAbsoluteValue(int delta, int gamma)
    {
        return (Mathf.Abs(delta) == Mathf.Abs(gamma));
    }

    void AddActiveVector(int x, int y)
    {
        if(m_CurrentActiveVectors == null)
            m_CurrentActiveVectors = new List<Vector2>();
                m_CurrentActiveVectors.Add(new Vector2(x, y));
    }

    void GenerateComponentsFromVector(List<Vector2> vectors)
    {
        foreach (Vector2 vector in vectors)
        {
            MaterializeCube(m_GameCubePrefab, vector);
        }
    }

    void MaterializeCube(GameObject o, Vector2 v)
    {
        var cube = Instantiate(o);
        cube.transform.parent = gameObject.transform;
            m_DeltaVector = new Vector3(v.x, 0, v.y);
                cube.GetComponent<Transform>().localPosition = m_DeltaVector;
                cube.name = string.Format("Cube @[{0}]", v);

        m_BoardObject.AttachBoardObject(cube, v);
    }
}
