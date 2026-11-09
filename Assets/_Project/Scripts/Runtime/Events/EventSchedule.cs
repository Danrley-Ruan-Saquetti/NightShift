using System;
using System.Collections.Generic;

namespace NightShift
{
    public class EventSchedule
    {
        private readonly IReadOnlyList<ScheduledEvent> entries;
        private readonly float minimumGap;
        private readonly bool[] isQueued;
        private readonly bool[] isConsumed;

        private float? lastEnd;

        public EventSchedule(IReadOnlyList<ScheduledEvent> entries, float minimumGap)
        {
            this.entries = entries;
            this.minimumGap = minimumGap;
            isQueued = new bool[entries.Count];
            isConsumed = new bool[entries.Count];
        }

        public static IReadOnlyList<ScheduledEvent> Default { get; } = new[]
        {
            new ScheduledEvent(2, EventKind.Visitor),
            new ScheduledEvent(5, EventKind.Shadow),
            new ScheduledEvent(8, EventKind.Visitor),
            new ScheduledEvent(11, EventKind.Shadow)
        };

        public bool IsRunning { get; private set; }

        public event Action<EventKind> Queued;

        public void SetCompletedCount(int count)
        {
            for (int i = 0; i < entries.Count; i++)
            {
                if (isConsumed[i] || isQueued[i] || entries[i].TaskCount > count)
                {
                    continue;
                }

                isQueued[i] = true;
                Queued?.Invoke(entries[i].Kind);
            }
        }

        public bool CanStart(float now, bool isTeleporting)
        {
            if (IsRunning || isTeleporting)
            {
                return false;
            }

            return lastEnd == null || now - lastEnd.Value >= minimumGap;
        }

        public bool TryTakeNext(float now, bool isTeleporting, bool canStartShadow, out EventKind kind)
        {
            kind = default;
            int head = FindFirstUnconsumed(null);

            if (head < 0 || !isQueued[head] || !CanStart(now, isTeleporting))
            {
                return false;
            }

            kind = entries[head].Kind;

            if (kind == EventKind.Shadow && !canStartShadow)
            {
                return false;
            }

            isConsumed[head] = true;
            IsRunning = true;

            return true;
        }

        public bool TryTakeMirror(float now, bool isTeleporting)
        {
            if (!CanStart(now, isTeleporting))
            {
                return false;
            }

            int shadow = FindFirstUnconsumed(EventKind.Shadow);

            if (shadow >= 0)
            {
                isConsumed[shadow] = true;
            }

            IsRunning = true;

            return true;
        }

        public void BeginUnscheduled()
        {
            IsRunning = true;
        }

        public void End(float now)
        {
            IsRunning = false;
            lastEnd = now;
        }

        private int FindFirstUnconsumed(EventKind? kind)
        {
            for (int i = 0; i < entries.Count; i++)
            {
                if (!isConsumed[i] && (kind == null || entries[i].Kind == kind))
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
