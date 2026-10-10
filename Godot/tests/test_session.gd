extends SceneTree

const Session = preload("res://Game/Features/Quests/session.gd")
var failures := 0
var checks := 0

func expect(condition: bool, message: String) -> void:
	checks += 1
	if not condition:
		failures += 1
		push_error(message)

func _initialize() -> void:
	var session := Session.new()
	expect(session.last_error.is_empty(), "Catálogo legible")
	expect(session.version == 3, "Versión 3")
	expect(session.puzzles().size() == 13, "Trece pasos de act")
	expect(session.catalog.concepts.size() == 11, "Once conceptos")
	expect(session.puzzle("phish").solution.signals == ["dominio", "clave", "urgencia"], "Solución del correo")
	expect(session.puzzle("password").solution.code == "Condores2026", "Solución de la clave")
	expect(session.puzzle("box").solution.code == "4714", "Solución del candado")
	expect(session.puzzle("kill").solution.code == "7391", "Solución del cierre")
	expect(str(session.puzzle("phish").manual).begins_with(str(session.catalog.concepts[0].definition)), "El manual abre con la definición")
	expect(session.puzzle("phish").hints.size() == 3, "Tres pistas")
	expect(not session.objective().is_empty(), "Hay un objetivo inicial")
	expect(session.act("quiz", {}) != "", "Paso desconocido")
	expect(session.act("box", {"code": "4714"}) != "" and not session.flags.has("box_open"), "Candado antes de los dígitos")
	expect(session.act("traffic", {"color": "azul"}) != "", "Tráfico antes de la caja")
	expect(session.act("cable", {"color": "azul"}) != "", "Cable antes del tráfico")
	expect(session.act("fire", {"synced": true}) != "", "Fail-safe antes del cable")
	expect(session.act("osint", {"pet": "Pixel"}) != "", "Ficha antes de la caja")
	expect(session.act("base64", {"decoded": true, "key": "Pixel"}) != "", "Nota antes de la mascota")
	expect(session.act("rssi", {"cell": "C-4"}) != "", "Plano antes de la MAC")
	expect(session.act("hash", {"discard": "C"}) != "", "Huella antes de la tablet")
	expect(session.act("kill", {"code": "7391"}) != "" and session.ending == "", "Cierre antes de la huella")
	expect(session.act("phish", {"signals": ["dominio", "clave"]}) != "", "Faltan señales")
	expect(session.act("phish", {"signals": ["urgencia", "dominio", "clave"]}) == "", "Phishing en cualquier orden")
	expect(str(session.flags.phish_digit) == "4" and session.concept_applied("phishing"), "Dígito 4 y concepto")
	expect(session.act("phish", {"signals": []}) == "", "Acierto idempotente")
	expect(session.act("password", {"code": "1234"}) != "", "Clave incorrecta")
	expect(session.act("password", {"code": "Condores2026"}) == "", "Clave Condores2026")
	expect(session.act("logs", {"minute": 14}) == "", "Minuto 14")
	expect(str(session.flags.log_digits) == "14" and session.flags.insider == "aseo_turno_noche", "Registro e insider")
	expect(not session.flags.get("mfa_open", false), "La caja no exige el segundo factor")
	expect(session.act("box", {"code": "0000"}) != "" and not session.flags.get("box_open", false), "Candado incorrecto")
	expect(session.act("box", {"code": "4714"}) == "" and session.flags.box_open == true, "Candado 4714")
	expect(session.act("base64", {"decoded": true, "key": "Pixel"}) != "", "Base64 sigue esperando a Pixel")
	expect(session.act("osint", {"pet": "pixel"}) != "", "La mascota distingue mayúsculas")
	expect(session.act("osint", {"pet": "Pixel"}) == "" and session.flags.pixel_known == true, "Pixel con la caja abierta y sin tráfico")
	expect(session.act("rssi", {"cell": "C-4"}) != "", "Plano antes de revelar la MAC")
	expect(session.act("base64", {"decoded": false, "key": "Pixel"}) != "", "Hace falta reconocer el texto")
	expect(session.act("base64", {"decoded": true, "key": "Pixel"}) == "", "Nota con Pixel")
	expect(session.flags.kill_code == "7391" and session.flags.mac_suffix == "4F", "Código y MAC revelados")
	expect(session.act("hash", {"discard": "C"}) != "", "Huella antes de la tablet")
	expect(session.act("rssi", {"cell": "A1"}) != "", "Celda incorrecta")
	expect(session.act("rssi", {"cell": "C-4"}) == "" and session.flags.tablet_found == true, "Celda C-4")
	expect(session.act("kill", {"code": "7391"}) != "", "Cierre antes de descartar")
	expect(session.act("hash", {"discard": "A"}) != "", "No se descarta una copia buena")
	expect(session.act("hash", {"discard": "C"}) == "" and session.flags.hash_discarded == true, "Se aparta C")
	expect(session.act("mfa", {"token": "000000"}) != "", "Token incorrecto")
	expect(session.act("mfa", {"token": "492017"}) == "" and session.flags.mfa_open == true, "Token 492017")
	expect(session.act("fire", {"synced": true}) != "", "Fail-safe antes del cable azul")
	expect(session.act("traffic", {"color": "rojo"}) != "", "Color de fuga incorrecto")
	expect(session.act("traffic", {"color": "azul"}) == "" and session.flags.traffic_blue == true, "Fuga azul")
	expect(session.act("cable", {"color": "azul"}) == "" and session.flags.cable_blue == true, "Cable azul")
	session.elapsed = 10
	expect(session.act("fire", {"synced": false}) != "", "Detectores sin sincronizar")
	expect(session.act("fire", {"synced": true}) == "", "Detectores sincronizados")
	expect(is_equal_approx(float(session.flags.lab_open_until), 100.0), "El laboratorio queda abierto 90 s")
	expect(session.act("kill", {"code": "7391"}) == "" and session.ending == "victory", "7391 cierra la operación")
	for concept in ["phishing", "passwords", "mfa", "logs", "traffic", "containment", "failsafe", "osint", "obfuscation", "inventory", "integrity"]:
		expect(session.concept_applied(concept), "Concepto aplicado: " + concept)
	expect(session.applied.size() == 11, "Once conceptos, sin duplicar")
	expect(session.objective() == "Copia detenida. La lista sigue en el edificio.", "Texto de victoria")
	_cables()
	_kill_trap()
	_lockout()
	_clock()
	_manual_and_hint()
	_save_roundtrip()
	_legacy()
	_damaged()
	print("SESSION: %d checks / %d failures" % [checks, failures])
	quit(0 if failures == 0 else 1)

func through(stop: String) -> EscapeSession:
	var session := Session.new()
	var steps: Array = [
		["phish", {"signals": ["dominio", "clave", "urgencia"]}],
		["password", {"code": "Condores2026"}],
		["mfa", {"token": "492017"}],
		["logs", {"minute": "14"}],
		["box", {"code": "4714"}],
		["traffic", {"color": "azul"}],
		["cable", {"color": "azul"}],
		["fire", {"synced": true}],
		["osint", {"pet": "Pixel"}],
		["base64", {"key": "Pixel", "decoded": true}],
		["rssi", {"cell": "C-4"}],
		["hash", {"discard": "C"}],
	]
	for step in steps:
		if str(step[0]) == stop:
			break
		expect(session.act(str(step[0]), step[1]) == "", "Preparar " + str(step[0]))
	return session

func _cables() -> void:
	var yellow := through("cable")
	var before := yellow.elapsed
	var message := yellow.act("cable", {"color": "amarillo"})
	expect(message != "" and is_equal_approx(yellow.elapsed, before) and not yellow.flags.get("cable_blue", false), "El amarillo no avanza")
	var red := through("cable")
	expect(red.act("cable", {"color": "rojo"}) != "", "El rojo avisa")
	expect(is_equal_approx(red.elapsed, 120.0) and not red.flags.get("cable_blue", false) and red.ending == "", "El rojo suma 120 s y no termina")
	var green := through("cable")
	var evacuation := green.act("cable", {"color": "verde"})
	expect(evacuation.contains("Evacuación") and green.flags.get("life_safety_cut") == true, "El verde corta un sistema de vida")
	expect(is_equal_approx(green.elapsed, 300.0) and green.ending == "" and not green.flags.get("cable_blue", false), "El verde suma 300 s y la operación sigue")
	var again := green.elapsed
	green.act("cable", {"color": "verde"})
	expect(is_equal_approx(green.elapsed, again), "El verde no se penaliza dos veces")

func _kill_trap() -> void:
	var session := through("kill")
	var message := session.act("kill", {"code": "1793"})
	expect(message != "" and session.ending == "" and session.kill_attempts == 0, "1793 no cuenta como fallo ni cierra")
	expect(is_equal_approx(session.elapsed, 60.0), "1793 suma 60 s")

func _lockout() -> void:
	var session := through("kill")
	for code in ["0000", "1111", "2222"]:
		expect(session.act("kill", {"code": code}) != "", "Fallo " + code)
	expect(session.kill_attempts == 3, "Tres fallos")
	expect(is_equal_approx(session.lockout_until, session.elapsed + 90.0), "Bloqueo 90 s")
	expect(session.act("kill", {"code": "7391"}) != "" and session.ending == "" and session.kill_attempts == 3, "Durante el bloqueo no entra ni el código bueno")
	session.tick(90)
	expect(session.act("kill", {"code": "7391"}) == "" and session.ending == "victory", "Tras el bloqueo 7391 cierra")

func _clock() -> void:
	var operation := Session.new()
	operation.elapsed = 3599
	operation.paused = true
	expect(operation.tick(10) == "" and operation.ending == "" and is_equal_approx(operation.elapsed, 3599), "En pausa el reloj no corre")
	operation.paused = false
	expect(operation.tick(1) == "Descarga completa. La lista salió del edificio.", "Texto de tiempo agotado")
	expect(operation.ending == "timeout", "Final por tiempo en operación")
	expect(operation.tick(5) == "" and is_equal_approx(operation.elapsed, 3600), "El tiempo agotado no sigue sumando")
	var study := Session.new()
	study.set_mode("aprendizaje")
	study.elapsed = 3590
	expect(study.tick(30) == "" and study.ending == "" and is_equal_approx(study.elapsed, 3620), "Aprendizaje no pierde")
	study.manual_open = true
	expect(study.tick(15) == "" and is_equal_approx(study.elapsed, 3620), "En aprendizaje el manual detiene el reloj")
	var live := Session.new()
	live.manual_open = true
	live.tick(4)
	expect(is_equal_approx(live.elapsed, 4), "En operación el manual no detiene el reloj")
	var safe := Session.new()
	safe.elapsed = 12
	safe.safety_stop()
	expect(safe.ending == "safety" and safe.paused and is_equal_approx(safe.elapsed, 12), "CÓNDOR LIBRE no castiga")
	expect(safe.tick(20) == "" and is_equal_approx(safe.elapsed, 12), "La salida de emergencia deja el reloj quieto")
	safe.set_mode("otro")
	expect(safe.mode == "operacion", "Modo desconocido se ignora")

func _manual_and_hint() -> void:
	var session := Session.new()
	var half := float(session.puzzle("phish").seconds) * 0.5
	session.paused = true
	session.tick(half + 5)
	expect(int(session.hints.phish) == 0 and session.pending_radio == "", "En pausa no sale la pista")
	session.paused = false
	session.set_mode("aprendizaje")
	session.manual_open = true
	session.tick(half + 5)
	expect(int(session.hints.phish) == 0, "Con el manual abierto en aprendizaje no corre la pista")
	session.manual_open = false
	session.set_mode("operacion")
	session.tick(half)
	expect(int(session.hints.phish) == 1, "A la mitad del tiempo previsto sube la pista")
	expect(session.pending_radio == str(session.puzzle("phish").hints[0]), "La radio guarda el primer texto")
	expect(session.request_hint() == str(session.puzzle("phish").hints[1]), "La radio pide el segundo nivel")
	expect(int(session.hints.phish) == 2, "La pista queda en 2")
	var flags := session.flags.duplicate()
	var manual := session.read_manual("phish")
	expect(manual.begins_with(str(session.puzzle("phish").manual).substr(0, 20)), "El manual sale del catálogo")
	expect(session.manual_open and "phish" in session.manuals_read and session.flags == flags and not session.concept_applied("phishing"), "Leer el manual no aplica el concepto ni cambia banderas")
	expect(not session.read_manual("no-existe").is_empty(), "Un id desconocido avisa")
	expect("no-existe" not in session.manuals_read, "Un id desconocido no se anota como leído")

func _save_roundtrip() -> void:
	var path := "user://condor-session-mid.json"
	wipe(path)
	var session := Session.new()
	expect(session.act("phish", {"signals": ["dominio", "clave", "urgencia"]}) == "", "Guardar tras el correo")
	session.elapsed = 125.5
	session.mode = "aprendizaje"
	session.note = "bitácora"
	session.hints.phish = 2
	session.manuals_read = ["phish"]
	session.kill_attempts = 1
	session.lockout_until = 40
	session.pending_radio = "mira el dominio"
	session.manual_open = true
	expect(session.save(path), "Guardar a mitad")
	var loaded := Session.new()
	expect(loaded.load_save(path), "Reanudar")
	expect(str(loaded.flags.get("phish_digit", "")) == "4", "La bandera sobrevive")
	expect(is_equal_approx(loaded.elapsed, 125.5), "El reloj sobrevive")
	expect(loaded.mode == "aprendizaje" and loaded.note == "bitácora", "Modo y nota sobreviven")
	expect(int(loaded.hints.phish) == 2 and loaded.manuals_read.has("phish"), "Pista y manual sobreviven")
	expect(loaded.kill_attempts == 1 and is_equal_approx(loaded.lockout_until, 40), "Intentos del cierre sobreviven")
	expect(loaded.pending_radio == "mira el dominio" and loaded.manual_open and loaded.version == 3, "Versión 3 a mitad")
	expect(loaded.concept_applied("phishing"), "El concepto aplicado se reanuda")
	wipe(path)

func _legacy() -> void:
	var path := "user://condor-session-legacy.json"
	wipe(path)
	dump(path, '{"version":1,"missions":{"recon":{"completed":true}},"escaped":true}')
	var original := FileAccess.get_file_as_string(path)
	var session := Session.new()
	session.act("password", {"code": "Condores2026"})
	expect(session.load_save(path), "Una v1 se reconoce")
	expect(session.migration_notice.contains("no se convierte") and session.migration_notice.contains("operación"), "Aviso de operación nueva")
	expect(session.version == 3 and session.flags.is_empty() and session.ending == "", "La v1 no se convierte en banderas")
	expect(FileAccess.get_file_as_string(path) == original, "La v1 sigue en disco")
	session.reset()
	expect(session.migration_notice == "" and session.version == 3 and session.flags.is_empty(), "reset deja la versión 3")
	dump(path, '{"version":2,"missions":{"web":{"completed":true}},"restored":["recon"],"escaped":false}')
	original = FileAccess.get_file_as_string(path)
	var second := Session.new()
	expect(second.load_save(path), "Una v2 se reconoce")
	expect(not second.migration_notice.is_empty() and second.flags.is_empty() and second.version == 3, "La v2 tampoco se convierte")
	expect(FileAccess.get_file_as_string(path) == original, "La v2 sigue en disco")
	wipe(path)

func _damaged() -> void:
	var path := "user://condor-session-broken.json"
	wipe(path)
	var session := Session.new()
	expect(session.act("mfa", {"token": "492017"}) == "", "Estado previo al archivo dañado")
	for payload in ["{partida incompleta", "null", "", "[]", '{"version":3}']:
		dump(path, payload)
		expect(not session.load_save(path), "Rechaza " + payload.left(24))
		expect(not session.last_error.is_empty() and session.migration_notice == "", "El daño no es una migración")
		expect(session.flags.get("mfa_open") == true, "El daño no pisa el estado")
		expect(FileAccess.get_file_as_string(path) == payload, "El daño no borra el archivo")
	var fresh := Session.new()
	var stamp := _stamp(fresh)
	dump(path, "null")
	expect(not fresh.load_save(path) and _stamp(fresh) == stamp, "Una sesión nueva no adopta el daño")
	wipe(path)
	var absent := Session.new()
	expect(not absent.load_save(path) and absent.last_error.is_empty(), "Partida ausente")

func _stamp(session: EscapeSession) -> String:
	return JSON.stringify({
		"version": session.version,
		"elapsed": session.elapsed,
		"ending": session.ending,
		"flags": session.flags,
		"mode": session.mode,
		"note": session.note,
	})

func dump(path: String, payload: String) -> void:
	var file := FileAccess.open(path, FileAccess.WRITE)
	file.store_string(payload)
	file.close()

func wipe(path: String) -> void:
	for suffix in ["", ".bak", ".tmp"]:
		if FileAccess.file_exists(path + suffix):
			DirAccess.remove_absolute(path + suffix)
