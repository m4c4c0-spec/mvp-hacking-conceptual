class_name Sala1
extends RefCounted

const Builder = preload("res://Game/Levels/office_builder.gd")
const SurfaceFinish = preload("res://Game/Shared/surface_finish.gd")

const NAVY := Color("182b35")
const PAPER := Color("d5d4c1")
const MINT := Color("55d4be")
const WOOD := Color("966746")
const AMBER := Color("d9a34f")
const CORK_COLOR := Color("8a5a38")

const QUIZ := [
	"Tray0", "Tray1", "Tray2",
	"Folder0", "Folder1", "Folder2", "Folder3",
	"Control0", "Control1", "Control2", "Control3",
	"ReportInbox", "MissionBoard",
]
const SIGNALS := ["dominio", "clave", "urgencia"]
const SIGNAL_NAMES := {"dominio": "Dominio", "clave": "Clave", "urgencia": "Urgencia"}
const MANUALS := {
	"phish": "El phishing es un mensaje que se hace pasar por un remitente de confianza para que entregues una clave. Compara el dominio letra a letra, fíjate si pide la clave y si apura.",
	"password": "Una contraseña débil es una clave predecible hecha con datos a la vista. En el escritorio se arma con dos fichas, sin teclado: el equipo y el año.",
	"mfa": "Un segundo factor es una prueba distinta de la clave. En este acceso, la serie va impresa en el aparato y los dos últimos dígitos son el minuto del último ingreso.",
	"logs": "Un registro de accesos lista quién entró, a qué hora y con qué privilegio. La hora anómala es la de la línea que no corresponde a esa cuenta.",
}
const LOG_LINES := [
	{"minute": "03", "text": "20:03   tarjeta de V. Soto   DENEGADA   vestíbulo"},
	{"minute": "06", "text": "22:06   guardia   cierra recepción   OK"},
	{"minute": "11", "text": "22:11   clima   consigna nocturna   OK"},
	{"minute": "48", "text": "22:48   aseo_turno_noche   ingreso vestíbulo   OK"},
	{"minute": "16", "text": "23:16   impresora   cola vacía   OK"},
	{"minute": "40", "text": "00:40   aseo_turno_noche   pasillo norte   OK"},
	{"minute": "05", "text": "01:05   energía   modo noche   OK"},
	{"minute": "22", "text": "01:22   aseo_turno_noche   oficina UCD   OK"},
	{"minute": "09", "text": "02:09   copia local programada   OK"},
	{"minute": "41", "text": "02:41   aseo_turno_noche   baños de planta   OK"},
	{"minute": "14", "text": "03:14   aseo_turno_noche   ROOT   RACK-03   sesión abierta"},
	{"minute": "37", "text": "03:37   aseo_turno_noche   salida vestíbulo   OK"},
]
const LETTERS := {
	"EmailPhish": "Remitente: soporte-ti@ucd-ch1le.invalid\nAsunto: URGENTE · tu clave vence en minutos\n\nSoporte necesita la clave ahora mismo para no cerrar tu acceso. Respóndela en este correo antes de que expire el plazo.",
	"EmailMaintenance": "Remitente: soporte-ti@ucd-chile.invalid\nAsunto: Mantenimiento del correo, jueves 09:00\n\nAviso interno. No pide la clave ni apura. El dominio se escribe chile, con ele, no con un uno. Por eso este correo no es el falso. No trae ningún dígito.",
	"EmailRoster": "Remitente: soporte-ti@ucd-chile.invalid\nAsunto: Turnos de la semana\n\nPublica horarios de la oficina. No solicita la clave ni marca un plazo. No trae ningún dígito.",
	"EmailMeeting": "Remitente: soporte-ti@ucd-chile.invalid\nAsunto: Acta breve del jueves\n\nResume una reunión ya hecha. No pide credenciales ni apura. No trae ningún dígito.",
}
const BOX_TEXT := "CREDENCIAL DE ACCESO\nNombre: V. Soto\nFunción: aseo nocturno\nAlta: hace meses\nEn la ficha figura un gato, Pixel.\nEsta credencial no abre la puerta magnética.\n\nTARJETA DE TRÁFICO\nColor: azul\nDestino anotado: 203.0.113.44\nSirve para contrastar el tráfico de servidores. Aquí solo se lee."


static func build(office: Node3D) -> void:
	_hide_quiz(office)
	_refresh_guides(office)
	_retitle_legacy(office)
	_ensure_magnetic_sign(office)
	_seal_door_prompt(office)
	_seat_keyboards(office)
	_frame_exit(office)
	if office.get_node_or_null("Cork") == null:
		_build_briefing(office)
		_build_cork(office)
		_build_desk(office)
		_build_token(office)
		_build_logs(office)
		_build_box(office)
		_build_radio(office)
		_build_safety(office)
		_manual(office, "ManualPhish", "phish", Vector3(-1.55, 1.15, -5.78), Vector3(0.18, 0.24, 0.06), "correo", Vector3(0, 0, 0.04), 0.0)
		_manual(office, "ManualPassword", "password", Vector3(1.22, 0.88, 0.08), Vector3(0.2, 0.04, 0.14), "clave", Vector3(0, 0.03, 0), 0.0)
		_manual(office, "ManualMfa", "mfa", Vector3(3.35, 1.15, 5.84), Vector3(0.22, 0.28, 0.06), "token", Vector3(0, 0, -0.04), PI)
		_manual(office, "ManualLogs", "logs", Vector3(9.84, 1.15, -1.35), Vector3(0.08, 0.22, 0.16), "registros", Vector3(-0.05, 0, 0), -PI / 2.0)
	_tidy_desk(office)


static func handle(main: Node, target: OfficeInteractable) -> bool:
	var office := main.get("office") as Node3D
	if office:
		_hide_quiz(office)
		_seal_door_prompt(office)
	_sync_reveals(main)
	match target.action:
		"radio":
			_radio(main)
		"safety":
			_safety(main)
		"phish":
			_phish(main, target)
		"password":
			_password(main, target)
		"mfa":
			_mfa(main, target)
		"logs":
			_logs(main, target)
		"box":
			_box(main, target)
		"exit":
			if target.name != "ExitDoor":
				return false
			_exit(main, target)
		"manual":
			if str(target.get_meta("puzzle", "")) not in MANUALS:
				return false
			_manual_card(main, target)
		"archive_door":
			_archive(main)
		_:
			return false
	return true


static func _hide_quiz(office: Node3D) -> void:
	for node_name in QUIZ:
		var node := office.get_node_or_null(node_name)
		if node == null:
			continue
		node.visible = false
		if node is CollisionObject3D:
			node.collision_layer = 0
			node.collision_mask = 0


static func _refresh_guides(office: Node3D) -> void:
	for i in OfficeGuide.ROLES.size():
		var guide := office.get_node_or_null("Guide%d" % i) as Node3D
		if guide == null:
			continue
		var role: Dictionary = OfficeGuide.ROLES[i]
		var assist := guide.get_node_or_null("Assistance") as OfficeInteractable
		if assist:
			assist.prompt = "%s · %s · E consultar" % [role.name, role.action]
		for child in guide.get_children():
			if child is Label3D and str((child as Label3D).text).begins_with("ASISTENCIA GRABADA"):
				(child as Label3D).text = "ASISTENCIA GRABADA\n" + str(role.action)


static func _retitle_legacy(office: Node3D) -> void:
	for node in office.find_children("*", "Label3D", true, false):
		var sign := node as Label3D
		var text := sign.text
		if text.contains("PROTOCOLO SALIDA") or text.contains("Acepta una fase"):
			sign.text = "OPERACIÓN CÓNDOR SILENCIOSO\nEl enemigo ya tiene la llave."
			sign.font_size = 32
		elif text.contains("APLICA LO APRENDIDO"):
			sign.text = "OFICINA UCD\nCorreo · clave · token · registros"
			sign.font_size = 28
		elif text.contains("CUATRO SEGUROS") or text == "SALIDA":
			sign.text = "SALA DE SERVIDORES · PUERTA MAGNÉTICA"
			sign.font_size = 28
		elif text == "LOCKDOWN ACTIVO":
			sign.text = "CORREDOR\nLA EMERGENCIA SIGUE ABIERTA"
			sign.font_size = 28


static func _seat_keyboards(office: Node3D) -> void:
	var guide := office.get_node_or_null("Guide3") as Node3D
	if guide and not bool(guide.get_meta("keys_seated", false)):
		guide.set_meta("keys_seated", true)
		for child in guide.get_children():
			if child.name == "DemoKeyboard":
				child.position = Vector3(0, 0.925, 0.52)
				_paint_keys(child, "DemoKeyboard")
			elif str(child.name).begins_with("DemoKey"):
				child.position = Vector3(child.position.x, 0.968, 0.52)
				_paint_keys(child, "DemoKey")
			elif child.name == "DemoDisplay":
				child.position = Vector3(0, 1.15, 0.82)
			elif child.name == "DisplayFace":
				child.position = Vector3(0, 1.15, 0.784)
			elif child is Label3D and str((child as Label3D).text).contains("DIAGNÓSTICO"):
				child.position = Vector3(0, 1.15, 0.771)
	var laptop := office.get_node_or_null("Laptop") as Node3D
	if laptop and not bool(laptop.get_meta("keys_seated", false)):
		laptop.set_meta("keys_seated", true)
		var plate := laptop.get_node_or_null("Keyboard") as Node3D
		if plate:
			plate.position = Vector3(0, 0.097, -0.12)
			_paint_keys(plate, "Keyboard")
		for child in laptop.get_children():
			if not child is MeshInstance3D or absf(child.position.y - 0.11) > 0.02:
				continue
			var placed := child as Node3D
			var row: float = (placed.position.z + 0.16) / 0.066
			placed.position = Vector3(placed.position.x, 0.11, -0.2 + row * 0.05)
			_paint_keys(child, "Key")


static func _paint_keys(node: Node, surface_name: String) -> void:
	var mesh := node as MeshInstance3D
	if mesh == null or not mesh.material_override is StandardMaterial3D:
		return
	var color: Color = (mesh.material_override as StandardMaterial3D).albedo_color
	mesh.material_override = SurfaceFinish.make(color, surface_name)


static func _frame_exit(office: Node3D) -> void:
	var hinge := office.get_node_or_null("ExitHinge") as Node3D
	if hinge and absf(hinge.position.z - 5.95) < 0.04:
		hinge.position.z = 6.0
	if office.get_node_or_null("DoorJambLeft"):
		return
	var wood := Color("966746")
	# El marco se queda en el vano. No es hijo de la hoja, así que no gira con ella.
	Builder.box(office, "DoorJambLeft", Vector3(-4.04, 1.35, 5.86), Vector3(0.1, 2.7, 0.08), wood, false)
	Builder.box(office, "DoorJambRight", Vector3(-1.96, 1.35, 5.86), Vector3(0.1, 2.7, 0.08), wood, false)
	Builder.box(office, "DoorHead", Vector3(-3, 2.72, 5.86), Vector3(2.18, 0.12, 0.08), wood, false)


static func _ensure_magnetic_sign(office: Node3D) -> void:
	for node in office.find_children("*", "Label3D", true, false):
		if (node as Label3D).text.contains("PUERTA MAGNÉTICA"):
			return
	var sign := Builder.label(office, "SALA DE SERVIDORES · PUERTA MAGNÉTICA", Vector3(-3, 2.72, 5.72), 28, PAPER, PI)
	sign.name = "MagneticSign"


static func _seal_door_prompt(office: Node3D) -> void:
	var door := office.get_node_or_null("ExitHinge/ExitDoor") as OfficeInteractable
	if door:
		door.prompt = "PUERTA MAGNÉTICA · E comprobar"


static func _build_briefing(office: Node3D) -> void:
	Builder.box(office, "Briefing", Vector3(-0.15, 1.4, -5.82), Vector3(2.1, 0.72, 0.06), NAVY, false)
	Builder.label(office, "OPERACIÓN CÓNDOR SILENCIOSO", Vector3(-0.15, 1.62, -5.76), 26, PAPER)
	Builder.label(office, "El enemigo ya tiene la llave.", Vector3(-0.15, 1.44, -5.76), 20, MINT)
	Builder.label(office, "UCD · Santiago · 60 minutos", Vector3(-0.15, 1.26, -5.76), 18, PAPER)
	Builder.label(office, "Correo, clave, token, registros, caja.", Vector3(-0.15, 1.1, -5.76), 16, PAPER)
	var clock := Builder.label(office, "60:00", Vector3(2.35, 1.35, -5.55), 64, NAVY)
	clock.name = "ClockLabel"


static func _build_cork(office: Node3D) -> void:
	var cork := Builder.box(office, "Cork", Vector3(-3.05, 1.18, -5.84), Vector3(1.16, 0.48, 0.08), CORK_COLOR)
	var spots := {
		"EmailPhish": Vector3(0.28, 0.11, 0.055),
		"EmailMaintenance": Vector3(-0.28, 0.11, 0.055),
		"EmailRoster": Vector3(-0.28, -0.11, 0.055),
		"EmailMeeting": Vector3(0.28, -0.11, 0.055),
	}
	for email_name in spots:
		var sender := _sender_of(email_name)
		var email := _prop(cork, email_name, "phish", "CORREO · %s · E" % sender, spots[email_name], Vector3(0.4, 0.2, 0.02), PAPER, true)
		email.set_meta("letter", LETTERS[email_name])
		_ink(email, sender, Vector3(0, 0.02, 0.02), 0.0, 0.36)
		if email_name == "EmailPhish":
			var reverse := Builder.label(email, "", Vector3(0, 0, -0.02), 48, MINT, PI)
			reverse.name = "Reverse"
			reverse.visible = false


static func _sender_of(email_name: String) -> String:
	var letter := str(LETTERS[email_name])
	var line := letter.split("\n")[0]
	return line.trim_prefix("Remitente: ")


static func _tidy_desk(office: Node3D) -> void:
	# Superficie: y 0,86, x −1,8..1,8, z −1,0..0,4. Cada pieza queda en su hueco.
	_place(office, "Pennant", Vector3(-1.58, 0.96, -0.55))
	_place(office, "Mouse", Vector3(-1.15, 0.89, 0.02))
	_place(office, "PostIt", Vector3(-0.22, 0.868, 0.08))
	_place(office, "Notebook", Vector3(0.05, 0.90, -0.72))
	_place(office, "Mug", Vector3(0.95, 0.945, -0.78))
	_place(office, "Coffee", Vector3(0.95, 1.032, -0.78))
	_place(office, "MugHandle", Vector3(1.05, 0.945, -0.78))
	_place(office, "Calendar", Vector3(1.48, 0.99, -0.72))
	_place(office, "Login", Vector3(0.62, 0.91, 0.02))
	_place(office, "ManualPassword", Vector3(1.22, 0.88, 0.08))
	for child in office.get_children():
		if not child is Label3D:
			continue
		var sign := child as Label3D
		if sign.text not in ["1", "2", "3"]:
			continue
		if sign.position.y < 1.3 and sign.position.z < 1.0:
			sign.visible = false


static func _place(office: Node3D, node_name: String, position: Vector3) -> void:
	var node := office.get_node_or_null(node_name) as Node3D
	if node == null:
		return
	node.position = position
	if node is OfficeInteractable:
		(node as OfficeInteractable).home_transform = node.transform


static func _build_desk(office: Node3D) -> void:
	var note := _prop(office, "PostIt", "prop", "POST-IT · G tomar · G soltar", Vector3(-0.22, 0.868, 0.08), Vector3(0.16, 0.012, 0.16), AMBER, true)
	var note_label := Builder.label(note, "no olvidar:\nmi equipo + año", Vector3(0, 0.02, 0), 16, NAVY)
	note_label.pixel_size = 0.001
	var pennant := _prop(office, "Pennant", "prop", "BANDERÍN · G tomar · G soltar", Vector3(-1.58, 0.96, -0.55), Vector3(0.28, 0.2, 0.02), Color("bb5150"), true)
	Builder.label(pennant, "Cóndores", Vector3(0, 0, 0.02), 22, PAPER)
	var calendar := _prop(office, "Calendar", "prop", "CALENDARIO · G tomar · G soltar", Vector3(1.48, 0.99, -0.72), Vector3(0.24, 0.26, 0.02), PAPER, true)
	Builder.label(calendar, "2026", Vector3(0, 0, 0.02), 32, NAVY)
	var login := _prop(office, "Login", "password", "ACCESO · fichas del escritorio · E", Vector3(0.62, 0.91, 0.02), Vector3(0.46, 0.1, 0.28), NAVY)
	login.set_meta("team", false)
	login.set_meta("year", false)
	var digit := Builder.label(login, "", Vector3(0, 0.1, 0.16), 42, MINT)
	digit.name = "Digit"
	digit.visible = false


static func _build_token(office: Node3D) -> void:
	var wheel := _prop(office, "TokenWheel", "mfa", "TOKEN · último ingreso 22:17 · E", Vector3(-1.65, 1.18, 5.84), Vector3(0.42, 0.4, 0.08), NAVY)
	wheel.set_meta("minute", 0)
	_ink(wheel, "Último inicio de sesión: 22:17\nSERIE 4920", Vector3(0, 0.02, -0.05), PI, 0.44)


static func _build_logs(office: Node3D) -> void:
	var sheet := _prop(office, "LogSheet", "logs", "REGISTRO · doce líneas · E", Vector3(9.84, 1.16, -2.35), Vector3(0.08, 0.46, 0.36), PAPER)
	_ink(sheet, "REGISTRO\n12 LÍNEAS", Vector3(-0.05, 0, 0), -PI / 2.0)


static func _build_box(office: Node3D) -> void:
	var box := _prop(office, "EvidenceBox", "box", "CAJA · cuatro ruedas · E", Vector3(4.55, 1.08, -5.82), Vector3(0.42, 0.28, 0.1), WOOD)
	box.set_meta("digits", [0, 0, 0, 0])
	Builder.label(box, "CAJA", Vector3(0, 0.04, 0.06), 28, PAPER)


static func _build_radio(office: Node3D) -> void:
	var radio := _prop(office, "Radio", "radio", "RADIO · pedir pista · E", Vector3(-5.15, 1.15, 5.84), Vector3(0.36, 0.24, 0.08), NAVY)
	_ink(radio, "RADIO", Vector3(0, 0, -0.05), PI)


static func _build_safety(office: Node3D) -> void:
	var exit := _prop(office, "SafetyExit", "safety", "SALIDA DE EMERGENCIA · SIEMPRE ABIERTA · E", Vector3(-3, 1.15, 8.3), Vector3(0.5, 0.36, 0.08), MINT)
	_ink(exit, "SALIDA DE EMERGENCIA · SIEMPRE ABIERTA", Vector3(0, 0, -0.05), PI, 0.46)


static func _manual(parent: Node3D, node_name: String, puzzle: String, position: Vector3, size: Vector3, caption: String, label_pos: Vector3, yaw: float) -> void:
	var book := _prop(parent, node_name, "manual", "MANUAL · E", position, size, WOOD)
	book.set_meta("puzzle", puzzle)
	_ink(book, caption, label_pos, yaw)


static func _prop(parent: Node3D, node_name: String, action: String, prompt: String, position: Vector3, size: Vector3, color: Color, grabbable := false) -> OfficeInteractable:
	return Builder.prop(parent, node_name, action, 0, prompt, position, size, color, grabbable)


static func _ink(parent: Node3D, text: String, position: Vector3, yaw: float, width := 0.0) -> void:
	var sign := Builder.label(parent, text, position, 18, NAVY, yaw)
	sign.pixel_size = 0.00115
	sign.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	sign.vertical_alignment = VERTICAL_ALIGNMENT_CENTER
	if width > 0.0:
		sign.width = width / sign.pixel_size
		sign.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART


static func _radio(main: Node) -> void:
	var ui := _ui(main)
	if ui == null:
		return
	ui.show_card("RADIO", _hint_text(main))
	ui.add_button("Cerrar", ui.close)


static func _hint_text(main: Node) -> String:
	var session: Variant = main.get("session")
	if session == null or not session.has_method("request_hint"):
		return "La radio está en su sitio. La sesión todavía no entrega pistas."
	var result: Variant = session.call("request_hint")
	if result is String and not (result as String).is_empty():
		session.set("pending_radio", "")
		return result
	var pending: Variant = session.get("pending_radio")
	if not (pending is String) or (pending as String).is_empty():
		var state: Variant = session.get("state")
		if state is Dictionary:
			pending = (state as Dictionary).get("pending_radio", "")
	if pending is String and not (pending as String).is_empty():
		if session.get("pending_radio") is String:
			session.set("pending_radio", "")
		return pending
	return "La radio no tiene una pista nueva."


static func _safety(main: Node) -> void:
	var session: Variant = main.get("session")
	if session != null and session.has_method("safety_stop"):
		session.call("safety_stop")
	else:
		_toast(main, "La salida está abierta, pero la sesión todavía no registra la parada.")
	if main.has_method("_show_debrief"):
		main.call("_show_debrief")
		var ui := _ui(main)
		if ui and ui.content.get_child_count() > 1:
			var body := ui.content.get_child(1)
			if body is Label and not str((body as Label).text).contains("CÓNDOR LIBRE"):
				(body as Label).text = "CÓNDOR LIBRE\n\n" + (body as Label).text
		_commit(main)
		return
	var ui := _ui(main)
	if ui == null:
		return
	ui.show_card("SALIDA DE EMERGENCIA", "Siempre abierta. Palabra de seguridad: CÓNDOR LIBRE. La operación se detiene aquí, sin castigo.")
	ui.add_button("Cerrar", ui.close)
	_commit(main)


static func _phish(main: Node, target: OfficeInteractable) -> void:
	if target.name != "EmailPhish":
		var ui := _ui(main)
		if ui == null:
			return
		ui.show_card("CORREO", str(target.get_meta("letter", "")))
		ui.add_button("Cerrar", ui.close)
		return
	if _flag_text(main, "phish_digit") == "4":
		_reveal_phish(target)
		_show_simple(main, "REVERSO DEL CORREO", "El reverso muestra el 4. Ese dígito ya quedó registrado.")
		return
	_show_phish_card(main, target)


static func _show_phish_card(main: Node, target: OfficeInteractable) -> void:
	var ui := _ui(main)
	if ui == null:
		return
	ui.show_card("CORREO FALSO", str(target.get_meta("letter", "")))
	ui.add_text("Marca las tres señales y confirma.")
	for key in SIGNALS:
		var on := bool(target.get_meta("mark_" + key, false))
		ui.add_button("%s · %s" % [SIGNAL_NAMES[key], "sí" if on else "no"], _toggle_mark.bind(main, target, key))
	ui.add_button("Confirmar señales", _confirm_phish.bind(main, target))
	ui.add_button("Cerrar", ui.close)


static func _toggle_mark(main: Node, target: OfficeInteractable, key: String) -> void:
	target.set_meta("mark_" + key, not bool(target.get_meta("mark_" + key, false)))
	_show_phish_card(main, target)


static func _confirm_phish(main: Node, target: OfficeInteractable) -> void:
	var marked: Array = []
	for key in SIGNALS:
		if bool(target.get_meta("mark_" + key, false)):
			marked.append(key)
	if _act(main, "phish", {"signals": marked}) != "":
		return
	_reveal_phish(target)
	_show_simple(main, "REVERSO DEL CORREO", "El reverso del correo falso muestra el 4.")


static func _reveal_phish(email: Node) -> void:
	var sign := email.get_node_or_null("Reverse") as Label3D
	if sign == null:
		return
	sign.text = "4"
	sign.visible = true
	if email is OfficeInteractable and (email as OfficeInteractable).is_held:
		return
	if absf(email.rotation.y - PI) < 0.05:
		return
	email.rotation.y = PI
	if email is OfficeInteractable:
		(email as OfficeInteractable).home_transform = email.transform


static func _password(main: Node, target: OfficeInteractable) -> void:
	if _flag_text(main, "password_digit") == "7":
		_reveal_password(target)
		_show_simple(main, "ACCESO", "La clave ya estaba. El dígito es 7.")
		return
	_show_password_card(main, target)


static func _show_password_card(main: Node, target: OfficeInteractable) -> void:
	var ui := _ui(main)
	if ui == null:
		return
	var team := bool(target.get_meta("team", false))
	var year := bool(target.get_meta("year", false))
	ui.show_card("ACCESO DEL ESCRITORIO", "Elige las fichas. No hay teclado.\n\nEquipo: %s\nAño: %s" % ["Condores" if team else "—", "2026" if year else "—"])
	ui.add_button("Ficha Condores · %s" % ("sí" if team else "no"), _toggle_chip.bind(main, target, "team"))
	ui.add_button("Ficha 2026 · %s" % ("sí" if year else "no"), _toggle_chip.bind(main, target, "year"))
	ui.add_button("Confirmar clave", _confirm_password.bind(main, target))
	ui.add_button("Cerrar", ui.close)


static func _toggle_chip(main: Node, target: OfficeInteractable, key: String) -> void:
	target.set_meta(key, not bool(target.get_meta(key, false)))
	_show_password_card(main, target)


static func _confirm_password(main: Node, target: OfficeInteractable) -> void:
	var code := ""
	if bool(target.get_meta("team", false)):
		code = "Condores"
	if bool(target.get_meta("year", false)):
		code += "2026"
	if _act(main, "password", {"code": code}) != "":
		return
	_reveal_password(target)
	_show_simple(main, "ACCESO", "La clave encaja. El dígito es 7.")


static func _reveal_password(login: Node) -> void:
	var sign := login.get_node_or_null("Digit") as Label3D
	if sign == null:
		return
	sign.text = "7"
	sign.visible = true


static func _mfa(main: Node, target: OfficeInteractable) -> void:
	if _truthy(_flag(main, "mfa_open")):
		_show_simple(main, "TOKEN", "El segundo factor ya está aceptado. La puerta magnética puede abrirse.")
		return
	_show_mfa_card(main, target)


static func _show_mfa_card(main: Node, target: OfficeInteractable) -> void:
	var ui := _ui(main)
	if ui == null:
		return
	var minute := int(target.get_meta("minute", 0))
	ui.show_card("TOKEN", "Último inicio de sesión: 22:17\nSerie impresa: 4920\n\nRuleta de minutos: %02d\nToken: 4920%02d" % [minute, minute])
	ui.add_button("−5 min", _spin_minute.bind(main, target, -5))
	ui.add_button("−1 min", _spin_minute.bind(main, target, -1))
	ui.add_button("+1 min", _spin_minute.bind(main, target, 1))
	ui.add_button("+5 min", _spin_minute.bind(main, target, 5))
	ui.add_button("Confirmar token", _confirm_mfa.bind(main, target))
	ui.add_button("Cerrar", ui.close)


static func _spin_minute(main: Node, target: OfficeInteractable, delta: int) -> void:
	target.set_meta("minute", posmod(int(target.get_meta("minute", 0)) + delta, 60))
	_show_mfa_card(main, target)


static func _confirm_mfa(main: Node, target: OfficeInteractable) -> void:
	var minute := int(target.get_meta("minute", 0))
	var token := "4920%02d" % minute
	if _act(main, "mfa", {"token": token}) != "":
		return
	_show_simple(main, "TOKEN", "Segundo factor aceptado. La puerta magnética puede abrirse.")


static func _logs(main: Node, target: OfficeInteractable) -> void:
	var ui := _ui(main)
	if ui == null:
		return
	var solved := _flag_text(main, "log_digits") == "14"
	var log_title := "Doce líneas. Elige la hora que no encaja." if not solved else "El minuto 14 ya está anotado."
	ui.show_card("REGISTRO DE ACCESOS", log_title)
	if solved:
		for entry in LOG_LINES:
			ui.add_text(str(entry["text"]))
		ui.add_button("Cerrar", ui.close)
		return
	for entry in LOG_LINES:
		ui.add_button(str(entry["text"]), _choose_log.bind(main, str(entry["minute"])))
	ui.add_button("Cerrar", ui.close)


static func _choose_log(main: Node, minute: String) -> void:
	if _act(main, "logs", {"minute": minute}) != "":
		return
	_show_simple(main, "REGISTRO", "Quedó anotado el minuto 14.")


static func _box(main: Node, target: OfficeInteractable) -> void:
	if _truthy(_flag(main, "box_open")):
		_show_box_contents(main)
		return
	_show_box_card(main, target)


static func _show_box_card(main: Node, target: OfficeInteractable) -> void:
	var ui := _ui(main)
	if ui == null:
		return
	var digits: Array = target.get_meta("digits", [0, 0, 0, 0])
	var shown := ""
	for digit in digits:
		shown += str(int(digit))
	ui.show_card("CAJA DE EVIDENCIA", "Cuatro ruedas. Junta lo que el correo, la clave y el registro ya te dieron.\n\n%s" % shown)
	for i in range(4):
		ui.add_button("Rueda %d · %d · subir" % [i + 1, int(digits[i])], _spin_digit.bind(main, target, i, 1))
		ui.add_button("Rueda %d · bajar" % [i + 1], _spin_digit.bind(main, target, i, -1))
	ui.add_button("Abrir", _confirm_box.bind(main, target))
	ui.add_button("Cerrar", ui.close)


static func _spin_digit(main: Node, target: OfficeInteractable, index: int, delta: int) -> void:
	var digits: Array = (target.get_meta("digits", [0, 0, 0, 0]) as Array).duplicate()
	digits[index] = posmod(int(digits[index]) + delta, 10)
	target.set_meta("digits", digits)
	_show_box_card(main, target)


static func _confirm_box(main: Node, target: OfficeInteractable) -> void:
	var digits: Array = target.get_meta("digits", [0, 0, 0, 0])
	var code := ""
	for digit in digits:
		code += str(int(digit))
	if _act(main, "box", {"code": code}) != "":
		return
	target.tint(MINT)
	_show_box_contents(main)


static func _show_box_contents(main: Node) -> void:
	_show_simple(main, "CAJA ABIERTA", BOX_TEXT)


static func _archive(main: Node) -> void:
	var office := main.get("office") as Node3D
	if office == null:
		return
	var hinge := office.get_node_or_null("ArchiveHinge") as Node3D
	if hinge == null:
		return
	var tracked: Variant = main.get("archive_open")
	var open := not bool(hinge.get_meta("archive_open", false))
	if tracked != null:
		open = not bool(tracked)
		main.set("archive_open", open)
	hinge.set_meta("archive_open", open)
	var angle := -PI / 2.0 if open else 0.0
	var tween := hinge.create_tween()
	tween.tween_property(hinge, "rotation:y", angle, 0.45)


static func _guide(main: Node, target: OfficeInteractable) -> void:
	var ui := _ui(main)
	if ui == null:
		return
	var roles: Array = OfficeGuide.ROLES
	var index := int(target.argument)
	if index < 0 or index >= roles.size():
		return
	var role: Dictionary = roles[index]
	var body := "ASISTENCIA GRABADA · %s\n\n%s" % [str(role.action), str(role.lesson)]
	var session: Variant = main.get("session")
	if session != null and session.has_method("objective"):
		var next := str(session.call("objective"))
		if not next.is_empty():
			body += "\n\nAHORA\n" + next
	ui.show_card("%s · %s" % [str(role.name), str(role.zone)], body)
	ui.add_button("Volver a la oficina", ui.close)


static func _exit(main: Node, target: OfficeInteractable) -> void:
	var hinge := target.get_parent() as Node3D
	if hinge == null or hinge.name != "ExitHinge":
		var office := main.get("office") as Node3D
		hinge = null
		if office:
			hinge = office.get_node_or_null("ExitHinge") as Node3D
	if hinge == null:
		_toast(main, "No encuentro la puerta magnética.")
		return
	if bool(hinge.get_meta("door_open", false)) or bool(target.get_meta("door_open", false)):
		_toast(main, "La puerta magnética ya está abierta.")
		return
	if not _truthy(_flag(main, "mfa_open")):
		_toast(main, "Puerta magnética cerrada. Falta el segundo factor.")
		return
	hinge.set_meta("door_open", true)
	target.set_meta("door_open", true)
	var existing: Variant = main.get("exit_tween")
	if existing is Tween and (existing as Tween).is_valid():
		(existing as Tween).kill()
	var tween := hinge.create_tween()
	tween.tween_property(hinge, "rotation:y", -PI / 2.0, 0.55)
	main.set("exit_tween", tween)


static func _manual_card(main: Node, target: OfficeInteractable) -> void:
	var ui := _ui(main)
	if ui == null:
		return
	var puzzle := str(target.get_meta("puzzle", ""))
	var session: Variant = main.get("session")
	if session != null and session.get("manual_open") != null:
		session.set("manual_open", true)
	ui.show_card("MANUAL · %s" % puzzle, _manual_body(main, puzzle))
	ui.add_button("Cerrar", _close_manual.bind(main))


static func _manual_body(main: Node, puzzle: String) -> String:
	var session: Variant = main.get("session")
	if session != null and session.has_method("read_manual"):
		session.call("read_manual", puzzle)
	if session != null and session.has_method("puzzle"):
		var data: Variant = session.call("puzzle", puzzle)
		if data is Dictionary:
			var text := str((data as Dictionary).get("manual", ""))
			if not text.is_empty():
				return text
	return str(MANUALS.get(puzzle, "Manual breve no disponible."))


static func _close_manual(main: Node) -> void:
	var session: Variant = main.get("session")
	if session != null:
		if session.get("manual_open") != null:
			session.set("manual_open", false)
		var state: Variant = session.get("state")
		if state is Dictionary and (state as Dictionary).has("manual_open"):
			(state as Dictionary)["manual_open"] = false
	var ui := _ui(main)
	if ui:
		ui.close()


static func _sync_reveals(main: Node) -> void:
	var office := main.get("office") as Node3D
	if office == null:
		return
	if _flag_text(main, "phish_digit") == "4":
		var email := office.get_node_or_null("Cork/EmailPhish")
		if email:
			_reveal_phish(email)
	if _flag_text(main, "password_digit") == "7":
		var login := office.get_node_or_null("Login")
		if login:
			_reveal_password(login)


static func _act(main: Node, id: String, payload: Dictionary) -> String:
	var session: Variant = main.get("session")
	if session == null or not session.has_method("act"):
		_toast(main, "La sesión todavía no acepta esta acción.")
		return "sin-act"
	var result: Variant = session.call("act", id, payload)
	if not (result is String):
		_toast(main, "La sesión no respondió a %s." % id)
		return "tipo"
	if not (result as String).is_empty():
		_toast(main, result)
		return result
	_commit(main)
	return ""


static func _commit(main: Node) -> void:
	if main.has_method("_commit"):
		main.call("_commit")
	var office := main.get("office") as Node3D
	if office == null:
		return
	_hide_quiz(office)
	_seal_door_prompt(office)
	_hold_door(main, office)


static func _hold_door(main: Node, office: Node3D) -> void:
	var hinge := office.get_node_or_null("ExitHinge") as Node3D
	if hinge == null or not bool(hinge.get_meta("door_open", false)):
		return
	var existing: Variant = main.get("exit_tween")
	if existing is Tween and (existing as Tween).is_valid():
		(existing as Tween).kill()
	hinge.rotation.y = -PI / 2.0


static func _flag(main: Node, key: String) -> Variant:
	var session: Variant = main.get("session")
	if session == null:
		return null
	var sources: Array = [session.get("flags")]
	var state: Variant = session.get("state")
	if state is Dictionary:
		sources.append((state as Dictionary).get("flags", null))
		sources.append(state)
	for source in sources:
		if source is Dictionary and (source as Dictionary).has(key):
			return (source as Dictionary)[key]
	return null


static func _flag_text(main: Node, key: String) -> String:
	var value: Variant = _flag(main, key)
	if value == null:
		return ""
	return str(value)


static func _truthy(value: Variant) -> bool:
	if value == null or value == false:
		return false
	if value is String:
		return not (value as String).is_empty()
	if value is int or value is float:
		return value != 0
	return true


static func _show_simple(main: Node, title: String, body: String) -> void:
	var ui := _ui(main)
	if ui == null:
		return
	ui.show_card(title, body)
	ui.add_button("Cerrar", ui.close)


static func _toast(main: Node, text: String) -> void:
	var ui := _ui(main)
	if ui:
		ui.toast(text)


static func _ui(main: Node) -> EscapeUI:
	return main.get("ui") as EscapeUI
