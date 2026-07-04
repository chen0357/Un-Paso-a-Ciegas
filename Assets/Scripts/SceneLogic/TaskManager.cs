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
    public event Action OnTasksChanged;

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

    public int GetTaskIndex(string taskId)
    {
        if (tasks == null || string.IsNullOrEmpty(taskId))
            return -1;

        for (int i = 0; i < tasks.Count; i++)
        {
            if (tasks[i] != null && tasks[i].id == taskId)
                return i;
        }

        return -1;
    }

    public int GetNextPendingTaskIndex()
    {
        if (tasks == null || tasks.Count == 0)
            return -1;

        for (int i = 0; i < tasks.Count; i++)
        {
            TaskDefinition task = tasks[i];
            if (task == null || string.IsNullOrEmpty(task.id))
                continue;

            if (!completedTaskIds.Contains(task.id))
                return i;
        }

        return -1;
    }

    public bool IsTaskCurrent(string taskId)
    {
        int taskIndex = GetTaskIndex(taskId);
        if (taskIndex < 0)
            return false;

        return taskIndex == GetNextPendingTaskIndex();
    }

    public bool IsTaskLocked(string taskId)
    {
        int taskIndex = GetTaskIndex(taskId);
        int nextPendingIndex = GetNextPendingTaskIndex();

        if (taskIndex < 0 || nextPendingIndex < 0)
            return false;

        return !IsTaskCompleted(taskId) && taskIndex > nextPendingIndex;
    }

    public bool CanCompleteTask(string taskId)
    {
        if (string.IsNullOrEmpty(taskId) || IsTaskCompleted(taskId))
            return false;

        int taskIndex = GetTaskIndex(taskId);
        int nextPendingIndex = GetNextPendingTaskIndex();

        if (taskIndex < 0)
            return false;

        return nextPendingIndex < 0 || taskIndex == nextPendingIndex;
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
        if (!CanCompleteTask(taskId))
            return false;

        if (GameManager.Instance != null && !GameManager.Instance.CanProcessGameplay())
            return false;

        completedTaskIds.Add(taskId);

        if (GameManager.Instance != null)
            GameManager.Instance.RegisterHint();

        OnTaskCompleted?.Invoke(taskId);
        OnTasksChanged?.Invoke();
        return true;
    }
}
