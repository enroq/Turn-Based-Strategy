using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoardComponentBehaviour : MonoBehaviour
{
    public Material m_InactiveMaterial;
    public Material m_ActiveMaterial;

    public GameObject m_VectorDisplayGUI;

    private GameBoardComponent m_ComponentRelative;

    void Start()
    {
        UpdateOverheadDisplay();
    }

    internal void ToggleSelection(bool singular)
    {
        m_ComponentRelative.ToggleObjectSelection(singular);
    }

    void UpdateOverheadDisplay()
    {
        var textComponent = m_VectorDisplayGUI.GetComponentInChildren<Text>();
        if (textComponent != null)
        {
            try
            {
                textComponent.text = string.Format
                    ("[{0},{1}]", m_ComponentRelative.Vector.x, m_ComponentRelative.Vector.y);
            }

            catch (Exception e)
                { Debug.Log(e.ToString()); gameObject.SetActive(false); }
        }
    }

    public void SetActiveMaterial()
    {
        Material[] mats = GetComponent<Renderer>().sharedMaterials;
        mats[0] = m_ActiveMaterial;
        GetComponent<Renderer>().sharedMaterials = mats;
    }

    public void SetInactiveMaterial()
    {
        Material[] mats = GetComponent<Renderer>().sharedMaterials;
        mats[0] = m_InactiveMaterial;
        GetComponent<Renderer>().sharedMaterials = mats;
    }

    internal void SetComponent(GameBoardComponent component)
    {
        m_ComponentRelative = component;
    }

    internal void ToggleOverheadDisplay()
    {
        int count = transform.childCount;
        for (int i = 0; i < count; i++)
        {
            var obj = transform.GetChild(i).gameObject;
            if (obj.name == "OverheadCanvas")
            {
                bool active = obj.activeInHierarchy;
                obj.SetActive(!active);
                break;
            }
        }
    }
}