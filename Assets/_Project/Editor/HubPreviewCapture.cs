using System.IO;
using EthicalLab.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EthicalLab.Editor
{
    public static class HubPreviewCapture
    {
        [MenuItem("EthicalLab/Office/Capture modeled office preview")]
        public static void Capture() => CaptureViews("");

        public static void CaptureBefore() => CaptureViews("before-");

        static void CaptureViews(string prefix)
        {
            if (UnityEngine.Application.isBatchMode)
                EditorSceneManager.OpenScene("Assets/Scenes/Hub.unity");
            var refs = Object.FindFirstObjectByType<HubSceneRefs>();
            if (!HubSceneLoader.TryCreate(refs, out var office))
            {
                Debug.LogError("Abre Hub o Hub_Art con el prefab de oficina antes de capturar.");
                return;
            }
            var camera = office.Camera;
            var position = camera.transform.position;
            var rotation = camera.transform.rotation;
            float fov = camera.fieldOfView;
            try
            {
                if (prefix.Length == 0) OfficeRenderQuality.Apply(camera);
                foreach (var label in office.Root.GetComponentsInChildren<HubWorldLabel>()) label.FitNow();
                camera.transform.position = new Vector3(-4.4f, 2.1f, -4.8f);
                camera.transform.LookAt(new Vector3(0.6f, 1.4f, 1.5f));
                camera.fieldOfView = 72f;
                Save(camera, prefix + "office-preview.png");
                camera.transform.position = new Vector3(6.3f, 1.75f, -3.7f);
                camera.transform.LookAt(new Vector3(8.8f, 1.35f, 2.1f));
                Save(camera, prefix + "archive-preview.png");
                camera.transform.position = new Vector3(-2.5f, 1.75f, 2.8f);
                camera.transform.LookAt(new Vector3(-3.8f, 1.4f, -4.8f));
                Save(camera, prefix + "reception-preview.png");
                camera.transform.position = new Vector3(-5.75f, 1.65f, 0.35f);
                camera.transform.LookAt(new Vector3(-28f, 1.1f, 1.6f));
                camera.fieldOfView = 70f;
                Save(camera, prefix + "window-preview.png");
                if (prefix.Length == 0)
                {
                    camera.transform.position = new Vector3(-1.85f, 1.65f, -1.6f);
                    camera.transform.LookAt(new Vector3(-0.8f, 0.96f, -0.15f));
                    camera.fieldOfView = 55f;
                    Save(camera, "materials-preview.png");
                    camera.transform.position = new Vector3(-3.5f, 1.5f, 2.45f);
                    camera.transform.LookAt(new Vector3(-3.5f, 1.13f, 4.05f));
                    Save(camera, "character-preview.png");
                }
                int triangles = 0;
                foreach (var mesh in office.Root.GetComponentsInChildren<MeshFilter>())
                    if (mesh.sharedMesh != null) triangles += mesh.sharedMesh.triangles.Length / 3;
                Debug.Log("Office geometry: " + triangles + " triangles; " + office.Root.GetComponentsInChildren<Renderer>().Length + " renderers.");
            }
            finally
            {
                camera.transform.SetPositionAndRotation(position, rotation);
                camera.fieldOfView = fov;
            }
        }

        static void Save(Camera camera, string name)
        {
            var previous = RenderTexture.active;
            var render = new RenderTexture(1600, 900, 24);
            render.antiAliasing = name.StartsWith("before-") ? 1 : 4;
            var pixels = new Texture2D(1600, 900, TextureFormat.RGB24, false);
            try
            {
                camera.targetTexture = render;
                camera.Render();
                RenderTexture.active = render;
                pixels.ReadPixels(new Rect(0, 0, 1600, 900), 0, 0);
                pixels.Apply();
                Directory.CreateDirectory("docs/previews");
                File.WriteAllBytes("docs/previews/" + name, pixels.EncodeToPNG());
            }
            finally
            {
                camera.targetTexture = null;
                RenderTexture.active = previous;
                Object.DestroyImmediate(render);
                Object.DestroyImmediate(pixels);
            }
        }
    }
}
