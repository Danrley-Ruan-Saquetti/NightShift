using UnityEngine;

namespace NightShift
{
    public class RoomVolume : MonoBehaviour
    {
        [SerializeField]
        private Room room;

        public Room Room => room;

        public bool Contains(Vector3 worldPoint)
        {
            return UnitBox.Contains(transform, worldPoint);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
        }
    }
}
