using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using Object = UnityEngine.Object;

namespace NightShift
{
    public static class RoomRenderingLayersTool
    {
        private const string MenuPath = "NightShift/Tools/Apply Room Rendering Layers";
        private const string DefaultLayer = "Default";
        private const string ClosetInteriorLayer = "ClosetInterior";
        private const string UndoName = "Apply Room Rendering Layers";

        [MenuItem(MenuPath)]
        private static void Apply()
        {
            if (!TryGetMask(DefaultLayer, out uint defaultMask) || !TryGetMask(ClosetInteriorLayer, out uint closetMask))
            {
                return;
            }

            if (!TryGetAllRoomsMask(out uint allRoomsMask))
            {
                return;
            }

            RoomLights[] roomLights = Object.FindObjectsByType<RoomLights>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            foreach (RoomLights room in roomLights)
            {
                TryGetMask(room.Room.ToString(), out uint roomMask);
                ApplyToRoomLights(room, roomMask | defaultMask);
                ApplyToRoomRenderers(room.transform, roomMask, allRoomsMask, closetMask);
            }

            ApplyToLooseRenderers(roomLights, defaultMask | allRoomsMask);
            ApplyToDirectionalLights(defaultMask | allRoomsMask | closetMask);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        [MenuItem(MenuPath, true)]
        private static bool CanApply()
        {
            return !EditorApplication.isPlaying;
        }

        private static void ApplyToRoomLights(RoomLights room, uint roomMask)
        {
            SerializedProperty lights = new SerializedObject(room).FindProperty("lights");

            for (int i = 0; i < lights.arraySize; i++)
            {
                Light roomLight = (Light)lights.GetArrayElementAtIndex(i).objectReferenceValue;
                SetLightMask(roomLight, roomMask);
            }
        }

        private static void ApplyToRoomRenderers(Transform roomRoot, uint roomMask, uint allRoomsMask, uint closetMask)
        {
            foreach (Renderer renderer in roomRoot.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer.renderingLayerMask == closetMask)
                {
                    continue;
                }

                SetRendererMask(renderer, IsSharedBetweenRooms(renderer) ? allRoomsMask : roomMask);
            }
        }

        private static void ApplyToLooseRenderers(RoomLights[] roomLights, uint looseMask)
        {
            Renderer[] renderers = Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            foreach (Renderer renderer in renderers)
            {
                if (!IsInsideAnyRoom(renderer.transform, roomLights))
                {
                    SetRendererMask(renderer, looseMask);
                }
            }
        }

        private static void ApplyToDirectionalLights(uint mask)
        {
            foreach (Light sceneLight in Object.FindObjectsByType<Light>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (sceneLight.type == LightType.Directional)
                {
                    SetLightMask(sceneLight, mask);
                }
            }
        }

        private static bool IsSharedBetweenRooms(Renderer renderer)
        {
            return renderer.GetComponentInParent<XRGrabInteractable>(true) != null
                || renderer.GetComponentInParent<HingedDoor>(true) != null;
        }

        private static bool IsInsideAnyRoom(Transform target, RoomLights[] roomLights)
        {
            foreach (RoomLights room in roomLights)
            {
                if (target.IsChildOf(room.transform))
                {
                    return true;
                }
            }

            return false;
        }

        private static void SetRendererMask(Renderer renderer, uint mask)
        {
            if (renderer.renderingLayerMask == mask)
            {
                return;
            }

            Undo.RecordObject(renderer, UndoName);
            renderer.renderingLayerMask = mask;
            PrefabUtility.RecordPrefabInstancePropertyModifications(renderer);
        }

        private static void SetLightMask(Light light, uint mask)
        {
            UniversalAdditionalLightData lightData = light.GetUniversalAdditionalLightData();
            Undo.RecordObject(light, UndoName);
            Undo.RecordObject(lightData, UndoName);
            lightData.renderingLayers = mask;
            light.renderingLayerMask = (int)mask;
            PrefabUtility.RecordPrefabInstancePropertyModifications(light);
            PrefabUtility.RecordPrefabInstancePropertyModifications(lightData);
        }

        private static bool TryGetAllRoomsMask(out uint mask)
        {
            mask = 0;

            foreach (string roomName in Enum.GetNames(typeof(Room)))
            {
                if (!TryGetMask(roomName, out uint roomMask))
                {
                    return false;
                }

                mask |= roomMask;
            }

            return true;
        }

        private static bool TryGetMask(string layerName, out uint mask)
        {
            mask = RenderingLayerMask.GetMask(layerName);

            if (mask != 0)
            {
                return true;
            }

            Debug.LogError($"Rendering layer '{layerName}' is not defined in Tags and Layers");
            return false;
        }
    }
}
