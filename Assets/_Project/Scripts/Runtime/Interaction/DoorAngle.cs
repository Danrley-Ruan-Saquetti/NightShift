using UnityEngine;

namespace NightShift
{
    public static class DoorAngle
    {
        public static float FromHandPosition(Vector3 localHandPosition)
        {
            Vector3 flat = new Vector3(localHandPosition.x, 0f, localHandPosition.z);
            return Vector3.SignedAngle(Vector3.right, flat, Vector3.up);
        }
    }
}
