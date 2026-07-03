using UnityEngine;
using UnityEngine.SceneManagement;

public class VisionModeManager : MonoBehaviour
{
    public static VisionModeManager Instance;

    public enum VisionMode
    {
        NearlyBlind = 0,
        Blurry = 1,
        VisualDisability = 2
    }

    public VisionMode currentMode = VisionMode.Blurry;

    private GameObject blurryOverlay;
    private GameObject nearlyBlindOverlay;
    private GameObject visualDisabilityOverlay;

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

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        DisableAllOverlays();
    }

    public void RegisterSceneOverlays(GameObject blurry, GameObject nearlyBlind, GameObject visualDisability)
    {
        blurryOverlay = blurry;
        nearlyBlindOverlay = nearlyBlind;
        visualDisabilityOverlay = visualDisability;
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
        currentMode = (VisionMode)PlayerPrefs.GetInt(VisionModeKey, (int)VisionMode.Blurry);
    }

    public void ApplyVisionMode()
    {
        if (IsMenuScene(SceneManager.GetActiveScene().name))
        {
            DisableAllOverlays();
            return;
        }

        if (blurryOverlay != null)
            blurryOverlay.SetActive(currentMode == VisionMode.Blurry);

        if (nearlyBlindOverlay != null)
            nearlyBlindOverlay.SetActive(currentMode == VisionMode.NearlyBlind);

        if (visualDisabilityOverlay != null)
            visualDisabilityOverlay.SetActive(currentMode == VisionMode.VisualDisability);
    }

    public void DisableAllOverlays()
    {
        if (blurryOverlay != null)
            blurryOverlay.SetActive(false);

        if (nearlyBlindOverlay != null)
            nearlyBlindOverlay.SetActive(false);

        if (visualDisabilityOverlay != null)
            visualDisabilityOverlay.SetActive(false);
    }

    private static bool IsMenuScene(string sceneName)
    {
        return sceneName == UIManager.MainMenuSceneName;
    }
}