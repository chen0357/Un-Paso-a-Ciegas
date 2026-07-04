using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TaskChecklistUI : MonoBehaviour
{
    [Header("Layout")]
    public RectTransform taskListRoot;
    public TMP_Text rowTemplate;

    [Header("Style")]
    public string title = "OBJETIVOS";
    public string pendingMarker = "○";
    public string completedMarker = "✓";
    public string lockedMarker = "·";
    public Color pendingColor = new Color(0.86f, 0.86f, 0.86f, 1f);
    public Color completedColor = new Color(0.68f, 0.68f, 0.68f, 1f);
    public Color lockedColor = new Color(0.45f, 0.45f, 0.45f, 0.95f);
    public Color completedStrikeColor = new Color(0.85f, 0.2f, 0.2f, 0.95f);
    public Color panelColor = new Color(0.04f, 0.04f, 0.04f, 0.58f);
    public Color borderColor = new Color(0.42f, 0.42f, 0.42f, 0.9f);
    public Color titleColor = new Color(0.94f, 0.94f, 0.94f, 1f);
    public Color titleBarColor = new Color(0.14f, 0.14f, 0.14f, 0.86f);
    public Color rowColor = new Color(0.11f, 0.11f, 0.11f, 0.34f);
    public Color completedRowColor = new Color(0.19f, 0.19f, 0.19f, 0.5f);
    public Color lockedRowColor = new Color(0.08f, 0.08f, 0.08f, 0.24f);
    public Vector2 panelSize = new Vector2(290f, 0f);
    public Vector2 topRightMargin = new Vector2(18f, 14f);
    public Vector2 panelPadding = new Vector2(12f, 10f);
    public float titleHeight = 24f;
    public float rowHeight = 24f;
    public float rowSpacing = 5f;
    public float titleBottomSpacing = 7f;

    private readonly Dictionary<string, TaskRowView> rowsByTaskId = new Dictionary<string, TaskRowView>();
    private TMP_Text titleLabel;
    private Image titleBarBackground;

    private sealed class TaskRowView
    {
        public Image Background;
        public TMP_Text Label;
        public Image StrikeLine;
    }

    private void Start()
    {
        ConfigurePanelFrame();
        BuildChecklist();

        if (TaskManager.Instance != null)
        {
            TaskManager.Instance.OnTaskCompleted += HandleTaskCompleted;
            TaskManager.Instance.OnTasksChanged += HandleTasksChanged;
        }
    }

    private void OnDestroy()
    {
        if (TaskManager.Instance != null)
        {
            TaskManager.Instance.OnTaskCompleted -= HandleTaskCompleted;
            TaskManager.Instance.OnTasksChanged -= HandleTasksChanged;
        }
    }

    private void BuildChecklist()
    {
        if (TaskManager.Instance == null || taskListRoot == null)
            return;

        ConfigureLayout();
        ClearGeneratedRows();

        IReadOnlyList<TaskDefinition> tasks = TaskManager.Instance.GetTasks();
        for (int i = 0; i < tasks.Count; i++)
        {
            TaskDefinition task = tasks[i];
            if (task == null || string.IsNullOrEmpty(task.id))
                continue;

            TaskRowView row = CreateRow(i);
            rowsByTaskId[task.id] = row;
            RefreshRow(task.id);
        }
    }

    private void ConfigurePanelFrame()
    {
        Image panelImage = GetOrAddComponent<Image>(gameObject);
        panelImage.color = panelColor;
        panelImage.raycastTarget = false;

        Outline outline = GetOrAddComponent<Outline>(gameObject);
        outline.effectColor = borderColor;
        outline.effectDistance = new Vector2(2f, -2f);
        outline.useGraphicAlpha = true;
    }

    private void ConfigureLayout()
    {
        RectTransform rootRect = GetComponent<RectTransform>();
        int taskCount = 0;
        if (TaskManager.Instance != null && TaskManager.Instance.GetTasks() != null)
            taskCount = TaskManager.Instance.GetTasks().Count;

        float rowsHeight = taskCount > 0
            ? taskCount * rowHeight + (taskCount - 1) * rowSpacing
            : rowHeight;
        float totalHeight = panelPadding.y * 2f + titleHeight + titleBottomSpacing + rowsHeight;

        if (rootRect != null)
        {
            rootRect.anchorMin = new Vector2(1f, 1f);
            rootRect.anchorMax = new Vector2(1f, 1f);
            rootRect.pivot = new Vector2(1f, 1f);
            rootRect.anchoredPosition = new Vector2(-topRightMargin.x, -topRightMargin.y);
            rootRect.sizeDelta = new Vector2(panelSize.x, totalHeight);
        }

        EnsureTitleBar();
        EnsureTitleLabel();

        taskListRoot.anchorMin = new Vector2(0f, 0f);
        taskListRoot.anchorMax = new Vector2(1f, 1f);
        taskListRoot.pivot = new Vector2(0f, 1f);
        taskListRoot.offsetMin = new Vector2(panelPadding.x, panelPadding.y);
        taskListRoot.offsetMax = new Vector2(-panelPadding.x, -(panelPadding.y + titleHeight + titleBottomSpacing));
    }

    private void EnsureTitleBar()
    {
        if (titleBarBackground == null)
        {
            Transform existing = transform.Find("TaskHeader");
            if (existing != null)
                titleBarBackground = existing.GetComponent<Image>();
        }

        if (titleBarBackground == null)
        {
            GameObject headerObject = new GameObject(
                "TaskHeader",
                typeof(RectTransform),
                typeof(Image));
            headerObject.transform.SetParent(transform, false);
            titleBarBackground = headerObject.GetComponent<Image>();
        }

        RectTransform rect = titleBarBackground.rectTransform;
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.offsetMin = new Vector2(4f, -(panelPadding.y + titleHeight));
        rect.offsetMax = new Vector2(-4f, -4f);

        titleBarBackground.color = titleBarColor;
        titleBarBackground.raycastTarget = false;
        titleBarBackground.transform.SetAsFirstSibling();
    }

    private void EnsureTitleLabel()
    {
        if (titleLabel == null)
        {
            Transform existing = transform.Find("TaskTitle");
            if (existing != null)
                titleLabel = existing.GetComponent<TMP_Text>();
        }

        if (titleLabel == null)
        {
            GameObject titleObject = new GameObject(
                "TaskTitle",
                typeof(RectTransform),
                typeof(TextMeshProUGUI));
            titleObject.transform.SetParent(transform, false);
            titleLabel = titleObject.GetComponent<TextMeshProUGUI>();
        }

        RectTransform rect = titleLabel.rectTransform;
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.offsetMin = new Vector2(panelPadding.x, -(panelPadding.y + titleHeight));
        rect.offsetMax = new Vector2(-panelPadding.x, -panelPadding.y);

        titleLabel.text = title;
        titleLabel.fontSize = 18;
        titleLabel.fontStyle = FontStyles.Bold;
        titleLabel.alignment = TextAlignmentOptions.MidlineLeft;
        titleLabel.color = titleColor;
        titleLabel.raycastTarget = false;
        titleLabel.transform.SetAsLastSibling();
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

    private TaskRowView CreateRow(int index)
    {
        GameObject rowObject = new GameObject(
            $"TaskRow_{index + 1}",
            typeof(RectTransform),
            typeof(Image));
        rowObject.transform.SetParent(taskListRoot, false);

        RectTransform rect = rowObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.offsetMin = new Vector2(0f, -(index + 1) * rowHeight - index * rowSpacing);
        rect.offsetMax = new Vector2(0f, -index * (rowHeight + rowSpacing));

        Image background = rowObject.GetComponent<Image>();
        background.color = rowColor;
        background.raycastTarget = false;

        Outline outline = GetOrAddComponent<Outline>(rowObject);
        outline.effectColor = new Color(borderColor.r, borderColor.g, borderColor.b, 0.55f);
        outline.effectDistance = new Vector2(1f, -1f);
        outline.useGraphicAlpha = true;

        TMP_Text label;
        if (rowTemplate != null)
        {
            label = Instantiate(rowTemplate, rowObject.transform);
            label.gameObject.SetActive(true);
        }
        else
        {
            GameObject textObject = new GameObject(
                "Label",
                typeof(RectTransform),
                typeof(TextMeshProUGUI));
            textObject.transform.SetParent(rowObject.transform, false);
            label = textObject.GetComponent<TextMeshProUGUI>();
        }

        label.name = "Label";
        RectTransform labelRect = label.rectTransform;
        labelRect.anchorMin = new Vector2(0f, 0f);
        labelRect.anchorMax = new Vector2(1f, 1f);
        labelRect.offsetMin = new Vector2(12f, 0f);
        labelRect.offsetMax = new Vector2(-12f, 0f);
        labelRect.pivot = new Vector2(0.5f, 0.5f);

        label.fontSize = 16;
        label.alignment = TextAlignmentOptions.MidlineLeft;
        label.color = pendingColor;
        label.raycastTarget = false;

        GameObject strikeObject = new GameObject(
            "StrikeLine",
            typeof(RectTransform),
            typeof(Image));
        strikeObject.transform.SetParent(rowObject.transform, false);

        Image strikeLine = strikeObject.GetComponent<Image>();
        strikeLine.color = completedStrikeColor;
        strikeLine.raycastTarget = false;
        strikeLine.enabled = false;

        RectTransform strikeRect = strikeObject.GetComponent<RectTransform>();
        strikeRect.anchorMin = new Vector2(0f, 0.5f);
        strikeRect.anchorMax = new Vector2(1f, 0.5f);
        strikeRect.pivot = new Vector2(0.5f, 0.5f);
        strikeRect.offsetMin = new Vector2(28f, -1.5f);
        strikeRect.offsetMax = new Vector2(-12f, 1.5f);

        return new TaskRowView
        {
            Background = background,
            Label = label,
            StrikeLine = strikeLine
        };
    }

    private void HandleTaskCompleted(string taskId)
    {
        RefreshAllRows();
    }

    private void HandleTasksChanged()
    {
        RefreshAllRows();
    }

    private void RefreshAllRows()
    {
        foreach (string taskId in rowsByTaskId.Keys)
            RefreshRow(taskId);
    }

    private void RefreshRow(string taskId)
    {
        if (!rowsByTaskId.TryGetValue(taskId, out TaskRowView row))
            return;

        TaskDefinition task = FindTask(taskId);
        if (task == null)
            return;

        bool completed = TaskManager.Instance != null && TaskManager.Instance.IsTaskCompleted(taskId);
        bool locked = TaskManager.Instance != null && TaskManager.Instance.IsTaskLocked(taskId);

        if (completed)
        {
            row.Label.text = $"{completedMarker} {task.description}";
            row.Label.color = completedColor;
            if (row.StrikeLine != null)
            {
                row.StrikeLine.color = completedStrikeColor;
                row.StrikeLine.enabled = true;
            }
        }
        else if (locked)
        {
            row.Label.text = $"{lockedMarker} {task.description}";
            row.Label.color = lockedColor;
            if (row.StrikeLine != null)
                row.StrikeLine.enabled = false;
        }
        else
        {
            row.Label.text = $"{pendingMarker} {task.description}";
            row.Label.color = pendingColor;
            if (row.StrikeLine != null)
                row.StrikeLine.enabled = false;
        }

        if (row.Background != null)
            row.Background.color = completed ? completedRowColor : locked ? lockedRowColor : rowColor;
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

    private static T GetOrAddComponent<T>(GameObject target) where T : Component
    {
        T component = target.GetComponent<T>();
        if (component == null)
            component = target.AddComponent<T>();

        return component;
    }
}
