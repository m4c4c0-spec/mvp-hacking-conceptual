using System;
using UnityEngine;
using EthicalLab.Application;
using EthicalLab.Domain;
using EthicalLab.Shared;

namespace EthicalLab.Presentation
{
    public sealed class HubHud : MonoBehaviour
    {
        enum HubPage { Office, Home, Missions, Terminal, Case, Glossary }

        public LabUseCases App;
        public PcInteractor Person;
        HubPage page = HubPage.Office;
        string term = "ACADEMY OS  /  primera persona\nhelp · scan · inspect · notes · report\n";
        string command = "";
        string toast = "";
        string worldToast = "";
        float worldToastUntil;
        string reading = "";
        string selectedStep = "";
        Vector2 scroll;
        bool resetConfirm;
        bool controlsHintVisible = true;

        /// <summary>Mensaje breve en el mundo (feedback de una acción física).</summary>
        public void Toast(string text, bool playBeep = true)
        {
            worldToast = text ?? "";
            worldToastUntil = Time.unscaledTime + Mathf.Clamp(2.5f + worldToast.Length * 0.04f, 3f, 12f);
            if (playBeep && !string.IsNullOrEmpty(worldToast)) HubAudio.PlayToast();
        }

        /// <summary>Panel de lectura de la pista en mano. Vacío = ocultar.</summary>
        public void Reading(string text) => reading = text ?? "";

        void Update()
        {
            if (Person == null) return;
            Person.MenuOpen = page != HubPage.Office;
            if (PcButtons.Escape)
                page = page == HubPage.Office ? HubPage.Home : HubPage.Office;
            if (page == HubPage.Office && PcButtons.Help)
                controlsHintVisible = !controlsHintVisible;
            App.Tick(Time.unscaledDeltaTime);
        }

        void OnGUI()
        {
            if (page == HubPage.Office)
            {
                DrawCrosshair();
                DrawControlsHint();
                DrawNextStepGuide();
                GUI.Label(new Rect(24, Screen.height - 64, Screen.width - 48, 48), Person != null ? Person.Prompt : "");
                if (!string.IsNullOrEmpty(reading))
                {
                    float w = Mathf.Min(460f, Screen.width * 0.4f);
                    GUI.Box(new Rect(Screen.width - w - 24, 24, w, Screen.height * 0.6f), "");
                    GUI.Label(new Rect(Screen.width - w - 12, 36, w - 24, Screen.height * 0.6f - 24), reading);
                }
                if (!string.IsNullOrEmpty(worldToast) && Time.unscaledTime < worldToastUntil)
                {
                    float w = Mathf.Min(680f, Screen.width - 48);
                    GUI.Box(new Rect((Screen.width - w) * 0.5f, 24, w, 96), "");
                    GUI.Label(new Rect((Screen.width - w) * 0.5f + 12, 32, w - 24, 88), worldToast);
                }
                return;
            }

            GUI.Box(new Rect(0, 0, Screen.width, Screen.height), "");
            GUILayout.BeginArea(new Rect(40, 24, Screen.width - 80, Screen.height - 48));
            GUILayout.BeginHorizontal();
            Tab("Inicio", HubPage.Home);
            Tab("Misiones", HubPage.Missions);
            Tab("Terminal", HubPage.Terminal);
            Tab("Expediente", HubPage.Case);
            Tab("Glosario", HubPage.Glossary);
            Tab("Oficina", HubPage.Office);
            GUILayout.EndHorizontal();
            if (!string.IsNullOrEmpty(toast)) GUILayout.Label(toast);
            scroll = GUILayout.BeginScrollView(scroll);
            DrawPage();
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        void DrawPage()
        {
            switch (page)
            {
                case HubPage.Office:
                    break;
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
                    throw new ArgumentOutOfRangeException(nameof(page), page, null);
            }
        }

        void Tab(string label, HubPage id)
        {
            if (GUILayout.Button(page == id ? "· " + label : label, GUILayout.Height(32)))
            {
                page = id;
                toast = "";
                resetConfirm = false;
            }
        }

        void Home()
        {
            GUILayout.Label("BLUE / RED · ANALYST ACADEMY");
            GUILayout.Label("Primera persona. La UI no decide reglas: pide StartMission, CompleteStep, UnlockConcept, SubmitReport.");
            GUILayout.Label("Completadas: " + App.Progress.CompletedCount + " / " + App.Catalog.Missions.Count);
            if (GUILayout.Button("Abrir buzón de misiones", GUILayout.Height(40))) page = HubPage.Missions;
            if (GUILayout.Button("Volver a caminar por la oficina", GUILayout.Height(40))) page = HubPage.Office;
            GUILayout.Space(16);
            DrawDemoReset();
        }

        void DrawDemoReset()
        {
            if (!resetConfirm)
            {
                if (GUILayout.Button("Reiniciar demo", GUILayout.Height(36)))
                    resetConfirm = true;
                return;
            }
            GUILayout.BeginVertical("box");
            GUILayout.Label("¿Seguro? Se borrará el progreso guardado de esta demo.");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Sí, reiniciar", GUILayout.Height(36)))
            {
                ConfirmDemoReset();
            }
            if (GUILayout.Button("Cancelar", GUILayout.Height(36)))
                resetConfirm = false;
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        void ConfirmDemoReset()
        {
            resetConfirm = false;
            if (Person != null) Person.Drop();
            Reading("");
            App.ResetDemo();
            UnityEngine.PlayerPrefs.DeleteKey(HubGuide.WelcomePrefsKey);
            UnityEngine.PlayerPrefs.Save();
            toast = "Demo reiniciada.";
            Toast(HubGuide.WelcomeTip);
            page = HubPage.Office;
        }

        void Missions()
        {
            GUILayout.Label("Buzón de tickets (datos, no código)");
            for (int i = 0; i < App.Catalog.Missions.Count; i++)
            {
                var mission = App.Catalog.Missions[i];
                bool open = MissionCatalogRules.IsAvailable(App.Catalog.Missions, App.Progress, mission.Id);
                GUILayout.BeginVertical("box");
                GUILayout.Label(mission.Number + "  " + mission.Title + "  ·  " + mission.Client);
                GUILayout.Label(mission.Brief);
                var row = App.Progress.Get(mission.Id.Value);
                GUILayout.Label(row.Completed ? "CERRADO · " + row.Score + "/100" : open ? "Disponible" : "Bloqueada");
                GUI.enabled = open;
                if (GUILayout.Button(row.Completed ? "Consultar" : row.Started ? "Continuar" : "Aceptar ticket"))
                {
                    var result = App.StartMission.Execute(mission.Id);
                    toast = result.Ok ? "Ticket aceptado: " + mission.Title : result.Error;
                    if (result.Ok) { selectedStep = ""; page = HubPage.Case; }
                    App.SaveLoad.Save();
                }
                GUI.enabled = true;
                GUILayout.EndVertical();
            }
        }

        void Terminal()
        {
            GUILayout.Label(term);
            command = GUILayout.TextField(command);
            if (GUILayout.Button("Ejecutar comando ficticio") || (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return))
            {
                if (command.Trim().Length > 0)
                {
                    if (command.Trim().ToLowerInvariant() == "clear") term = "pantalla limpia\n";
                    else term += "\nanalista > " + command + "\n" + NarrativeTerminal.Execute(App, command) + "\n";
                    command = "";
                    App.SaveLoad.Save();
                }
            }
        }

        void Case()
        {
            var mission = App.Active;
            if (mission == null || !App.Progress.Get(mission.Id.Value).Started)
            {
                GUILayout.Label("Acepta un ticket en Misiones.");
                return;
            }
            GUILayout.Label(mission.Title + "  ·  " + mission.Client);
            GUILayout.Label("Alcance: " + mission.Scope);
            for (int i = 0; i < mission.Steps.Count; i++)
            {
                var step = mission.Steps[i];
                if (step.Kind == StepKind.UseTerminal) continue;
                var state = App.Progress.Step(mission.Id.Value, step.Id.Value);
                if (GUILayout.Toggle(selectedStep == step.Id.Value, (state.Completed ? "✓ " : "○ ") + step.Kind + " · " + step.Prompt))
                    selectedStep = step.Id.Value;
            }
            var current = mission.Step(new StepId(selectedStep));
            if (current == null) return;
            GUILayout.BeginVertical("box");
            GUILayout.Label(current.Prompt);
            if (!string.IsNullOrEmpty(current.Body)) GUILayout.Label(current.Body);
            DrawStep(current);
            GUILayout.EndVertical();
        }

        void DrawStep(MissionStep current)
        {
            switch (current.Kind)
            {
                case StepKind.CollectClue:
                    if (GUILayout.Button("Inspeccionar pista"))
                    {
                        var result = App.CompleteStep.Execute(current.Id, new StepAnswer());
                        toast = result.Ok ? "Pista leída." : result.Error;
                        App.SaveLoad.Save();
                    }
                    break;
                case StepKind.ClassifyItem:
                case StepKind.AnswerQuiz:
                case StepKind.WriteReportSection:
                    DrawChoices(current);
                    break;
                case StepKind.UseTerminal:
                    GUILayout.Label("Usa la pestaña Terminal. Vocabulario: scan, inspect, notes, report.");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(current.Kind), current.Kind, null);
            }
        }

        void DrawChoices(MissionStep current)
        {
            var mission = App.Active;
            for (int o = 0; o < current.Options.Count; o++)
            {
                if (!GUILayout.Button(current.Options[o])) continue;
                var result = current.Kind == StepKind.WriteReportSection
                    ? App.SubmitReport.Execute(o)
                    : App.CompleteStep.Execute(current.Id, new StepAnswer { Choice = o });
                toast = result.Ok ? "Registrado." : result.Error;
                if (current.Kind == StepKind.AnswerQuiz && result.Ok)
                    toast = App.SubmitReport.CloseIfReady().Ok ? "Misión cerrada · " + App.Progress.Get(mission.Id.Value).Score : "Correcto.";
                App.SaveLoad.Save();
            }
        }

        void Glossary()
        {
            GUILayout.Label("Fichas (se desbloquean al defender un hallazgo)");
            for (int i = 0; i < App.Catalog.Concepts.Count; i++)
            {
                var card = App.Catalog.Concepts[i];
                GUILayout.BeginVertical("box");
                GUILayout.Label(card.Name + "  ·  " + card.Category);
                if (!App.Progress.IsUnlocked(card.Id))
                {
                    GUILayout.Label("Pendiente.");
                }
                else
                {
                    GUILayout.Label(card.Definition);
                    GUILayout.Label("Defensa: " + card.Defense);
                    if (!App.Progress.IsReviewed(card.Id) && GUILayout.Button("He leído y comprendido esta ficha"))
                    {
                        App.UnlockConcept.Execute(card.Id);
                        App.SaveLoad.Save();
                    }
                }
                GUILayout.EndVertical();
            }
        }

        /// <summary>Línea compacta de controles (esquina). H / F1 para mostrar/ocultar.</summary>
        void DrawControlsHint()
        {
            if (!controlsHintVisible)
            {
                GUI.Label(new Rect(24, 8, 140, 20), "H · ayuda");
                return;
            }
            float w = Mathf.Min(740f, Screen.width - 48f);
            GUI.Label(new Rect(24, 8, w, 22), HubGuide.ControlsHint);
        }

        void DrawCrosshair()
        {
            float x = Screen.width * 0.5f;
            float y = Screen.height * 0.5f;
            GUI.Box(new Rect(x - 1, y - 8, 2, 16), "");
            GUI.Box(new Rect(x - 8, y - 1, 16, 2), "");
        }

        /// <summary>Pista persistente del siguiente paso físico (oficina, no storybook).</summary>
        void DrawNextStepGuide()
        {
            if (App == null) return;
            string guide = HubGuide.NextStep(App, Person);
            if (string.IsNullOrEmpty(guide)) return;
            float w = Mathf.Min(560f, Screen.width - 48f);
            GUI.Box(new Rect(24, Screen.height - 120, w, 48), "");
            GUI.Label(new Rect(36, Screen.height - 112, w - 24, 36), "→ " + guide);
        }

        public void Open(string target)
        {
            switch (target)
            {
                case "terminal": page = HubPage.Terminal; break;
                case "glossary": page = HubPage.Glossary; break;
                case "missions": page = HubPage.Missions; break;
                case "case": page = HubPage.Case; break;
                case "home": page = HubPage.Home; break;
                default: page = HubPage.Home; break;
            }
            toast = "";
        }
    }
}
