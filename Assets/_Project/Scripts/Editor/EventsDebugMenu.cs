using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NightShift
{
    [InitializeOnLoad]
    public static class EventsDebugMenu
    {
        private const string VisitorMenuPath = "NightShift/Debug/Visitor";
        private const string ShadowMenuPath = "NightShift/Debug/Shadow";
        private const string ForceVisitorMenuPath = "NightShift/Debug/Force Visitor";
        private const string ForceShadowMenuPath = "NightShift/Debug/Force Shadow";

        private static EventDirector watchedDirector;

        static EventsDebugMenu()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        [MenuItem(VisitorMenuPath)]
        private static void RequestVisitor()
        {
            Request(EventKind.Visitor, false);
        }

        [MenuItem(ShadowMenuPath)]
        private static void RequestShadow()
        {
            Request(EventKind.Shadow, false);
        }

        [MenuItem(ForceVisitorMenuPath)]
        private static void ForceVisitor()
        {
            Request(EventKind.Visitor, true);
        }

        [MenuItem(ForceShadowMenuPath)]
        private static void ForceShadow()
        {
            Request(EventKind.Shadow, true);
        }

        [MenuItem(VisitorMenuPath, true)]
        [MenuItem(ShadowMenuPath, true)]
        [MenuItem(ForceVisitorMenuPath, true)]
        [MenuItem(ForceShadowMenuPath, true)]
        private static bool IsPlaying()
        {
            return EditorApplication.isPlaying;
        }

        private static void Request(EventKind kind, bool ignoreGap)
        {
            EventDirector director = Object.FindFirstObjectByType<EventDirector>();

            if (director == null)
            {
                Debug.LogWarning("EventDirector was not found in the open scene");
                return;
            }

            if (!director.Request(kind, ignoreGap))
            {
                Debug.LogWarning($"Director rejected {kind} (busy, in gap, teleporting, stopped or no doorway in the current room)");
            }
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange change)
        {
            if (change == PlayModeStateChange.EnteredPlayMode)
            {
                Watch(Object.FindFirstObjectByType<EventDirector>());
                SceneManager.sceneLoaded += OnSceneLoaded;
                return;
            }

            if (change == PlayModeStateChange.ExitingPlayMode)
            {
                SceneManager.sceneLoaded -= OnSceneLoaded;
                Watch(null);
            }
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Watch(Object.FindFirstObjectByType<EventDirector>());
        }

        private static void Watch(EventDirector director)
        {
            if (watchedDirector != null)
            {
                watchedDirector.Queued -= OnQueued;
                watchedDirector.Started -= OnStarted;
                watchedDirector.Ended -= OnEnded;
                watchedDirector.Failed -= OnFailed;
            }

            watchedDirector = director;

            if (watchedDirector != null)
            {
                watchedDirector.Queued += OnQueued;
                watchedDirector.Started += OnStarted;
                watchedDirector.Ended += OnEnded;
                watchedDirector.Failed += OnFailed;
            }
        }

        private static void OnQueued(EventKind kind)
        {
            Debug.Log($"Director: queued {kind} (t={Now()})");
        }

        private static void OnStarted(EventKind kind)
        {
            Debug.Log($"Director: started {kind} (t={Now()})");
            Debug.Log($"Event: {kind} start");
        }

        private static void OnEnded(EventKind kind)
        {
            Debug.Log($"Director: ended {kind} (t={Now()})");
            Debug.Log($"Event: {kind} success");
        }

        private static void OnFailed(EventKind kind, GameOverReason reason)
        {
            Debug.Log($"Event: {kind} fail ({reason})");
        }

        private static string Now()
        {
            return Time.time.ToString("F2");
        }
    }
}
