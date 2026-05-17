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
            Time.timeScale = 0f;
            if (uiManager != null)
                uiManager.ShowPauseMenu();
        }
        else
        {
            isPaused = false;
            Time.timeScale = 1f;
            if (uiManager != null)
                uiManager.HideAllPanels();
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

        //  如果在其他UI界面，禁止触发
        if (uiManager.currentState != UIManager.UIState.None &&
            uiManager.currentState != UIManager.UIState.Pause)
        {
            Debug.Log("Pause ignored: 当前在其他UI界面");
            return;
        }

        isPaused = !isPaused;

        if (isPaused)
        {
            Time.timeScale = 0f;
            uiManager.ShowPauseMenu();

            if (GameManager.Instance != null)
            {
                GameManager.Instance.UpdateStateUI("Paused");
            }

            Debug.Log("Game Paused");
        }
        else
        {
            Time.timeScale = 1f;
            uiManager.HideAllPanels();
            uiManager.currentState = UIManager.UIState.None;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.UpdateStateUI("Playing");
            }

            Debug.Log("Game Resumed");
        }
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;

        if (uiManager != null)
        {
            uiManager.HideAllPanels();
            uiManager.currentState = UIManager.UIState.None;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.UpdateStateUI("Playing");
        }

        Debug.Log("ResumeGame called");
    }

    public bool IsPaused()
    {
        return isPaused;
    }
}