using System;
using UnityEngine;

namespace NightShift
{
    [Serializable]
    public struct TaskDefinition
    {
        [SerializeField]
        private string id;
        [SerializeField]
        private TaskType type;

        public TaskDefinition(string id, TaskType type)
        {
            this.id = id;
            this.type = type;
        }

        public string Id => id;
        public TaskType Type => type;
    }
}
