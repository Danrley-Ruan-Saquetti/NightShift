using UnityEngine;

namespace NightShift
{
    public class EntranceDoorCloser : MonoBehaviour
    {
        [SerializeField]
        private HingedDoor door;
        [SerializeField]
        private PlayerState playerState;
        [SerializeField]
        private AudioSource slamSource;
        [SerializeField]
        private AudioClip slamClip;

        private bool hasClosed;

        private void OnEnable()
        {
            playerState.RoomChanged += OnRoomChanged;
            door.Released += OnDoorReleased;
        }

        private void OnDisable()
        {
            playerState.RoomChanged -= OnRoomChanged;
            door.Released -= OnDoorReleased;
        }

        public void Disarm()
        {
            hasClosed = true;
        }

        private void CloseIfPlayerIsInside()
        {
            if (hasClosed || playerState.CurrentRoom != Room.LivingRoom || door.IsHeld)
            {
                return;
            }

            hasClosed = true;
            door.CloseAndLock(OnDoorClosed);
        }

        private void OnDoorClosed()
        {
            slamSource.PlayOneShot(slamClip);
        }

        private void OnRoomChanged(Room _)
        {
            CloseIfPlayerIsInside();
        }

        private void OnDoorReleased()
        {
            CloseIfPlayerIsInside();
        }
    }
}
