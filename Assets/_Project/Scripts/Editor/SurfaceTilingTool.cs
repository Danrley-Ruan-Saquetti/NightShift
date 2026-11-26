using System.Collections.Generic;
using System.Globalization;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NightShift
{
    public static class SurfaceTilingTool
    {
        private const string MenuPath = "NightShift/Tools/Apply Surface Tiling";
        private const string UndoName = "Apply Surface Tiling";
        private const string BaseFolder = "Assets/_Project/Materials/Surfaces";
        private const string GeneratedFolder = BaseFolder + "/Generated";
        private const string TilingProperty = "_BaseMap";
        private const float RoundingStep = 0.05f;

        private static readonly Dictionary<string, float> TileMetersByBase = new Dictionary<string, float>
        {
            { "FloorWood", 1f },
            { "FloorTileBathroom", 1.2f },
            { "FloorTileKitchen", 1.2f },
            { "WallTileBathroom", 1.2f },
            { "WallLivingRoom", 2f },
            { "WallKitchen", 2f },
            { "WallLaundry", 2f },
            { "WallHallway", 2f },
            { "WallOffice", 2f },
            { "WallBedroom", 2f },
            { "WallPorch", 2f },
            { "Ceiling", 2f },
        };

        [MenuItem(MenuPath)]
        private static void Apply()
        {
            EnsureGeneratedFolder();

            MeshRenderer[] renderers = Object.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            foreach (MeshRenderer renderer in renderers)
            {
                ApplyToRenderer(renderer);
            }

            DeleteUnusedGenerated(renderers);
            AssetDatabase.SaveAssets();
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        [MenuItem(MenuPath, true)]
        private static bool CanApply()
        {
            return !EditorApplication.isPlaying;
        }

        private static void ApplyToRenderer(MeshRenderer renderer)
        {
            Material[] materials = renderer.sharedMaterials;
            bool changed = false;

            for (int i = 0; i < materials.Length; i++)
            {
                if (!TryResolveBase(materials[i], out Material baseMaterial))
                {
                    continue;
                }

                Vector2 tiling = SurfaceTiling.ForCube(renderer.transform.lossyScale, TileMetersByBase[baseMaterial.name]);
                Material derived = GetOrCreateDerived(baseMaterial, Round(tiling.x), Round(tiling.y));

                if (materials[i] != derived)
                {
                    materials[i] = derived;
                    changed = true;
                }
            }

            if (!changed)
            {
                return;
            }

            Undo.RecordObject(renderer, UndoName);
            renderer.sharedMaterials = materials;
            PrefabUtility.RecordPrefabInstancePropertyModifications(renderer);
        }

        private static bool TryResolveBase(Material material, out Material baseMaterial)
        {
            baseMaterial = null;

            if (material == null)
            {
                return false;
            }

            string path = AssetDatabase.GetAssetPath(material);

            if (path.StartsWith(GeneratedFolder + "/"))
            {
                string baseName = material.name.Substring(0, material.name.LastIndexOf('_'));
                baseMaterial = AssetDatabase.LoadAssetAtPath<Material>($"{BaseFolder}/{baseName}.mat");
            }
            else if (path.StartsWith(BaseFolder + "/") && path.IndexOf('/', BaseFolder.Length + 1) < 0)
            {
                baseMaterial = material;
            }

            return baseMaterial != null && TileMetersByBase.ContainsKey(baseMaterial.name);
        }

        private static Material GetOrCreateDerived(Material baseMaterial, float u, float v)
        {
            string name = $"{baseMaterial.name}_{Format(u)}x{Format(v)}";
            string path = $"{GeneratedFolder}/{name}.mat";
            Material derived = AssetDatabase.LoadAssetAtPath<Material>(path);

            if (derived == null)
            {
                derived = new Material(baseMaterial) { name = name };
                AssetDatabase.CreateAsset(derived, path);
            }

            ResyncWithCalibratedBase(derived, baseMaterial, u, v);
            return derived;
        }

        private static void ResyncWithCalibratedBase(Material derived, Material baseMaterial, float u, float v)
        {
            derived.CopyPropertiesFromMaterial(baseMaterial);
            derived.SetTextureScale(TilingProperty, new Vector2(u, v));
            derived.SetTextureOffset(TilingProperty, Vector2.zero);
            EditorUtility.SetDirty(derived);
        }

        private static void DeleteUnusedGenerated(MeshRenderer[] renderers)
        {
            HashSet<string> used = new HashSet<string>();

            foreach (MeshRenderer renderer in renderers)
            {
                foreach (Material material in renderer.sharedMaterials)
                {
                    used.Add(AssetDatabase.GetAssetPath(material));
                }
            }

            foreach (string guid in AssetDatabase.FindAssets("t:Material", new[] { GeneratedFolder }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);

                if (!used.Contains(path))
                {
                    AssetDatabase.DeleteAsset(path);
                }
            }
        }

        private static void EnsureGeneratedFolder()
        {
            if (!AssetDatabase.IsValidFolder(GeneratedFolder))
            {
                AssetDatabase.CreateFolder(BaseFolder, "Generated");
            }
        }

        private static float Round(float value)
        {
            return Mathf.Round(value / RoundingStep) * RoundingStep;
        }

        private static string Format(float value)
        {
            return value.ToString("0.##", CultureInfo.InvariantCulture);
        }
    }
}
