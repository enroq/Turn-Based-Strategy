using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#pragma warning disable
public class ScreenSizeFitter : MonoBehaviour
{
    [SerializeField]
    private RectTransform m_TargetRect;

    float m_RectLeft, m_RectTop, m_RectRight, m_RectBottom;

    Vector2 m_DeltaVector = Vector2.zero;

    float m_RatioCache = 0f;
    float m_TargetRatio = (float)16 / (float)9;

    float[] m_ValueCache = new float[4];

	void Start ()
    {
        InitializeStartingValues();
	}

    void InitializeStartingValues()
    {
        m_RectLeft = m_TargetRect.offsetMax.x;
        m_RectTop = m_TargetRect.offsetMax.y;

        m_RectRight = m_TargetRect.offsetMin.x;
        m_RectBottom = m_TargetRect.offsetMin.y;
    }

    void SetRectValues(float[] values)
    {
        m_DeltaVector.x = values[0];
        m_DeltaVector.y = values[1];

        m_TargetRect.offsetMax = m_DeltaVector;

        m_DeltaVector.x = values[2];
        m_DeltaVector.y = values[3];

        m_TargetRect.offsetMin = m_DeltaVector;
    }

    float[] GetModifiedValues(float ratio)
    {
        m_ValueCache[0] = m_RectLeft * ratio;
        m_ValueCache[1] = m_RectTop * ratio;
        m_ValueCache[2] = m_RectRight * ratio;
        m_ValueCache[3] = m_RectBottom * ratio;

        return 
            m_ValueCache;
    }

    float GetCurrentScreenRatio()
    {
        return (float)Screen.width / (float)Screen.height;
    }

	void Update ()
    {
        float tempRatio = GetCurrentScreenRatio();
        if(m_RatioCache != tempRatio)
        {
            m_RatioCache = tempRatio;
            float r = (float)m_TargetRatio / (float)tempRatio;
            SetRectValues(GetModifiedValues(r));
        }
	}
}
