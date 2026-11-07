using UnityEngine;

namespace NightShift
{
    public class TaskRegistry : MonoBehaviour
    {
        [SerializeField]
        private TaskDefinition[] tasks;

        public TaskProgress Progress => progress ??= new TaskProgress(tasks);

        private TaskProgress progress;
    }
}
