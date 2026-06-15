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
        if (triggered)
            return;

        if (GameManager.Instance != null && !GameManager.Instance.CanProcessGameplay())
            return;

        if (!other.CompareTag("Player"))
            return;

        triggered = true;

        if (TaskManager.Instance != null)
            TaskManager.Instance.CompleteTask(taskId);

        if (!string.IsNullOrEmpty(hintMessage))
            Debug.Log(hintMessage);

        ObjectiveZoneVisual visual = GetComponent<ObjectiveZoneVisual>();
        if (visual != null)
            visual.Hide();
    }
}
