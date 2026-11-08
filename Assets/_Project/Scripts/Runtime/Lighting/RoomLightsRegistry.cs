using UnityEngine;

namespace NightShift
{
    public class RoomLightsRegistry : MonoBehaviour
    {
        [SerializeField]
        private RoomLights[] rooms;

        public RoomLights Get(Room room)
        {
            foreach (RoomLights roomLights in rooms)
            {
                if (roomLights.Room == room)
                {
                    return roomLights;
                }
            }

            Debug.LogError($"No RoomLights registered for {room}", this);
            return null;
        }
    }
}
