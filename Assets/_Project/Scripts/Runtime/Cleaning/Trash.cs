using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace NightShift
{
    [RequireComponent(typeof(XRGrabInteractable), typeof(Rigidbody))]
    public class Trash : MonoBehaviour
    {
        [SerializeField]
        private TaskRegistry registry;
        [SerializeField]
        private string taskId;
        [SerializeField]
        private float fallLimitY = -0.5f;

        private XRGrabInteractable grabInteractable;
        private Rigidbody body;
        private Pose startPose;

        private void Awake()
        {
            grabInteractable = GetComponent<XRGrabInteractable>();
            body = GetComponent<Rigidbody>();
            startPose = new Pose(transform.position, transform.rotation);
        }

        private void Update()
        {
            if (transform.position.y < fallLimitY)
            {
                ReturnToStart();
            }
        }

        public bool IsHeld()
        {
            return grabInteractable.isSelected;
        }

        public void Discard()
        {
            registry.Progress.Complete(taskId);
            gameObject.SetActive(false);
        }

        private void ReturnToStart()
        {
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            transform.SetPositionAndRotation(startPose.position, startPose.rotation);
        }
    }
}
