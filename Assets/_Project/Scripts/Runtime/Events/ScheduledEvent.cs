namespace NightShift
{
    public readonly struct ScheduledEvent
    {
        public ScheduledEvent(int taskCount, EventKind kind)
        {
            TaskCount = taskCount;
            Kind = kind;
        }

        public int TaskCount { get; }
        public EventKind Kind { get; }
    }
}
