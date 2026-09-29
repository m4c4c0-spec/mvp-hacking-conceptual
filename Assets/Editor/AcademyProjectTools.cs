using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Academy.Editor
{
    [InitializeOnLoad]
    public static class AcademyProjectTools
    {
        private const string ScenePath = "Assets/Scenes/Academy.unity";
        static AcademyProjectTools() { EditorApplication.delayCall += Initialize; }

        private static void Initialize()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling) return;
            // First import only: never overwrite later customization of missions or player settings.
            if (!File.Exists("ProjectSettings/Academy.initialized"))
            {
                Configure();
                File.WriteAllText("ProjectSettings/Academy.initialized", "Academy project initialized. Delete this marker to apply defaults again.\n");
            }
            EnsureMaterial();
            if (AssetDatabase.LoadAssetAtPath<MissionCatalog>("Assets/Resources/MissionCatalog.asset") == null) ImportContent();
        }

        [MenuItem("Academy/Abrir escena principal")]
        public static void OpenScene()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EditorSceneManager.OpenScene(ScenePath);
        }

        [MenuItem("Academy/Aplicar configuración del proyecto")]
        public static void Configure()
        {
            PlayerSettings.companyName = "BlueRedAcademy";
            PlayerSettings.productName = "Blue Red Analyst Academy";
            PlayerSettings.bundleVersion = "0.1.0";
            PlayerSettings.defaultScreenWidth = 1600; PlayerSettings.defaultScreenHeight = 1000;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.resizableWindow = true; PlayerSettings.runInBackground = false;
            PlayerSettings.colorSpace = ColorSpace.Linear;
            EditorSettings.serializationMode = SerializationMode.ForceText;
            QualitySettings.vSyncCount = 1;
            if (EditorBuildSettings.scenes.Length == 0)
                EditorBuildSettings.scenes = new[]
                {
                    new EditorBuildSettingsScene("Assets/Scenes/Boot.unity", true),
                    new EditorBuildSettingsScene("Assets/Scenes/Hub.unity", true),
                    new EditorBuildSettingsScene(ScenePath, false)
                };
            EnsureMaterial();
            AssetDatabase.SaveAssets();
            Debug.Log("Proyecto configurado. Abre Assets/Scenes/Boot.unity para iniciar Analyst Academy.");
        }

        private static void EnsureMaterial()
        {
            const string path = "Assets/Resources/AcademySurface.mat";
            if (AssetDatabase.LoadAssetAtPath<Material>(path) != null) return;
            var shader = Shader.Find("Standard");
            if (shader == null) throw new InvalidOperationException("Este proyecto usa Built-in Render Pipeline y necesita el shader Standard.");
            AssetDatabase.CreateAsset(new Material(shader) { name = "AcademySurface" }, path);
            AssetDatabase.SaveAssets();
        }

        [MenuItem("Academy/Contenido/Importar JSON a ScriptableObjects (conserva existentes)")]
        public static void ImportContent()
        {
            var text = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Resources/Content/missions.json");
            var data = JsonUtility.FromJson<ContentData>(text.text);
            if (!AssetDatabase.IsValidFolder("Assets/Content")) AssetDatabase.CreateFolder("Assets", "Content");
            if (!AssetDatabase.IsValidFolder("Assets/Content/Missions")) AssetDatabase.CreateFolder("Assets/Content", "Missions");
            if (!AssetDatabase.IsValidFolder("Assets/Content/Concepts")) AssetDatabase.CreateFolder("Assets/Content", "Concepts");
            var catalog = AssetDatabase.LoadAssetAtPath<MissionCatalog>("Assets/Resources/MissionCatalog.asset");
            bool newCatalog = catalog == null;
            if (newCatalog) catalog = ScriptableObject.CreateInstance<MissionCatalog>();
            var missions = data.missions.Select(m =>
            {
                string path = "Assets/Content/Missions/" + m.id + ".asset";
                var asset = AssetDatabase.LoadAssetAtPath<MissionDefinition>(path);
                if (asset == null) { asset = ScriptableObject.CreateInstance<MissionDefinition>(); asset.data = m; AssetDatabase.CreateAsset(asset, path); }
                return asset;
            }).ToArray();
            var concepts = data.concepts.Select(c =>
            {
                string path = "Assets/Content/Concepts/" + c.id + ".asset";
                var asset = AssetDatabase.LoadAssetAtPath<ConceptDefinition>(path);
                if (asset == null) { asset = ScriptableObject.CreateInstance<ConceptDefinition>(); asset.data = c; AssetDatabase.CreateAsset(asset, path); }
                return asset;
            }).ToArray();
            // Reimport adds missing entries but preserves designer-added assets and ordering.
            catalog.missions = (catalog.missions ?? Array.Empty<MissionDefinition>()).Concat(missions).Where(x => x != null).Distinct().ToArray();
            catalog.concepts = (catalog.concepts ?? Array.Empty<ConceptDefinition>()).Concat(concepts).Where(x => x != null).Distinct().ToArray();
            if (newCatalog) AssetDatabase.CreateAsset(catalog, "Assets/Resources/MissionCatalog.asset");
            else EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
            Debug.Log("Contenido listo: " + catalog.missions.Length + " misiones y " + catalog.concepts.Length + " conceptos. Editables desde el Inspector.");
        }

        [MenuItem("Academy/Build/Linux x86_64")]
        public static void BuildLinux() => Build(BuildTarget.StandaloneLinux64, "Builds/Linux/AnalystAcademy.x86_64");
        [MenuItem("Academy/Build/Windows x86_64")]
        public static void BuildWindows() => Build(BuildTarget.StandaloneWindows64, "Builds/Windows/AnalystAcademy.exe");

        private static void Build(BuildTarget target, string path)
        {
            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Standalone, target))
                throw new InvalidOperationException("Instala el módulo de build " + target + " en Unity Hub para este editor.");
            Initialize(); EnsureMaterial();
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes = new[] { ScenePath }, locationPathName = path, target = target, options = BuildOptions.None
            });
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Build fallido: " + report.summary.result + " / " + report.summary.totalErrors + " errores");
            Debug.Log("Build listo: " + Path.GetFullPath(path));
        }
    }
}
