using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace NightShift
{
    [RequireComponent(typeof(XRGrabInteractable), typeof(Rigidbody))]
    public class ReturningItem : MonoBehaviour
    {
        [SerializeField]
        private XRSocketInteractor home;
        [SerializeField]
        private Transform head;
        [SerializeField]
        private float maxDistance = 1.5f;
        [SerializeField]
        private float abandonTime = 3f;
        [SerializeField]
        private float fallLimitY = -0.5f;

        private XRGrabInteractable grabInteractable;
        private Rigidbody body;
        private AbandonTimer timer;
        private bool isAwaitingFirstDock = true;

        private void Awake()
        {
            grabInteractable = GetComponent<XRGrabInteractable>();
            body = GetComponent<Rigidbody>();
            timer = new AbandonTimer(maxDistance, abandonTime);
        }

        private void Update()
        {
            if (isAwaitingFirstDock)
            {
                DockAtStart();
                return;
            }

            if (transform.position.y < fallLimitY)
            {
                ReturnHome();
                return;
            }

            float distanceToHead = AbandonTimer.HorizontalDistance(transform.position, head.position);
            timer.Tick(Time.deltaTime, !grabInteractable.isSelected, distanceToHead);

            if (timer.IsExpired)
            {
                ReturnHome();
            }
        }

        public void ReturnHome()
        {
            Transform attachPoint = home.attachTransform;
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            transform.SetPositionAndRotation(attachPoint.position, attachPoint.rotation);
            home.interactionManager.SelectEnter((IXRSelectInteractor)home, (IXRSelectInteractable)grabInteractable);
            timer.Reset();
        }

        private void DockAtStart()
        {
            if (grabInteractable.isSelected)
            {
                isAwaitingFirstDock = false;
                return;
            }

            if (CanDock())
            {
                ReturnHome();
            }
        }

        private bool CanDock()
        {
            XRInteractionManager manager = home.interactionManager;
            return manager != null
                && home.isActiveAndEnabled
                && manager.IsRegistered((IXRInteractor)home)
                && manager.IsRegistered((IXRInteractable)grabInteractable);
        }
    }
}
