using EthicalLab.Domain;
using EthicalLab.Shared;

namespace EthicalLab.Application
{
    public static class NarrativeTerminal
    {
        public static string Execute(LabUseCases app, string input)
        {
            string line = (input ?? "").Trim();
            if (line.Length == 0) return "Escribe help para ver los comandos de este simulador.";
            int space = line.IndexOf(' ');
            string command = (space < 0 ? line : line.Substring(0, space)).ToLowerInvariant();
            string arg = space < 0 ? "" : line.Substring(space + 1).Trim();
            if (command == "help")
                return "help — ayuda\nscan — listar las pistas de la misión\nmap — mostrar el alcance y el inventario\ninspect <id> — leer una ficha local\nsimulate — explicar el escenario de identidad\nnotes [texto] — leer o añadir una nota\nreport — consultar el estado del informe\nclear — limpiar la pantalla";
            var mission = app.Active;
            if (mission == null || !app.Progress.Get(mission.Id.Value).Started)
                return "Acepta primero un ticket en Misiones. Después vuelve a la terminal.";
            if (command == "clear") return "";
            if (!TerminalVocabulary.IsKnown(command))
                return "Comando fuera del vocabulario del simulador. Escribe help. No se ejecutan comandos del sistema.";
            app.CompleteStep.Execute(new StepId("terminal"), new StepAnswer());
            switch (command)
            {
                case "scan":
                    return "INVENTARIO LOCAL · " + mission.Client + "\n" + ListClues(mission) + "\n\nUsa inspect <id>. No se ejecutó ningún escaneo de red.";
                case "map":
                    return "ALCANCE\n" + mission.Scope + "\n\n" + ListLabels(mission);
                case "inspect":
                    return Inspect(app, mission, arg);
                case "simulate":
                    if (mission.Id.Value != "identity") return "El simulador de políticas se usa en la misión 03. Aquí puedes usar scan e inspect.";
                    return "SIMULACIÓN NARRATIVA · sin credenciales reales\n" + ListBodies(mission);
                case "notes":
                    var row = app.Progress.Get(mission.Id.Value);
                    if (arg.Length > 0)
                    {
                        string next = (row.Note + "\n" + arg).Trim();
                        if (next.Length > 4000) next = next.Substring(0, 4000);
                        row.Note = next;
                    }
                    return string.IsNullOrWhiteSpace(row.Note) ? "Aún no tienes notas. Usa notes <texto>." : row.Note;
                case "report":
                    return AllClassified(app, mission)
                        ? "Evidencias completas. Abre Informe para justificar el cierre y completar el quiz."
                        : "Faltan evidencias por resolver. Abre Misiones para registrar tus decisiones.";
                default:
                    return "Comando fuera del vocabulario del simulador. Escribe help.";
            }
        }

        static string Inspect(LabUseCases app, MissionDefinition mission, string arg)
        {
            for (int i = 0; i < mission.Steps.Count; i++)
            {
                var step = mission.Steps[i];
                if (step.Kind != StepKind.CollectClue) continue;
                if (!step.Id.ClueGroup.Equals(arg, System.StringComparison.OrdinalIgnoreCase)) continue;
                app.CompleteStep.Execute(step.Id, new StepAnswer());
                return step.Prompt + "\n\n" + step.Body + "\n\nRegistra tu observación y defensa en Misiones → Evidencias.";
            }
            return "Ficha no encontrada. Usa scan para ver los identificadores disponibles.";
        }

        static string ListClues(MissionDefinition mission)
        {
            var parts = new System.Text.StringBuilder();
            for (int i = 0; i < mission.Steps.Count; i++)
            {
                var step = mission.Steps[i];
                if (step.Kind != StepKind.CollectClue) continue;
                if (parts.Length > 0) parts.Append('\n');
                parts.Append(step.Id.ClueGroup).Append("  /  ").Append(step.Prompt);
            }
            return parts.ToString();
        }

        static string ListLabels(MissionDefinition mission)
        {
            var parts = new System.Text.StringBuilder();
            for (int i = 0; i < mission.Steps.Count; i++)
            {
                var step = mission.Steps[i];
                if (step.Kind != StepKind.CollectClue) continue;
                parts.Append("• ").Append(step.Prompt).Append('\n');
            }
            return parts.ToString();
        }

        static string ListBodies(MissionDefinition mission)
        {
            var parts = new System.Text.StringBuilder();
            for (int i = 0; i < mission.Steps.Count; i++)
            {
                var step = mission.Steps[i];
                if (step.Kind != StepKind.CollectClue) continue;
                parts.Append('\n').Append(step.Prompt).Append('\n').Append(step.Body);
            }
            return parts.ToString();
        }

        static bool AllClassified(LabUseCases app, MissionDefinition mission)
        {
            for (int i = 0; i < mission.Steps.Count; i++)
            {
                var step = mission.Steps[i];
                if (step.Kind == StepKind.ClassifyItem && !app.Progress.Step(mission.Id.Value, step.Id.Value).Completed)
                    return false;
            }
            return true;
        }
    }
}
