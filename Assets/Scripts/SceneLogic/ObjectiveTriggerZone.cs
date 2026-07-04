using UnityEngine;

public class ObjectiveTriggerZone : MonoBehaviour
{
    [Header("Task")]
    public string taskId;

    [Header("Optional")]
    [TextArea]
    public string hintMessage;

    private bool triggered;

    private void OnTriggerEnter(Collider other)
    {
        TryCompleteTask(other);
    }

    private void OnTriggerStay(Collider other)
    {
        TryCompleteTask(other);
    }

    private void TryCompleteTask(Collider other)
    {
        if (triggered)
            return;

        if (GameManager.Instance != null && !GameManager.Instance.CanProcessGameplay())
            return;

        if (!IsPlayerCollider(other))
            return;

        if (TaskManager.Instance == null)
            return;

        bool completed = TaskManager.Instance.CompleteTask(taskId);
        if (!completed)
            return;

        triggered = true;

        if (!string.IsNullOrEmpty(hintMessage))
            Debug.Log(hintMessage);

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
