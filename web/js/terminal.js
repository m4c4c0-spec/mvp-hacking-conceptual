const ALIASES = {
  ls: "scan",
  cat: "inspect",
  man: "help",
  open: "inspect",
  scope: "map",
};

export function parseCommand(line) {
  const trimmed = String(line ?? "").trim();
  if (!trimmed) return { cmd: "", arg: "" };
  const parts = trimmed.split(/\s+/);
  const raw = parts[0].toLowerCase();
  const cmd = ALIASES[raw] ?? raw;
  const arg = parts.slice(1).join(" ");
  return { cmd, arg, raw };
}

/** Espejo de Academy.FictionTerminal. Vocabulario cerrado, sin shell. */
export function executeTerminal(session, input) {
  const { cmd, arg } = parseCommand(input);
  if (!cmd) return "Escribe help para ver los comandos de este simulador.";
  if (cmd === "help") {
    return [
      "help — ayuda",
      "scan — listar las pistas de la misión",
      "map — mostrar el alcance y el inventario",
      "inspect <id> — leer una ficha local",
      "simulate — explicar el escenario de identidad",
      "notes [texto] — leer o añadir una nota",
      "report — consultar el estado del informe",
      "clear — limpiar la pantalla",
    ].join("\n");
  }
  if (!session.progress().started) return "Acepta primero un ticket en Misiones. Después vuelve a la terminal.";
  if (cmd === "clear") return { clear: true, text: "" };
  const mission = session.active();
  switch (cmd) {
    case "scan":
      return (
        "INVENTARIO LOCAL · " +
        mission.client +
        "\n" +
        mission.evidence.map((item) => item.id + "  /  " + item.label).join("\n") +
        "\n\nUsa inspect <id>. No se ejecutó ningún escaneo de red."
      );
    case "map":
      return "ALCANCE\n" + mission.scope + "\n\n" + mission.evidence.map((item) => "• " + item.label).join("\n");
    case "inspect": {
      const evidence = mission.evidence.find((item) => item.id.toLowerCase() === arg.trim().toLowerCase());
      if (!evidence) return "Ficha no encontrada. Usa scan para ver los identificadores disponibles.";
      session.inspect(evidence.id);
      return (
        evidence.label +
        "\n" +
        evidence.source +
        "\n\n" +
        evidence.body +
        "\n\nRegistra tu observación y defensa en Misiones → Evidencias."
      );
    }
    case "simulate":
      if (mission.id !== "identity") return "El simulador de políticas se usa en la misión 03. Aquí puedes usar scan e inspect.";
      return (
        "SIMULACIÓN NARRATIVA · sin credenciales reales\n" +
        mission.evidence.map((item) => "\n" + item.label + "\n" + item.body).join("\n")
      );
    case "notes": {
      if (arg.length > 0) {
        const next = (session.progress().note + "\n" + arg).trim();
        session.note(next.slice(0, 4000));
      }
      const note = session.progress().note;
      return note && note.trim() ? note : "Aún no tienes notas. Usa notes <texto> o abre el cuaderno.";
    }
    case "report":
      return session.readyToReport()
        ? "Evidencias completas. Abre Informe para justificar el cierre y completar el quiz."
        : "Faltan evidencias por resolver. Abre Misiones para registrar tus decisiones.";
    default:
      return "Comando fuera del vocabulario del simulador. Escribe help. No se ejecutan comandos del sistema.";
  }
}
