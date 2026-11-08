using UnityEngine;

namespace NightShift
{
    public static class JumpscarePlacement
    {
        public static Vector3 FlatForward(Quaternion headRotation)
        {
            return Quaternion.Euler(0f, headRotation.eulerAngles.y, 0f) * Vector3.forward;
        }

        public static Vector3 ChooseDirection(Quaternion headRotation, Vector3? closetExit)
        {
            if (closetExit.HasValue)
            {
                Vector3 flat = closetExit.Value;
                flat.y = 0f;
                return flat.normalized;
            }

            return FlatForward(headRotation);
        }

        public static float ResolveDistance(float preferred, float obstacleDistance, float margin, float minimum)
        {
            return Mathf.Min(preferred, Mathf.Max(obstacleDistance - margin, minimum));
        }

        public static float AdvancedDistance(float distance, float advance, float minimum)
        {
            return Mathf.Max(distance - advance, minimum);
        }

        public static Pose Place(Vector3 headPosition, Vector3 direction, float distance)
        {
            return new Pose(headPosition + direction * distance, Quaternion.LookRotation(-direction, Vector3.up));
        }
    }
}
