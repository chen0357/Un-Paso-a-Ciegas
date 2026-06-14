using UnityEngine;

public class GoalTrigger : MonoBehaviour
{
    [Header("Optional Task")]
    public string completeTaskId = "task_3";

    [Header("Level End")]
    public bool finishLevelOnTrigger = true;
    public bool requireAllTasksCompleted = false;

    private bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;

        if (GameManager.Instance != null && !GameManager.Instance.CanProcessGameplay())
            return;

        if (!other.CompareTag("Player"))
            return;

        if (requireAllTasksCompleted &&
            TaskManager.Instance != null &&
            !TaskManager.Instance.AreAllTasksCompleted())
            return;

        triggered = true;

        if (TaskManager.Instance != null && !string.IsNullOrEmpty(completeTaskId))
            TaskManager.Instance.CompleteTask(completeTaskId);

        if (finishLevelOnTrigger && GameManager.Instance != null)
            GameManager.Instance.CompleteGame();
    }
}