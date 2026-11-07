using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace NightShift
{
    [RequireComponent(typeof(XRGrabInteractable))]
    public class Sponge : MonoBehaviour
    {
        private XRGrabInteractable grabInteractable;

        private void Awake()
        {
            grabInteractable = GetComponent<XRGrabInteractable>();
        }

        public bool IsHeld()
        {
            foreach (IXRSelectInteractor interactor in grabInteractable.interactorsSelecting)
            {
                if (interactor is not XRSocketInteractor)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
