using UnityEngine;

namespace NightShift
{
    public class BeltAnchor : MonoBehaviour
    {
        [SerializeField]
        private Transform head;
        [SerializeField]
        private float dropBelowHead = 0.6f;
        [SerializeField]
        private float sideOffset = 0.25f;
        [SerializeField]
        private float forwardOffset = 0.1f;

        private void LateUpdate()
        {
            Pose pose = BeltPose.Compute(head.position, head.rotation, dropBelowHead, sideOffset, forwardOffset);
            transform.SetPositionAndRotation(pose.position, pose.rotation);
        }
    }
}
