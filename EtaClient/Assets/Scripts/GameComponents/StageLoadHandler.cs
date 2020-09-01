using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class StageLoadHandler : MonoBehaviour
{
    static StageState m_CurrentStageState = new StageState();
    static List<GamePieceBoardState> m_LoadedGamePieceStates;

    static string m_StageFileName = "stage.bin";
    static string m_StageFilePath;

    public static StageState CurrentStageState { get { return m_CurrentStageState; } }
    public static List<GamePieceBoardState> 
        LoadedGamePieceStates { get { return m_LoadedGamePieceStates; } }

    private void Awake()
    {
        m_StageFilePath = Path.Combine
            (Application.persistentDataPath, m_StageFileName);

        Debug.Log("Stage File Path: " + m_StageFilePath);
    }

    private void Start()
    {
		if(File.Exists(m_StageFilePath))
        {
            FileStream file = File.OpenRead(m_StageFilePath);
            BinaryFormatter formatter = new BinaryFormatter();

            m_CurrentStageState = (StageState)formatter.Deserialize(file);

            file.Close();

            TranslateStageStateToPieces();
        }
	}

    static void TranslateStageStateToPieces()
    {
        if(m_CurrentStageState.StageObjects.Count > 0)
            m_LoadedGamePieceStates = new List<GamePieceBoardState>();

        foreach(StageObject stageObject in m_CurrentStageState.StageObjects)
        {
            if (GamePieceHandler.GamePieceDictionary.ContainsKey(stageObject.PieceName))
            {
                GameObject obj = GamePieceHandler.GamePieceDictionary[stageObject.PieceName];

                GamePiece piece = obj.GetComponent<GamePiece>();

                m_LoadedGamePieceStates.Add(new GamePieceBoardState
                    (obj, new Vector2(stageObject.PositionX, stageObject.PositionY)));
            }
        }
    }

    public static void UpdateCurrentStageState(List<GameObject> pieces)
    {
        m_CurrentStageState.UpdateGamePieces(pieces);
        TranslateStageStateToPieces();

        SaveCurrentState();
    }

    static void SaveCurrentState()
    {
        FileStream file;

        if (File.Exists(m_StageFilePath))
            file = File.OpenWrite(m_StageFilePath);
        else
            file = File.Create(m_StageFilePath);

        BinaryFormatter formatter = new BinaryFormatter();
        formatter.Serialize(file, m_CurrentStageState);

        file.Close();
    }
}

[Serializable]
public class StageState
{
    private List<StageObject> m_StageObjects = null;
    internal List<StageObject> StageObjects { get { return m_StageObjects; } }

    internal void UpdateGamePieces(List<GameObject> pieces)
    {
        m_StageObjects = new List<StageObject>();
        foreach(GameObject obj in pieces)
        {
            GamePiece piece = obj.GetComponent<GamePiece>();
            m_StageObjects.Add
                (new StageObject(piece.GamePieceName, piece.StartingVector));
        }
    }
}

[Serializable]
public class StageObject
{
    string m_PieceName;

    float m_PositionX;
    float m_PositionY;

    public string PieceName { get { return m_PieceName; } }
    public float PositionX { get { return m_PositionX; } }
    public float PositionY { get { return m_PositionY; } }

    public StageObject(string pieceName, Vector2 position)
    {
        m_PieceName = pieceName;
        m_PositionX = position.x;
        m_PositionY = position.y;
    }
}
