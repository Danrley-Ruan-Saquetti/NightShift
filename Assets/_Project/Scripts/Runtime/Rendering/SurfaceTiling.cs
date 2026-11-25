using UnityEngine;

namespace NightShift
{
    public static class SurfaceTiling
    {
        public static Vector2 ForCube(Vector3 size, float tileMeters)
        {
            Vector2 visibleFace = ThinnestAxis(size) switch
            {
                0 => new Vector2(size.z, size.y),
                1 => new Vector2(size.x, size.z),
                _ => new Vector2(size.x, size.y),
            };

            return visibleFace / tileMeters;
        }

        private static int ThinnestAxis(Vector3 size)
        {
            if (size.x <= size.y && size.x <= size.z)
            {
                return 0;
            }

            return size.y <= size.z ? 1 : 2;
        }
    }
}
