using System;
using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;

namespace NightShift
{
    public class GameOver : MonoBehaviour
    {
        [SerializeField]
        private Transform head;
        [SerializeField]
        private PlayerState playerState;
        [SerializeField]
        private FadeTeleportationProvider teleportationProvider;
        [SerializeField]
        private RoomLightsRegistry roomLights;
        [SerializeField]
        private JumpscareFigure figure;
        [SerializeField]
        private LayerMask obstacleMask;
        [SerializeField]
        private float scareDelay = 0.1f;
        [SerializeField]
        private float scareDuration = 2f;
        [SerializeField]
        private float blackHoldDuration = 1f;
        [SerializeField]
        private float figureDistance = 0.6f;
        [SerializeField]
        private float advanceDistance = 0.1f;
        [SerializeField]
        private float advanceDuration = 0.3f;
        [SerializeField]
        private float obstacleMargin = 0.1f;
        [SerializeField]
        private float minimumDistance = 0.35f;
        [SerializeField]
        private float minimumAdvancedDistance = 0.3f;
        [SerializeField]
        private float closetDoorAngle = 90f;
        [SerializeField]
        private float closetDoorDuration = 0.08f;
        [SerializeField]
        private float hapticAmplitude = 1f;
        [SerializeField]
        private float hapticDuration = 1.5f;
        [SerializeField]
        private float maxTeleportWait = 1.5f;

        public bool IsRunning { get; private set; }

        public event Action<GameOverReason> Triggered;

        public void Trigger(GameOverReason reason)
        {
            SceneTransition transition = SceneTransition.Instance;

            if (IsRunning)
            {
                return;
            }

            if (transition == null)
            {
                Debug.LogError("SceneTransition is missing, game over cannot reload the scene", this);
                return;
            }

            if (transition.IsTransitioning)
            {
                return;
            }

            IsRunning = true;
            Triggered?.Invoke(reason);
            StartCoroutine(Run(transition));
        }

        private IEnumerator Run(SceneTransition transition)
        {
            transition.BlockInteractors();
            AudioListener.pause = true;
            SwitchOffCurrentRoomLight();

            Closet closet = playerState.FindClosetAroundHead();
            if (closet != null)
            {
                closet.BurstOpen(closetDoorAngle, closetDoorDuration);
            }

            yield return new WaitForSecondsRealtime(scareDelay);

            float waited = 0f;
            while (teleportationProvider.IsFading && waited < maxTeleportWait)
            {
                waited += Time.unscaledDeltaTime;
                yield return null;
            }

            ShowFigure(closet);
            SendHaptics();

            yield return new WaitForSecondsRealtime(scareDuration);
            transition.ReloadCurrentScene(blackHoldDuration);
        }

        private void SwitchOffCurrentRoomLight()
        {
            if (playerState.CurrentRoom == Room.Porch)
            {
                return;
            }

            RoomLights lights = roomLights.Get(playerState.CurrentRoom);
            if (lights != null)
            {
                lights.SetOn(false);
            }
        }

        private void ShowFigure(Closet closet)
        {
            Vector3? exit = closet != null ? closet.ExitDirection : (Vector3?)null;
            Vector3 direction = JumpscarePlacement.ChooseDirection(head.rotation, exit);
            float obstacleDistance = closet != null ? float.PositiveInfinity : FindObstacleDistance(direction);
            float start = JumpscarePlacement.ResolveDistance(figureDistance, obstacleDistance, obstacleMargin, minimumDistance);
            float end = JumpscarePlacement.AdvancedDistance(start, advanceDistance, minimumAdvancedDistance);
            figure.Show(head.position, direction, start, end, advanceDuration);
        }

        private float FindObstacleDistance(Vector3 direction)
        {
            bool hit = Physics.Raycast(head.position, direction, out RaycastHit info, figureDistance, obstacleMask, QueryTriggerInteraction.Ignore);
            return hit ? info.distance : float.PositiveInfinity;
        }

        private void SendHaptics()
        {
            foreach (HapticImpulsePlayer player in FindObjectsByType<HapticImpulsePlayer>(FindObjectsSortMode.None))
            {
                player.SendHapticImpulse(hapticAmplitude, hapticDuration);
            }
        }
    }
}
