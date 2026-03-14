using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance;

    [Header("UI References")]
    public Dropdown visualModeDropdown;
    public Toggle voiceHintToggle;
    public Slider uiVolumeSlider;
    public Toggle hapticToggle;
    public Slider hapticStrengthSlider;

    [Header("Current Settings")]
    public int visualMode; // 0=正常, 1=低视力, 2=无视觉
    public bool voiceHintsEnabled;
    public float uiVolume;
    public bool hapticsEnabled;
    public float hapticStrength;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadSettings();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        RefreshUI();
        ApplySettings();
    }

    public void RefreshUI()
    {
        if (visualModeDropdown != null)
            visualModeDropdown.value = visualMode;

        if (voiceHintToggle != null)
            voiceHintToggle.isOn = voiceHintsEnabled;

        if (uiVolumeSlider != null)
            uiVolumeSlider.value = uiVolume;

        if (hapticToggle != null)
            hapticToggle.isOn = hapticsEnabled;

        if (hapticStrengthSlider != null)
            hapticStrengthSlider.value = hapticStrength;
    }

    public void OnVisualModeChanged(int value)
    {
        visualMode = value;
        SaveSettings();
        ApplySettings();
    }

    public void OnVoiceHintChanged(bool value)
    {
        voiceHintsEnabled = value;
        SaveSettings();
    }

    public void OnUIVolumeChanged(float value)
    {
        uiVolume = value;
        AudioListener.volume = uiVolume;
        SaveSettings();
    }

    public void OnHapticChanged(bool value)
    {
        hapticsEnabled = value;
        SaveSettings();
    }

    public void OnHapticStrengthChanged(float value)
    {
        hapticStrength = value;
        SaveSettings();
    }

    public void SaveSettings()
    {
        PlayerPrefs.SetInt("VisualMode", visualMode);
        PlayerPrefs.SetInt("VoiceHintsEnabled", voiceHintsEnabled ? 1 : 0);
        PlayerPrefs.SetFloat("UIVolume", uiVolume);
        PlayerPrefs.SetInt("HapticsEnabled", hapticsEnabled ? 1 : 0);
        PlayerPrefs.SetFloat("HapticStrength", hapticStrength);
        PlayerPrefs.Save();
    }

    public void LoadSettings()
    {
        visualMode = PlayerPrefs.GetInt("VisualMode", 0);
        voiceHintsEnabled = PlayerPrefs.GetInt("VoiceHintsEnabled", 1) == 1;
        uiVolume = PlayerPrefs.GetFloat("UIVolume", 1f);
        hapticsEnabled = PlayerPrefs.GetInt("HapticsEnabled", 1) == 1;
        hapticStrength = PlayerPrefs.GetFloat("HapticStrength", 0.5f);
    }

    public void ApplySettings()
    {
        AudioListener.volume = uiVolume;

        Debug.Log("Apply Settings:");
        Debug.Log("Visual Mode: " + visualMode);
        Debug.Log("Voice Hints: " + voiceHintsEnabled);
        Debug.Log("UI Volume: " + uiVolume);
        Debug.Log("Haptics: " + hapticsEnabled);
        Debug.Log("Haptic Strength: " + hapticStrength);

        switch (visualMode)
        {
            case 0:
                Debug.Log("正常模式");
                break;
            case 1:
                Debug.Log("低视力模式");
                break;
            case 2:
                Debug.Log("无视觉模式");
                break;
        }
    }
}