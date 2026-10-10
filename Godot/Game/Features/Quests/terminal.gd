extends RefCounted

const COMMANDS: PackedStringArray = ["help", "scan", "map", "notes", "clear"]

static func execute(session: EscapeSession, raw: String) -> Dictionary:
	var outcome := {"text": "", "save": false, "commit": false}
	var line := raw.strip_edges()
	if line.is_empty():
		outcome.text = "Escribe help para ver los comandos de este simulador."
		return outcome
	var space := line.find(" ")
	var command := line.substr(0, space if space >= 0 else line.length()).to_lower()
	var argument := "" if space < 0 else line.substr(space + 1).strip_edges()
	if command == "help":
		outcome.text = "help — ayuda\nscan — estado del incidente\nmap — salas de la operación\nnotes [texto] — leer o añadir una nota\nclear — limpiar la pantalla\n\nNo se ejecutan comandos del sistema."
		return outcome
	if command == "clear":
		return outcome
	if command not in COMMANDS:
		outcome.text = "Comando fuera del vocabulario del simulador. Escribe help. No se ejecutan comandos del sistema."
		return outcome
	match command:
		"scan":
			outcome.text = _scan(session)
		"map":
			outcome.text = _map(session)
		"notes":
			outcome.text = _notes(session, argument, outcome)
		_:
			outcome.text = "Comando fuera del vocabulario del simulador. Escribe help. No se ejecutan comandos del sistema."
	return outcome

static func _scan(session: EscapeSession) -> String:
	var lines := PackedStringArray()
	lines.append("ESTADO DEL INCIDENTE")
	lines.append(session.objective())
	if str(session.flags.get("phish_digit", "")) == "4":
		lines.append("Correo falso marcado.")
	if str(session.flags.get("password_digit", "")) == "7":
		lines.append("Clave del escritorio comprobada.")
	if session.flags.get("mfa_open", false) == true:
		lines.append("Segundo factor aceptado. La puerta magnética puede abrirse.")
	if str(session.flags.get("log_digits", "")) == "14":
		lines.append("Registro: la línea anómala ya está señalada.")
	if session.flags.get("box_open", false) == true:
		lines.append("Caja de evidencias abierta.")
	if session.flags.get("traffic_blue", false) == true:
		lines.append("Fuga identificada en azul.")
	if session.flags.get("cable_blue", false) == true:
		lines.append("Enlace externo cortado.")
	if session.flags.get("life_safety_cut", false) == true:
		lines.append("Se cortó un sistema de vida. La operación sigue.")
	if session.flags.has("lab_open_until"):
		lines.append("El laboratorio tuvo vía de fail-safe.")
	if session.flags.get("pixel_known", false) == true:
		lines.append("Ficha de V. Soto leída.")
	if str(session.flags.get("mac_suffix", "")) == "4F":
		lines.append("La nota ya mostró el final de la MAC.")
	if session.flags.get("tablet_found", false) == true:
		lines.append("Tablet encontrada.")
	if session.flags.get("hash_discarded", false) == true:
		lines.append("Copia discrepante apartada.")
	if session.ending != "":
		lines.append(session.objective())
	return "\n".join(lines)

static func _map(session: EscapeSession) -> String:
	return "MAPA\nOficina UCD · detección\nSala de servidores · contención\nLaboratorio · cierre\n\n" + session.objective()

static func _notes(session: EscapeSession, argument: String, outcome: Dictionary) -> String:
	if not argument.is_empty():
		var next := (session.note + "\n" + argument).strip_edges()
		session.note = next.left(4000)
		outcome.save = true
	return session.note if not session.note.is_empty() else "Aún no tienes notas. Usa notes <texto>."
