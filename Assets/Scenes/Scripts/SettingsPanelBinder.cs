using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsPanelBinder : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Dropdown visualModeDropdown;
    public Toggle voiceHintToggle;
    public Slider uiVolumeSlider;
    public Toggle hapticToggle;
    public Slider hapticStrengthSlider;

    private void Start()
    {
        if (SettingsManager.Instance == null)
        {
            Debug.LogWarning("SettingsPanelBinder: SettingsManager.Instance not found.");
            return;
        }

        // 绑定UI到SettingsManager
        SettingsManager.Instance.BindUI(
            visualModeDropdown,
            voiceHintToggle,
            uiVolumeSlider,
            hapticToggle,
            hapticStrengthSlider
        );

        // 绑定 UI 事件
        BindEvents();
    }

    private void BindEvents()
    {
        if (visualModeDropdown != null)
        {
            visualModeDropdown.onValueChanged.RemoveAllListeners();
            visualModeDropdown.onValueChanged.AddListener(SettingsManager.Instance.OnVisualModeChanged);
        }

        if (voiceHintToggle != null)
        {
            voiceHintToggle.onValueChanged.RemoveAllListeners();
            voiceHintToggle.onValueChanged.AddListener(SettingsManager.Instance.OnVoiceHintChanged);
        }

        if (uiVolumeSlider != null)
        {
            uiVolumeSlider.onValueChanged.RemoveAllListeners();
            uiVolumeSlider.onValueChanged.AddListener(SettingsManager.Instance.OnUIVolumeChanged);
        }

        if (hapticToggle != null)
        {
            hapticToggle.onValueChanged.RemoveAllListeners();
            hapticToggle.onValueChanged.AddListener(SettingsManager.Instance.OnHapticChanged);
        }

        if (hapticStrengthSlider != null)
        {
            hapticStrengthSlider.onValueChanged.RemoveAllListeners();
            hapticStrengthSlider.onValueChanged.AddListener(SettingsManager.Instance.OnHapticStrengthChanged);
        }
    }
}