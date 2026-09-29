using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.Rendering;

namespace EthicalLab.Editor
{
    /// <summary>Build canónico: nunca incluye la escena Academy congelada.</summary>
    public static class HubDesktopBuild
    {
        [MenuItem("EthicalLab/Build/Linux first person")]
        public static void Linux() => Build(BuildTarget.StandaloneLinux64, "Builds/Linux/AnalystAcademy.x86_64");

        [MenuItem("EthicalLab/Build/Windows first person")]
        public static void Windows() => Build(BuildTarget.StandaloneWindows64, "Builds/Windows/AnalystAcademy.exe");

        [MenuItem("EthicalLab/Build/Mac universal first person")]
        public static void Mac()
        {
            var standalone = NamedBuildTarget.Standalone;
            // Unity 6.6 usa UserBuildSettings; SetArchitecture por sí solo deja ARM64.
            // Reflexión mantiene el proyecto compilable en editores sin el módulo Mac.
            var settings = Type.GetType("UnityEditor.OSXStandalone.UserBuildSettings, UnityEditor.OSXStandalone.Extensions");
            var architecture = settings?.GetProperty("architecture", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            if (architecture == null || !architecture.CanWrite)
                throw new InvalidOperationException("No está disponible la configuración de arquitectura del módulo Mac de Unity 6.6.");
            object previousArchitecture = architecture.GetValue(null);
            var previousBackend = PlayerSettings.GetScriptingBackend(standalone);
            bool previousDefaultApi = PlayerSettings.GetUseDefaultGraphicsAPIs(BuildTarget.StandaloneOSX);
            var previousApis = PlayerSettings.GetGraphicsAPIs(BuildTarget.StandaloneOSX);
            try
            {
                architecture.SetValue(null, Enum.Parse(architecture.PropertyType, "x64ARM64"));
                PlayerSettings.SetScriptingBackend(standalone, ScriptingImplementation.Mono2x);
                PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.StandaloneOSX, false);
                PlayerSettings.SetGraphicsAPIs(BuildTarget.StandaloneOSX, new[] { GraphicsDeviceType.Metal });
                Build(BuildTarget.StandaloneOSX, "Builds/Mac/AnalystAcademy.app");
                ValidateMacUniversal("Builds/Mac/AnalystAcademy.app");
            }
            finally
            {
                architecture.SetValue(null, previousArchitecture);
                PlayerSettings.SetScriptingBackend(standalone, previousBackend);
                PlayerSettings.SetGraphicsAPIs(BuildTarget.StandaloneOSX, previousApis);
                PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.StandaloneOSX, previousDefaultApi);
            }
        }

        static void ValidateMacUniversal(string app)
        {
            string executable = Path.Combine(app, "Contents/MacOS", PlayerSettings.productName);
            using (var stream = File.OpenRead(executable))
            using (var reader = new BinaryReader(stream))
            {
                // Mach-O fat header, big-endian; validar ambas CPUs, no solo el nombre del build.
                if (ReadBigEndian(reader) != 0xcafebabe)
                    throw new InvalidOperationException("El ejecutable generado no es Mach-O universal.");
                uint count = ReadBigEndian(reader);
                bool intel = false, arm = false;
                for (uint i = 0; i < count; i++)
                {
                    uint cpu = ReadBigEndian(reader);
                    intel |= cpu == 0x01000007;
                    arm |= cpu == 0x0100000c;
                    reader.ReadBytes(16);
                }
                if (!intel || !arm) throw new InvalidOperationException("Falta Intel o Apple Silicon en el ejecutable.");
            }
            Debug.Log("MAC UNIVERSAL VERIFIED: x86_64 + arm64");
        }

        static uint ReadBigEndian(BinaryReader reader)
        {
            var b = reader.ReadBytes(4);
            if (b.Length != 4) throw new EndOfStreamException();
            return ((uint)b[0] << 24) | ((uint)b[1] << 16) | ((uint)b[2] << 8) | b[3];
        }

        static void Build(BuildTarget target, string output)
        {
            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Standalone, target))
                throw new InvalidOperationException("Instala el módulo " + target + " de este editor desde Unity Hub.");
            if (!UnityEngine.Application.isBatchMode &&
                !UnityEditor.SceneManagement.EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (Resources.Load<TMPro.TMP_Settings>("TMP Settings") == null)
                throw new InvalidOperationException("Faltan los recursos TMP. Importa TMP Essential Resources antes del build.");
            PlayerSettings.defaultScreenWidth = 1600;
            PlayerSettings.defaultScreenHeight = 900;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.resizableWindow = true;
            PlayerSettings.runInBackground = false;
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { "Assets/Scenes/Boot.unity", "Assets/Scenes/Hub.unity" },
                target = target,
                locationPathName = output,
                options = BuildOptions.None
            });
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Build fallido: " + report.summary.totalErrors + " errores.");
            Debug.Log("FIRST PERSON BUILD READY: " + Path.GetFullPath(output));
        }
    }
}
