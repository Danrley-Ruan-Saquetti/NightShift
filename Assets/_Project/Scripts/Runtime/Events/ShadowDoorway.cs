using UnityEngine;

namespace NightShift
{
    public class ShadowDoorway : MonoBehaviour
    {
        [SerializeField]
        private Room room;
        [SerializeField]
        private Transform from;
        [SerializeField]
        private Transform to;

        public Room Room => room;
        public Transform From => from;
        public Transform To => to;
    }
}
