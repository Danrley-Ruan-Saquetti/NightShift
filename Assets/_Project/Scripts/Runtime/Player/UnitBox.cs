using UnityEngine;

namespace NightShift
{
    public static class UnitBox
    {
        private const float HalfExtent = 0.5f;

        public static bool Contains(Transform box, Vector3 worldPoint)
        {
            Vector3 local = box.InverseTransformPoint(worldPoint);
            return Mathf.Abs(local.x) <= HalfExtent
                && Mathf.Abs(local.y) <= HalfExtent
                && Mathf.Abs(local.z) <= HalfExtent;
        }
    }
}
