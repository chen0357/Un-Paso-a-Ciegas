using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game State")]
    public bool gameStarted = false;
    public bool gameFinished = false;
    public bool gameFailed = false;

    [Header("Statistics")]
    public float startTime;
    public float finishTime;
    /// <summary>Physical obstacle hits only (DamageObject). Danger zones do not increment this.</summary>
    public int collisionCount = 0;
    public int hintCount = 0;

    [Header("HUD UI")]
    public GameObject hudPanel;

    [Header("Simple UI")]
    public TMP_Text healthText;
    public TMP_Text timeText;
    public TMP_Text stateText;

    [Header("Result UI")]
    public ResultUIController resultUIController;

    private string lastFailReason = "";
    private bool isGameplayPaused;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        if (GetComponent<GameplayFreezeController>() == null)
            gameObject.AddComponent<GameplayFreezeController>();
    }

    private void Start()
    {
        Time.timeScale = 1f;
        StartGame();
    }

    private void Update()
    {
        if (!gameStarted || gameFinished || gameFailed) return;

        float currentTime = Time.time - startTime;

        if (timeText != null)
        {
            timeText.text = "Time: " + currentTime.ToString("F1") + "s";
        }
    }

    public bool IsPaused()
    {
        return isGameplayPaused;
    }

    public bool IsGameplayFrozen()
    {
        return IsGameOver() || isGameplayPaused;
    }

    public bool CanProcessGameplay()
    {
        return gameStarted && !IsGameplayFrozen();
    }

    public void SetGameplayPaused(bool paused)
    {
        isGameplayPaused = paused;
    }

    public void StartGame()
    {
        gameStarted = true;
        gameFinished = false;
        gameFailed = false;
        isGameplayPaused = false;

        startTime = Time.time;
        finishTime = 0f;
        collisionCount = 0;
        hintCount = 0;

        UpdateStateUI("Playing");
    }

    public bool IsGameOver()
    {
        return gameFinished || gameFailed;
    }

    public void UpdateHealthUI(int currentHealth, int maxHealth)
    {
        if (healthText != null)
        {
            healthText.text = "Health: " + currentHealth + " / " + maxHealth;
        }
    }

    public void UpdateStateUI(string state)
    {
        if (stateText != null)
        {
            stateText.text = "State: " + state;
        }
    }

    /// <summary>Call from physical obstacles (DamageObject) only — not from DangerZone.</summary>
    public void RegisterCollision()
    {
        if (!CanProcessGameplay()) return;

        collisionCount++;
    }

    public void RegisterHint()
    {
        if (!CanProcessGameplay()) return;

        hintCount++;
    }

    public void CompleteGame()
    {
        if (IsGameOver()) return;

        gameFinished = true;
        finishTime = Time.time - startTime;

        PresentResult(true);
    }

    public void FailGame(string reason)
    {
        if (IsGameOver()) return;

        gameFailed = true;
        finishTime = Time.time - startTime;
        lastFailReason = reason;

        PresentResult(false, lastFailReason);
    }

    public void PresentResult(bool success, string failReason = "")
    {
        if (!success)
            lastFailReason = failReason;

        isGameplayPaused = false;

        UIManager uiManager = FindFirstObjectByType<UIManager>();
        if (uiManager != null)
            uiManager.HideGameplayPanels();

        if (hudPanel != null)
            hudPanel.SetActive(false);

        if (resultUIController != null)
            resultUIController.ShowResult(success, finishTime, collisionCount, hintCount, failReason);
        else
            Debug.LogError("ResultUIController is not assigned in GameManager!");

        if (uiManager != null)
            uiManager.currentState = UIManager.UIState.Result;

        Time.timeScale = 0f;
    }

    public void RestoreResultScreen()
    {
        if (!IsGameOver()) return;

        PresentResult(gameFinished, lastFailReason);
    }

    public void RestartScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ReturnToMainMenu()
    {
        UIManager uiManager = FindFirstObjectByType<UIManager>();
        if (uiManager != null)
        {
            uiManager.ReturnToMainMenuScene();
            return;
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(UIManager.MainMenuSceneName);
    }

    public void LoadNextScene(string sceneName)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }
}