using UnityEngine;

public class VisionModeManager : MonoBehaviour
{
    public static VisionModeManager Instance;

    public enum VisionMode
    {
        Normal = 0,
        Blurry = 1,
        Blind = 2
    }

    public VisionMode currentMode = VisionMode.Normal;

    private GameObject blurryOverlay;
    private GameObject blindOverlay;

    private const string VisionModeKey = "VisionMode";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadVisionMode();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void RegisterSceneOverlays(GameObject blurry, GameObject blind)
    {
        blurryOverlay = blurry;
        blindOverlay = blind;
        ApplyVisionMode();
    }

    public void SetVisionModeFromDropdown(int value)
    {
        SetVisionMode((VisionMode)value);
    }

    public void SetVisionMode(VisionMode mode)
    {
        currentMode = mode;
        PlayerPrefs.SetInt(VisionModeKey, (int)currentMode);
        PlayerPrefs.Save();
        ApplyVisionMode();
    }

    private void LoadVisionMode()
    {
        currentMode = (VisionMode)PlayerPrefs.GetInt(VisionModeKey, 0);
    }

    public void ApplyVisionMode()
    {
        if (blurryOverlay != null)
            blurryOverlay.SetActive(currentMode == VisionMode.Blurry);

        if (blindOverlay != null)
            blindOverlay.SetActive(currentMode == VisionMode.Blind);
    }
}