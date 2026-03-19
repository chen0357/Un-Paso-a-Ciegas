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

    [Header("UI")]
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
        StartGame();
    }

    public void StartGame()
    {
        gameStarted = true;
        gameFinished = false;
        gameFailed = false;
        startTime = Time.time;
        collisionCount = 0;
        hintCount = 0;
    }

    public void RegisterCollision()
    {
        if (gameFinished || gameFailed) return;
        collisionCount++;
        Debug.Log("Collision Count: " + collisionCount);
    }

    public void RegisterHint()
    {
        if (gameFinished || gameFailed) return;
        hintCount++;
        Debug.Log("Hint Count: " + hintCount);
    }

    public void CompleteGame()
    {
        if (gameFinished || gameFailed) return;

        gameFinished = true;
        finishTime = Time.time - startTime;

        Debug.Log("Game Completed! Time: " + finishTime);

        if (resultUIController != null)
        {
            resultUIController.ShowResult(true, finishTime, collisionCount, hintCount);
        }
    }

    public void FailGame(string reason)
    {
        if (gameFinished || gameFailed) return;

        gameFailed = true;
        finishTime = Time.time - startTime;

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