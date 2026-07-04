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
        TryCompleteGoal(other);
    }

    private void OnTriggerStay(Collider other)
    {
        TryCompleteGoal(other);
    }

    private void TryCompleteGoal(Collider other)
    {
        if (triggered)
            return;

        if (GameManager.Instance != null && !GameManager.Instance.CanProcessGameplay())
            return;

        if (!IsPlayerCollider(other))
            return;

        if (requireAllTasksCompleted &&
            TaskManager.Instance != null &&
            !TaskManager.Instance.AreAllTasksCompleted())
            return;

        bool completedTask = true;
        if (TaskManager.Instance != null && !string.IsNullOrEmpty(completeTaskId))
            completedTask = TaskManager.Instance.CompleteTask(completeTaskId);

        if (!completedTask)
            return;

        triggered = true;

        if (finishLevelOnTrigger && GameManager.Instance != null)
            GameManager.Instance.CompleteGame();

        ObjectiveZoneVisual visual = GetComponent<ObjectiveZoneVisual>();
        if (visual != null)
            visual.Hide();
    }

    private static bool IsPlayerCollider(Collider other)
    {
        if (other == null)
            return false;

        if (other.CompareTag("Player"))
            return true;

        if (other.attachedRigidbody != null && other.attachedRigidbody.CompareTag("Player"))
            return true;

        Transform parent = other.transform.parent;
        return parent != null && parent.CompareTag("Player");
    }
}