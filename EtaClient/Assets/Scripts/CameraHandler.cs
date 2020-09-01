using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraHandler : MonoBehaviour
{
    [SerializeField]
    Camera m_TargetCamera;

    [SerializeField]
    List<Camera> m_PlayerCameras;

    public static List<Camera> PlayerCameras = new List<Camera>();

    public static Camera CurrentCamera { get; set; }

	void Awake()
    {
        CurrentCamera = m_TargetCamera;

        for (int i = 0; i < m_PlayerCameras.Count; i++)
        {
            if (i > 0)
                m_PlayerCameras[i].gameObject.SetActive(false);

            PlayerCameras.Add(m_PlayerCameras[i]);
        }
	}

    public static void ActivateCamera(int playerPosition)
    {
        for(int i = 0; i < PlayerCameras.Count; i++)
        {
            if (i == playerPosition - 1)
            {
                PlayerCameras[i].gameObject.SetActive(true);
                CurrentCamera = PlayerCameras[i];
            }

            else
                PlayerCameras[i].gameObject.SetActive(false);
        }
    }
}
