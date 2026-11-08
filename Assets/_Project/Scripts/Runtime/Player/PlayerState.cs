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
        [SerializeField]
        private Closet[] closets;

        public Room CurrentRoom { get; private set; }
        public Closet CurrentCloset { get; private set; }
        public bool IsHidden => CurrentCloset != null;

        private RoomVolume currentVolume;
        private Transform[] hidingVolumes;
        private bool[] closedDoors;

        public event Action<Room> RoomChanged;
        public event Action Teleported;
        public event Action HidingChanged;

        private void Awake()
        {
            hidingVolumes = new Transform[closets.Length];
            closedDoors = new bool[closets.Length];

            for (int i = 0; i < closets.Length; i++)
            {
                hidingVolumes[i] = closets[i].HidingVolume;
            }
        }

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
            UpdateHiding();
            UpdateRoom();
        }

        private void OnDisable()
        {
            teleportationProvider.locomotionEnded -= OnLocomotionEnded;
        }

        private void UpdateRoom()
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

        private void UpdateHiding()
        {
            for (int i = 0; i < closets.Length; i++)
            {
                closedDoors[i] = closets[i].IsDoorClosed;
            }

            int index = HidingRule.FindHidingIndex(head.position, hidingVolumes, closedDoors);
            Closet closet = index >= 0 ? closets[index] : null;

            if (closet == CurrentCloset)
            {
                return;
            }

            CurrentCloset = closet;
            HidingChanged?.Invoke();
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
