using EthicalLab.Application;

namespace EthicalLab.Presentation
{
    /// <summary>
    /// Pista corta del siguiente paso físico en la oficina (solo lee ClueWorkflow / progreso).
    /// </summary>
    public static class HubGuide
    {
        public const string WelcomeTip =
            "Bienvenida · WASD caminar · botón derecho mirar · E usar · G tomar carpeta\nMira la pizarra y acepta un ticket disponible (E).";

        public const string ControlsHint =
            "WASD · RMB mirar · E usar · G carpeta · ESC menú · Reiniciar en Inicio · H/F1 ayuda";

        public const string WelcomePrefsKey = "EthicalLab.HubWelcomeShown";

        public static string NextStep(LabUseCases app, PcInteractor person)
        {
            if (app == null) return "";
            var mission = app.Active;
            var row = mission != null ? app.Progress.Get(mission.Id.Value) : null;
            if (mission == null || row == null || !row.Started || row.Completed)
                return "Mira la pizarra · E en un ticket disponible";

            var groups = ClueWorkflow.ClueGroups(mission);
            bool anyCollected = false;
            for (int i = 0; i < groups.Count; i++)
            {
                if (ClueWorkflow.Collected(mission, app.Progress, groups[i])) anyCollected = true;
            }

            if (ClueWorkflow.AllDocumented(mission, app.Progress))
                return "Acércate al buzón de informe del escritorio · E para el expediente";

            var held = person != null ? person.Held : null;
            if (held != null && held.Id.Kind == "clue")
            {
                string group = held.Id.Arg;
                if (!ClueWorkflow.Collected(mission, app.Progress, group))
                    return "Pulsa E sobre la carpeta en mano para leerla";
                if (ClueWorkflow.Pending(mission, app.Progress, group) != null)
                    return "Lleva la carpeta al escritorio · E en la bandeja correcta";
                return "Devuelve la carpeta (G) · sigue con otra del archivo";
            }

            if (!anyCollected)
                return "Pasa la puerta al archivo · E en una carpeta · G para tomar";

            return "Archivo: E/G en carpeta pendiente · llévala al escritorio para clasificar";
        }

        public static bool AnyMissionStarted(LabUseCases app)
        {
            if (app == null) return false;
            for (int i = 0; i < app.Catalog.Missions.Count; i++)
            {
                var mission = app.Catalog.Missions[i];
                if (app.Progress.Get(mission.Id.Value).Started) return true;
            }
            return false;
        }
    }
}
