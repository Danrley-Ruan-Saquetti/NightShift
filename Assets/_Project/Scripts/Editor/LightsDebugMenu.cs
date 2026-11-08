using UnityEditor;
using UnityEngine;

namespace NightShift
{
    [InitializeOnLoad]
    public static class LightsDebugMenu
    {
        private const string FlickerMenuPath = "NightShift/Debug/Flicker Current Room";
        private const string ToggleMenuPath = "NightShift/Debug/Toggle Current Room Light";
        private const float FlickerDuration = 5f;

        private static PlayerState watchedPlayer;

        static LightsDebugMenu()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        [MenuItem(FlickerMenuPath)]
        private static void FlickerCurrentRoom()
        {
            FindCurrentRoomLights()?.Flicker(FlickerDuration);
        }

        [MenuItem(ToggleMenuPath)]
        private static void ToggleCurrentRoomLight()
        {
            FindCurrentRoomLights()?.Toggle();
        }

        [MenuItem(FlickerMenuPath, true)]
        [MenuItem(ToggleMenuPath, true)]
        private static bool IsPlaying()
        {
            return EditorApplication.isPlaying;
        }

        private static RoomLights FindCurrentRoomLights()
        {
            PlayerState playerState = Object.FindFirstObjectByType<PlayerState>();
            RoomLightsRegistry registry = Object.FindFirstObjectByType<RoomLightsRegistry>();

            if (playerState == null || registry == null)
            {
                Debug.LogWarning("PlayerState or RoomLightsRegistry was not found in the open scene");
                return null;
            }

            return registry.Get(playerState.CurrentRoom);
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange change)
        {
            if (change == PlayModeStateChange.EnteredPlayMode)
            {
                watchedPlayer = Object.FindFirstObjectByType<PlayerState>();

                if (watchedPlayer != null)
                {
                    watchedPlayer.HidingChanged += OnHidingChanged;
                }

                return;
            }

            if (change == PlayModeStateChange.ExitingPlayMode && watchedPlayer != null)
            {
                watchedPlayer.HidingChanged -= OnHidingChanged;
                watchedPlayer = null;
            }
        }

        private static void OnHidingChanged()
        {
            string closetName = watchedPlayer.IsHidden ? $" ({watchedPlayer.CurrentCloset.name})" : string.Empty;
            Debug.Log($"Hidden: {watchedPlayer.IsHidden.ToString().ToLowerInvariant()}{closetName}");
        }
    }
}
