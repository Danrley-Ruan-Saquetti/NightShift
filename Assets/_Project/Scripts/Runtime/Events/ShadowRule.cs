namespace NightShift
{
    public class ShadowRule
    {
        private readonly float reactWindow;
        private readonly float holdDuration;

        private float phaseElapsed;

        public ShadowRule(float reactWindow, float holdDuration)
        {
            this.reactWindow = reactWindow;
            this.holdDuration = holdDuration;
            Outcome = EventOutcome.Running;
        }

        public ShadowPhase Phase { get; private set; }
        public EventOutcome Outcome { get; private set; }

        public EventOutcome Tick(
            float deltaTime,
            bool isTargetLightOn,
            bool isHidden,
            bool hasSwitchedOffOtherRoom,
            bool hasTeleported)
        {
            if (Outcome.Status != EventStatus.Running)
            {
                return Outcome;
            }

            if (Phase == ShadowPhase.Reacting)
            {
                Outcome = AdvanceReacting(deltaTime, isTargetLightOn, isHidden, hasSwitchedOffOtherRoom);
            }

            if (Phase == ShadowPhase.Holding && Outcome.Status == EventStatus.Running)
            {
                Outcome = AdvanceHolding(deltaTime, isTargetLightOn, isHidden, hasSwitchedOffOtherRoom, hasTeleported);
            }

            return Outcome;
        }

        private EventOutcome AdvanceReacting(
            float deltaTime,
            bool isTargetLightOn,
            bool isHidden,
            bool hasSwitchedOffOtherRoom)
        {
            if (!isTargetLightOn && !isHidden)
            {
                Phase = ShadowPhase.Holding;
                phaseElapsed = 0f;

                return EventOutcome.Running;
            }

            phaseElapsed += deltaTime;

            if (phaseElapsed >= reactWindow)
            {
                return EventOutcome.Failed(isTargetLightOn ? GameOverReason.ShadowLightOn : GameOverReason.ShadowHidden);
            }

            return CheckWrongRoomSwitch(hasSwitchedOffOtherRoom);
        }

        private EventOutcome AdvanceHolding(
            float deltaTime,
            bool isTargetLightOn,
            bool isHidden,
            bool hasSwitchedOffOtherRoom,
            bool hasTeleported)
        {
            if (isTargetLightOn)
            {
                return EventOutcome.Failed(GameOverReason.ShadowLightOn);
            }

            if (isHidden)
            {
                return EventOutcome.Failed(GameOverReason.ShadowHidden);
            }

            EventOutcome wrongRoomSwitch = CheckWrongRoomSwitch(hasSwitchedOffOtherRoom);

            if (wrongRoomSwitch.Status == EventStatus.Failed)
            {
                return wrongRoomSwitch;
            }

            if (hasTeleported)
            {
                return EventOutcome.Failed(GameOverReason.ShadowTeleported);
            }

            phaseElapsed += deltaTime;

            return phaseElapsed >= holdDuration ? EventOutcome.Succeeded : EventOutcome.Running;
        }

        private static EventOutcome CheckWrongRoomSwitch(bool hasSwitchedOffOtherRoom)
        {
            return hasSwitchedOffOtherRoom
                ? EventOutcome.Failed(GameOverReason.ShadowWrongRoomSwitch)
                : EventOutcome.Running;
        }
    }
}
