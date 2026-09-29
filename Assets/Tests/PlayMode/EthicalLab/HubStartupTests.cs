using System.Collections;
using EthicalLab.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace EthicalLab.Tests
{
    public sealed class HubStartupTests
    {
        [UnityTest]
        public IEnumerator BootLoadsOnePlayableOfficeAndWaitsForEntry()
        {
            yield return SceneManager.LoadSceneAsync("Boot");
            float deadline = Time.realtimeSinceStartup + 20f;
            while (SceneManager.GetActiveScene().name != "Hub" && Time.realtimeSinceStartup < deadline)
                yield return null;
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("Hub"));
            yield return null;
            Assert.That(Object.FindObjectsByType<LabBootstrap>(FindObjectsSortMode.None).Length, Is.EqualTo(1));
            Assert.That(Object.FindObjectsByType<PcInteractor>(FindObjectsSortMode.None).Length, Is.EqualTo(1));
            Assert.That(Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None).Length, Is.EqualTo(1));
            var refs = Object.FindFirstObjectByType<HubSceneRefs>();
            Assert.That(refs.IsValid, Is.True);
            Assert.That(refs.person.MenuOpen, Is.True, "No caminar detrás de la bienvenida.");
            var start = Object.FindFirstObjectByType<HubStartScreen>();
            Assert.That(start.IsVisible, Is.True);
            foreach (var label in Object.FindObjectsByType<TMPro.TMP_Text>(FindObjectsSortMode.None))
                Assert.That(label.font, Is.Not.Null, "Las fuentes de la interfaz deben estar disponibles.");
            CaptureCanvas(refs.mainCamera, start.GetComponent<Canvas>(), "welcome-preview.png");
            foreach (var button in start.GetComponentsInChildren<Button>())
                if (button.name == "Entrar a la oficina") button.onClick.Invoke();
            yield return null;
            Assert.That(start.IsVisible, Is.False);
            var onboarding = Object.FindFirstObjectByType<HubOnboardingOverlay>();
            var hud = Object.FindFirstObjectByType<HubHud>();
            Assert.That(onboarding.IsVisible || hud.SessionStarted, Is.True);
            if (onboarding.IsVisible)
            {
                Assert.That(refs.person.MenuOpen, Is.True, "La introducción también bloquea el movimiento.");
                bool hadKey = PlayerPrefs.HasKey(HubOnboardingOverlay.PrefsKey);
                int oldValue = PlayerPrefs.GetInt(HubOnboardingOverlay.PrefsKey);
                bool hadWelcome = PlayerPrefs.HasKey(HubGuide.WelcomePrefsKey);
                int oldWelcome = PlayerPrefs.GetInt(HubGuide.WelcomePrefsKey);
                try
                {
                    onboarding.GetComponentInChildren<Button>().onClick.Invoke();
                    yield return null;
                    Assert.That(hud.SessionStarted, Is.True);
                    Assert.That(refs.person.MenuOpen, Is.False);
                }
                finally
                {
                    if (hadKey) PlayerPrefs.SetInt(HubOnboardingOverlay.PrefsKey, oldValue);
                    else PlayerPrefs.DeleteKey(HubOnboardingOverlay.PrefsKey);
                    if (hadWelcome) PlayerPrefs.SetInt(HubGuide.WelcomePrefsKey, oldWelcome);
                    else PlayerPrefs.DeleteKey(HubGuide.WelcomePrefsKey);
                    PlayerPrefs.Save();
                }
            }
            LogAssert.NoUnexpectedReceived();
        }

        internal static void CaptureCanvas(Camera camera, Canvas canvas, string filename, int width = 1600, int height = 900)
        {
            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null) return;
            var target = new RenderTexture(width, height, 24);
            var pixels = new Texture2D(width, height, TextureFormat.RGB24, false);
            var previous = RenderTexture.active;
            try
            {
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = camera;
                canvas.planeDistance = 0.2f;
                camera.targetTexture = target;
                Canvas.ForceUpdateCanvases();
                camera.Render();
                RenderTexture.active = target;
                pixels.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                pixels.Apply();
                System.IO.Directory.CreateDirectory("docs/previews");
                System.IO.File.WriteAllBytes("docs/previews/" + filename, pixels.EncodeToPNG());
            }
            finally
            {
                camera.targetTexture = null;
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.worldCamera = null;
                RenderTexture.active = previous;
                Object.Destroy(target);
                Object.Destroy(pixels);
            }
        }

        [UnityTest]
        public IEnumerator ReloadingHubDoesNotDuplicateWorldAndScreenRemainsUsable()
        {
            yield return SceneManager.LoadSceneAsync("Hub");
            yield return null;
            yield return SceneManager.LoadSceneAsync("Hub");
            yield return null;
            Assert.That(Object.FindObjectsByType<HubSceneRefs>(FindObjectsSortMode.None).Length, Is.EqualTo(1));
            Assert.That(Object.FindObjectsByType<LabBootstrap>(FindObjectsSortMode.None).Length, Is.EqualTo(1));
            Physics.SyncTransforms();
            var screen = GameObject.Find("Laptop display").transform.position;
            Assert.That(Physics.Raycast(screen + Vector3.back * 0.8f, Vector3.forward, out var hit, 2f,
                Physics.DefaultRaycastLayers), Is.True);
            var target = hit.collider.GetComponent<InteractableView>();
            Assert.That(target, Is.Not.Null, "La decoración de la pantalla no debe interceptar el uso.");
            Assert.That(target.Id.Kind, Is.EqualTo("laptop"));
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator ModeledDoorBlocksWhenClosedAndAllowsPassageWhenOpen()
        {
            yield return SceneManager.LoadSceneAsync("Hub");
            yield return null;
            var refs = Object.FindFirstObjectByType<HubSceneRefs>();
            var controller = refs.person.GetComponent<CharacterController>();
            var doorCenter = refs.door.GetComponent<Collider>().bounds.center;
            controller.enabled = false;
            refs.person.transform.position = new Vector3(doorCenter.x - 1.3f, 0.05f, doorCenter.z);
            controller.enabled = true;
            Physics.SyncTransforms();
            controller.Move(new Vector3(2.5f, 0, 0));
            Assert.That(refs.person.transform.position.x, Is.LessThan(doorCenter.x - 0.2f));
            refs.door.Toggle();
            yield return new WaitForSeconds(1);
            Physics.SyncTransforms();
            controller.Move(new Vector3(2.5f, 0, 0));
            Assert.That(refs.person.transform.position.x, Is.GreaterThan(doorCenter.x + 0.7f));
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator WindowsHaveClearGlassRealOpeningsAndPlayerContainment()
        {
            yield return SceneManager.LoadSceneAsync("Hub");
            yield return null;
            Physics.SyncTransforms();
            var refs = Object.FindFirstObjectByType<HubSceneRefs>();
            var quality = refs.GetComponent<OfficeRenderQuality>();
            Assert.That(quality.sky, Is.Not.Null);
            Assert.That(RenderSettings.skybox, Is.EqualTo(quality.sky));
            Assert.That(refs.mainCamera.clearFlags, Is.EqualTo(CameraClearFlags.Skybox));
            int windows = 0;
            foreach (var renderer in refs.GetComponentsInChildren<Renderer>())
            {
                if (renderer.name != "Transparent glazing") continue;
                windows++;
                Assert.That(renderer.sharedMaterial.color.a, Is.LessThan(0.15f));
                Assert.That(renderer.sharedMaterial.renderQueue, Is.EqualTo(3000));
                Assert.That(renderer.GetComponent<BoxCollider>().enabled, Is.True);
                var center = renderer.bounds.center;
                // Debe encontrar el vidrio desde dentro, no un muro opaco.
                Assert.That(Physics.Raycast(center + Vector3.right * 0.5f, Vector3.left, out var hit, 0.8f), Is.True);
                Assert.That(hit.collider.gameObject, Is.EqualTo(renderer.gameObject));
                // No hay pared sólida inmediatamente detrás del vidrio.
                Assert.That(Physics.Raycast(center + Vector3.left * 0.03f, Vector3.left, 1f), Is.False);
            }
            Assert.That(windows, Is.EqualTo(4));
            Assert.That(refs.transform.Find("Exterior courtyard and city"), Is.Not.Null);
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator AuthoredTexturesAreLoadedOnOfficeAndCharacterClothing()
        {
            yield return SceneManager.LoadSceneAsync("Hub");
            yield return null;
            foreach (string name in new[] { "oak", "fabric", "plaster" })
            {
                var texture = Resources.Load<Texture2D>("OfficeTextures/" + name);
                Assert.That(texture, Is.Not.Null, name);
                Assert.That(texture.width, Is.EqualTo(1024));
                Assert.That(texture.wrapMode, Is.EqualTo(TextureWrapMode.Repeat));
                Assert.That(texture.mipmapCount, Is.GreaterThan(1));
            }
            var refs = Object.FindFirstObjectByType<HubSceneRefs>();
            int clothing = 0, surfaces = 0;
            foreach (var renderer in refs.GetComponentsInChildren<Renderer>())
                foreach (var material in renderer.sharedMaterials)
                {
                    string name = material.name.ToLowerInvariant();
                    bool isClothing = name.Contains("jacket") || name.Contains("shirt") || name.Contains("trousers");
                    if (isClothing)
                    {
                        clothing++;
                        Assert.That(material.mainTexture, Is.EqualTo(Resources.Load<Texture2D>("OfficeTextures/fabric")), name);
                        Assert.That(material.GetFloat("_TextureStrength"), Is.GreaterThan(0));
                    }
                    if (material.shader.name == "EthicalLab/Office Surface" && material.GetFloat("_TextureStrength") > 0)
                    {
                        surfaces++;
                        Assert.That(material.mainTexture, Is.Not.Null, name);
                    }
                }
            Assert.That(clothing, Is.GreaterThanOrEqualTo(12));
            Assert.That(surfaces, Is.GreaterThan(200));
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator BakedGeometryAndSurfaceShadersSurviveSceneLoading()
        {
            yield return SceneManager.LoadSceneAsync("Hub");
            yield return null;
            var refs = Object.FindFirstObjectByType<HubSceneRefs>();
            int surfaces = 0;
            foreach (var filter in refs.GetComponentsInChildren<MeshFilter>())
                Assert.That(filter.sharedMesh, Is.Not.Null, filter.name);
            foreach (var renderer in refs.GetComponentsInChildren<Renderer>())
                foreach (var material in renderer.sharedMaterials)
                {
                    Assert.That(material, Is.Not.Null, renderer.name);
                    Assert.That(material.shader.name, Is.Not.EqualTo("Hidden/InternalErrorShader"), renderer.name);
                    if (material.shader.name == "EthicalLab/Office Surface") surfaces++;
                }
            Assert.That(surfaces, Is.GreaterThan(300));
            Assert.That(QualitySettings.antiAliasing, Is.EqualTo(4));
            LogAssert.NoUnexpectedReceived();
        }
    }
}
