using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

namespace NightShift
{
    public class HeadOcclusionGuard : MonoBehaviour
    {
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        [SerializeField]
        private Transform head;
        [SerializeField]
        private TeleportationProvider teleportationProvider;
        [SerializeField]
        private Renderer overlay;
        [SerializeField]
        private LayerMask blockingLayers;
        [SerializeField]
        private float headRadius = 0.08f;
        [SerializeField]
        private float darkenDuration = 0.1f;
        [SerializeField]
        private float clearDuration = 0.2f;

        private HeadOcclusion occlusion;
        private Material overlayMaterial;
        private float alpha;

        private void OnEnable()
        {
            teleportationProvider.afterStepLocomotion += OnTeleported;
        }

        private void Start()
        {
            occlusion = new HeadOcclusion(head.position);
            overlayMaterial = overlay.material;
            ApplyAlpha(0f);
        }

        private void LateUpdate()
        {
            EvaluateHead();
            Fade();
        }

        private void OnDisable()
        {
            teleportationProvider.afterStepLocomotion -= OnTeleported;
        }

        private void OnDestroy()
        {
            Destroy(overlayMaterial);
        }

        private void EvaluateHead()
        {
            Vector3 headPosition = head.position;
            bool isOverlapping = Physics.CheckSphere(
                headPosition, headRadius, blockingLayers, QueryTriggerInteraction.Ignore);
            bool hasLineToLastValid = !Physics.Linecast(
                occlusion.LastValidPosition, headPosition, blockingLayers, QueryTriggerInteraction.Ignore);

            occlusion.Evaluate(headPosition, isOverlapping, hasLineToLastValid);
        }

        private void Fade()
        {
            float targetAlpha = occlusion.IsDark ? 1f : 0f;

            if (Mathf.Approximately(alpha, targetAlpha))
            {
                return;
            }

            float duration = occlusion.IsDark ? darkenDuration : clearDuration;
            ApplyAlpha(Mathf.MoveTowards(alpha, targetAlpha, Time.unscaledDeltaTime / duration));
        }

        private void ApplyAlpha(float value)
        {
            alpha = value;
            overlayMaterial.SetColor(ColorId, new Color(0f, 0f, 0f, alpha));
            overlay.enabled = alpha > 0f;
        }

        private void OnTeleported(LocomotionProvider _)
        {
            occlusion.Reset(head.position);
            ApplyAlpha(0f);
        }
    }
}
