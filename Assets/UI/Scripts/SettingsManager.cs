using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance;

    [Header("UI References")]
    public TMP_Dropdown visualModeDropdown;
    public Toggle voiceHintToggle;
    public Slider uiVolumeSlider;
    public Toggle hapticToggle;
    public Slider hapticStrengthSlider;

    [Header("Audio Mixer")]
    public AudioMixer mainAudioMixer;

    [Header("Audio Mixer Parameter Names")]
    public string masterVolumeParameter = "Master";

    [Header("Current Settings")]
    public int visualMode;
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
            ApplySettings();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void BindUI(
        TMP_Dropdown visualDropdown,
        Toggle voiceToggle,
        Slider volumeSlider,
        Toggle hapticToggleUI,
        Slider hapticSlider)
    {
        visualModeDropdown = visualDropdown;
        voiceHintToggle = voiceToggle;
        uiVolumeSlider = volumeSlider;
        hapticToggle = hapticToggleUI;
        hapticStrengthSlider = hapticSlider;

        RefreshUI();
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
        SaveSettings();
        ApplySettings();
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
        SetMixerVolume(masterVolumeParameter, uiVolume);

        Debug.Log($"Apply Settings | VisualMode={visualMode}, MasterVolume={uiVolume}, Haptics={hapticsEnabled}, HapticStrength={hapticStrength}");
    }

    private void SetMixerVolume(string parameterName, float sliderValue)
    {
        if (mainAudioMixer == null)
        {
            Debug.LogWarning("MainAudioMixer is not assigned in SettingsManager.");
            return;
        }

        float clampedValue = Mathf.Clamp(sliderValue, 0.0001f, 1f);
        float decibelValue = Mathf.Log10(clampedValue) * 20f;

        bool success = mainAudioMixer.SetFloat(parameterName, decibelValue);

        if (!success)
        {
            Debug.LogWarning($"AudioMixer parameter not found: {parameterName}");
        }
    }
}