using System;
using UnityEngine;
using EthicalLab.Application;
using EthicalLab.Domain;
using EthicalLab.Shared;

namespace EthicalLab.Presentation
{
    [DefaultExecutionOrder(-100)]
    public sealed class HubHud : MonoBehaviour
    {
        public LabUseCases App;
        public PcInteractor Person;

        readonly HubPageRouter router = new HubPageRouter();
        string term = "ACADEMY OS  /  primera persona\nhelp · scan · inspect · notes · report\n";
        string command = "";
        string toast = "";
        string worldToast = "";
        float worldToastUntil;
        string reading = "";
        string selectedStep = "";
        bool resetConfirm;
        bool controlsHintVisible = true;

        HubOfficeOverlayUi overlayUi;
        HubMenuUi menuUi;
        HubOnboardingOverlay onboarding;
        HubStartScreen startScreen;
        public bool SessionStarted { get; private set; }
        TMPro.TMP_InputField terminalField;

        void Awake()
        {
            overlayUi = HubOfficeOverlayUi.Create(transform);
            menuUi = HubMenuUi.Create(transform);
            onboarding = HubOnboardingOverlay.Create(transform);
            startScreen = HubStartScreen.Create(transform);
            overlayUi.SetVisible(false);
            menuUi.SetVisible(false);
            menuUi.BackToOffice += () => { router.Open("office"); toast = ""; resetConfirm = false; };
            menuUi.TabClicked += id => router.Open(id);
        }

        void Start()
        {
            Person.ControlInterrupted += PauseForFocusLoss;
            Person.MenuOpen = true;
            startScreen.Show(HubGuide.AnyMissionStarted(App), App.Progress.CompletedCount, App.Catalog.Missions.Count,
                () => onboarding.ShowIfNeeded(() =>
                {
                    SessionStarted = true;
                    hudWelcomeIfNeeded();
                }));
        }

        void PauseForFocusLoss()
        {
            if (!SessionStarted) return;
            router.Open("home");
            toast = "Juego en pausa · vuelve a la oficina para continuar.";
        }

        void OnDestroy()
        {
            if (Person != null) Person.ControlInterrupted -= PauseForFocusLoss;
        }

        void hudWelcomeIfNeeded()
        {
            if (HubGuide.AnyMissionStarted(App)) return;
            if (PlayerPrefs.GetInt(HubGuide.WelcomePrefsKey, 0) == 1) return;
            PlayerPrefs.SetInt(HubGuide.WelcomePrefsKey, 1);
            PlayerPrefs.Save();
            Toast(HubGuide.WelcomeTip);
        }

        public void Toast(string text, bool playBeep = true)
        {
            worldToast = text ?? "";
            worldToastUntil = Time.unscaledTime + Mathf.Clamp(2.5f + worldToast.Length * 0.04f, 3f, 12f);
            if (playBeep && !string.IsNullOrEmpty(worldToast)) HubAudio.PlayToast();
        }

        public void Reading(string text) => reading = text ?? "";

        void Update()
        {
            if (Person == null) return;
            if (!SessionStarted || startScreen.IsVisible || onboarding.IsVisible)
            {
                Person.MenuOpen = true;
                return;
            }
            if (!Person.HasFocus) { Person.MenuOpen = true; return; }
            if (PcButtons.Escape)
                router.ToggleEscape();
            Person.MenuOpen = router.MenuOpen;
            if (router.Page == HubPage.Office && PcButtons.Help)
                controlsHintVisible = !controlsHintVisible;
            if (!router.MenuOpen) App.Tick(Time.unscaledDeltaTime);
        }

        void LateUpdate()
        {
            if (Person == null || overlayUi == null || menuUi == null) return;
            if (!SessionStarted)
            {
                overlayUi.SetVisible(false);
                menuUi.SetVisible(false);
                return;
            }

            if (router.Page == HubPage.Office)
            {
                menuUi.SetVisible(false);
                overlayUi.SetVisible(true);
                overlayUi.Refresh(
                    Person.Prompt,
                    HubGuide.ControlsHint,
                    OfficeGuide(),
                    worldToast,
                    Time.unscaledTime < worldToastUntil,
                    reading,
                    controlsHintVisible);
                return;
            }

            overlayUi.SetVisible(false);
            menuUi.SetVisible(true);
            if (router.MenuDirty)
            {
                router.ConsumeRebuild();
                RebuildMenu();
            }
        }

        void RebuildMenu()
        {
            menuUi.ClearContent();
            menuUi.SetStatus(toast);
            switch (router.Page)
            {
                case HubPage.Home:
                    Home();
                    break;
                case HubPage.Missions:
                    Missions();
                    break;
                case HubPage.Terminal:
                    Terminal();
                    break;
                case HubPage.Case:
                    Case();
                    break;
                case HubPage.Glossary:
                    Glossary();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(router.Page), router.Page, null);
            }
        }

        void Home()
        {
            menuUi.AddHeading("BLUE / RED · ANALYST ACADEMY");
            menuUi.AddBody("Tu oficina de investigación. Acepta un ticket, documenta las pistas y entrega tu informe.");
            menuUi.AddBody("Completadas: " + App.Progress.CompletedCount + " / " + App.Catalog.Missions.Count);
            menuUi.AddButton("Abrir buzón de misiones", () => router.Open("missions"));
            menuUi.AddButton("Volver a caminar por la oficina", () => router.Open("office"));
            menuUi.AddHeading("Cámara en primera persona");
            menuUi.AddSlider("Sensibilidad del mouse", Person.Sensitivity, 0.25f, 2.5f,
                value => Person.ApplyViewSettings(value, Person.InvertY, Person.FieldOfView));
            menuUi.AddSlider("Campo de visión", Person.FieldOfView, 60f, 90f,
                value => Person.ApplyViewSettings(Person.Sensitivity, Person.InvertY, value), true);
            menuUi.AddButton("Invertir mirada vertical: " + (Person.InvertY ? "Sí" : "No"), () =>
            {
                Person.ApplyViewSettings(Person.Sensitivity, !Person.InvertY, Person.FieldOfView);
                router.MarkDirty();
            });
            menuUi.AddButton("Restaurar cámara", () =>
            {
                Person.ApplyViewSettings(1f, false, 70f);
                router.MarkDirty();
            });
            menuUi.AddButton("Volver a la entrada (conservar progreso)", () =>
            {
                Person.ReturnToEntrance();
                router.Open("office");
            });
            menuUi.AddButton("Guardar y salir", () =>
            {
                App.SaveLoad.Save();
                HubStartScreen.Quit();
            });
            if (!resetConfirm)
                menuUi.AddButton("Reiniciar demo", () => { resetConfirm = true; router.MarkDirty(); });
            else
            {
                menuUi.AddBody("¿Seguro? Se borrará el progreso guardado.");
                menuUi.AddButton("Sí, reiniciar", ConfirmDemoReset);
                menuUi.AddButton("Cancelar", () => { resetConfirm = false; router.MarkDirty(); });
            }
        }

        void ConfirmDemoReset()
        {
            resetConfirm = false;
            if (Person != null) Person.Drop();
            Reading("");
            App.ResetDemo();
            PlayerPrefs.DeleteKey(HubGuide.WelcomePrefsKey);
            if (Person != null) Person.GetComponent<MovementCoach>()?.Restart();
            PlayerPrefs.Save();
            toast = "Demo reiniciada.";
            Toast(HubGuide.WelcomeTip);
            router.Open("office");
        }

        void Missions()
        {
            menuUi.AddHeading("Buzón de tickets");
            for (int i = 0; i < App.Catalog.Missions.Count; i++)
            {
                var mission = App.Catalog.Missions[i];
                bool open = MissionCatalogRules.IsAvailable(App.Catalog.Missions, App.Progress, mission.Id);
                var row = App.Progress.Get(mission.Id.Value);
                menuUi.AddBody(mission.Number + " · " + mission.Title + " · " + mission.Client);
                menuUi.AddBody(mission.Brief);
                menuUi.AddBody(row.Completed ? "CERRADO · " + row.Score + "/100" : open ? "Disponible" : "Bloqueada");
                if (open)
                {
                    int idx = i;
                    menuUi.AddButton(row.Completed ? "Consultar" : row.Started ? "Continuar" : "Aceptar ticket", () =>
                    {
                        var m = App.Catalog.Missions[idx];
                        var result = App.StartMission.Execute(m.Id);
                        toast = result.Ok ? "Ticket aceptado: " + m.Title : result.Error;
                        if (result.Ok) { selectedStep = ""; router.Open("case"); }
                        App.SaveLoad.Save();
                        router.MarkDirty();
                    });
                }
            }
        }

        void Terminal()
        {
            menuUi.AddHeading("Terminal narrativa");
            menuUi.AddBody(term);
            terminalField = menuUi.AddTerminalInput(command);
            terminalField.onValueChanged.AddListener(value => command = value);
            terminalField.onSubmit.AddListener(SubmitTerminal);
            terminalField.ActivateInputField();
            menuUi.AddButton("Ejecutar comando ficticio", () => SubmitTerminal(terminalField != null ? terminalField.text : command));
        }

        void SubmitTerminal(string raw)
        {
            command = raw ?? "";
            if (command.Trim().Length == 0) return;
            if (command.Trim().ToLowerInvariant() == "clear") term = "pantalla limpia\n";
            else term += "\nanalista > " + command + "\n" + NarrativeTerminal.Execute(App, command) + "\n";
            command = "";
            App.SaveLoad.Save();
            router.MarkDirty();
        }

        void Case()
        {
            var mission = App.Active;
            if (mission == null || !App.Progress.Get(mission.Id.Value).Started)
            {
                menuUi.AddBody("Acepta un ticket en Misiones.");
                return;
            }
            menuUi.AddHeading(mission.Title);
            menuUi.AddBody("Alcance: " + mission.Scope);
            for (int i = 0; i < mission.Steps.Count; i++)
            {
                var step = mission.Steps[i];
                if (step.Kind == StepKind.UseTerminal) continue;
                var state = App.Progress.Step(mission.Id.Value, step.Id.Value);
                string mark = (state.Completed ? "OK · " : "○ ") + StepLabel(step) + " · " + step.Prompt;
                int si = i;
                menuUi.AddButton(mark, () => { selectedStep = mission.Steps[si].Id.Value; router.MarkDirty(); });
            }
            var current = mission.Step(new StepId(selectedStep));
            if (current == null) return;
            menuUi.AddBody(current.Prompt);
            if (ShowStepBody(mission, current)) menuUi.AddBody(current.Body);
            DrawStep(current);
        }

        /// <summary>
        /// En el quiz, Body es la explicación (revela la respuesta). Solo se muestra tras un acierto.
        /// En el resto de pasos, Body es el texto del paso.
        /// </summary>
        bool ShowStepBody(MissionDefinition mission, MissionStep step)
        {
            if (step == null || string.IsNullOrEmpty(step.Body)) return false;
            if (step.Kind != StepKind.AnswerQuiz) return true;
            return App.Progress.Step(mission.Id.Value, step.Id.Value).Completed;
        }

        string OfficeGuide()
        {
            var coach = Person.GetComponent<MovementCoach>();
            string lesson = coach != null ? coach.Lesson : "";
            string next = HubGuide.NextStep(App, Person);
            if (!CaseOpen()) return string.IsNullOrEmpty(lesson) ? next : lesson;
            return string.IsNullOrEmpty(lesson) ? next : next + "\n" + lesson;
        }

        bool CaseOpen()
        {
            if (App == null) return false;
            var mission = App.Active;
            if (mission == null) return false;
            var row = App.Progress.Get(mission.Id.Value);
            return row != null && row.Started && !row.Completed;
        }

        void DrawStep(MissionStep current)
        {
            switch (current.Kind)
            {
                case StepKind.CollectClue:
                    menuUi.AddButton("Inspeccionar pista", () =>
                    {
                        var result = App.CompleteStep.Execute(current.Id, new StepAnswer());
                        toast = result.Ok ? "Pista leída." : result.Error;
                        App.SaveLoad.Save();
                        router.MarkDirty();
                    });
                    break;
                case StepKind.ClassifyItem:
                case StepKind.AnswerQuiz:
                case StepKind.WriteReportSection:
                    DrawChoices(current);
                    break;
                case StepKind.UseTerminal:
                    menuUi.AddBody("Usa la pestaña Terminal.");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(current.Kind), current.Kind, null);
            }
        }

        static string StepLabel(MissionStep step)
        {
            switch (step.Kind)
            {
                case StepKind.CollectClue: return "Pista";
                case StepKind.ClassifyItem: return step.Id.Value.EndsWith(".defend") ? "Defensa" : "Observación";
                case StepKind.WriteReportSection: return "Informe";
                case StepKind.AnswerQuiz: return "Pregunta";
                default: return "Actividad";
            }
        }

        void DrawChoices(MissionStep current)
        {
            var mission = App.Active;
            for (int o = 0; o < current.Options.Count; o++)
            {
                int choice = o;
                menuUi.AddButton(current.Options[o], () =>
                {
                    var result = current.Kind == StepKind.WriteReportSection
                        ? App.SubmitReport.Execute(choice)
                        : App.CompleteStep.Execute(current.Id, new StepAnswer { Choice = choice });
                    toast = result.Ok ? "Registrado." : result.Error;
                    if (current.Kind == StepKind.AnswerQuiz && result.Ok)
                    {
                        if (App.SubmitReport.CloseIfReady().Ok)
                            AnnounceMissionClosed(mission, current.Body);
                        else
                            toast = "Correcto.";
                    }
                    else if (result.Ok && mission != null && App.Progress.Get(mission.Id.Value).Completed)
                        AnnounceMissionClosed(mission);
                    App.SaveLoad.Save();
                    router.MarkDirty();
                });
            }
        }

        void AnnounceMissionClosed(MissionDefinition mission, string quizNote = null)
        {
            if (mission == null) return;
            int score = App.Progress.Get(mission.Id.Value).Score;
            toast = "Misión cerrada · " + score + "/100";
            HubAudio.PlaySuccess();
            var notice = "MISIÓN CERRADA · " + score + "/100 · mira la pizarra";
            if (!string.IsNullOrEmpty(quizNote)) notice += "\n" + quizNote;
            if (!string.IsNullOrEmpty(mission.Learned)) notice += "\n" + mission.Learned;
            Toast(notice, false);
            router.Open("office");
        }

        void Glossary()
        {
            menuUi.AddHeading("Glosario");
            for (int i = 0; i < App.Catalog.Concepts.Count; i++)
            {
                var card = App.Catalog.Concepts[i];
                menuUi.AddBody(card.Name + " · " + card.Category);
                if (!App.Progress.IsUnlocked(card.Id))
                    menuUi.AddBody("Pendiente.");
                else
                {
                    menuUi.AddBody(card.Definition);
                    if (!string.IsNullOrEmpty(card.Importance))
                        menuUi.AddBody("Por qué importa: " + card.Importance);
                    menuUi.AddBody("Defensa: " + card.Defense);
                    if (!App.Progress.IsReviewed(card.Id))
                    {
                        int ci = i;
                        menuUi.AddButton("He leído y comprendido esta ficha", () =>
                        {
                            App.UnlockConcept.Execute(App.Catalog.Concepts[ci].Id);
                            App.SaveLoad.Save();
                            router.MarkDirty();
                        });
                    }
                }
            }
        }

        public void Open(string target)
        {
            router.Open(target);
            toast = "";
        }
    }
}
