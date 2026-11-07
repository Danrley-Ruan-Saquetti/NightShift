using UnityEngine;

namespace NightShift
{
    public static class BeltPose
    {
        public static Pose Compute(
            Vector3 headPosition,
            Quaternion headRotation,
            float dropBelowHead,
            float sideOffset,
            float forwardOffset)
        {
            Quaternion yaw = GetYaw(headRotation);
            Vector3 offset = yaw * new Vector3(-sideOffset, 0f, forwardOffset);
            Vector3 position = headPosition - Vector3.up * dropBelowHead + offset;
            return new Pose(position, yaw);
        }

        private static Quaternion GetYaw(Quaternion headRotation)
        {
            Vector3 forward = headRotation * Vector3.forward;
            Vector3 up = headRotation * Vector3.up;
            Vector3 flat = Vector3.ProjectOnPlane(forward - up * forward.y, Vector3.up);
            return Quaternion.LookRotation(flat, Vector3.up);
        }
    }
}
