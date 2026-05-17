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

    public void StartGame()
    {
        gameStarted = true;
        gameFinished = false;
        gameFailed = false;

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

    public void RegisterCollision()
    {
        if (IsGameOver()) return;

        collisionCount++;
        Debug.Log("Collision Count: " + collisionCount);
    }

    public void RegisterHint()
    {
        if (IsGameOver()) return;

        hintCount++;
        Debug.Log("Hint Count: " + hintCount);
    }

    public void CompleteGame()
    {
        if (IsGameOver()) return;

        gameFinished = true;
        finishTime = Time.time - startTime;

        Debug.Log("Game Completed! Time: " + finishTime);
        if (hudPanel != null)
        {
            hudPanel.SetActive(false);
        }
        if (resultUIController != null)
        {
            resultUIController.ShowResult(true, finishTime, collisionCount, hintCount);
        }
        else
        {
            Debug.LogError("ResultUIController is not assigned in GameManager!");
        }
    }

    public void FailGame(string reason)
    {
        if (IsGameOver()) return;

        gameFailed = true;
        finishTime = Time.time - startTime;

        UpdateStateUI("Failed");

        Debug.Log("Game Failed: " + reason);

        if (resultUIController != null)
        {
            resultUIController.ShowResult(false, finishTime, collisionCount, hintCount, reason);
        }
    }

    public void RestartScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MenuScene");
    }

    public void LoadNextScene(string sceneName)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }
}