using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public const string MainMenuSceneName = "Main";

    public enum UIManagerMode
    {
        MainMenuScene,
        GameplayScene
    }
    public enum UIState
    {
        None,
        MainMenu,
        Settings,
        GameIntro,
        Pause,
        LevelSelect,
        VisionModeSelect,
        Result
    }

    public UIState currentState = UIState.None;
    [Header("Mode")]
    public UIManagerMode mode = UIManagerMode.MainMenuScene;

    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject settingsPanel;
    public GameObject gameIntroPanel;
    public GameObject pausePanel;
    public GameObject levelSelectPanel;
    public GameObject visionModeSelectPanel;
    public GameObject resultPanel;

    [Header("Gameplay HUD")]
    public GameObject gameplayUICanvas;

    [Header("Optional")]
    public string firstLevelSceneName = "Level_Street";

    private UIState previousState;
    private string pendingLevelSceneName;

    private void Start()
    {
        HideAllPanels();

        switch (mode)
        {
            case UIManagerMode.MainMenuScene:
                ShowMainMenu();
                break;

            case UIManagerMode.GameplayScene:
                break;
        }
    }

    public void HideAllPanels()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (gameIntroPanel != null) gameIntroPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (levelSelectPanel != null) levelSelectPanel.SetActive(false);
        if (visionModeSelectPanel != null) visionModeSelectPanel.SetActive(false);
        if (resultPanel != null) resultPanel.SetActive(false);
    }

    public void HideGameplayPanels()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (gameIntroPanel != null) gameIntroPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (levelSelectPanel != null) levelSelectPanel.SetActive(false);
        if (visionModeSelectPanel != null) visionModeSelectPanel.SetActive(false);
    }

    public void ShowMainMenu()
    {
        HideAllPanels();
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);

        currentState = UIState.MainMenu;
    }

    public void ShowSettings()
    {
        previousState = currentState;

        HideAllPanels();
        if (settingsPanel != null) settingsPanel.SetActive(true);

        if (SettingsManager.Instance != null)
            SettingsManager.Instance.RefreshUI();

        currentState = UIState.Settings;
    }

    public void ShowGameIntro()
    {
        previousState = currentState;

        HideAllPanels();
        if (gameIntroPanel != null) gameIntroPanel.SetActive(true);

        currentState = UIState.GameIntro;
    }

    public void ShowPauseMenu()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver())
            return;

        HideGameplayPanels();

        if (pausePanel != null)
            pausePanel.SetActive(true);

        if (SettingsManager.Instance != null)
            SettingsManager.Instance.RefreshUI();

        HideGameplayHUD();
        Time.timeScale = 0f;
        currentState = UIState.Pause;

        if (GameManager.Instance != null)
            GameManager.Instance.UpdateStateUI("Paused");
    }

    public void ShowLevelSelect()
    {
        pendingLevelSceneName = null;
        HideAllPanels();
        if (levelSelectPanel != null)
            levelSelectPanel.SetActive(true);

        currentState = UIState.LevelSelect;
    }

    public void ShowVisionModeSelect(string sceneName)
    {
        pendingLevelSceneName = sceneName;
        HideAllPanels();

        if (visionModeSelectPanel == null)
            visionModeSelectPanel = FindInactiveVisionModeSelectPanel();

        if (visionModeSelectPanel != null)
        {
            visionModeSelectPanel.SetActive(true);
            var panelTransform = visionModeSelectPanel.transform as RectTransform;
            if (panelTransform != null)
                panelTransform.SetAsLastSibling();

            var controller = visionModeSelectPanel.GetComponent<VisionModeSelectController>();
            if (controller != null)
                controller.BeginSelection(sceneName);
            else
                Debug.LogError("VisionModeSelectPanel is missing VisionModeSelectController.");
        }
        else
        {
            Debug.LogError("VisionModeSelectPanel reference is missing, cannot continue to gameplay without choosing a vision mode.");
            ShowLevelSelect();
            return;
        }

        currentState = UIState.VisionModeSelect;
    }

    private GameObject FindInactiveVisionModeSelectPanel()
    {
        VisionModeSelectController[] controllers = Resources.FindObjectsOfTypeAll<VisionModeSelectController>();
        for (int i = 0; i < controllers.Length; i++)
        {
            VisionModeSelectController controller = controllers[i];
            if (controller == null)
                continue;

            GameObject panel = controller.gameObject;
            if (panel == null || !panel.scene.IsValid() || !panel.scene.isLoaded)
                continue;

            if (panel.name == "VisionModeSelectPanel")
                return panel;
        }

        return null;
    }

    public void SelectLevel(string sceneName)
    {
        ShowVisionModeSelect(sceneName);
    }

    public void BackFromVisionModeSelect()
    {
        ShowLevelSelect();
    }

    public void ShowResult()
    {
        HideAllPanels();
        if (resultPanel != null) resultPanel.SetActive(true);

        HideGameplayHUD();
        currentState = UIState.Result;
    }

    public void HideGameplayHUD()
    {
        if (gameplayUICanvas != null)
            gameplayUICanvas.SetActive(false);
    }

    public void ShowGameplayHUD()
    {
        if (gameplayUICanvas != null)
            gameplayUICanvas.SetActive(true);
    }

    public void StartGame()
    {
        SelectLevel(firstLevelSceneName);
    }

    public void LoadLevel(string sceneName)
    {
        Time.timeScale = 1f;
        pendingLevelSceneName = null;
        SceneManager.LoadScene(sceneName);
    }

    public void ReloadCurrentScene()
    {
        Time.timeScale = 1f;
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    public void ReturnToMainMenuScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(MainMenuSceneName);
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game");
        Application.Quit();
    }

    public void BackFromSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        switch (previousState)
        {
            case UIState.MainMenu:
                ShowMainMenu();
                break;

            case UIState.GameIntro:
                ShowGameIntro();
                break;

            case UIState.Pause:
                ShowPauseMenu();
                break;

            case UIState.LevelSelect:
                ShowLevelSelect();
                break;

            case UIState.VisionModeSelect:
                ShowLevelSelect();
                break;

            default:
                HideAllPanels();
                currentState = UIState.None;
                break;
        }
    }
}
