using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NightShift
{
    [InitializeOnLoad]
    public static class GameOverDebugMenu
    {
        private const string MenuPath = "NightShift/Debug/Game Over";

        private static GameOver watched;

        static GameOverDebugMenu()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        [MenuItem(MenuPath)]
        private static void TriggerGameOver()
        {
            GameOver gameOver = Object.FindFirstObjectByType<GameOver>();

            if (gameOver == null)
            {
                Debug.LogWarning("GameOver was not found in the open scene");
                return;
            }

            gameOver.Trigger(GameOverReason.Debug);
        }

        [MenuItem(MenuPath, true)]
        private static bool IsPlaying()
        {
            return EditorApplication.isPlaying;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange change)
        {
            if (change == PlayModeStateChange.EnteredPlayMode)
            {
                SceneManager.sceneLoaded += OnSceneLoaded;
                Watch();
                return;
            }

            if (change == PlayModeStateChange.ExitingPlayMode)
            {
                SceneManager.sceneLoaded -= OnSceneLoaded;
                Unwatch();
            }
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Watch();
        }

        private static void Watch()
        {
            Unwatch();
            watched = Object.FindFirstObjectByType<GameOver>();

            if (watched != null)
            {
                watched.Triggered += OnTriggered;
            }
        }

        private static void Unwatch()
        {
            if (watched != null)
            {
                watched.Triggered -= OnTriggered;
            }

            watched = null;
        }

        private static void OnTriggered(GameOverReason reason)
        {
            Debug.Log($"Game over: {reason}");
        }
    }
}
