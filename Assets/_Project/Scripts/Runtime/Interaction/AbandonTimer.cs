using UnityEngine;

namespace NightShift
{
    public class AbandonTimer
    {
        private readonly float maxDistance;
        private readonly float requiredTime;

        private float elapsed;

        public AbandonTimer(float maxDistance, float requiredTime)
        {
            this.maxDistance = maxDistance;
            this.requiredTime = requiredTime;
        }

        public bool IsExpired { get; private set; }

        public static float HorizontalDistance(Vector3 itemPosition, Vector3 headPosition)
        {
            Vector3 offset = itemPosition - headPosition;
            offset.y = 0f;
            return offset.magnitude;
        }

        public void Tick(float deltaTime, bool isFree, float distanceToHead)
        {
            if (!isFree || distanceToHead <= maxDistance)
            {
                Reset();
                return;
            }

            elapsed += deltaTime;
            IsExpired = elapsed >= requiredTime;
        }

        public void Reset()
        {
            elapsed = 0f;
            IsExpired = false;
        }
    }
}
