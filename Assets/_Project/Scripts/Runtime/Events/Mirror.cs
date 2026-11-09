using UnityEngine;

namespace NightShift
{
    public class Mirror : MonoBehaviour
    {
        [SerializeField]
        private EventDirector director;

        private void OnTriggerEnter(Collider other)
        {
            if (other.attachedRigidbody == null || !other.attachedRigidbody.TryGetComponent(out Sponge sponge))
            {
                return;
            }

            if (sponge.IsHeld())
            {
                director.RequestMirrorShadow();
            }
        }
    }
}
