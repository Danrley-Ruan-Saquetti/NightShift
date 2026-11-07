using UnityEditor;
using UnityEngine;

namespace NightShift
{
    public static class OfficeDoorDebugMenu
    {
        private const string MenuPath = "NightShift/Debug/Unlock P3";

        [MenuItem(MenuPath)]
        private static void UnlockOfficeDoor()
        {
            GameObject officeDoor = GameObject.Find("P3");

            if (officeDoor == null)
            {
                Debug.LogWarning("P3 was not found in the open scene");
                return;
            }

            officeDoor.GetComponentInChildren<HingedDoor>().Unlock();
        }

        [MenuItem(MenuPath, true)]
        private static bool CanUnlockOfficeDoor()
        {
            return EditorApplication.isPlaying;
        }
    }
}
