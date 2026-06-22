using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Dropdown))]
public class VisionModeDropdownSync : MonoBehaviour
{
    private const string VisionModeKey = "VisionMode";
    private const string LegacyVisualModeKey = "VisualMode";

    private TMP_Dropdown dropdown;

    private void Awake()
    {
        dropdown = GetComponent<TMP_Dropdown>();
    }

    private void OnEnable()
    {
        SyncFromSavedMode();
    }

    public void SyncFromSavedMode()
    {
        if (dropdown == null)
            return;

        int mode = GetSavedVisionMode();
        dropdown.SetValueWithoutNotify(mode);
        dropdown.RefreshShownValue();
    }

    private static int GetSavedVisionMode()
    {
        if (VisionModeManager.Instance != null)
            return (int)VisionModeManager.Instance.currentMode;

        if (PlayerPrefs.HasKey(VisionModeKey))
            return PlayerPrefs.GetInt(VisionModeKey, 0);

        if (PlayerPrefs.HasKey(LegacyVisualModeKey))
            return PlayerPrefs.GetInt(LegacyVisualModeKey, 0);

        return 0;
    }
}
