using System.IO;
using System.Collections.Generic;
using EthicalLab.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EthicalLab.Editor
{
    public static class HubOfficeBakeTool
    {
        const string PrefabPath = "Assets/Art/Office/Prefabs/Office_AnalystAcademy.prefab";
        const string ArtScenePath = "Assets/Scenes/Hub_Art.unity";
        const string MaterialsPath = "Assets/Art/Office/Materials/Generated";

        [MenuItem("EthicalLab/Office/Bake modeled office prefab")]
        [MenuItem("EthicalLab/Office/Bake procedural greybox prefab")]
        public static void BakePrefab()
        {
            if (!UnityEngine.Application.isBatchMode && string.IsNullOrEmpty(SceneManager.GetActiveScene().path))
            {
                EditorUtility.DisplayDialog("Guarda la escena", "Guarda la escena actual o abre Hub antes de generar el prefab.", "Entendido");
                return;
            }
            // En batchmode la escena inicial no tiene ruta: no admite otra escena aditiva.
            var previous = SceneManager.GetActiveScene();
            bool untitled = string.IsNullOrEmpty(previous.path);
            Scene temporary = untitled
                ? EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single)
                : EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            try
            {
                SceneManager.SetActiveScene(temporary);
                var built = HubOffice.Build();
                built.Root.name = "Office_Art";
                PersistMaterials(built.Root);
                PersistMeshes(built.Root);
                Directory.CreateDirectory(Path.GetDirectoryName(PrefabPath));
                PrefabUtility.SaveAsPrefabAsset(built.Root.gameObject, PrefabPath);
                AssetDatabase.SaveAssets();
            }
            finally
            {
                if (!untitled)
                {
                    EditorSceneManager.CloseScene(temporary, true);
                    if (previous.IsValid() && previous.isLoaded) SceneManager.SetActiveScene(previous);
                }
            }
            Debug.Log("Oficina modelada guardada: " + PrefabPath);
        }

        [MenuItem("EthicalLab/Office/Create or refresh Hub_Art scene")]
        public static void CreateArtScene()
        {
            if (!UnityEngine.Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            BakePrefab();
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (prefab == null) return;

            var scene = EditorSceneManagerHelper.OpenOrCreate(ArtScenePath);
            var existing = GameObject.Find("Office_Art");
            if (existing != null) Object.DestroyImmediate(existing);

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.name = "Office_Art";
            ApplyEnvironment(instance);
            EditorSceneManagerHelper.Save(scene);
            Debug.Log("Escena " + ArtScenePath + " lista. Play aquí para probar arte + mismas reglas.");
        }

        /// <summary>
        /// Hub es la escena del ejecutable. Reemplaza la instancia vieja para que
        /// sillas físicas y el coach no queden fuera del prefab horneado.
        /// </summary>
        public static void RefreshPlayableHub()
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/Hub.unity", OpenSceneMode.Single);
            PlaceCurrentPrefab(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Hub actualizado con el prefab de oficina.");
        }

        static void PlaceCurrentPrefab(Scene scene)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            var roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                if (roots[i].GetComponentInChildren<HubSceneRefs>(true) != null)
                    Object.DestroyImmediate(roots[i]);
            }
            if (prefab == null) return;
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            instance.name = "Office_Art";
            ApplyEnvironment(instance);
        }

        static void ApplyEnvironment(GameObject instance)
        {
            OfficeRenderQuality.Apply(instance.GetComponentInChildren<Camera>());
            RenderSettings.skybox = instance.GetComponent<OfficeRenderQuality>().sky;
        }

        static void PersistMeshes(Transform root)
        {
            const string directory = "Assets/Art/Office/Meshes/Generated";
            Directory.CreateDirectory(directory);
            AssetDatabase.Refresh();
            var saved = new Dictionary<Mesh, Mesh>();
            foreach (var filter in root.GetComponentsInChildren<MeshFilter>(true))
            {
                var mesh = filter.sharedMesh;
                if (mesh == null || EditorUtility.IsPersistent(mesh)) continue;
                if (!saved.TryGetValue(mesh, out var asset))
                {
                    string path = directory + "/" + mesh.name + ".asset";
                    asset = AssetDatabase.LoadAssetAtPath<Mesh>(path);
                    if (asset == null)
                    {
                        asset = Object.Instantiate(mesh);
                        asset.name = mesh.name;
                        AssetDatabase.CreateAsset(asset, path);
                    }
                    else EditorUtility.CopySerialized(mesh, asset);
                    saved.Add(mesh, asset);
                }
                filter.sharedMesh = asset;
            }
        }

        static void PersistMaterials(Transform root)
        {
            Directory.CreateDirectory(MaterialsPath);
            AssetDatabase.Refresh();
            var saved = new Dictionary<Material, Material>();
            foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                var materials = renderer.sharedMaterials;
                for (int i = 0; i < materials.Length; i++)
                {
                    var material = materials[i];
                    if (material == null || EditorUtility.IsPersistent(material)) continue;
                    if (!saved.TryGetValue(material, out var asset))
                    {
                        string path = MaterialsPath + "/" + material.name.Replace('/', '-') + ".mat";
                        asset = AssetDatabase.LoadAssetAtPath<Material>(path);
                        if (asset == null)
                        {
                            asset = new Material(material);
                            AssetDatabase.CreateAsset(asset, path);
                        }
                        else EditorUtility.CopySerialized(material, asset);
                        saved.Add(material, asset);
                    }
                    materials[i] = asset;
                }
                renderer.sharedMaterials = materials;
            }
            foreach (var transient in saved.Keys) Object.DestroyImmediate(transient);
            var quality = root.GetComponent<OfficeRenderQuality>();
            if (quality != null && quality.sky != null && !EditorUtility.IsPersistent(quality.sky))
            {
                string path = MaterialsPath + "/Office afternoon sky.mat";
                var sky = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (sky == null)
                {
                    sky = new Material(quality.sky);
                    AssetDatabase.CreateAsset(sky, path);
                }
                else EditorUtility.CopySerialized(quality.sky, sky);
                Object.DestroyImmediate(quality.sky);
                quality.sky = sky;
                RenderSettings.skybox = sky;
            }
        }

        [MenuItem("EthicalLab/Play from Boot")]
        public static void PlayFromBoot()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EditorSceneManager.OpenScene("Assets/Scenes/Boot.unity");
            EditorApplication.isPlaying = true;
        }
    }

    /// <summary>Evita dependencia circular con UnityEditor.SceneManagement en runtime.</summary>
    static class EditorSceneManagerHelper
    {
        public static Scene OpenOrCreate(string path)
        {
            if (File.Exists(path))
                return EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorSceneManager.SaveScene(scene, path);
            return EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
        }

        public static void Save(Scene scene) => EditorSceneManager.SaveScene(scene);
    }
}
