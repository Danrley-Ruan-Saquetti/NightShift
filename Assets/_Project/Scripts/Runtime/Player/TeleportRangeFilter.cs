using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace NightShift
{
    public class TeleportRangeFilter : MonoBehaviour, IXRHoverFilter
    {
        [SerializeField]
        private XRRayInteractor rayInteractor;
        [SerializeField]
        private FadeTeleportationProvider teleportationProvider;

        public bool canProcess => isActiveAndEnabled;

        public bool Process(IXRHoverInteractor interactor, IXRHoverInteractable interactable)
        {
            return rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit)
                && teleportationProvider.IsWithinRange(hit.point);
        }
    }
}
