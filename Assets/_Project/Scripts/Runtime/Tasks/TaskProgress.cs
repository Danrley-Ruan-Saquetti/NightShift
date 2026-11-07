using System;
using System.Collections.Generic;
using System.Linq;

namespace NightShift
{
    public class TaskProgress
    {
        private readonly Dictionary<string, TaskDefinition> tasks = new();
        private readonly HashSet<string> completedIds = new();

        public TaskProgress(IEnumerable<TaskDefinition> tasks)
        {
            foreach (TaskDefinition task in tasks)
            {
                this.tasks.Add(task.Id, task);
            }
        }

        public int CompletedCount => completedIds.Count;
        public int TotalCount => tasks.Count;

        public event Action<string, int> TaskCompleted;
        public event Action ProgressReset;

        public int CompletedCountOf(TaskType type)
        {
            return completedIds.Count(id => tasks[id].Type == type);
        }

        public int TotalCountOf(TaskType type)
        {
            return tasks.Values.Count(task => task.Type == type);
        }

        public bool IsCompleted(string taskId)
        {
            return completedIds.Contains(taskId);
        }

        public void Complete(string taskId)
        {
            if (!tasks.ContainsKey(taskId))
            {
                throw new ArgumentException($"Unknown task id '{taskId}'", nameof(taskId));
            }

            if (!completedIds.Add(taskId))
            {
                return;
            }

            TaskCompleted?.Invoke(taskId, CompletedCount);
        }

        public void Reset()
        {
            completedIds.Clear();
            ProgressReset?.Invoke();
        }
    }
}
