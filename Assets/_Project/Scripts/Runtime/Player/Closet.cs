using UnityEngine;

namespace NightShift
{
    public class Closet : MonoBehaviour
    {
        [SerializeField]
        private HingedDoor door;
        [SerializeField]
        private Transform hidingVolume;

        public Transform HidingVolume => hidingVolume;
        public bool IsDoorClosed => door.IsClosed();

        private void OnDrawGizmosSelected()
        {
            if (hidingVolume == null)
            {
                return;
            }

            Gizmos.matrix = hidingVolume.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
        }
    }
}
