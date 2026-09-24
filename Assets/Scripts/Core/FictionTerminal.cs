using System;
using System.Linq;
using System.Text;

namespace Academy
{
    public static class FictionTerminal
    {
        // Deliberately closed vocabulary. No shell, subprocess, URL or network API.
        public static string Execute(AcademySession session, string input)
        {
            string[] words = (input ?? "").Trim().Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
            if (words.Length == 0) return "Escribe help para ver los comandos de este simulador.";
            string command = words[0].ToLowerInvariant();
            string arg = words.Length > 1 ? words[1].Trim() : "";
            if (command == "help") return "help — ayuda\nscan — listar las pistas de la misión\nmap — mostrar el alcance y el inventario\ninspect <id> — leer una ficha local\nsimulate — explicar el escenario de identidad\nnotes [texto] — leer o añadir una nota\nreport — consultar el estado del informe\nclear — limpiar la pantalla";
            if (!session.Progress.started) return "Acepta primero un ticket en Misiones. Después vuelve a la terminal.";
            switch (command)
            {
                case "clear": return "";
                case "scan":
                    return "INVENTARIO LOCAL · " + session.Active.client + "\n" + string.Join("\n", session.Active.evidence.Select(e => e.id + "  /  " + e.label)) + "\n\nUsa inspect <id>. No se ejecutó ningún escaneo de red.";
                case "map": return "ALCANCE\n" + session.Active.scope + "\n\n" + string.Join("\n", session.Active.evidence.Select(e => "• " + e.label));
                case "inspect":
                    var evidence = session.Active.evidence.FirstOrDefault(e => e.id.Equals(arg, StringComparison.OrdinalIgnoreCase));
                    if (evidence == null) return "Ficha no encontrada. Usa scan para ver los identificadores disponibles.";
                    session.Inspect(evidence.id);
                    return evidence.label + "\n" + evidence.source + "\n\n" + evidence.body + "\n\nRegistra tu observación y defensa en Misiones → Evidencias.";
                case "simulate":
                    if (session.Active.id != "identity") return "El simulador de políticas se usa en la misión 03. Aquí puedes usar scan e inspect.";
                    var result = new StringBuilder("SIMULACIÓN NARRATIVA · sin credenciales reales\n");
                    foreach (var e in session.Active.evidence) result.AppendLine("\n" + e.label + "\n" + e.body);
                    return result.ToString();
                case "notes":
                    if (arg.Length > 0) session.Note((session.Progress.note + "\n" + arg).Trim().Substring(0, Math.Min(4000, (session.Progress.note + "\n" + arg).Trim().Length)));
                    return string.IsNullOrWhiteSpace(session.Progress.note) ? "Aún no tienes notas. Usa notes <texto> o abre el cuaderno." : session.Progress.note;
                case "report": return session.ReadyToReport ? "Evidencias completas. Abre Informe para justificar el cierre y completar el quiz." : "Faltan evidencias por resolver. Abre Misiones para registrar tus decisiones.";
                default: return "Comando fuera del vocabulario del simulador. Escribe help. No se ejecutan comandos del sistema.";
            }
        }
    }
}
