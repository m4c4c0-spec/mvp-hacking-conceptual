class_name EscapeSession
extends RefCounted

const Catalog = preload("res://Game/Infrastructure/catalog_store.gd")
const Store = preload("res://Game/Infrastructure/progress_store.gd")
const SAVE_PATH := "user://escape-progress-v1.json"
const ORDER: PackedStringArray = ["phish", "password", "mfa", "logs", "box", "traffic", "cable", "fire", "osint", "base64", "rssi", "hash", "kill"]
const TIMEOUT_LINE := "Descarga completa. La lista salió del edificio."
const VICTORY_LINE := "Copia detenida. La lista sigue en el edificio."
const SAFETY_LINE := "Salida de emergencia. La operación se detiene sin castigo."
const MIGRATION_LINE := "Esta partida es del protocolo anterior y no se convierte. Empieza una operación nueva."

var catalog: Dictionary = {}
var state := {
	"version": 3,
	"mode": "operacion",
	"elapsed": 0.0,
	"paused": false,
	"manual_open": false,
	"flags": {},
	"hints": {},
	"manuals_read": [],
	"applied": [],
	"ending": "",
	"kill_attempts": 0,
	"lockout_until": 0.0,
	"note": "",
	"pending_radio": "",
	"segment": "",
	"objective_since": 0.0,
}
var last_error := ""
var migration_notice := ""
var _store = Store.new()
var version: int:
	get:
		return int(state.get("version", 3))
	set(value):
		state["version"] = value
var mode: String:
	get:
		return str(state.get("mode", "operacion"))
	set(value):
		state["mode"] = value
var elapsed: float:
	get:
		return float(state.get("elapsed", 0.0))
	set(value):
		state["elapsed"] = value
var paused: bool:
	get:
		return bool(state.get("paused", false))
	set(value):
		state["paused"] = value
var manual_open: bool:
	get:
		return bool(state.get("manual_open", false))
	set(value):
		state["manual_open"] = value
var flags: Dictionary:
	get:
		return state["flags"]
	set(value):
		state["flags"] = value
var hints: Dictionary:
	get:
		return state["hints"]
	set(value):
		state["hints"] = value
var manuals_read: Array:
	get:
		return state["manuals_read"]
	set(value):
		state["manuals_read"] = value
var applied: Array:
	get:
		return state["applied"]
	set(value):
		state["applied"] = value
var ending: String:
	get:
		return str(state.get("ending", ""))
	set(value):
		state["ending"] = value
var kill_attempts: int:
	get:
		return int(state.get("kill_attempts", 0))
	set(value):
		state["kill_attempts"] = value
var lockout_until: float:
	get:
		return float(state.get("lockout_until", 0.0))
	set(value):
		state["lockout_until"] = value
var note: String:
	get:
		return str(state.get("note", ""))
	set(value):
		state["note"] = value
var pending_radio: String:
	get:
		return str(state.get("pending_radio", ""))
	set(value):
		state["pending_radio"] = value
var segment: String:
	get:
		return str(state.get("segment", ""))
	set(value):
		state["segment"] = value
var objective_since: float:
	get:
		return float(state.get("objective_since", 0.0))
	set(value):
		state["objective_since"] = value

func _init() -> void:
	# Cada partida tiene su propio diccionario: el valor por defecto del script no se comparte.
	state = {
		"version": 3,
		"mode": "operacion",
		"elapsed": 0.0,
		"paused": false,
		"manual_open": false,
		"flags": {},
		"hints": {},
		"manuals_read": [],
		"applied": [],
		"ending": "",
		"kill_attempts": 0,
		"lockout_until": 0.0,
		"note": "",
		"pending_radio": "",
		"segment": "",
		"objective_since": 0.0,
	}
	catalog = Catalog.new().load_catalog()
	if catalog.is_empty():
		last_error = "No se pudo leer el catálogo de la operación."
		return
	reset()

func reset() -> void:
	migration_notice = ""
	last_error = ""
	version = 3
	mode = "operacion"
	elapsed = 0.0
	paused = false
	manual_open = false
	flags = {}
	hints = {}
	for item in puzzles():
		hints[str(item.get("id", ""))] = 0
	manuals_read = []
	applied = []
	ending = ""
	kill_attempts = 0
	lockout_until = 0.0
	note = ""
	pending_radio = ""
	segment = "phish" if hints.has("phish") else ""
	objective_since = 0.0

func puzzles() -> Array:
	return catalog.get("puzzles", [])

func puzzle(id: String) -> Dictionary:
	for item in puzzles():
		if str(item.get("id", "")) == id:
			return item
	return {}

func read_manual(id: String) -> String:
	var found := puzzle(id)
	if found.is_empty():
		return "No hay manual de ese tema."
	if id not in manuals_read:
		manuals_read.append(id)
	manual_open = true
	return str(found.get("manual", ""))

func act(id: String, payload: Variant = null) -> String:
	if ending != "":
		return objective()
	if id not in ORDER:
		return "Ese paso no está en la operación."
	var blocked := _blocked(id)
	if not blocked.is_empty():
		return blocked
	if id != "cable" and id != "fire" and _solved(id):
		return ""
	var data: Dictionary = payload if payload is Dictionary else {}
	var result := ""
	match id:
		"phish":
			result = _act_phish(data)
		"password":
			result = _act_code(data, "Condores2026", "password_digit", "7", "Esa clave no sale de lo que está a la vista.")
		"mfa":
			result = _act_mfa(data)
		"logs":
			result = _act_logs(data)
		"box":
			result = _act_code(data, "4714", "box_open", true, "El candado no cede.")
		"traffic":
			result = _act_color_flag(data, "azul", "traffic_blue", "Ese color no es el de la fuga.")
		"cable":
			result = _act_cable(data)
		"fire":
			result = _act_fire(data)
		"osint":
			result = _act_text(data, "pet", "Pixel", "pixel_known", true, "Esa mascota no es la que está escrita en la ficha.")
		"base64":
			result = _act_base64(data)
		"rssi":
			result = _act_text(data, "cell", "C-4", "tablet_found", true, "Esa celda no cruza las distancias del plano.")
		"hash":
			result = _act_hash(data)
		"kill":
			result = _act_kill(data)
		_:
			result = "Ese paso no está en la operación."
	if result.is_empty():
		_mark_concept(id)
		_roll_segment()
	return result

func objective() -> String:
	if ending == "victory":
		return VICTORY_LINE
	if ending == "timeout":
		return TIMEOUT_LINE
	if ending == "safety":
		return SAFETY_LINE
	var id := _current_id()
	if id == "":
		return "Recorre la oficina y reúne lo que el lugar ya muestra."
	return str(puzzle(id).get("objective", id))

func request_hint() -> String:
	if ending != "":
		return objective()
	var id := _current_id()
	if id == "":
		return objective()
	var lines: Array = puzzle(id).get("hints", [])
	if lines.is_empty():
		return ""
	var level := int(hints.get(id, 0))
	if level < 3 and level < lines.size():
		level += 1
		hints[id] = level
	var index := mini(level, lines.size()) - 1
	if index < 0:
		index = 0
	var text := str(lines[index])
	pending_radio = ""
	return text

func tick(delta: float) -> String:
	if paused or (mode == "aprendizaje" and manual_open):
		return ""
	if ending != "":
		return ""
	if delta > 0.0:
		elapsed += delta
	_roll_segment()
	_auto_hint()
	if mode == "operacion" and elapsed >= 3600.0:
		ending = "timeout"
		return TIMEOUT_LINE
	return ""

func set_mode(next: String) -> void:
	if next == "operacion" or next == "aprendizaje":
		mode = next

func safety_stop() -> String:
	if ending == "":
		ending = "safety"
	paused = true
	return SAFETY_LINE if ending == "safety" else objective()

func concept_applied(id: String) -> bool:
	return id in applied

func save(path: String = SAVE_PATH) -> bool:
	var error := _store.write(_snapshot(), path)
	if error != "":
		last_error = error
		return false
	last_error = ""
	return true

func load_save(path: String = SAVE_PATH) -> bool:
	var parsed = _store.read(path)
	if parsed == null:
		if not _store.last_error.is_empty():
			last_error = _store.last_error
		else:
			last_error = ""
		return false
	if not parsed is Dictionary or not _whole(parsed.get("version")):
		last_error = "Guardado incompatible o dañado; se conserva y puedes iniciar una partida nueva."
		return false
	var number := int(parsed.get("version"))
	if number == 1 or number == 2:
		reset()
		migration_notice = MIGRATION_LINE
		return true
	if number != 3 or not _valid_v3(parsed):
		last_error = "Guardado incompatible o dañado; se conserva y puedes iniciar una partida nueva."
		return false
	_apply(parsed)
	last_error = ""
	migration_notice = ""
	return true

func _snapshot() -> Dictionary:
	return {
		"version": 3,
		"mode": mode,
		"elapsed": elapsed,
		"paused": paused,
		"manual_open": manual_open,
		"flags": flags.duplicate(true),
		"hints": hints.duplicate(true),
		"manuals_read": manuals_read.duplicate(),
		"applied": applied.duplicate(),
		"ending": ending,
		"kill_attempts": kill_attempts,
		"lockout_until": lockout_until,
		"note": note,
		"pending_radio": pending_radio,
		"segment": segment,
		"objective_since": objective_since,
	}

func _apply(parsed: Dictionary) -> void:
	version = 3
	mode = str(parsed.mode)
	elapsed = float(parsed.elapsed)
	paused = bool(parsed.paused)
	manual_open = bool(parsed.manual_open)
	flags = (parsed.flags as Dictionary).duplicate(true)
	hints = {}
	for id in ORDER:
		hints[id] = int(parsed.hints.get(id, 0)) if parsed.hints is Dictionary else 0
	manuals_read = []
	for item in parsed.manuals_read:
		manuals_read.append(str(item))
	applied = []
	for item in parsed.applied:
		applied.append(str(item))
	ending = str(parsed.ending)
	kill_attempts = int(parsed.kill_attempts)
	lockout_until = float(parsed.lockout_until)
	note = str(parsed.note)
	pending_radio = str(parsed.pending_radio)
	objective_since = float(parsed.get("objective_since", elapsed))
	if objective_since < 0.0 or objective_since > elapsed:
		objective_since = elapsed
	segment = str(parsed.get("segment", ""))
	if segment not in ORDER:
		segment = _current_id()
		objective_since = elapsed

func _valid_v3(parsed: Dictionary) -> bool:
	for key in ["mode", "elapsed", "paused", "manual_open", "flags", "hints", "manuals_read", "applied", "ending", "kill_attempts", "lockout_until", "note", "pending_radio"]:
		if not parsed.has(key):
			return false
	if str(parsed.mode) not in ["operacion", "aprendizaje"]:
		return false
	if str(parsed.ending) not in ["", "victory", "timeout", "safety"]:
		return false
	if not parsed.paused is bool or not parsed.manual_open is bool:
		return false
	if not parsed.note is String or not parsed.pending_radio is String:
		return false
	if not _nonnegative(parsed.elapsed) or not _nonnegative(parsed.lockout_until):
		return false
	if not _whole(parsed.kill_attempts) or int(parsed.kill_attempts) < 0:
		return false
	if not parsed.flags is Dictionary or not parsed.hints is Dictionary:
		return false
	if not _string_list(parsed.manuals_read) or not _string_list(parsed.applied):
		return false
	for key in parsed.flags.keys():
		if not key is String or not _flag_value(parsed.flags[key]):
			return false
	for key in parsed.hints.keys():
		if str(key) not in ORDER or not _whole(parsed.hints[key]):
			return false
		var level := int(parsed.hints[key])
		if level < 0 or level > 3:
			return false
	if parsed.has("objective_since") and not _nonnegative(parsed.objective_since):
		return false
	if parsed.has("segment") and not parsed.segment is String:
		return false
	return true

func _flag_value(value: Variant) -> bool:
	return value is bool or value is String or _is_number(value)

func _string_list(value: Variant) -> bool:
	if not value is Array:
		return false
	for item in value:
		if not item is String:
			return false
	return true

func _current_id() -> String:
	if ending != "":
		return ""
	for id in ORDER:
		if not _solved(id) and _available(id):
			return id
	return ""

func _roll_segment() -> void:
	var id := _current_id()
	if id == segment:
		return
	segment = id
	objective_since = elapsed

func _auto_hint() -> void:
	if segment == "" or _solved(segment):
		return
	if int(hints.get(segment, 0)) != 0:
		return
	var budget := float(puzzle(segment).get("seconds", 0))
	if budget <= 0.0:
		return
	if elapsed - objective_since < budget * 0.5:
		return
	hints[segment] = 1
	var lines: Array = puzzle(segment).get("hints", [])
	if not lines.is_empty():
		pending_radio = str(lines[0])

func _mark_concept(id: String) -> void:
	var concept := str(puzzle(id).get("concept", ""))
	if concept != "" and concept not in applied:
		applied.append(concept)

func _blocked(id: String) -> String:
	if _available(id):
		return ""
	match id:
		"box":
			return "Aún faltan dígitos de las pruebas anteriores."
		"traffic", "osint":
			return "La caja de evidencias sigue cerrada."
		"cable":
			return "Primero identifica el color de la fuga."
		"fire":
			return "El enlace externo sigue abierto. El manual de emergencia espera."
		"base64":
			return "Sin la mascota de la ficha no puedes completar la nota."
		"rssi":
			return "Todavía no sabes cómo termina la MAC del intruso."
		"hash":
			return "La tablet con las huellas sigue escondida."
		"kill":
			return "Antes hay que apartar la copia cuya huella no coincide."
		_:
			return "Ese paso todavía no está disponible."

func _available(id: String) -> bool:
	match id:
		"phish", "password", "mfa", "logs":
			return true
		"box":
			return _digits_ready()
		"traffic", "osint":
			return _flag_true("box_open")
		"cable":
			return _flag_true("traffic_blue")
		"fire":
			return _flag_true("cable_blue")
		"base64":
			return _flag_true("pixel_known")
		"rssi":
			return str(flags.get("mac_suffix", "")) == "4F"
		"hash":
			return _flag_true("tablet_found")
		"kill":
			return _flag_true("hash_discarded")
		_:
			return false

func _solved(id: String) -> bool:
	match id:
		"phish":
			return str(flags.get("phish_digit", "")) == "4"
		"password":
			return str(flags.get("password_digit", "")) == "7"
		"mfa":
			return _flag_true("mfa_open")
		"logs":
			return str(flags.get("log_digits", "")) == "14"
		"box":
			return _flag_true("box_open")
		"traffic":
			return _flag_true("traffic_blue")
		"cable":
			return _flag_true("cable_blue")
		"fire":
			return _is_number(flags.get("lab_open_until"))
		"osint":
			return _flag_true("pixel_known")
		"base64":
			return str(flags.get("kill_code", "")) == "7391" and str(flags.get("mac_suffix", "")) == "4F"
		"rssi":
			return _flag_true("tablet_found")
		"hash":
			return _flag_true("hash_discarded")
		"kill":
			return ending == "victory"
		_:
			return false

func _digits_ready() -> bool:
	return str(flags.get("phish_digit", "")) == "4" and str(flags.get("password_digit", "")) == "7" and str(flags.get("log_digits", "")) == "14"

func _flag_true(key: String) -> bool:
	return flags.get(key, false) == true

func _act_phish(data: Dictionary) -> String:
	if not _has_signals(data.get("signals")):
		return "Faltan señales. Revisa el dominio, si piden la clave y si apuran."
	flags.phish_digit = "4"
	return ""

func _has_signals(value: Variant) -> bool:
	if not value is Array or value.size() != 3:
		return false
	for token in ["dominio", "clave", "urgencia"]:
		if token not in value:
			return false
	return true

func _act_code(data: Dictionary, expected: String, flag: String, stored: Variant, failure: String) -> String:
	if _scalar(data.get("code", "")) != expected:
		return failure
	flags[flag] = stored
	return ""

func _act_mfa(data: Dictionary) -> String:
	if _scalar(data.get("token", "")) != "492017":
		return "Ese token no corresponde al minuto del último inicio."
	flags.mfa_open = true
	return ""

func _act_logs(data: Dictionary) -> String:
	if _scalar(data.get("minute", "")) != "14":
		return "Ese minuto no es el de la línea anómala."
	flags.log_digits = "14"
	flags.insider = "aseo_turno_noche"
	return ""

func _act_color_flag(data: Dictionary, expected: String, flag: String, failure: String) -> String:
	if _scalar(data.get("color", "")).to_lower() != expected:
		return failure
	flags[flag] = true
	return ""

func _act_cable(data: Dictionary) -> String:
	var color := _scalar(data.get("color", "")).to_lower()
	if color == "verde":
		if _flag_true("life_safety_cut"):
			return "Ese sistema de vida ya está cortado. No se restan más minutos."
		flags.life_safety_cut = true
		elapsed += 300.0
		return "Evacuación: cortaste un sistema de vida. Pierdes cinco minutos. La operación sigue."
	if color == "rojo":
		elapsed += 120.0
		return "Ese cable quita energía de más. Pierdes dos minutos."
	if color == "amarillo":
		return "El cable de voz no abre el paso ni detiene la copia."
	if color == "azul":
		flags.cable_blue = true
		return ""
	return "Ese cable no está en el panel."

func _act_fire(data: Dictionary) -> String:
	if data.get("synced") != true:
		return "Los dos detectores tienen que coincidir."
	flags.lab_open_until = elapsed + 90.0
	return ""

func _act_text(data: Dictionary, field: String, expected: String, flag: String, stored: Variant, failure: String) -> String:
	if str(data.get(field, "")) != expected:
		return failure
	flags[flag] = stored
	return ""

func _act_base64(data: Dictionary) -> String:
	if str(data.get("key", "")) != "Pixel" or data.get("decoded") != true:
		return "La nota no queda leída. Hace falta reconocer el texto y la palabra justa."
	flags.kill_code = "7391"
	flags.mac_suffix = "4F"
	return ""

func _act_hash(data: Dictionary) -> String:
	if _scalar(data.get("discard", "")).to_upper() != "C":
		return "Esa copia no es la que discrepa de la huella oficial."
	flags.hash_discarded = true
	return ""

func _act_kill(data: Dictionary) -> String:
	if elapsed < lockout_until:
		return "El interruptor sigue bloqueado."
	var code := _scalar(data.get("code", ""))
	if code == "7391":
		ending = "victory"
		return ""
	if code == "1793":
		elapsed += 60.0
		return "Ese orden es una trampa. El reloj pierde un minuto."
	kill_attempts += 1
	if kill_attempts == 3:
		lockout_until = elapsed + 90.0
		return "Tercer fallo. El interruptor queda bloqueado 90 segundos."
	return "Ese código no detiene la copia."

func _scalar(value: Variant) -> String:
	if value is bool:
		return "true" if value else "false"
	if value is int:
		return str(value)
	if value is float and is_finite(value) and is_equal_approx(value, round(value)):
		return str(int(round(value)))
	return str(value).strip_edges()

func _is_number(value: Variant) -> bool:
	return (value is int or value is float) and is_finite(float(value))

func _nonnegative(value: Variant) -> bool:
	return _is_number(value) and float(value) >= 0.0

func _whole(value: Variant) -> bool:
	return _is_number(value) and is_equal_approx(float(value), float(int(round(float(value)))))
