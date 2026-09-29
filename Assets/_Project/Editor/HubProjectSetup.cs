using System.IO;
using UnityEditor;
using UnityEngine;

namespace EthicalLab.Editor
{
    [InitializeOnLoad]
    public static class HubProjectSetup
    {
        static HubProjectSetup() => EditorApplication.delayCall += EnsureUiResources;

        // TMP requiere fuentes/settings adicionales al paquete de código uGUI.
        public static void EnsureUiResources()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling) return;
            if (Resources.Load<Object>("TMP Settings") != null) return;
            var package = UnityEditor.PackageManager.PackageInfo.FindForAssetPath("Packages/com.unity.ugui/package.json");
            if (package == null) return;
            string essentials = Path.Combine(package.resolvedPath, "Package Resources", "TMP Essential Resources.unitypackage");
            if (File.Exists(essentials)) AssetDatabase.ImportPackage(essentials, false);
        }

        [MenuItem("EthicalLab/Office/Prepare modeled scene")]
        public static void PrepareModeledScene()
        {
            if (!UnityEngine.Application.isBatchMode &&
                !UnityEditor.SceneManagement.EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EnsureUiResources();
            HubOfficeBakeTool.CreateArtScene();
            HubOfficeBakeTool.RefreshPlayableHub();
        }
    }
}
