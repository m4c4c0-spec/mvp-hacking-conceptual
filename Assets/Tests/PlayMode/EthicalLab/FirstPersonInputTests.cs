using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using EthicalLab.Application;
using EthicalLab.Infrastructure;
using EthicalLab.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace EthicalLab.Tests
{
    /// <summary>Teclado y mouse virtuales atraviesan PcButtons y Update; no mueve el controller a mano.</summary>
    public sealed class FirstPersonInputTests
    {
        Keyboard keyboard;
        Mouse mouse;
        InputTestFixture input;
        float previousCaptureDelta;
        readonly string[] preferenceKeys = { HubOnboardingOverlay.PrefsKey, HubGuide.WelcomePrefsKey,
            PcInteractor.SensitivityKey, PcInteractor.InvertYKey, PcInteractor.FieldOfViewKey, MovementCoach.PrefsKey };
        bool[] hadPreferences;
        float[] previousPreferences;
        HubSceneRefs scene;
        HubHud hud;
        HubMenuUi menu;
        Vector3 entrance;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
#if !ENABLE_INPUT_SYSTEM
            Assert.Fail("Activa Input System (Both o New) en Player Settings para verificar teclado/mouse reales.");
#endif
            hadPreferences = preferenceKeys.Select(PlayerPrefs.HasKey).ToArray();
            previousPreferences = preferenceKeys.Select((key, i) => i == 2 || i == 4
                ? PlayerPrefs.GetFloat(key) : PlayerPrefs.GetInt(key)).ToArray();
            PlayerPrefs.SetInt(HubOnboardingOverlay.PrefsKey, 1);
            PlayerPrefs.SetInt(HubGuide.WelcomePrefsKey, 1);
            PlayerPrefs.SetFloat(PcInteractor.SensitivityKey, 1);
            PlayerPrefs.SetInt(PcInteractor.InvertYKey, 0);
            PlayerPrefs.SetFloat(PcInteractor.FieldOfViewKey, 70);
            PlayerPrefs.SetInt(MovementCoach.PrefsKey, 6);
            previousCaptureDelta = Time.captureDeltaTime;
            Time.captureDeltaTime = 1f / 60f;
            input = new InputTestFixture();
            input.Setup();
            keyboard = InputSystem.AddDevice<Keyboard>();
            mouse = InputSystem.AddDevice<Mouse>();
            yield return SceneManager.LoadSceneAsync("Hub");
            scene = Object.FindFirstObjectByType<HubSceneRefs>();
            hud = Object.FindFirstObjectByType<HubHud>();
            menu = Object.FindFirstObjectByType<HubMenuUi>();
            var bootstrap = Object.FindFirstObjectByType<LabBootstrap>();
            var app = new LabUseCases(JsonMissionRepository.FromResources(), new JsonProgressStore(
                Path.Combine(Path.GetTempPath(), "ethicallab-input-" + Guid.NewGuid().ToString("N"))));
            typeof(LabBootstrap).GetField("app", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(bootstrap, app);
            hud.App = app;
            Object.FindFirstObjectByType<WorldBinder>().App = app;
            entrance = scene.person.transform.position;
            yield return InputFrames(2);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            var loaded = SceneManager.GetActiveScene();
            var cleanup = SceneManager.CreateScene("Input test cleanup");
            SceneManager.SetActiveScene(cleanup);
            yield return SceneManager.UnloadSceneAsync(loaded);
            input?.TearDown();
            Time.captureDeltaTime = previousCaptureDelta;
            if (hadPreferences != null)
                for (int i = 0; i < preferenceKeys.Length; i++)
                {
                    if (!hadPreferences[i]) PlayerPrefs.DeleteKey(preferenceKeys[i]);
                    else if (i == 2 || i == 4) PlayerPrefs.SetFloat(preferenceKeys[i], previousPreferences[i]);
                    else PlayerPrefs.SetInt(preferenceKeys[i], (int)previousPreferences[i]);
                }
            PlayerPrefs.Save();
        }

        IEnumerator InputFrames(int count, Key[] held = null, Vector2 look = default)
        {
            for (int i = 0; i < count; i++)
            {
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(held ?? Array.Empty<Key>()));
                InputSystem.QueueStateEvent(mouse, new MouseState { delta = look });
                yield return null;
            }
        }

        IEnumerator Enter()
        {
            Object.FindFirstObjectByType<HubStartScreen>().GetComponentsInChildren<Button>()
                .Single(button => button.name == "Entrar a la oficina").onClick.Invoke();
            yield return InputFrames(12);
            Assert.That(hud.SessionStarted, Is.True);
            Assert.That(scene.person.MenuOpen, Is.False);
            if (!UnityEngine.Application.isBatchMode)
                Assert.That(Cursor.lockState, Is.EqualTo(CursorLockMode.Locked));
        }

        IEnumerator ResetPosition()
        {
            scene.person.ReturnToEntrance();
            yield return InputFrames(2);
        }

        static float PlanarDistance(Vector3 a, Vector3 b) => Vector2.Distance(new Vector2(a.x, a.z), new Vector2(b.x, b.z));

        [UnityTest]
        public IEnumerator WelcomeBlocksInputThenWasdArrowsAndSprintMoveThePlayer()
        {
            yield return InputFrames(10, new[] { Key.W }, new Vector2(100, 30));
            Assert.That(scene.person.transform.position, Is.EqualTo(entrance));
            Assert.That(scene.person.transform.eulerAngles.y, Is.EqualTo(0).Within(0.01f));
            yield return Enter();
            var inputs = new[] { Key.W, Key.S, Key.A, Key.D, Key.UpArrow, Key.DownArrow, Key.LeftArrow, Key.RightArrow };
            var directions = new[] { Vector3.forward, Vector3.back, Vector3.left, Vector3.right,
                Vector3.forward, Vector3.back, Vector3.left, Vector3.right };
            for (int i = 0; i < inputs.Length; i++)
            {
                yield return ResetPosition();
                yield return InputFrames(12, new[] { inputs[i] });
                Assert.That(Vector3.Dot(scene.person.transform.position - entrance, directions[i]), Is.GreaterThan(0.18f), inputs[i].ToString());
            }
            yield return ResetPosition();
            yield return InputFrames(40, new[] { Key.W });
            float walk = PlanarDistance(entrance, scene.person.transform.position);
            yield return ResetPosition();
            // La física actual permite sprint al avanzar, no al desplazarse lateralmente.
            yield return InputFrames(40, new[] { Key.W, Key.RightShift });
            float sprint = PlanarDistance(entrance, scene.person.transform.position);
            Assert.That(sprint, Is.GreaterThan(walk * 1.35f));
            yield return ResetPosition();
            yield return InputFrames(40, new[] { Key.D, Key.W });
            Assert.That(PlanarDistance(entrance, scene.person.transform.position), Is.EqualTo(walk).Within(0.08f), "Sin ventaja diagonal.");
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator MouseTurnsFirstPersonCameraAndMovementFollowsItsHeading()
        {
            yield return Enter();
            yield return InputFrames(1, look: new Vector2(750, 0));
            Assert.That(scene.person.transform.eulerAngles.y, Is.EqualTo(90).Within(0.1f));
            yield return InputFrames(20, new[] { Key.W });
            Assert.That(scene.person.transform.position.x, Is.GreaterThan(0.6f));
            Assert.That(scene.person.transform.position.z, Is.EqualTo(entrance.z).Within(0.02f));
            yield return InputFrames(1, look: new Vector2(0, 5000));
            Assert.That(Mathf.DeltaAngle(0, scene.mainCamera.transform.localEulerAngles.x), Is.EqualTo(-80).Within(0.1f));
            Assert.That(scene.person.transform.eulerAngles.x, Is.EqualTo(0).Within(0.01f));
            Assert.That(scene.mainCamera.transform.parent, Is.EqualTo(scene.person.transform));
            yield return ResetPosition();
            scene.person.ApplyViewSettings(2, true, 80, false);
            yield return InputFrames(1, look: new Vector2(100, 50));
            Assert.That(scene.person.transform.eulerAngles.y, Is.EqualTo(24).Within(0.1f));
            Assert.That(Mathf.DeltaAngle(0, scene.mainCamera.transform.localEulerAngles.x), Is.EqualTo(20).Within(0.1f));
            Assert.That(scene.mainCamera.fieldOfView, Is.EqualTo(80));
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator WalkingCannotCrossWallsAndStopsAfterKeyRelease()
        {
            yield return Enter();
            yield return InputFrames(120, new[] { Key.S, Key.LeftShift });
            float insideWall = GameObject.Find("Front wall").GetComponent<Collider>().bounds.max.z;
            Assert.That(scene.person.transform.position.z, Is.GreaterThan(insideWall + 0.2f));
            Assert.That(scene.person.GetComponent<CharacterController>().isGrounded, Is.True);
            yield return ResetPosition();
            yield return InputFrames(20, new[] { Key.D });
            yield return InputFrames(15);
            Vector3 stopped = scene.person.transform.position;
            yield return InputFrames(15);
            Assert.That(PlanarDistance(stopped, scene.person.transform.position), Is.LessThan(0.005f));
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator EscapeAndFocusLossPauseMovementWithoutCameraJumpOnResume()
        {
            yield return Enter();
            yield return InputFrames(1, new[] { Key.Escape });
            Vector3 position = scene.person.transform.position;
            Quaternion rotation = scene.mainCamera.transform.rotation;
            yield return InputFrames(20, new[] { Key.W }, new Vector2(50, 30));
            Assert.That(scene.person.transform.position, Is.EqualTo(position));
            Assert.That(scene.mainCamera.transform.rotation, Is.EqualTo(rotation));
            Assert.That(Cursor.lockState, Is.EqualTo(CursorLockMode.None));
            yield return InputFrames(1, new[] { Key.Escape }, new Vector2(1000, 1000));
            yield return InputFrames(2);
            Assert.That(scene.mainCamera.transform.rotation, Is.EqualTo(rotation), "Descartar delta al cerrar menú.");
            scene.person.SendMessage("OnApplicationFocus", false);
            yield return InputFrames(2);
            scene.person.SendMessage("OnApplicationFocus", true);
            yield return InputFrames(5, new[] { Key.W });
            Assert.That(scene.person.MenuOpen, Is.True, "Alt-tab no recaptura el cursor por sí solo.");
            Assert.That(scene.person.transform.position, Is.EqualTo(position));
            Assert.That(menu.GetComponent<Canvas>().enabled, Is.True);
            yield return InputFrames(1, new[] { Key.Escape });
            yield return InputFrames(12, new[] { Key.W });
            Assert.That(scene.person.transform.position.z, Is.GreaterThan(position.z + 0.15f));
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator PlayerWalksToDoorUsesEAndEntersArchive()
        {
            yield return Enter();
            // Solo colocación inicial del escenario: todo el cruce posterior utiliza teclado/mouse.
            var controller = scene.person.GetComponent<CharacterController>();
            Vector3 doorCenter = scene.door.GetComponent<Collider>().bounds.center;
            controller.enabled = false;
            scene.person.transform.position = new Vector3(doorCenter.x - 1.3f, 0.04f, doorCenter.z);
            controller.enabled = true;
            Physics.SyncTransforms();
            yield return InputFrames(1, look: new Vector2(750, 0));
            yield return InputFrames(35, new[] { Key.W });
            Assert.That(scene.person.transform.position.x, Is.LessThan(doorCenter.x - 0.2f));
            Assert.That(scene.person.Prompt, Does.Contain("E"));
            yield return InputFrames(1, new[] { Key.E });
            Assert.That(scene.door.IsOpen, Is.True, "E desde el raycast central debe abrir la puerta.");
            yield return InputFrames(60);
            yield return InputFrames(35, new[] { Key.W });
            Assert.That(scene.person.transform.position.x, Is.GreaterThan(doorCenter.x + 0.7f));
            HubStartupTests.CaptureCanvas(scene.mainCamera, Object.FindFirstObjectByType<HubOfficeOverlayUi>().GetComponent<Canvas>(),
                "first-person-archive.png", 1600, 900);
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator ViewSettingsPersistAndReturnToEntrancePreservesProgress()
        {
            yield return Enter();
            yield return InputFrames(1, new[] { Key.Escape });
            yield return InputFrames(1);
            var sliders = menu.GetComponentsInChildren<Slider>();
            Assert.That(sliders.Length, Is.EqualTo(2));
            sliders.Single(s => s.transform.parent.name == "Sensibilidad del mouse").value = 1.75f;
            sliders.Single(s => s.transform.parent.name == "Campo de visión").value = 85;
            Assert.That(scene.person.Sensitivity, Is.EqualTo(1.75f));
            Assert.That(scene.mainCamera.fieldOfView, Is.EqualTo(85));
            Assert.That(PlayerPrefs.GetFloat(PcInteractor.SensitivityKey), Is.EqualTo(1.75f));
            Assert.That(PlayerPrefs.GetFloat(PcInteractor.FieldOfViewKey), Is.EqualTo(85));
            menu.GetComponentsInChildren<Button>().Single(b => b.name == "Invertir mirada vertical: No").onClick.Invoke();
            yield return InputFrames(2);
            Assert.That(scene.person.InvertY, Is.True);
            Assert.That(PlayerPrefs.GetInt(PcInteractor.InvertYKey), Is.EqualTo(1));
            HubStartupTests.CaptureCanvas(scene.mainCamera, menu.GetComponent<Canvas>(), "first-person-settings.png", 1600, 900);
            // Una nueva instancia usa los valores persistidos.
            var probe = new GameObject("Settings reload probe").AddComponent<PcInteractor>();
            Assert.That(probe.Sensitivity, Is.EqualTo(1.75f));
            Assert.That(probe.InvertY, Is.True);
            Assert.That(probe.FieldOfView, Is.EqualTo(85));
            Object.Destroy(probe.gameObject);
            hud.App.StartMission.Execute(hud.App.Catalog.Missions[0].Id);
            menu.GetComponentsInChildren<Button>().Single(b => b.name == "Volver a la entrada (conservar progreso)").onClick.Invoke();
            yield return InputFrames(2);
            Assert.That(PlanarDistance(scene.person.transform.position, entrance), Is.LessThan(0.01f));
            Assert.That(hud.App.Progress.Get(hud.App.Catalog.Missions[0].Id.Value).Started, Is.True);
            Assert.That(scene.person.MenuOpen, Is.False);
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator FallingOutsideWorldRecoversToEntrance()
        {
            yield return Enter();
            var controller = scene.person.GetComponent<CharacterController>();
            controller.enabled = false;
            scene.person.transform.position = new Vector3(0, -4, -2);
            controller.enabled = true;
            yield return InputFrames(3);
            Assert.That(PlanarDistance(scene.person.transform.position, entrance), Is.LessThan(0.01f));
            Assert.That(scene.person.transform.position.y, Is.GreaterThan(-0.1f));
            LogAssert.NoUnexpectedReceived();
        }
    }
}
