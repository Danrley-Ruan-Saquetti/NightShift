using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace NightShift
{
    public class NearOnlyGrabFilter : MonoBehaviour, IXRHoverFilter, IXRSelectFilter
    {
        [SerializeField]
        private float maxGrabDistance = 0.15f;

        public bool canProcess => isActiveAndEnabled;

        public bool Process(IXRHoverInteractor interactor, IXRHoverInteractable interactable)
        {
            return IsWithinReach(interactor, interactable);
        }

        public bool Process(IXRSelectInteractor interactor, IXRSelectInteractable interactable)
        {
            return interactor.IsSelecting(interactable) || IsWithinReach(interactor, interactable);
        }

        private bool IsWithinReach(IXRInteractor interactor, IXRInteractable interactable)
        {
            if (interactor is XRSocketInteractor)
            {
                return true;
            }

            Vector3 handPosition = interactor.transform.position;

            foreach (Collider itemCollider in interactable.colliders)
            {
                if (Vector3.Distance(handPosition, itemCollider.ClosestPoint(handPosition)) <= maxGrabDistance)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
