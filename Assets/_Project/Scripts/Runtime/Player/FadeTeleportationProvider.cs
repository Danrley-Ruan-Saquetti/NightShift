using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

namespace NightShift
{
    public class FadeTeleportationProvider : TeleportationProvider
    {
        private const float MaxTeleportWait = 1f;

        [SerializeField]
        private float fadeOutDuration = 0.1f;
        [SerializeField]
        private float fadeInDuration = 0.15f;
        [SerializeField]
        private Transform head;
        [SerializeField]
        private float maxHorizontalDistance = 2.5f;

        private bool isFading;
        private bool hasTeleported;

        public bool IsFading => isFading;

        public bool IsWithinRange(Vector3 destination)
        {
            Vector3 offset = destination - head.position;
            offset.y = 0f;

            return offset.sqrMagnitude <= maxHorizontalDistance * maxHorizontalDistance;
        }

        public override bool QueueTeleportRequest(TeleportRequest teleportRequest)
        {
            if (!IsWithinRange(teleportRequest.destinationPosition))
            {
                return false;
            }

            SceneTransition transition = SceneTransition.Instance;
            if (transition == null)
            {
                return base.QueueTeleportRequest(teleportRequest);
            }

            if (isFading || transition.IsTransitioning)
            {
                return false;
            }

            StartCoroutine(FadeAndTeleport(transition, teleportRequest));
            return true;
        }

        private IEnumerator FadeAndTeleport(SceneTransition transition, TeleportRequest teleportRequest)
        {
            isFading = true;
            hasTeleported = false;
            yield return transition.FadeScreen(1f, fadeOutDuration);

            locomotionEnded += OnTeleportEnded;
            float waited = 0f;

            if (QueueRequest(teleportRequest))
            {
                while (!hasTeleported && waited < MaxTeleportWait)
                {
                    waited += Time.unscaledDeltaTime;
                    yield return null;
                }
            }

            locomotionEnded -= OnTeleportEnded;
            yield return transition.FadeScreen(0f, fadeInDuration);
            isFading = false;
        }

        private bool QueueRequest(TeleportRequest teleportRequest)
        {
            return base.QueueTeleportRequest(teleportRequest);
        }

        private void OnTeleportEnded(LocomotionProvider _)
        {
            hasTeleported = true;
        }
    }
}
