using System;
using System.Collections;
using UnityEngine;

namespace NightShift
{
    public class ShadowEvent : MonoBehaviour, IHouseEvent
    {
        [SerializeField]
        private PlayerState playerState;
        [SerializeField]
        private RoomLightsRegistry roomLights;
        [SerializeField]
        private GameOver gameOver;
        [SerializeField]
        private ShadowDoorway[] doorways;
        [SerializeField]
        private ShadowSilhouette silhouette;
        [SerializeField]
        private AudioSource whisperSource;
        [SerializeField]
        private float reactWindow = 5f;
        [SerializeField]
        private float holdDuration = 8f;
        [SerializeField]
        private float flickerDuration = 5f;
        [SerializeField]
        private float silhouetteDelay = 2f;
        [SerializeField]
        private float crossDuration = 2.5f;

        public bool IsRunning { get; private set; }

        private ShadowRule rule;
        private RoomLights target;
        private ShadowDoorway doorway;
        private Coroutine crossing;
        private bool hasSwitchedOffOtherRoom;
        private bool hasTeleported;
        private bool isCrossScheduled;

        public event Action Succeeded;

        private void Update()
        {
            if (!IsRunning)
            {
                return;
            }

            EventOutcome outcome = rule.Tick(
                Time.deltaTime,
                target.IsOn,
                playerState.IsHidden,
                hasSwitchedOffOtherRoom,
                hasTeleported);
            hasSwitchedOffOtherRoom = false;
            hasTeleported = false;

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

            if (rule.Phase == ShadowPhase.Holding && !isCrossScheduled)
            {
                isCrossScheduled = true;
                crossing = StartCoroutine(CrossAfterDelay());
            }
        }

        public bool CanStartIn(Room room)
        {
            return room != Room.Porch && FindDoorway(room) != null;
        }

        public void Begin()
        {
            Room room = playerState.CurrentRoom;
            doorway = FindDoorway(room);
            target = roomLights.Get(room);

            if (doorway == null || target == null)
            {
                Debug.LogError($"ShadowEvent cannot begin in {room}: missing doorway or room lights", this);
                return;
            }

            rule = new ShadowRule(reactWindow, holdDuration);
            hasSwitchedOffOtherRoom = false;
            hasTeleported = false;
            isCrossScheduled = false;
            IsRunning = true;

            target.Flicker(flickerDuration);
            PlayWhisper();
            Subscribe();
        }

        public void Cancel()
        {
            Unsubscribe();

            if (crossing != null)
            {
                StopCoroutine(crossing);
                crossing = null;
            }

            if (target != null)
            {
                target.StopFlicker();
            }

            whisperSource.Stop();
            silhouette.Hide();
            IsRunning = false;
        }

        private ShadowDoorway FindDoorway(Room room)
        {
            foreach (ShadowDoorway candidate in doorways)
            {
                if (candidate.Room == room)
                {
                    return candidate;
                }
            }

            return null;
        }

        private void PlayWhisper()
        {
            whisperSource.transform.position = (doorway.From.position + doorway.To.position) * 0.5f;
            whisperSource.Play();
        }

        private void Subscribe()
        {
            foreach (RoomLights lights in roomLights.Rooms)
            {
                lights.StateChanged += OnLightsChanged;
            }

            playerState.Teleported += OnTeleported;
        }

        private void Unsubscribe()
        {
            foreach (RoomLights lights in roomLights.Rooms)
            {
                lights.StateChanged -= OnLightsChanged;
            }

            playerState.Teleported -= OnTeleported;
        }

        private IEnumerator CrossAfterDelay()
        {
            yield return new WaitForSeconds(silhouetteDelay);

            silhouette.Cross(doorway.From.position, doorway.To.position, crossDuration);
            crossing = null;
        }

        private void OnLightsChanged(Room room, bool isOn)
        {
            if (!isOn && room != target.Room)
            {
                hasSwitchedOffOtherRoom = true;
            }
        }

        private void OnTeleported()
        {
            hasTeleported = true;
        }
    }
}
