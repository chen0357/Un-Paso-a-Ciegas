using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsPanelBinder : MonoBehaviour
{
    [Header("UI References")]
    public GameObject visualModeSection;
    public TMP_Dropdown visualModeDropdown;
    public Toggle voiceHintToggle;
    public Slider uiVolumeSlider;
    public Toggle hapticToggle;
    public Slider hapticStrengthSlider;

    private bool eventsBound;

    private void Awake()
    {
        if (visualModeSection == null)
        {
            var sectionTransform = transform.Find("Configuración de simulación visual");
            if (sectionTransform != null)
                visualModeSection = sectionTransform.gameObject;
        }

        if (visualModeSection != null)
            visualModeSection.SetActive(false);
    }

    private void OnEnable()
    {
        if (SettingsManager.Instance == null)
        {
            Debug.LogWarning("SettingsPanelBinder: SettingsManager.Instance not found.");
            return;
        }

        SettingsManager.Instance.BindUI(
            voiceHintToggle,
            uiVolumeSlider,
            hapticToggle,
            hapticStrengthSlider
        );

        EnsureEventsBound();
    }

    private void EnsureEventsBound()
    {
        if (eventsBound)
            return;

        if (voiceHintToggle != null)
        {
            voiceHintToggle.onValueChanged.RemoveListener(SettingsManager.Instance.OnVoiceHintChanged);
            voiceHintToggle.onValueChanged.AddListener(SettingsManager.Instance.OnVoiceHintChanged);
        }

        if (uiVolumeSlider != null)
        {
            uiVolumeSlider.onValueChanged.RemoveListener(SettingsManager.Instance.OnUIVolumeChanged);
            uiVolumeSlider.onValueChanged.AddListener(SettingsManager.Instance.OnUIVolumeChanged);
        }

        if (hapticToggle != null)
        {
            hapticToggle.onValueChanged.RemoveListener(SettingsManager.Instance.OnHapticChanged);
            hapticToggle.onValueChanged.AddListener(SettingsManager.Instance.OnHapticChanged);
        }

        if (hapticStrengthSlider != null)
        {
            hapticStrengthSlider.onValueChanged.RemoveListener(SettingsManager.Instance.OnHapticStrengthChanged);
            hapticStrengthSlider.onValueChanged.AddListener(SettingsManager.Instance.OnHapticStrengthChanged);
        }

        eventsBound = true;
    }

    private void OnDisable()
    {
        eventsBound = false;
    }
}
