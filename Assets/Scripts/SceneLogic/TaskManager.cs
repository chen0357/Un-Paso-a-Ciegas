using System;
using System.Collections.Generic;
using UnityEngine;

public class TaskManager : MonoBehaviour
{
    public static TaskManager Instance { get; private set; }

    [Header("Level Tasks")]
    public List<TaskDefinition> tasks = new List<TaskDefinition>
    {
        new TaskDefinition { id = "task_1", description = "Cruzando la acera" },
        new TaskDefinition { id = "task_2", description = "Cruzando el carril" },
        new TaskDefinition { id = "task_3", description = "Encuentra la entrada del metro" }
    };

    public event Action<string> OnTaskCompleted;

    private readonly HashSet<string> completedTaskIds = new HashSet<string>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    public IReadOnlyList<TaskDefinition> GetTasks()
    {
        return tasks;
    }

    public bool IsTaskCompleted(string taskId)
    {
        return !string.IsNullOrEmpty(taskId) && completedTaskIds.Contains(taskId);
    }

    public bool AreAllTasksCompleted()
    {
        if (tasks == null || tasks.Count == 0)
            return true;

        foreach (TaskDefinition task in tasks)
        {
            if (task == null || string.IsNullOrEmpty(task.id))
                continue;

            if (!completedTaskIds.Contains(task.id))
                return false;
        }

        return true;
    }

    public bool CompleteTask(string taskId)
    {
        if (string.IsNullOrEmpty(taskId) || completedTaskIds.Contains(taskId))
            return false;

        if (GameManager.Instance != null && !GameManager.Instance.CanProcessGameplay())
            return false;

        completedTaskIds.Add(taskId);

        if (GameManager.Instance != null)
            GameManager.Instance.RegisterHint();

        OnTaskCompleted?.Invoke(taskId);
        return true;
    }
}
