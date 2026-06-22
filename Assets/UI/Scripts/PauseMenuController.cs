using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenuController : MonoBehaviour
{
    [Header("References")]
    public UIManager uiManager;
    public InputActionReference pauseAction;

    [Header("Options")]
    public bool pauseOnEnable = false;

    private bool isPaused = false;
    private bool isActionBound = false;

    private void Start()
    {
        if (pauseOnEnable)
        {
            isPaused = true;
            if (GameManager.Instance != null)
                GameManager.Instance.SetGameplayPaused(true);

            Time.timeScale = 0f;
            if (uiManager != null)
                uiManager.ShowPauseMenu();
        }
        else
        {
            isPaused = false;
            Time.timeScale = 1f;
            if (uiManager != null)
                uiManager.HideGameplayPanels();
        }
    }

    private void OnEnable()
    {
        BindPauseAction();
    }

    private void OnDisable()
    {
        UnbindPauseAction();
    }

    private void BindPauseAction()
    {
        if (pauseAction == null || pauseAction.action == null || isActionBound)
            return;

        pauseAction.action.Enable();
        pauseAction.action.performed += OnPausePressed;
        isActionBound = true;
    }

    private void UnbindPauseAction()
    {
        if (pauseAction == null || pauseAction.action == null || !isActionBound)
            return;

        pauseAction.action.performed -= OnPausePressed;
        pauseAction.action.Disable();
        isActionBound = false;
    }

    private void OnPausePressed(InputAction.CallbackContext context)
    {
        TogglePause();
    }

    public void TogglePause()
    {
        if (uiManager == null) return;

        if (GameManager.Instance != null && GameManager.Instance.IsGameOver())
            return;

        if (uiManager.currentState != UIManager.UIState.None &&
            uiManager.currentState != UIManager.UIState.Pause)
            return;

        isPaused = !isPaused;

        if (isPaused)
        {
            if (GameManager.Instance != null)
                GameManager.Instance.SetGameplayPaused(true);

            Time.timeScale = 0f;
            uiManager.ShowPauseMenu();

            if (GameManager.Instance != null)
                GameManager.Instance.UpdateStateUI("Paused");
        }
        else
        {
            ResumeGame();
        }
    }

    public void ResumeGame()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver())
        {
            isPaused = false;
            GameManager.Instance.RestoreResultScreen();
            return;
        }

        isPaused = false;

        if (GameManager.Instance != null)
            GameManager.Instance.SetGameplayPaused(false);

        Time.timeScale = 1f;

        if (uiManager != null)
        {
            uiManager.HideGameplayPanels();
            uiManager.ShowGameplayHUD();
            uiManager.currentState = UIManager.UIState.None;
        }

        if (GameManager.Instance != null)
            GameManager.Instance.UpdateStateUI("Playing");
    }

    public bool IsPaused()
    {
        return isPaused;
    }
}
