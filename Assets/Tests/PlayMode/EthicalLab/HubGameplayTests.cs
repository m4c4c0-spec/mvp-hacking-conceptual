using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using EthicalLab.Application;
using EthicalLab.Domain;
using EthicalLab.Infrastructure;
using EthicalLab.Presentation;
using EthicalLab.Shared;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace EthicalLab.Tests
{
    /// <summary>
    /// Integración con contenido real, objetos y botones de la escena. Despacha las
    /// acciones de interacción directamente; no simula el teclado ni un recorrido WASD.
    /// Cada prueba guarda en su propio directorio temporal.
    /// </summary>
    public sealed class HubGameplayTests
    {
        LabBootstrap bootstrap;
        HubSceneRefs scene;
        HubHud hud;
        HubMenuUi menu;
        LabUseCases app;
        JsonProgressStore store;
        string directory;
        int oldOnboarding, oldWelcome, oldCoach;
        bool hadOnboarding, hadWelcome, hadCoach;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            hadOnboarding = PlayerPrefs.HasKey(HubOnboardingOverlay.PrefsKey);
            oldOnboarding = PlayerPrefs.GetInt(HubOnboardingOverlay.PrefsKey);
            hadWelcome = PlayerPrefs.HasKey(HubGuide.WelcomePrefsKey);
            oldWelcome = PlayerPrefs.GetInt(HubGuide.WelcomePrefsKey);
            hadCoach = PlayerPrefs.HasKey(MovementCoach.PrefsKey);
            oldCoach = PlayerPrefs.GetInt(MovementCoach.PrefsKey);
            PlayerPrefs.SetInt(MovementCoach.PrefsKey, 6);
            PlayerPrefs.SetInt(HubOnboardingOverlay.PrefsKey, 1);
            PlayerPrefs.SetInt(HubGuide.WelcomePrefsKey, 1);
            directory = Path.Combine(Path.GetTempPath(), "ethicallab-playtest-" + Guid.NewGuid().ToString("N"));
            store = new JsonProgressStore(directory);
            yield return SceneManager.LoadSceneAsync("Hub");
            bootstrap = Object.FindFirstObjectByType<LabBootstrap>();
            scene = Object.FindFirstObjectByType<HubSceneRefs>();
            hud = Object.FindFirstObjectByType<HubHud>();
            menu = Object.FindFirstObjectByType<HubMenuUi>();
            app = new LabUseCases(JsonMissionRepository.FromResources(), store);
            Bind(app);
            yield return Frames();
            Object.FindFirstObjectByType<HubStartScreen>().GetComponentsInChildren<Button>()
                .Single(button => button.name == "Entrar a la oficina").onClick.Invoke();
            yield return Frames();
            Assert.That(hud.SessionStarted, Is.True);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (scene != null) scene.person.Drop();
            var loaded = SceneManager.GetActiveScene();
            var empty = SceneManager.CreateScene("Playtest cleanup");
            SceneManager.SetActiveScene(empty);
            yield return SceneManager.UnloadSceneAsync(loaded);
            if (hadOnboarding) PlayerPrefs.SetInt(HubOnboardingOverlay.PrefsKey, oldOnboarding);
            else PlayerPrefs.DeleteKey(HubOnboardingOverlay.PrefsKey);
            if (hadWelcome) PlayerPrefs.SetInt(HubGuide.WelcomePrefsKey, oldWelcome);
            else PlayerPrefs.DeleteKey(HubGuide.WelcomePrefsKey);
            if (hadCoach) PlayerPrefs.SetInt(MovementCoach.PrefsKey, oldCoach);
            else PlayerPrefs.DeleteKey(MovementCoach.PrefsKey);
            PlayerPrefs.Save();
        }

        void Bind(LabUseCases session)
        {
            typeof(LabBootstrap).GetField("app", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(bootstrap, session);
            hud.App = session;
            Object.FindFirstObjectByType<WorldBinder>().App = session;
        }

        void Use(InteractableView target)
        {
            Assert.That(target, Is.Not.Null);
            Assert.That(target.gameObject.activeInHierarchy, Is.True, target.name);
            bootstrap.SendMessage("OnUsed", target.Id, SendMessageOptions.RequireReceiver);
        }

        static IEnumerator Frames()
        {
            yield return null;
            yield return null;
        }

        IEnumerator Click(string label)
        {
            Assert.That(menu.GetComponent<Canvas>().enabled, Is.True);
            var button = menu.GetComponentsInChildren<Button>()
                .FirstOrDefault(candidate => candidate.gameObject.activeInHierarchy && candidate.name == label);
            Assert.That(button, Is.Not.Null, "Botón visible: " + label);
            button.onClick.Invoke();
            yield return Frames();
        }

        IEnumerator Answer(MissionStep step)
        {
            var state = app.Progress.Step(app.Active.Id.Value, step.Id.Value);
            yield return Click(StepButtonLabel(step));
            yield return Click(step.Options[step.CorrectIndex]);
            Assert.That(state.Completed, Is.True, step.Id.Value);
        }

        string StepButtonLabel(MissionStep step) => menu.GetComponentsInChildren<Button>()
            .Single(button => button.name.EndsWith(" · " + step.Prompt)).name;

        [UnityTest]
        public IEnumerator FourCasesCanBeCompletedThroughWorldAndReportUi()
        {
            for (int i = 0; i < app.Catalog.Missions.Count; i++)
            {
                var mission = app.Catalog.Missions[i];
                Assert.That(MissionCatalogRules.IsAvailable(app.Catalog.Missions, app.Progress, mission.Id), Is.True);
                if (i + 1 < app.Catalog.Missions.Count)
                    Assert.That(MissionCatalogRules.IsAvailable(app.Catalog.Missions, app.Progress, app.Catalog.Missions[i + 1].Id), Is.False);
                Use(scene.tickets[i]);
                yield return Frames();
                Assert.That(app.Active.Id, Is.EqualTo(mission.Id));
                var groups = ClueWorkflow.ClueGroups(mission);
                for (int clue = 0; clue < groups.Count; clue++)
                {
                    if (clue == 3 && !scene.drawer.IsOpen)
                    {
                        Use(scene.drawer.GetComponent<InteractableView>());
                        yield return new WaitForSeconds(0.8f);
                    }
                    Use(scene.folders[clue]);
                    Assert.That(scene.person.Held, Is.EqualTo(scene.folders[clue]));
                    for (int choice = 0; choice < 2; choice++)
                    {
                        var pending = ClueWorkflow.Pending(mission, app.Progress, groups[clue]);
                        Assert.That(pending, Is.Not.Null);
                        Use(scene.trays[pending.CorrectIndex]);
                        yield return Frames();
                    }
                    Assert.That(ClueWorkflow.Documented(mission, app.Progress, groups[clue]), Is.True);
                    Assert.That(scene.person.Held, Is.Null);
                }

                hud.Open("glossary");
                yield return Frames();
                while (menu.GetComponentsInChildren<Button>().Any(button => button.name == "He leído y comprendido esta ficha"))
                    yield return Click("He leído y comprendido esta ficha");
                hud.Open("office");
                yield return Frames();
                Assert.That(scene.reportInbox.prompt, Does.Contain("listo"));
                Use(scene.reportInbox);
                yield return Frames();
                if (i == 0)
                    HubStartupTests.CaptureCanvas(scene.mainCamera, menu.GetComponent<Canvas>(), "case-playtest-1080.png", 1920, 1080);
                yield return Answer(mission.Step(new StepId("report")));
                foreach (var quiz in mission.Steps.Where(step => step.Kind == StepKind.AnswerQuiz))
                    yield return Answer(quiz);
                Assert.That(app.Progress.Get(mission.Id.Value).Completed, Is.True);
                Assert.That(app.Progress.Get(mission.Id.Value).Score, Is.EqualTo(100));
                Assert.That(scene.person.MenuOpen, Is.False);
                Assert.That(scene.tickets[i].prompt, Does.Contain("cerrado 100/100"));
            }
            Assert.That(app.Progress.CompletedCount, Is.EqualTo(4));
            Assert.That(app.Progress.Unlocked.Count, Is.EqualTo(8));
            var restored = new LabUseCases(JsonMissionRepository.FromResources(), new JsonProgressStore(directory));
            Assert.That(restored.Progress.CompletedCount, Is.EqualTo(4));
            Assert.That(restored.Progress.Reviewed.Count, Is.EqualTo(8));
            Assert.That(restored.Progress.Missions.Where(m => m.Started).All(m => m.Score == 100), Is.True);
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator WrongTrayKeepsClueHeldUntilObservationAndDefenseAreCorrect()
        {
            Use(scene.tickets[0]);
            yield return Frames();
            var clue = scene.folders[0];
            Use(clue);
            var pending = ClueWorkflow.Pending(app.Active, app.Progress, clue.Id.Arg);
            Use(scene.trays[(pending.CorrectIndex + 1) % pending.Options.Count]);
            yield return Frames();
            Assert.That(app.Progress.Step(app.Active.Id.Value, pending.Id.Value).Completed, Is.False);
            Assert.That(scene.person.Held, Is.EqualTo(clue));
            Assert.That(app.Progress.Unlocked, Is.Empty);
            Use(scene.trays[pending.CorrectIndex]);
            yield return Frames();
            Assert.That(scene.person.Held, Is.EqualTo(clue));
            pending = ClueWorkflow.Pending(app.Active, app.Progress, clue.Id.Arg);
            Assert.That(pending.Id.Value, Does.EndWith(".defend"));
            Use(scene.trays[pending.CorrectIndex]);
            yield return Frames();
            Assert.That(scene.person.Held, Is.Null);
            Assert.That(app.Progress.Unlocked.Count, Is.EqualTo(1));
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator LockedTicketsAndEarlyReportCannotAdvanceTheCase()
        {
            Use(scene.tickets[3]);
            yield return Frames();
            Assert.That(app.Progress.Get(app.Catalog.Missions[3].Id.Value).Started, Is.False);
            Use(scene.tickets[0]);
            yield return Frames();
            Use(scene.reportInbox);
            yield return Frames();
            Assert.That(scene.person.MenuOpen, Is.False);
            hud.Open("case");
            yield return Frames();
            var report = app.Active.Step(new StepId("report"));
            yield return Click(StepButtonLabel(report));
            yield return Click(report.Options[report.CorrectIndex]);
            Assert.That(app.Progress.Get(app.Active.Id.Value).ReportAccepted, Is.False);
            var status = menu.GetComponentsInChildren<TMP_Text>().Single(label => label.name == "Status");
            Assert.That(status.text, Does.Contain("Documenta todas las pistas"));
            var canvasRect = menu.GetComponent<RectTransform>();
            var bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(canvasRect, status.rectTransform);
            Assert.That(bounds.min.x, Is.GreaterThanOrEqualTo(canvasRect.rect.xMin), "El mensaje de estado debe quedar dentro del menú.");
            Assert.That(bounds.max.x, Is.LessThanOrEqualTo(canvasRect.rect.xMax));
            var quiz = app.Active.Steps.First(step => step.Kind == StepKind.AnswerQuiz);
            yield return Click(StepButtonLabel(quiz));
            yield return Click(quiz.Options[quiz.CorrectIndex]);
            Assert.That(app.Progress.Step(app.Active.Id.Value, quiz.Id.Value).Completed, Is.False);
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator DrawerClueIsReachableAndReturnsToItsMovingParent()
        {
            app.Progress.Get(app.Catalog.Missions[0].Id.Value).Completed = true;
            Use(scene.tickets[1]);
            yield return Frames();
            Use(scene.drawer.GetComponent<InteractableView>());
            yield return new WaitForSeconds(0.8f);
            var usb = scene.folders[3];
            Vector3 home = usb.transform.localPosition;
            Physics.SyncTransforms();
            // Altura humana y acercamiento al cajón: mirar por encima del frente abierto.
            var eye = usb.transform.position + new Vector3(0f, 0.72f, 0.68f);
            var ray = new Ray(eye, usb.transform.position - eye);
            Assert.That(Physics.Raycast(ray, out var hit, 3f, Physics.DefaultRaycastLayers), Is.True);
            Assert.That(hit.collider.GetComponentInParent<InteractableView>(), Is.EqualTo(usb),
                $"La cuarta pista debe poder enfocarse con el cajón abierto. Eye={eye}, USB={usb.transform.position}, hit={hit.point}, bounds={hit.collider.bounds}");
            scene.person.Take(usb);
            Assert.That(usb.GetComponent<Collider>().enabled, Is.False);
            Use(scene.drawer.GetComponent<InteractableView>());
            yield return new WaitForSeconds(0.8f);
            scene.person.Drop();
            Assert.That(usb.transform.parent, Is.EqualTo(scene.drawer.movingPart));
            Assert.That(usb.transform.localPosition, Is.EqualTo(home));
            Assert.That(usb.GetComponent<Collider>().enabled, Is.True);
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator MenuPausesMissionTimeAndResumeRestoresControls()
        {
            Use(scene.tickets[0]);
            yield return new WaitForSeconds(0.15f);
            var row = app.Progress.Get(app.Active.Id.Value);
            Assert.That(row.Seconds, Is.GreaterThan(0));
            hud.Open("home");
            yield return Frames();
            float pausedAt = row.Seconds;
            yield return new WaitForSeconds(0.15f);
            Assert.That(row.Seconds, Is.EqualTo(pausedAt));
            Assert.That(scene.person.MenuOpen, Is.True);
            yield return Click("Volver a caminar por la oficina");
            yield return new WaitForSeconds(0.1f);
            Assert.That(row.Seconds, Is.GreaterThan(pausedAt));
            Assert.That(scene.person.MenuOpen, Is.False);
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator TerminalAcceptsEnterAndRejectsNonNarrativeCommands()
        {
            Use(scene.tickets[0]);
            yield return Frames();
            bootstrap.SendMessage("OnUsed", InteractableId.Laptop);
            yield return Frames();
            var input = menu.GetComponentInChildren<TMP_InputField>();
            input.text = "help";
            yield return Frames();
            Assert.That(menu.GetComponentInChildren<TMP_InputField>(), Is.EqualTo(input), "Escribir no debe reconstruir la terminal.");
            input.onSubmit.Invoke(input.text);
            yield return Frames();
            string output = string.Join("\n", menu.GetComponentsInChildren<TMP_Text>().Select(t => t.text));
            Assert.That(output, Does.Contain("inspect <id>"), "Enter debe ejecutar el comando de ayuda.");
            HubStartupTests.CaptureCanvas(scene.mainCamera, menu.GetComponent<Canvas>(), "terminal-playtest-720.png", 1280, 720);
            input = menu.GetComponentInChildren<TMP_InputField>();
            input.text = "nmap example.invalid";
            yield return Click("Ejecutar comando ficticio");
            output = string.Join("\n", menu.GetComponentsInChildren<TMP_Text>().Select(t => t.text));
            Assert.That(output, Does.Contain("No se ejecutan comandos del sistema"));
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator DemoResetRequiresConfirmationAndPersistsCleanProgress()
        {
            Use(scene.tickets[0]);
            yield return Frames();
            Use(scene.folders[0]);
            hud.Open("home");
            yield return Frames();
            yield return Click("Reiniciar demo");
            Assert.That(app.Progress.Get(app.Active.Id.Value).Started, Is.True);
            yield return Click("Cancelar");
            Assert.That(app.Progress.Get(app.Active.Id.Value).Started, Is.True);
            yield return Click("Reiniciar demo");
            yield return Click("Sí, reiniciar");
            Assert.That(app.Progress.Get(app.Active.Id.Value).Started, Is.False);
            Assert.That(scene.person.Held, Is.Null);
            Assert.That(scene.person.MenuOpen, Is.False);
            Assert.That(store.Load().Missions.All(row => !row.Started), Is.True);
            LogAssert.NoUnexpectedReceived();
        }
    }
}
