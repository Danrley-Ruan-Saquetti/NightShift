using UnityEngine;

namespace NightShift
{
    public class RoomVolume : MonoBehaviour
    {
        private const float HalfExtent = 0.5f;

        [SerializeField]
        private Room room;

        public Room Room => room;

        public bool Contains(Vector3 worldPoint)
        {
            Vector3 local = transform.InverseTransformPoint(worldPoint);
            return Mathf.Abs(local.x) <= HalfExtent
                && Mathf.Abs(local.y) <= HalfExtent
                && Mathf.Abs(local.z) <= HalfExtent;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
        }
    }
}
