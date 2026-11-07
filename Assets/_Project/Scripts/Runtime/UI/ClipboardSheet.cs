using System;
using TMPro;
using UnityEngine;

namespace NightShift
{
    public class ClipboardSheet : MonoBehaviour
    {
        [SerializeField]
        private TaskRegistry registry;
        [SerializeField]
        private TMP_Text totalText;
        [SerializeField]
        private string totalLabel = "Total";
        [SerializeField]
        private ProgressLine[] lines;

        private TaskProgress Progress => registry.Progress;

        private void OnEnable()
        {
            Progress.TaskCompleted += OnTaskCompleted;
            Progress.ProgressReset += OnProgressReset;
            Refresh();
        }

        private void OnDisable()
        {
            Progress.TaskCompleted -= OnTaskCompleted;
            Progress.ProgressReset -= OnProgressReset;
        }

        private void Refresh()
        {
            totalText.text = Format(totalLabel, Progress.CompletedCount, Progress.TotalCount);

            foreach (ProgressLine line in lines)
            {
                line.Text.text = Format(line.Label, Progress.CompletedCountOf(line.Type), Progress.TotalCountOf(line.Type));
            }
        }

        private static string Format(string label, int completed, int total)
        {
            string text = $"{label}: {completed}/{total}";
            return completed >= total ? $"<s>{text}</s>" : text;
        }

        private void OnTaskCompleted(string _, int __)
        {
            Refresh();
        }

        private void OnProgressReset()
        {
            Refresh();
        }

        [Serializable]
        private class ProgressLine
        {
            [SerializeField]
            private TaskType type;
            [SerializeField]
            private TMP_Text text;
            [SerializeField]
            private string label;

            public TaskType Type => type;
            public TMP_Text Text => text;
            public string Label => label;
        }
    }
}
