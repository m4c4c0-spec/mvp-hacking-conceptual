using System.Text;
using UnityEngine;
using EthicalLab.Application;
using EthicalLab.Domain;
using EthicalLab.Shared;

namespace EthicalLab.Presentation
{
    /// <summary>
    /// Pinta el estado del dominio sobre los objetos del mundo: tickets, carpetas, bandejas, laptop.
    /// Solo lee. Nunca decide reglas.
    /// </summary>
    public sealed class WorldBinder : MonoBehaviour
    {
        public LabUseCases App;
        public HubScene Scene;

        void LateUpdate()
        {
            if (App == null || Scene == null) return;
            PaintBoard();
            PaintTickets();
            PaintLaptop();
            PaintFolders();
            PaintTrays();
            PaintDoor();
        }

        void PaintBoard()
        {
            var mission = App.Active;
            var row = mission != null ? App.Progress.Get(mission.Id.Value) : null;
            bool active = row != null && row.Started && !row.Completed;
            if (Scene.BoardTitle != null)
            {
                Scene.BoardTitle.text = active
                    ? "CASO · " + mission.Number + "  " + mission.Title
                    : "BLUE / RED · tickets · E para aceptar";
            }
            if (Scene.BoardObjective != null)
            {
                Scene.BoardObjective.text = active
                    ? "OBJETIVO · " + mission.Number + "\n" + Wrap(mission.Objective, 40)
                    : "Acepta un ticket";
            }
        }

        void PaintDoor()
        {
            // Prompt más claro; el TextMesh del dintel es estático en HubOffice.
            if (Scene.Door == null) return;
            var view = Scene.Door.GetComponent<InteractableView>();
            if (view == null) return;
            bool open = Scene.Door.IsOpen;
            view.prompt = open
                ? "PUERTA DEL ARCHIVO · abierta · E para cerrar"
                : "PUERTA DEL ARCHIVO · E para abrir · carpetas adentro";
        }

        void PaintTickets()
        {
            var missions = App.Catalog.Missions;
            for (int i = 0; i < Scene.Tickets.Count; i++)
            {
                var view = Scene.Tickets[i];
                if (i >= missions.Count) { view.gameObject.SetActive(false); continue; }
                var mission = missions[i];
                var row = App.Progress.Get(mission.Id.Value);
                bool open = MissionCatalogRules.IsAvailable(missions, App.Progress, mission.Id);
                view.id = InteractableId.Ticket(mission.Id.Value).Value;
                view.prompt = "TICKET " + mission.Number + " · " + mission.Title + (row.Completed ? " · cerrado " + row.Score + "/100" : open ? " · aceptar" : " · bloqueado");
                view.Tint(row.Completed ? HubOffice.Green : row.Started ? HubOffice.Mint : open ? HubOffice.Amber : HubOffice.Muted);
                view.Label(mission.Number + "\n" + (row.Completed ? row.Score + "/100" : open ? mission.Type : "BLOQUEADO"));
            }
        }

        void PaintLaptop()
        {
            var mission = App.Active;
            var row = mission != null ? App.Progress.Get(mission.Id.Value) : null;
            Scene.LaptopScreen.text = row != null && row.Started
                ? ">_ " + mission.Client.ToUpperInvariant() + "\n" + mission.CommandHint
                : ">_ ACADEMY OS\nacepta un ticket";
        }

        void PaintFolders()
        {
            var mission = App.Active;
            var row = mission != null ? App.Progress.Get(mission.Id.Value) : null;
            var groups = ClueWorkflow.ClueGroups(mission);
            bool active = row != null && row.Started;
            for (int i = 0; i < Scene.Folders.Count; i++)
            {
                var view = Scene.Folders[i];
                if (!active || i >= groups.Count)
                {
                    if (!view.IsHeld) view.gameObject.SetActive(false);
                    continue;
                }
                view.gameObject.SetActive(true);
                string group = groups[i];
                var clue = ClueWorkflow.Clue(mission, group);
                bool done = ClueWorkflow.Documented(mission, App.Progress, group);
                bool read = ClueWorkflow.Collected(mission, App.Progress, group);
                view.id = InteractableId.Clue(group).Value;
                view.prompt = (done ? "✓ " : read ? "○ " : "") + "PISTA · " + clue.Prompt + (done ? " · documentada" : read ? " · llevar a bandeja" : " · leer");
                view.Label((done ? "✓ " : "") + clue.Prompt);
                view.Tint(done ? HubOffice.Green : read ? HubOffice.Mint : HubOffice.Amber);
            }
        }

        void PaintTrays()
        {
            var held = Scene.Person != null ? Scene.Person.Held : null;
            var mission = App.Active;
            MissionStep pending = null;
            if (held != null && held.Id.Kind == "clue" && mission != null && ClueWorkflow.Collected(mission, App.Progress, held.Id.Arg))
                pending = ClueWorkflow.Pending(mission, App.Progress, held.Id.Arg);

            if (pending == null)
            {
                Scene.TrayHeader.text = held != null && held.Id.Kind == "clue"
                    ? "Lee la pista (E) antes de clasificarla"
                    : "BANDEJAS · trae una carpeta del archivo (G) y clasifica aquí";
                for (int i = 0; i < Scene.Trays.Count; i++)
                {
                    var tray = Scene.Trays[i];
                    tray.gameObject.SetActive(true);
                    tray.prompt = "BANDEJA " + (i + 1) + " · espera carpeta del archivo";
                    tray.Label("BANDEJA " + (i + 1) + "\n—");
                    tray.Tint(HubOffice.Muted);
                }
                return;
            }

            Scene.TrayHeader.text = (pending.Id.Value.EndsWith(".observe") ? "OBSERVACIÓN · " : "DEFENSA · ") + pending.Prompt;
            for (int i = 0; i < Scene.Trays.Count; i++)
            {
                var tray = Scene.Trays[i];
                bool visible = i < pending.Options.Count;
                tray.gameObject.SetActive(visible);
                if (!visible) continue;
                tray.prompt = "BANDEJA " + (i + 1) + " · " + pending.Options[i];
                tray.Label(Wrap(pending.Options[i], 22));
                tray.Tint(HubOffice.Amber);
            }
        }

        static string Wrap(string text, int width)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= width) return text ?? "";
            var words = text.Split(' ');
            var sb = new StringBuilder();
            int line = 0;
            for (int i = 0; i < words.Length; i++)
            {
                if (line + words[i].Length > width && line > 0) { sb.Append('\n'); line = 0; }
                else if (line > 0) { sb.Append(' '); line++; }
                sb.Append(words[i]);
                line += words[i].Length;
            }
            return sb.ToString();
        }
    }
}
