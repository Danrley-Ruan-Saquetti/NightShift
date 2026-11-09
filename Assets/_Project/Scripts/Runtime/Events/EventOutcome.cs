namespace NightShift
{
    public readonly struct EventOutcome
    {
        private EventOutcome(EventStatus status, GameOverReason reason)
        {
            Status = status;
            Reason = reason;
        }

        public static EventOutcome Running => new(EventStatus.Running, default);
        public static EventOutcome Succeeded => new(EventStatus.Succeeded, default);

        public EventStatus Status { get; }
        public GameOverReason Reason { get; }

        public static EventOutcome Failed(GameOverReason reason)
        {
            return new EventOutcome(EventStatus.Failed, reason);
        }
    }
}
