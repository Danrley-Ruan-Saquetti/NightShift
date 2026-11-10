namespace NightShift
{
    public class ShiftCompletion
    {
        private readonly float exitAngle;

        public ShiftCompletion(float exitAngle)
        {
            this.exitAngle = exitAngle;
        }

        public bool IsUnlocked { get; private set; }

        public bool HasExited { get; private set; }

        public bool TryUnlock(int completedCount, int totalCount, bool isEventRunning)
        {
            if (IsUnlocked || totalCount <= 0 || completedCount < totalCount || isEventRunning)
            {
                return false;
            }

            IsUnlocked = true;

            return true;
        }

        public bool TryExit(float doorAngle)
        {
            if (!IsUnlocked || HasExited || doorAngle < exitAngle)
            {
                return false;
            }

            HasExited = true;

            return true;
        }
    }
}
