using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    [SerializeField]
    GameObject m_SettingsPanel;
    [SerializeField]
    GameObject m_ResolutionButtonContainer;
    [SerializeField]
    GameObject m_ResolutionButtonPrefab;
    [SerializeField]
    Toggle m_FullScreenToggle;
    [SerializeField]
    Slider m_QualitySlider;

    List<Resolution> m_AvailableResolutions = new List<Resolution>();
    List<Button> m_CurrentResolutionButtons = new List<Button>();

    Resolution m_CurrentResolution;
    Button m_CurrentlySelectedButton;

    int m_MinResolutionWidth = 640;
    bool m_UseFullScreen = false;

    List<string> m_QualitySettings = new List<string>();

    public void DisplaySettingsPanel()
    {
        if (m_SettingsPanel != null)
            m_SettingsPanel.SetActive(true);
    }

    public void HideSettingsPanel()
    {
        if (m_SettingsPanel != null)
            m_SettingsPanel.SetActive(false);
    }

    public void ModifyQuality()
    {
        try
        {
            QualitySettings.SetQualityLevel
                ((int)m_QualitySlider.value, true);

            Debug.Log("Setting Quality Level: " + m_QualitySlider.value);
            Debug.Log("Setting Quality Name: " + QualitySettings.names[(int)m_QualitySlider.value]);
        }

        catch(Exception e)
        {
            Debug.Log(e.ToString());
        }
    }

    public void ToggleFullScreen()
    {
        Debug.Log("Toggle Fullscreen: " + m_FullScreenToggle.isOn);

        m_UseFullScreen = m_FullScreenToggle.isOn;
        Screen.fullScreen = m_UseFullScreen;
    }

    private void Start()
    {
        InitializeResolutions();
        InitializeQualitySlider();
    }

    internal void InitializeQualitySlider()
    {
        for (int i = 0; i < QualitySettings.names.Length; i++)
        {
            m_QualitySettings.Add(QualitySettings.names[i]);
        }

        foreach(string s in m_QualitySettings)
            Debug.Log("Quality Settings: " + s);

        Debug.Log("Number Of Quality Levels: " + m_QualitySettings.Count);

        m_QualitySlider.maxValue = m_QualitySettings.Count -1;
        m_QualitySlider.value = QualitySettings.GetQualityLevel();
    }

    internal void InitializeResolutions()
    {
        m_FullScreenToggle.isOn = Screen.fullScreen;

        foreach(Resolution res in Screen.resolutions)
        {
            if (!m_AvailableResolutions.Contains(res))
            {
                if(res.width >= m_MinResolutionWidth)
                    m_AvailableResolutions.Add(res);
            }
        }

        foreach (Resolution res in m_AvailableResolutions)
        {
            string resolutionText =
                    res.width +
                    " x " +
                    res.height +
                    " @ " +
                    res.refreshRate +
                    "Mhz";

            Debug.Log(resolutionText);

            GameObject buttonObj = Instantiate(m_ResolutionButtonPrefab);

            Button buttonComponent = buttonObj.GetComponent<Button>();
            buttonComponent.GetComponentInChildren<Text>().text = resolutionText;

            if(Screen.currentResolution.width == res.width
                && Screen.currentResolution.height == res.height
                && Screen.currentResolution.refreshRate == res.refreshRate)
            {
                m_CurrentlySelectedButton = buttonComponent;
                m_CurrentlySelectedButton.interactable = false;
            }

            if (!m_CurrentResolutionButtons.Contains(buttonComponent))
                m_CurrentResolutionButtons.Add(buttonComponent);

            buttonComponent.onClick.AddListener
                (() => OnResolutionButtonClick(res, buttonComponent));

            buttonObj.transform.SetParent
                (m_ResolutionButtonContainer.transform, false);
        }
	}

    void OnResolutionButtonClick(Resolution resolution, Button button)
    {
        if(m_CurrentlySelectedButton != null)
            m_CurrentlySelectedButton.interactable = true;

        button.interactable = false;
        m_CurrentlySelectedButton = button;

        SetResolution(resolution);
    }

    void SetResolution(Resolution resolution)
    {
        m_CurrentResolution = resolution;

        Screen.SetResolution
            (resolution.width, resolution.height, 
                m_UseFullScreen, resolution.refreshRate);
    }
}
