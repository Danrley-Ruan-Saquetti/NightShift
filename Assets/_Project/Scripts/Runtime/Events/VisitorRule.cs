namespace NightShift
{
    public class VisitorRule
    {
        private readonly float ringDuration;
        private readonly float hideWindow;
        private readonly float hiddenDuration;

        private float phaseElapsed;

        public VisitorRule(float ringDuration, float hideWindow, float hiddenDuration)
        {
            this.ringDuration = ringDuration;
            this.hideWindow = hideWindow;
            this.hiddenDuration = hiddenDuration;
            Outcome = EventOutcome.Running;
        }

        public VisitorPhase Phase { get; private set; }
        public EventOutcome Outcome { get; private set; }

        public EventOutcome Tick(float deltaTime, bool isHidden, bool isEntranceHandleHeld)
        {
            if (Outcome.Status != EventStatus.Running)
            {
                return Outcome;
            }

            EventOutcome phaseResult = AdvancePhase(deltaTime, isHidden);

            if (phaseResult.Status != EventStatus.Failed && isEntranceHandleHeld)
            {
                phaseResult = EventOutcome.Failed(GameOverReason.VisitorTouchedEntranceHandle);
            }

            Outcome = phaseResult;

            return Outcome;
        }

        private EventOutcome AdvancePhase(float deltaTime, bool isHidden)
        {
            if (Phase == VisitorPhase.Ringing)
            {
                AdvanceRinging(deltaTime, isHidden);

                return EventOutcome.Running;
            }

            if (Phase == VisitorPhase.Waiting && isHidden)
            {
                EnterPhase(VisitorPhase.Hiding);
            }

            return Phase == VisitorPhase.Waiting
                ? AdvanceWaiting(deltaTime)
                : AdvanceHiding(deltaTime, isHidden);
        }

        private void AdvanceRinging(float deltaTime, bool isHidden)
        {
            phaseElapsed += deltaTime;

            if (phaseElapsed >= ringDuration)
            {
                EnterPhase(isHidden ? VisitorPhase.Hiding : VisitorPhase.Waiting);
            }
        }

        private EventOutcome AdvanceWaiting(float deltaTime)
        {
            phaseElapsed += deltaTime;

            return phaseElapsed >= hideWindow
                ? EventOutcome.Failed(GameOverReason.VisitorNotHidden)
                : EventOutcome.Running;
        }

        private EventOutcome AdvanceHiding(float deltaTime, bool isHidden)
        {
            if (!isHidden)
            {
                return EventOutcome.Failed(GameOverReason.VisitorLeftEarly);
            }

            phaseElapsed += deltaTime;

            return phaseElapsed >= hiddenDuration
                ? EventOutcome.Succeeded
                : EventOutcome.Running;
        }

        private void EnterPhase(VisitorPhase phase)
        {
            Phase = phase;
            phaseElapsed = 0f;
        }
    }
}
