using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TaskChecklistUI : MonoBehaviour
{
    [Header("Layout")]
    public RectTransform taskListRoot;
    public TMP_Text rowTemplate;

    [Header("Style")]
    public string pendingMarker = "○";
    public string completedMarker = "✓";
    public Color pendingColor = Color.white;
    public Color completedColor = new Color(0.55f, 1f, 0.55f);

    private readonly Dictionary<string, TMP_Text> rowsByTaskId = new Dictionary<string, TMP_Text>();

    private void Start()
    {
        BuildChecklist();

        if (TaskManager.Instance != null)
            TaskManager.Instance.OnTaskCompleted += HandleTaskCompleted;
    }

    private void OnDestroy()
    {
        if (TaskManager.Instance != null)
            TaskManager.Instance.OnTaskCompleted -= HandleTaskCompleted;
    }

    private void BuildChecklist()
    {
        if (TaskManager.Instance == null || taskListRoot == null)
            return;

        ClearGeneratedRows();

        IReadOnlyList<TaskDefinition> tasks = TaskManager.Instance.GetTasks();
        for (int i = 0; i < tasks.Count; i++)
        {
            TaskDefinition task = tasks[i];
            if (task == null || string.IsNullOrEmpty(task.id))
                continue;

            TMP_Text row = CreateRow(i);
            rowsByTaskId[task.id] = row;
            RefreshRow(task.id);
        }
    }

    private TMP_Text CreateRow(int index)
    {
        if (rowTemplate != null)
        {
            TMP_Text row = Instantiate(rowTemplate, taskListRoot);
            row.gameObject.SetActive(true);
            row.name = $"TaskRow_{index + 1}";
            return row;
        }

        GameObject rowObject = new GameObject(
            $"TaskRow_{index + 1}",
            typeof(RectTransform),
            typeof(TextMeshProUGUI));

        rowObject.transform.SetParent(taskListRoot, false);

        RectTransform rect = rowObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.sizeDelta = new Vector2(0f, 36f);
        rect.anchoredPosition = new Vector2(0f, -index * 40f);

        TMP_Text text = rowObject.GetComponent<TextMeshProUGUI>();
        text.fontSize = 24;
        text.alignment = TextAlignmentOptions.MidlineLeft;
        text.color = pendingColor;
        text.raycastTarget = false;
        return text;
    }

    private void ClearGeneratedRows()
    {
        rowsByTaskId.Clear();

        for (int i = taskListRoot.childCount - 1; i >= 0; i--)
        {
            Transform child = taskListRoot.GetChild(i);
            if (rowTemplate != null && child == rowTemplate.transform)
                continue;

            Destroy(child.gameObject);
        }
    }

    private void HandleTaskCompleted(string taskId)
    {
        RefreshRow(taskId);
    }

    private void RefreshRow(string taskId)
    {
        if (!rowsByTaskId.TryGetValue(taskId, out TMP_Text row))
            return;

        TaskDefinition task = FindTask(taskId);
        if (task == null)
            return;

        bool completed = TaskManager.Instance != null && TaskManager.Instance.IsTaskCompleted(taskId);
        string marker = completed ? completedMarker : pendingMarker;
        row.text = $"{marker} {task.description}";
        row.color = completed ? completedColor : pendingColor;
    }

    private TaskDefinition FindTask(string taskId)
    {
        if (TaskManager.Instance == null)
            return null;

        foreach (TaskDefinition task in TaskManager.Instance.GetTasks())
        {
            if (task != null && task.id == taskId)
                return task;
        }

        return null;
    }
}
