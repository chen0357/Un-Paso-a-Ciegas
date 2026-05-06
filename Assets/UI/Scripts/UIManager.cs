using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
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
        Pause,
        LevelSelect,
        Result
    }

    public UIState currentState = UIState.None;
    [Header("Mode")]
    public UIManagerMode mode = UIManagerMode.MainMenuScene;

    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject settingsPanel;
    public GameObject pausePanel;
    public GameObject levelSelectPanel;
    public GameObject resultPanel;

    [Header("Optional")]
    public string firstLevelSceneName = "Level_Street";
    private UIState previousState;
    private void Start()
    {
        HideAllPanels();

        switch (mode)
        {
            case UIManagerMode.MainMenuScene:
                ShowMainMenu();
                break;

            case UIManagerMode.GameplayScene:
                // 游戏场景默认不显示任何面板
                break;
        }
    }

    public void HideAllPanels()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (levelSelectPanel != null) levelSelectPanel.SetActive(false);
        if (resultPanel != null) resultPanel.SetActive(false);
    }

    public void ShowMainMenu()
    {
        HideAllPanels();
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);

        currentState = UIState.MainMenu;
    }

    public void ShowSettings()
    {
        previousState = currentState; // 记录来源

        HideAllPanels();
        if (settingsPanel != null) settingsPanel.SetActive(true);

        currentState = UIState.Settings;
    }

    public void ShowPauseMenu()
    {
        HideAllPanels();
        if (pausePanel != null) pausePanel.SetActive(true);

        currentState = UIState.Pause;
    }

    public void ShowLevelSelect()
    {
        HideAllPanels();
        if (levelSelectPanel != null) levelSelectPanel.SetActive(true);

        currentState = UIState.LevelSelect;
    }

    public void ShowResult()
    {
        HideAllPanels();
        if (resultPanel != null) resultPanel.SetActive(true);

        currentState = UIState.Result;
    }

    public void StartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(firstLevelSceneName);
    }

    public void LoadLevel(string sceneName)
    {
        Time.timeScale = 1f;
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
        SceneManager.LoadScene("Main");
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

            case UIState.Pause:
                ShowPauseMenu();
                break;

            case UIState.LevelSelect:
                ShowLevelSelect();
                break;

            default:
                HideAllPanels();
                currentState = UIState.None;
                break;
        }
    }
}