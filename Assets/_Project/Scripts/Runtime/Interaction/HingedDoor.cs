using System;
using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace NightShift
{
    public class HingedDoor : MonoBehaviour
    {
        [SerializeField]
        private XRSimpleInteractable handle;
        [SerializeField]
        private float maxOpenAngle = 100f;
        [SerializeField]
        private float closedThreshold = 2f;
        [SerializeField]
        private float closeDuration = 1f;
        [SerializeField]
        private bool startsLocked;
        [SerializeField]
        private bool isMirrored;

        public bool IsLocked { get; private set; }
        public bool IsHeld { get; private set; }

        private Quaternion closedRotation;
        private IXRSelectInteractor holdingInteractor;
        private float angle;
        private float grabOffset;
        private bool isClosing;

        public event Action Released;

        private void Awake()
        {
            closedRotation = transform.localRotation;
            IsLocked = startsLocked;
        }

        private void OnEnable()
        {
            handle.selectEntered.AddListener(OnHandleSelected);
            handle.selectExited.AddListener(OnHandleReleased);
        }

        private void OnDisable()
        {
            handle.selectEntered.RemoveListener(OnHandleSelected);
            handle.selectExited.RemoveListener(OnHandleReleased);
        }

        private void Update()
        {
            if (!IsHeld || IsLocked || isClosing)
            {
                return;
            }

            ApplyAngle(Mathf.Clamp(GetHandAngle() - grabOffset, 0f, maxOpenAngle));
        }

        public bool IsClosed()
        {
            return angle < closedThreshold;
        }

        public void Lock()
        {
            IsLocked = true;
        }

        public void Unlock()
        {
            IsLocked = false;
            CaptureGrabOffset();
        }

        public void CloseAndLock()
        {
            if (isClosing)
            {
                return;
            }

            StartCoroutine(CloseAndLockRoutine());
        }

        private IEnumerator CloseAndLockRoutine()
        {
            isClosing = true;
            float closeSpeed = closeDuration > 0f ? maxOpenAngle / closeDuration : float.MaxValue;

            while (angle > 0f)
            {
                ApplyAngle(Mathf.MoveTowards(angle, 0f, closeSpeed * Time.deltaTime));
                yield return null;
            }

            isClosing = false;
            Lock();
        }

        private float GetHandAngle()
        {
            Transform attach = holdingInteractor.GetAttachTransform(handle);
            Vector3 inParent = transform.parent.InverseTransformPoint(attach.position);
            Vector3 inDoorSpace = Quaternion.Inverse(closedRotation) * (inParent - transform.localPosition);
            float handAngle = DoorAngle.FromHandPosition(inDoorSpace);
            return isMirrored ? -handAngle : handAngle;
        }

        private void CaptureGrabOffset()
        {
            if (!IsHeld)
            {
                return;
            }

            grabOffset = GetHandAngle() - angle;
        }

        private void ApplyAngle(float value)
        {
            angle = value;
            float rotation = isMirrored ? -angle : angle;
            transform.localRotation = Quaternion.Euler(0f, rotation, 0f) * closedRotation;
        }

        private void OnHandleSelected(SelectEnterEventArgs args)
        {
            holdingInteractor = args.interactorObject;
            IsHeld = true;
            CaptureGrabOffset();
        }

        private void OnHandleReleased(SelectExitEventArgs _)
        {
            IsHeld = false;
            holdingInteractor = null;
            Released?.Invoke();
        }
    }
}
