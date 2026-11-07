using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace NightShift
{
    [RequireComponent(typeof(XRGrabInteractable))]
    public class Sprayer : MonoBehaviour
    {
        [SerializeField]
        private Transform nozzle;
        [SerializeField]
        private ParticleSystem mist;
        [SerializeField]
        private float range = 0.5f;
        [SerializeField]
        private float radius = 0.05f;
        [SerializeField]
        private float cooldown = 0.3f;
        [SerializeField]
        private LayerMask hitMask;

        private XRGrabInteractable grabInteractable;
        private float lastSprayTime = float.NegativeInfinity;

        private void Awake()
        {
            grabInteractable = GetComponent<XRGrabInteractable>();
        }

        private void OnEnable()
        {
            grabInteractable.activated.AddListener(OnActivated);
        }

        private void OnDisable()
        {
            grabInteractable.activated.RemoveListener(OnActivated);
        }

        private void Spray()
        {
            lastSprayTime = Time.time;
            mist.Play();

            RaycastHit[] hits = Physics.SphereCastAll(
                nozzle.position,
                radius,
                nozzle.forward,
                range,
                hitMask,
                QueryTriggerInteraction.Collide);

            Stain nearestStain = null;
            float stainDistance = float.PositiveInfinity;
            float blockerDistance = float.PositiveInfinity;

            foreach (RaycastHit hit in hits)
            {
                Stain stain = hit.collider.GetComponentInParent<Stain>();

                if (stain != null && hit.distance < stainDistance)
                {
                    nearestStain = stain;
                    stainDistance = hit.distance;
                }
                else if (stain == null && !hit.collider.isTrigger)
                {
                    blockerDistance = Mathf.Min(blockerDistance, hit.distance);
                }
            }

            if (nearestStain != null && stainDistance <= blockerDistance)
            {
                nearestStain.Spray();
            }
        }

        private void OnActivated(ActivateEventArgs _)
        {
            if (Time.time < lastSprayTime + cooldown)
            {
                return;
            }

            Spray();
        }
    }
}
