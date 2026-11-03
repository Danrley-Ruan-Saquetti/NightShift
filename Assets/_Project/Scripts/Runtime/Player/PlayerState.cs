using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

namespace NightShift
{
    public class PlayerState : MonoBehaviour
    {
        [SerializeField]
        private Transform head;
        [SerializeField]
        private TeleportationProvider teleportationProvider;
        [SerializeField]
        private RoomVolume[] roomVolumes;

        public Room CurrentRoom { get; private set; }

        private RoomVolume currentVolume;

        public event Action<Room> RoomChanged;
        public event Action Teleported;

        private void OnEnable()
        {
            teleportationProvider.locomotionEnded += OnLocomotionEnded;
        }

        private void Start()
        {
            ResolveVolume();
        }

        private void Update()
        {
            if (currentVolume != null && currentVolume.Contains(head.position))
            {
                return;
            }

            Room previousRoom = CurrentRoom;
            ResolveVolume();

            if (CurrentRoom != previousRoom)
            {
                RoomChanged?.Invoke(CurrentRoom);
            }
        }

        private void OnDisable()
        {
            teleportationProvider.locomotionEnded -= OnLocomotionEnded;
        }

        private void ResolveVolume()
        {
            foreach (RoomVolume volume in roomVolumes)
            {
                if (volume.Contains(head.position))
                {
                    currentVolume = volume;
                    CurrentRoom = volume.Room;
                    return;
                }
            }
        }

        private void OnLocomotionEnded(LocomotionProvider _)
        {
            Teleported?.Invoke();
        }
    }
}
