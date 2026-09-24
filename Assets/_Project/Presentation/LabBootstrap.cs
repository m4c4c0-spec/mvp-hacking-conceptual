using System;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif
using EthicalLab.Application;
using EthicalLab.Domain;
using EthicalLab.Infrastructure;
using EthicalLab.Shared;

namespace EthicalLab.Presentation
{
    /// <summary>
    /// Une mundo 3D ↔ casos de uso. Cada objeto usado se traduce a StartMission / CompleteStep / etc.
    /// </summary>
    public sealed class LabBootstrap : MonoBehaviour
    {
        LabUseCases app;
        HubScene scene;
        HubHud hud;
        float saveTimer;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void AutoStart()
        {
            string name = SceneManager.GetActiveScene().name;
            if (name != "Boot" && name != "Hub") return;
            if (FindFirstObjectByType<LabBootstrap>() != null) return;
            if (GameObject.Find("Analyst Academy") != null) return;
            new GameObject("Ethical Lab").AddComponent<LabBootstrap>();
        }

        void Awake()
        {
            UnityEngine.Application.targetFrameRate = 60;
            app = new LabUseCases(JsonMissionRepository.FromResources(), new JsonProgressStore());
            scene = HubOffice.Build();
            scene.Person.Used += OnUsed;
            scene.Person.Grabbed += OnGrabbed;
            scene.Person.Released += OnReleased;
            EnsureEventSystem();
            hud = gameObject.AddComponent<HubHud>();
            hud.App = app;
            hud.Person = scene.Person;
            var binder = gameObject.AddComponent<WorldBinder>();
            binder.App = app;
            binder.Scene = scene;
        }

        void OnUsed(InteractableId id)
        {
            switch (id.Kind)
            {
                case "laptop":
                case "terminal":
                    hud.Open("terminal");
                    break;
                case "notebook":
                    hud.Open("glossary");
                    break;
                case "board":
                case "tickets":
                    hud.Open("missions");
                    break;
                case "ticket":
                    AcceptTicket(id.Arg);
                    break;
                case "clue":
                    ReadClue(id.Arg, scene.Person.Held == null);
                    break;
                case "tray":
                    Classify(id.Arg);
                    break;
                case "scope":
                    OutOfScope();
                    break;
                case "door":
                    scene.Door.Toggle();
                    break;
                case "drawer":
                    scene.Drawer.Toggle();
                    break;
                default:
                    hud.Toast("Ese objeto no forma parte del caso.");
                    break;
            }
        }

        void OnGrabbed(InteractableId id)
        {
            if (id.Kind != "clue") return;
            var mission = app.Active;
            if (mission == null || !app.Progress.Get(mission.Id.Value).Started) return;
            // Tomar una carpeta ya es leerla: mismo camino que E.
            if (ClueWorkflow.Collected(mission, app.Progress, id.Arg)) ShowClue(mission, id.Arg);
            else ReadClue(id.Arg, false);
        }

        void OnReleased(InteractableId id)
        {
            hud.Reading("");
        }

        void AcceptTicket(string missionId)
        {
            var result = app.StartMission.Execute(new MissionId(missionId));
            var mission = app.Catalog.GetMission(new MissionId(missionId));
            if (result.Ok && mission != null)
            {
                hud.Toast("Ticket aceptado: " + mission.Title + "\nALCANCE · " + mission.Scope + "\nLas pistas están en el archivo. La terminal también sirve: " + mission.CommandHint);
            }
            else hud.Toast(result.Error);
            app.SaveLoad.Save();
        }

        void ReadClue(string group, bool takeFirst)
        {
            var mission = app.Active;
            if (mission == null || !app.Progress.Get(mission.Id.Value).Started)
            {
                hud.Toast("Acepta un ticket en la pizarra antes de leer evidencias.");
                return;
            }
            var clue = ClueWorkflow.Clue(mission, group);
            if (clue == null) { hud.Toast("Esta carpeta no pertenece al caso activo."); return; }
            var result = app.CompleteStep.Execute(clue.Id, new StepAnswer());
            if (!result.Ok) { hud.Toast(result.Error); return; }
            app.SaveLoad.Save();
            if (takeFirst)
            {
                for (int i = 0; i < scene.Folders.Count; i++)
                    if (scene.Folders[i].Id.Value == InteractableId.Clue(group).Value) { scene.Person.Take(scene.Folders[i]); break; }
            }
            ShowClue(mission, group);
        }

        void ShowClue(MissionDefinition mission, string group)
        {
            var clue = ClueWorkflow.Clue(mission, group);
            var pending = ClueWorkflow.Pending(mission, app.Progress, group);
            var text = new StringBuilder();
            text.Append(clue.Prompt.ToUpperInvariant()).Append('\n').Append(clue.Body).Append("\n\n");
            if (pending == null)
            {
                text.Append("✓ Hallazgo documentado. Devuelve la carpeta (G).");
            }
            else
            {
                text.Append(pending.Id.Value.EndsWith(".observe") ? "OBSERVACIÓN · " : "DEFENSA · ").Append(pending.Prompt).Append('\n');
                for (int i = 0; i < pending.Options.Count; i++)
                    text.Append("  Bandeja ").Append(i + 1).Append(" → ").Append(pending.Options[i]).Append('\n');
                text.Append("\nLleva la carpeta al escritorio y pulsa E sobre la bandeja que corresponda.");
            }
            hud.Reading(text.ToString());
        }

        void Classify(string trayArg)
        {
            var held = scene.Person.Held;
            var mission = app.Active;
            if (held == null || held.Id.Kind != "clue" || mission == null)
            {
                hud.Toast("Toma una carpeta del archivo (G) y léela (E) antes de clasificar.");
                return;
            }
            string group = held.Id.Arg;
            var pending = ClueWorkflow.Pending(mission, app.Progress, group);
            if (pending == null)
            {
                hud.Toast("Esta pista ya está documentada.");
                return;
            }
            int choice = int.TryParse(trayArg, out int parsed) ? parsed : -1;
            var result = app.CompleteStep.Execute(pending.Id, new StepAnswer { Choice = choice });
            app.SaveLoad.Save();
            if (!result.Ok)
            {
                hud.Toast(result.Error + "\n" + pending.Body);
                ShowClue(mission, group);
                return;
            }
            var next = ClueWorkflow.Pending(mission, app.Progress, group);
            if (next != null)
            {
                hud.Toast("Observación registrada. Ahora la defensa: ¿qué harías al respecto?");
                ShowClue(mission, group);
                return;
            }
            var clue = mission.Step(new StepId(group + ".defend"));
            var card = clue != null && clue.UnlocksConcept ? app.Catalog.GetConcept(clue.Unlocks) : null;
            hud.Toast("Hallazgo documentado." + (card != null ? "\nFICHA DESBLOQUEADA · " + card.Name + "\n" + card.Defense : "") + "\nCarpeta devuelta al archivo.");
            hud.Reading("");
            scene.Person.Drop();
            if (AllDocumented(mission)) hud.Toast("Todas las pistas documentadas. Abre la laptop o el cuaderno para redactar el informe.");
        }

        bool AllDocumented(MissionDefinition mission)
        {
            var groups = ClueWorkflow.ClueGroups(mission);
            for (int i = 0; i < groups.Count; i++)
                if (!ClueWorkflow.Documented(mission, app.Progress, groups[i])) return false;
            return true;
        }

        void OutOfScope()
        {
            var mission = app.Active;
            string scope = mission != null ? mission.Scope : "No hay ticket activo.";
            hud.Toast("FUERA DE ALCANCE\nEste servidor no es de tu cliente. Sin autorización explícita no se investiga, aunque esté al alcance de la mano.\nALCANCE DEL TICKET · " + scope);
        }

        void Update()
        {
            saveTimer += Time.unscaledDeltaTime;
            if (saveTimer >= 20f)
            {
                saveTimer = 0f;
                app.SaveLoad.Save();
            }
        }

        void OnApplicationQuit() => app.SaveLoad.Save();
        void OnApplicationPause(bool pause) { if (pause) app.SaveLoad.Save(); }

        static void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null) return;
            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
            go.AddComponent<InputSystemUIInputModule>();
#else
            go.AddComponent<StandaloneInputModule>();
#endif
        }
    }
}
