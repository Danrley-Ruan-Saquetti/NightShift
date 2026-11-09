using System;
using System.Collections;
using UnityEngine;

namespace NightShift
{
    public class VisitorEvent : MonoBehaviour, IHouseEvent
    {
        [SerializeField]
        private PlayerState playerState;
        [SerializeField]
        private HingedDoor entranceDoor;
        [SerializeField]
        private GameOver gameOver;
        [SerializeField]
        private AudioSource doorbellSource;
        [SerializeField]
        private AudioClip doorbellClip;
        [SerializeField]
        private int ringCount = 3;
        [SerializeField]
        private float ringSpacing = 1.5f;
        [SerializeField]
        private float hideWindow = 10f;
        [SerializeField]
        private float hiddenDuration = 8f;
        [SerializeField]
        private AudioSource footstepsSource;
        [SerializeField]
        private Transform footstepsEntrance;
        [SerializeField]
        private float closetPassDistance = 1f;
        [SerializeField]
        private float footstepsFadeOut = 1f;

        public bool IsRunning { get; private set; }

        private VisitorRule rule;
        private Coroutine ringing;
        private float footstepsVolume;
        private float hidingElapsed;
        private bool isHidingStarted;
        private Vector3 closetPassPoint;

        public event Action Succeeded;

        private void Awake()
        {
            footstepsVolume = footstepsSource.volume;
        }

        private void Update()
        {
            if (!IsRunning)
            {
                return;
            }

            EventOutcome outcome = rule.Tick(Time.deltaTime, playerState.IsHidden, entranceDoor.IsHeld);

            if (outcome.Status == EventStatus.Failed)
            {
                Cancel();
                gameOver.Trigger(outcome.Reason);
                return;
            }

            if (outcome.Status == EventStatus.Succeeded)
            {
                Cancel();
                Succeeded?.Invoke();
                return;
            }

            if (rule.Phase == VisitorPhase.Hiding)
            {
                UpdateFootsteps();
            }
        }

        public void Begin()
        {
            if (!HasReferences())
            {
                return;
            }

            float ringDuration = (ringCount - 1) * ringSpacing + doorbellClip.length;
            rule = new VisitorRule(ringDuration, hideWindow, hiddenDuration);
            hidingElapsed = 0f;
            isHidingStarted = false;
            IsRunning = true;
            ringing = StartCoroutine(Ring());
        }

        public void Cancel()
        {
            if (ringing != null)
            {
                StopCoroutine(ringing);
                ringing = null;
            }

            doorbellSource.Stop();
            footstepsSource.Stop();
            IsRunning = false;
        }

        private bool HasReferences()
        {
            bool isComplete = playerState != null
                && entranceDoor != null
                && gameOver != null
                && doorbellSource != null
                && doorbellClip != null
                && footstepsSource != null
                && footstepsEntrance != null;

            if (!isComplete)
            {
                Debug.LogError("VisitorEvent has a missing reference and cannot begin", this);
            }

            return isComplete;
        }

        private IEnumerator Ring()
        {
            for (int ring = 0; ring < ringCount; ring++)
            {
                doorbellSource.PlayOneShot(doorbellClip);

                yield return new WaitForSeconds(ringSpacing);
            }

            ringing = null;
        }

        private void UpdateFootsteps()
        {
            if (!isHidingStarted)
            {
                StartFootsteps();
            }

            hidingElapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(hidingElapsed / hiddenDuration);
            float leg = progress < 0.5f ? progress * 2f : (1f - progress) * 2f;
            footstepsSource.transform.position = Vector3.Lerp(footstepsEntrance.position, closetPassPoint, leg);
            footstepsSource.volume = footstepsVolume * Mathf.Clamp01((hiddenDuration - hidingElapsed) / footstepsFadeOut);
        }

        private void StartFootsteps()
        {
            Vector3 entrance = footstepsEntrance.position;
            Closet closet = playerState.CurrentCloset;
            Vector3 pass = closet.transform.position + closet.ExitDirection * closetPassDistance;
            closetPassPoint = new Vector3(pass.x, entrance.y, pass.z);
            footstepsSource.transform.position = entrance;
            footstepsSource.volume = footstepsVolume;
            footstepsSource.Play();
            isHidingStarted = true;
        }
    }
}
