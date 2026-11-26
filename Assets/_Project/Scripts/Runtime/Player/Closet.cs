using UnityEngine;

namespace NightShift
{
    public class Closet : MonoBehaviour
    {
        [SerializeField]
        private HingedDoor door;
        [SerializeField]
        private Transform hidingVolume;
        [SerializeField]
        private Transform cartSpot;

        public Transform HidingVolume => hidingVolume;
        public Pose CartSpot => new(cartSpot.position, cartSpot.rotation);
        public bool IsDoorClosed => door.IsClosed();
        public Vector3 ExitDirection => transform.forward;

        public bool Contains(Vector3 worldPoint)
        {
            return UnitBox.Contains(hidingVolume, worldPoint);
        }

        public void BurstOpen(float angle, float duration)
        {
            door.OpenTo(angle, duration);
        }

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
