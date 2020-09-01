using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

#pragma warning disable
public class MouseInputHandler : MonoBehaviour
{
    [SerializeField]
    float m_ClickDecayTime = 0.65f;

    int m_LeftMouseButtonIndex = 0;
    float m_MaxRayDistance = 1000f;

    Ray m_Ray;

    internal MouseClickEventArgs 
        m_MouseClickEventArgs = new MouseClickEventArgs();

    bool m_FirstClickQueued;
    Coroutine m_DoubleClickDecayRoutine;

    Transform m_TransformCache;

    /// <remarks>
    /// Note that the transform cache is used to store the previous object 
    /// clicked so that clicking another object is not blocked by decay.
    /// </remarks>
    void Update ()
    {
        if(Input.GetMouseButtonDown(m_LeftMouseButtonIndex) && !EventSystem.current.IsPointerOverGameObject())
        {
            m_Ray = CameraHandler.CurrentCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit[] rayHits = GetNonAllocRaycastsTrimmedInOrder(m_Ray, m_MaxRayDistance);

            for (int i = 0; i < rayHits.Length; i++)
            {
                if (rayHits[i].transform != null)
                {
                    UpdateArgs(rayHits[i], m_LeftMouseButtonIndex);

                    if (IsDoubleClick(rayHits[i]))
                        EventSink.InvokeDoubleClickEvent(m_MouseClickEventArgs);

                    else
                        EventSink.InvokeSingleClickEvent(m_MouseClickEventArgs);

                    m_TransformCache = rayHits[i].transform;                    
                }   break;            
            }       BeginClickDecay();
        }
	}

    bool IsDoubleClick(RaycastHit hit)
    {
        return m_FirstClickQueued && hit.transform == m_TransformCache;
    }

    void UpdateArgs(RaycastHit hit, int buttonIndex)
    {
        m_MouseClickEventArgs.UpdateMouseClickEventArgs(hit.transform, buttonIndex);
    }

    void BeginClickDecay()
    {
        m_FirstClickQueued = true;

        if(m_DoubleClickDecayRoutine != null)
            StopCoroutine(m_DoubleClickDecayRoutine);

        m_DoubleClickDecayRoutine = StartCoroutine(ProcessClickDecay());
    }

    IEnumerator ProcessClickDecay()
    {
        yield return new 
            WaitForSecondsRealtime(m_ClickDecayTime);

        m_FirstClickQueued = false;
    }

    RaycastHit[] GetNonAllocRaycastsTrimmedInOrder(Ray ray, float maxDistance, int maxHits = 32)
    {
        RaycastHit[] rayHits = new RaycastHit[maxHits];
        Physics.RaycastNonAlloc(ray, rayHits, maxDistance);

        rayHits = rayHits.
            Where(hit => hit.transform != null).ToArray();

        rayHits = rayHits.
            OrderBy(hit => hit.distance).ToArray();

        return rayHits;
    }
}
