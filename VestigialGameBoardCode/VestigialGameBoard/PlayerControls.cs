using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerControls : MonoBehaviour 
{
    public Camera m_CurrentCamera;
    private bool m_AwaitingMouseRelease = false;

    private const int m_LeftMouseIndex = 0;

    void Update()
    {
        if (Input.GetMouseButton(m_LeftMouseIndex) && !m_AwaitingMouseRelease)
        {
            OnClick();
            m_AwaitingMouseRelease = true;
        }

        else if (Input.GetMouseButtonUp(m_LeftMouseIndex))
        {
            m_AwaitingMouseRelease = false;
        }
    }

    RaycastHit rayhit;
    GameObject objectHit;
    BoardComponentBehaviour componentBehaviour;

    private void OnClick()
    {
        if (EventSystem.current.IsPointerOverGameObject())
            return;

        if (Physics.Raycast
            (m_CurrentCamera.ScreenPointToRay(Input.mousePosition), out rayhit))
        {
            objectHit = rayhit.collider.gameObject;
            if (objectHit != null)
            {
                componentBehaviour = objectHit.GetComponentInParent<BoardComponentBehaviour>();
                if (componentBehaviour != null)
                {
                    componentBehaviour.ToggleSelection(true);
                    return;
                }
            }
        }
    }
}
