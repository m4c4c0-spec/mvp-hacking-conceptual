class_name Sala2
extends RefCounted

const Builder = preload("res://Game/Levels/office_builder.gd")

const PLASTER := Color("9daaa3")
const PARTITION := Color("bbc2b6")
const FLOOR_COLOR := Color("41494b")
const METAL := Color("778b91")
const WOOD := Color("966746")
const PAPER := Color("d5d4c1")
const INK := Color("182b35")
const LED := Color("ffe0ac")

const WALL_HEIGHT := 3.0
const SYNC_SECONDS := 3.0
const LAMP_REACH := 1.2

const TRAFFIC_TABLE := """01  RACK-01 → 198.51.100.12    0,2 GB   color azul · palabra VERDE
02  RACK-07 → 192.0.2.18       0,1 GB   color amarillo · palabra AMARILLO
03  RACK-03 → 203.0.113.44     2,3 GB   color azul · palabra AZUL ▲
04  RACK-02 → 198.51.100.40    0,4 GB   color verde · palabra VERDE
05  RACK-11 → 192.0.2.77       0,1 GB   color rojo · palabra ROJO
06  RACK-03 → 203.0.113.44     2,3 GB   color azul · palabra AZUL ▲
07  RACK-05 → 198.51.100.9     0,3 GB   color azul · palabra ROJO
08  RACK-03 → 203.0.113.44     2,3 GB   color azul · palabra AZUL ▲
09  RACK-08 → 192.0.2.4        0,2 GB   color verde · palabra VERDE
10  RACK-04 → 198.51.100.21    0,1 GB   color amarillo · palabra AMARILLO"""

const MANUAL_TITLE := {
	"traffic": "MANUAL · TRÁFICO",
	"cable": "MANUAL · CONTENCIÓN",
	"fire": "MANUAL · FAIL-SAFE",
}

const MANUAL_FALLBACK := {
	"traffic": "El tráfico es el registro de qué equipo habla con qué dirección y cuánto volumen mueve. La fuga es la fila que se repite, marcada con una palabra y una flecha además del color.",
	"cable": "Contener es cortar solo el enlace de la fuga y dejar en pie lo que sostiene a las personas. AZUL es el externo. VERDE es vida: cortarlo avisa la evacuación. ROJO es energía. AMARILLO es voz y no aísla la fuga.",
	"fire": "Un fail-safe abre un paso solo cuando dos detectores coinciden, y lo vuelve a cerrar al poco rato. Dos detectores a la vez abren la puerta 90 s. La lámpara es un prop LED, no una llama.",
}

const OURS := ["TrafficManual", "ContainmentManual", "EmergencyManual"]


static func build(office: Node3D) -> void:
	_open_corridor(office)
	_build_shell(office)
	_build_racks(office)
	_build_traffic(office)
	_build_cables(office)
	_build_detectors(office)
	_build_lab_door(office)
	_apply_emergency(office, _cable_already_cut(office))


static func handle(main: Node, target: OfficeInteractable) -> bool:
	match target.action:
		"traffic":
			if target.name != "TrafficSheet":
				return false
			_show_traffic(main)
			return true
		"cable":
			return _cut_cable(main, target)
		"manual":
			return _read_manual(main, target)
		"fire":
			if target.name == "DetectorLamp":
				return _arm_lamp(main, target)
			if target.name == "DetectorKey":
				return _turn_key(main, target)
			return false
		"lamp":
			return _hint_lamp(main, target)
		"labdoor":
			if target.name != "LabDoor":
				return false
			return _use_lab_door(main, target)
		_:
			return false


static func _open_corridor(office: Node3D) -> void:
	var wall := office.get_node_or_null("CorridorEnd")
	if wall is CollisionObject3D:
		wall.collision_layer = 0
		wall.collision_mask = 0
		wall.visible = false


static func _build_shell(office: Node3D) -> void:
	Builder.box(office, "ServerFloor", Vector3(-3, -0.12, 15), Vector3(8, 0.24, 10), FLOOR_COLOR, true, 2)
	Builder.box(office, "ServerBridgeFloor", Vector3(-3, -0.12, 9.5), Vector3(1.88, 0.24, 1.16), FLOOR_COLOR, true, 2)
	Builder.box(office, "ServerSillFloor", Vector3(-3, -0.12, 20.06), Vector3(1.8, 0.24, 0.16), FLOOR_COLOR, true, 2)
	Builder.box(office, "CeilingDeck", Vector3(-3, 3.04, 15), Vector3(8, 0.08, 10), PLASTER, false)
	_wall(office, "ServerWestWall", Vector3(-6.94, 0, 15), Vector3(0.12, WALL_HEIGHT, 10))
	_wall(office, "ServerEastWall", Vector3(0.94, 0, 15), Vector3(0.12, WALL_HEIGHT, 10))
	_wall(office, "ServerEntranceLeftWall", Vector3(-5.575, 0, 10.06), Vector3(2.85, WALL_HEIGHT, 0.12))
	_wall(office, "ServerEntranceRightWall", Vector3(-0.425, 0, 10.06), Vector3(2.85, WALL_HEIGHT, 0.12))
	_wall(office, "ServerLabLeftWall", Vector3(-5.375, 0, 19.94), Vector3(3.25, WALL_HEIGHT, 0.12))
	_wall(office, "ServerLabRightWall", Vector3(-0.625, 0, 19.94), Vector3(3.25, WALL_HEIGHT, 0.12))
	_wall(office, "ServerBridgeWestWall", Vector3(-4.0, 0, 9.5), Vector3(0.12, WALL_HEIGHT, 1.16))
	_wall(office, "ServerBridgeEastWall", Vector3(-2.0, 0, 9.5), Vector3(0.12, WALL_HEIGHT, 1.16))
	# Tabique con paso al este y ala larga: ningún punto ve los dos detectores a la vez.
	_wall(office, "ServerPartition", Vector3(-4.275, 0, 16.2), Vector3(5.45, WALL_HEIGHT, 0.12), PARTITION)
	_wall(office, "ServerPartitionEast", Vector3(0.215, 0, 16.2), Vector3(1.33, WALL_HEIGHT, 0.12), PARTITION)
	_wall(office, "ServerWingPartition", Vector3(-2.35, 0, 15.7), Vector3(0.12, WALL_HEIGHT, 3.1), PARTITION)
	Builder.label(office, "SALA DE SERVIDORES", Vector3(-4.5, 2.2, 16.02), 40, INK, PI)
	_omni(office, "ServerLightSouth", Vector3(-3, 2.55, 12.6))
	_omni(office, "ServerLightNorth", Vector3(-3, 2.55, 18.2))


static func _build_racks(office: Node3D) -> void:
	for spec in [["Rack01", "RACK-01", 11.15], ["RACK-03", "RACK-03", 12.35], ["Rack07", "RACK-07", 13.55]]:
		var at := Vector3(-6.58, 1.525, float(spec[2]))
		Builder.box(office, str(spec[0]), at, Vector3(0.6, 1.35, 0.48), METAL)
		Builder.label(office, str(spec[1]), at + Vector3(0.36, 0.12, 0), 34, PAPER, PI / 2.0)
	Builder.box(office, "LampShelf", Vector3(-5.15, 0.98, 11.4), Vector3(0.46, 0.26, 0.4), WOOD)
	var lamp := _prop(office, "LampProp", "lamp", "LÁMPARA LED · G tomar · colócala en el detector 1", Vector3(-5.15, 1.28, 11.4), Vector3(0.18, 0.32, 0.18), LED, "", true)
	Builder.box(lamp, "LampLed", Vector3(0, 0.18, 0), Vector3(0.08, 0.06, 0.08), Color("fff6d8"), false)


static func _build_traffic(office: Node3D) -> void:
	var sheet := _prop(office, "TrafficSheet", "traffic", "TABLA DE TRÁFICO · 10 filas · E", Vector3(-5.57, 1.28, 14.35), Vector3(0.12, 0.62, 0.9), PAPER, "traffic")
	_glyph(sheet, "▲  AZUL", Vector3(0.1, 0.08, 0), PI / 2.0)
	_prop(office, "TrafficManual", "manual", "MANUAL DE TRÁFICO · E", Vector3(-5.57, 1.16, 15.05), Vector3(0.12, 0.36, 0.32), WOOD, "traffic")


static func _build_cables(office: Node3D) -> void:
	Builder.box(office, "CableBackplate", Vector3(-0.3, 1.3, 13.15), Vector3(0.08, 0.62, 1.55), PARTITION)
	var specs := [
		["CableAzul", "azul", "AZUL · externo", "▲", Color("2a6fdb"), 12.55],
		["CableVerde", "verde", "VERDE · vida", "■", Color("2f9a52"), 12.95],
		["CableRojo", "rojo", "ROJO · energía", "●", Color("c4473d"), 13.35],
		["CableAmarillo", "amarillo", "AMARILLO · voz", "◆", Color("e2c04a"), 13.75],
	]
	for spec in specs:
		var cable := _prop(office, str(spec[0]), "cable", str(spec[2]), Vector3(-0.46, 1.3, float(spec[5])), Vector3(0.12, 0.4, 0.22), spec[4])
		cable.set_meta("color", str(spec[1]))
		_glyph(cable, "%s  %s" % [str(spec[3]), str(spec[2]).split(" · ")[0]], Vector3(-0.16, 0.34, 0), -PI / 2.0)
	_prop(office, "ContainmentManual", "manual", "MANUAL DE CONTENCIÓN · E", Vector3(-0.46, 1.16, 14.4), Vector3(0.12, 0.32, 0.34), WOOD, "cable")
	var drawer := _prop(office, "EmergencyManual", "manual", "MANUAL FAIL-SAFE · dos detectores · 90 s · E", Vector3(-0.46, 1.16, 11.85), Vector3(0.12, 0.22, 0.34), PAPER, "fire")
	drawer.set_meta("closed_at", drawer.position)
	drawer.visible = false
	drawer.collision_layer = 0
	drawer.collision_mask = 0


static func _build_detectors(office: Node3D) -> void:
	var lamp := _prop(office, "DetectorLamp", "fire", "DETECTOR 1 · lámpara LED · E", Vector3(-3.9, 1.22, 14.83), Vector3(0.4, 0.5, 0.12), METAL)
	_glyph(lamp, "1", Vector3(0, 0.34, -0.1), PI)
	var key := _prop(office, "DetectorKey", "fire", "DETECTOR 2 · llave · E", Vector3(-3.9, 1.22, 17.57), Vector3(0.4, 0.5, 0.12), METAL)
	_glyph(key, "2", Vector3(0, 0.34, 0.1), 0.0)


static func _build_lab_door(office: Node3D) -> void:
	var hinge := Node3D.new()
	hinge.name = "LabHinge"
	hinge.position = Vector3(-3.75, 0, 20)
	office.add_child(hinge)
	_prop(hinge, "LabDoor", "labdoor", "LABORATORIO · 90 s · E", Vector3(0.75, 1.35, 0), Vector3(1.5, 2.7, 0.12), INK)
	Builder.box(office, "ServerDoorLintel", Vector3(-3, 2.82, 19.94), Vector3(1.5, 0.24, 0.12), PLASTER)
	Builder.label(office, "LABORATORIO · 90 s", Vector3(-3, 2.55, 19.78), 40, INK, PI)


static func _wall(office: Node3D, node_name: String, position: Vector3, size: Vector3, color: Color = PLASTER) -> void:
	position.y = size.y * 0.5
	Builder.box(office, node_name, position, size, color)


static func _omni(office: Node3D, node_name: String, position: Vector3) -> void:
	var light := OmniLight3D.new()
	light.name = node_name
	light.position = position
	light.omni_range = 8.0
	light.light_energy = 0.4
	light.light_specular = 0.0
	light.shadow_enabled = false
	light.light_color = Color("fff0db")
	office.add_child(light)


static func _prop(parent: Node3D, node_name: String, action: String, prompt: String, position: Vector3, size: Vector3, color: Color, puzzle := "", grabbable := false) -> OfficeInteractable:
	var body := Builder.prop(parent, node_name, action, 0, prompt, position, size, color, grabbable)
	if not puzzle.is_empty():
		body.set_meta("puzzle", puzzle)
	return body


static func _glyph(parent: Node3D, text: String, position: Vector3, yaw: float) -> void:
	var sign := Builder.label(parent, text, position, 42, INK, yaw)
	var font := SystemFont.new()
	font.font_names = PackedStringArray(["DejaVu Sans", "Noto Sans", "Sans"])
	sign.font = font


static func _show_traffic(main: Node) -> void:
	var ui = main.get("ui")
	var body := "La fuga es la fila que se repite. La palabra y el icono cuentan además del color.\n\n" + TRAFFIC_TABLE
	if ui == null or not ui.has_method("show_card"):
		_toast(main, body)
		return
	ui.show_card("TRÁFICO · 10 FILAS", body)
	ui.add_button("La fuga es AZUL ▲", func(): _choose_traffic(main, "azul"))
	ui.add_button("La fuga es VERDE ■", func(): _choose_traffic(main, "verde"))
	ui.add_button("La fuga es ROJO ●", func(): _choose_traffic(main, "rojo"))
	ui.add_button("La fuga es AMARILLO ◆", func(): _choose_traffic(main, "amarillo"))
	ui.add_button("Cerrar", ui.close)


static func _choose_traffic(main: Node, color: String) -> void:
	var error := _invoke(main, "traffic", {"color": color})
	if not error.is_empty():
		_toast(main, error)
		return
	_close(main)
	if color == "azul":
		_toast(main, "Fuga marcada: RACK-03 → 203.0.113.44, 2,3 GB, AZUL ▲.")
	else:
		_toast(main, "Color registrado.")


static func _cut_cable(main: Node, target: OfficeInteractable) -> bool:
	var color := str(target.get_meta("color", ""))
	if color.is_empty():
		return false
	var error := _invoke(main, "cable", {"color": color})
	if not error.is_empty():
		_toast(main, error)
		return true
	if color != "azul":
		_toast(main, "Ese corte no aísla la fuga.")
		return true
	var office := _office_of(target)
	if office:
		_apply_emergency(office, true)
	_show_manual_card(main, "fire")
	return true


static func _read_manual(main: Node, target: OfficeInteractable) -> bool:
	if target.name not in OURS:
		return false
	var puzzle := str(target.get_meta("puzzle", ""))
	if puzzle not in MANUAL_FALLBACK:
		return false
	_show_manual_card(main, puzzle)
	return true


static func _show_manual_card(main: Node, puzzle: String) -> void:
	var session = main.get("session")
	if session and session.has_method("read_manual"):
		session.read_manual(puzzle)
		_commit(main)
	var ui = main.get("ui")
	var body := _manual_text(main, puzzle)
	var title := str(MANUAL_TITLE.get(puzzle, "MANUAL"))
	if ui == null or not ui.has_method("show_card"):
		_toast(main, body)
		return
	ui.show_card(title, body)
	ui.add_button("Cerrar", _close_manual.bind(main))


static func _hint_lamp(main: Node, lamp: OfficeInteractable) -> bool:
	if lamp.name != "LampProp":
		return false
	var office := _office_of(lamp)
	var detector: OfficeInteractable = null
	if office:
		detector = office.get_node_or_null("DetectorLamp") as OfficeInteractable
	if detector and lamp.global_position.distance_to(detector.global_position) < LAMP_REACH:
		return _arm_lamp(main, detector)
	_toast(main, "Lámpara LED. Tómala con G y úsala en el detector 1.")
	return true


static func _arm_lamp(main: Node, detector: OfficeInteractable) -> bool:
	var office := _office_of(detector)
	if office == null:
		return false
	var lamp := office.get_node_or_null("LampProp") as OfficeInteractable
	if lamp == null:
		_toast(main, "Falta la lámpara LED.")
		return true
	var holding := _held(main) == lamp
	var near := not lamp.is_held and lamp.global_position.distance_to(detector.global_position) < LAMP_REACH
	var seated := bool(lamp.get_meta("seated", false))
	if not holding and not near and not seated:
		_toast(main, "Trae la lámpara LED con G y úsala en este detector.")
		return true
	var slot := detector.position + Vector3(-0.46, 0.08, 0)
	var home := lamp.home_transform
	home.origin = slot
	lamp.home_transform = home
	lamp.set_meta("seated", true)
	if holding:
		var player = main.get("player")
		if player and player.has_method("drop"):
			player.drop()
	elif not lamp.is_held:
		lamp.position = slot
		lamp.collision_layer = 1
		lamp.collision_mask = 1
	detector.set_meta("sync_mark", _clock(main))
	_toast(main, "Lámpara LED en el detector 1. La llave del detector 2 tiene 3 s.")
	return true


static func _turn_key(main: Node, _key: OfficeInteractable) -> bool:
	var office := _office_of(_key)
	if office == null:
		return false
	var lamp := office.get_node_or_null("LampProp") as OfficeInteractable
	var detector := office.get_node_or_null("DetectorLamp") as OfficeInteractable
	if lamp == null or detector == null:
		return false
	var near := not lamp.is_held and lamp.global_position.distance_to(detector.global_position) < LAMP_REACH
	var seated := bool(lamp.get_meta("seated", false))
	var mark = detector.get_meta("sync_mark") if detector.has_meta("sync_mark") else null
	var fresh := (mark is float or mark is int) and _clock(main) - float(mark) <= SYNC_SECONDS
	if (near or seated) and fresh and not lamp.is_held:
		var error := _invoke(main, "fire", {"synced": true})
		if not error.is_empty():
			_toast(main, error)
			return true
		_toast(main, "Detectores a la vez. La puerta del laboratorio responde durante 90 s.")
		return true
	lamp.set_meta("seated", false)
	if detector.has_meta("sync_mark"):
		detector.remove_meta("sync_mark")
	_toast(main, "No coincidieron. Repite los dos detectores: lámpara y llave, dentro de 3 s.")
	return true


static func _use_lab_door(main: Node, door: OfficeInteractable) -> bool:
	var hinge := door.get_parent() as Node3D
	if hinge == null or hinge.name != "LabHinge":
		return false
	var until = _numeric_flag(main, "lab_open_until")
	var elapsed = _session_elapsed(main)
	var open := until != null and elapsed != null and float(elapsed) <= float(until)
	if open:
		if not bool(hinge.get_meta("door_open", false)):
			hinge.set_meta("door_open", true)
			_swing(hinge, PI / 2.0)
		_toast(main, "Laboratorio abierto. Quedan %d s." % maxi(int(ceil(float(until) - float(elapsed))), 0))
		return true
	if bool(hinge.get_meta("door_open", false)):
		hinge.set_meta("door_open", false)
		_swing(hinge, 0.0)
		_toast(main, "La puerta se cerró. Puedes repetir los dos detectores.")
		return true
	_toast(main, "El laboratorio sigue cerrado. Los dos detectores, a la vez, lo abren 90 s.")
	return true


static func _swing(hinge: Node3D, yaw: float) -> void:
	if hinge.has_meta("door_tween"):
		var running = hinge.get_meta("door_tween")
		if running is Tween and (running as Tween).is_valid():
			(running as Tween).kill()
	var tween := hinge.create_tween()
	hinge.set_meta("door_tween", tween)
	tween.tween_property(hinge, "rotation:y", yaw, 0.45)


static func _apply_emergency(office: Node3D, open: bool) -> void:
	var drawer := office.get_node_or_null("EmergencyManual") as OfficeInteractable
	if drawer == null:
		return
	var closed: Vector3 = drawer.get_meta("closed_at", drawer.position)
	drawer.set_meta("closed_at", closed)
	var shown := closed + Vector3(-0.28, 0, 0)
	if not open:
		drawer.visible = false
		drawer.position = closed
		drawer.collision_layer = 0
		drawer.collision_mask = 0
		drawer.set_meta("shown", false)
		return
	drawer.visible = true
	drawer.collision_layer = 1
	drawer.collision_mask = 1
	if bool(drawer.get_meta("shown", false)):
		drawer.position = shown
		return
	drawer.set_meta("shown", true)
	drawer.position = closed
	drawer.create_tween().tween_property(drawer, "position", shown, 0.35)


static func _cable_already_cut(office: Node3D) -> bool:
	var host := office.get_parent()
	if not host is Node:
		return false
	return bool(_flags(host).get("cable_blue", false))


static func _manual_text(main: Node, puzzle: String) -> String:
	var session = main.get("session")
	if session and session.has_method("puzzle"):
		var data = session.puzzle(puzzle)
		if data is Dictionary and not str(data.get("manual", "")).is_empty():
			return str(data.manual)
	return str(MANUAL_FALLBACK.get(puzzle, ""))


static func _invoke(main: Node, id: String, payload: Dictionary) -> String:
	var session = main.get("session")
	if session == null or not session.has_method("act"):
		return "La sesión todavía no acepta esta acción."
	var error := str(session.act(id, payload))
	_commit(main)
	return error


static func _commit(main: Node) -> void:
	if main.has_method("_commit"):
		main._commit()


static func _close(main: Node) -> void:
	var ui = main.get("ui")
	if ui and ui.has_method("close"):
		ui.close()


static func _close_manual(main: Node) -> void:
	var session = main.get("session")
	if session != null and ("manual_open" in session):
		session.manual_open = false
	_close(main)


static func _toast(main: Node, text: String) -> void:
	var ui = main.get("ui")
	if ui and ui.has_method("toast"):
		ui.toast(text)


static func _state(main: Node) -> Dictionary:
	var session = main.get("session")
	if session == null:
		return {}
	var state = session.get("state")
	if state is Dictionary:
		return state
	return {}


static func _flags(main: Node) -> Dictionary:
	var flags = _state(main).get("flags", null)
	if flags is Dictionary:
		return flags
	var session = main.get("session")
	if session != null and ("flags" in session):
		var direct = session.flags
		if direct is Dictionary:
			return direct
	return {}


static func _numeric_flag(main: Node, key: String):
	var value = _flags(main).get(key, null)
	if value is int or value is float:
		return float(value)
	return null


static func _session_elapsed(main: Node):
	var value = _state(main).get("elapsed", null)
	if value is int or value is float:
		return float(value)
	var session = main.get("session")
	if session != null and ("elapsed" in session):
		var direct = session.elapsed
		if direct is int or direct is float:
			return float(direct)
	return null


static func _clock(main: Node) -> float:
	var elapsed = _session_elapsed(main)
	if elapsed != null:
		return float(elapsed)
	return Time.get_ticks_msec() / 1000.0


static func _held(main: Node) -> OfficeInteractable:
	var player = main.get("player")
	if player == null or not ("held" in player):
		return null
	return player.held as OfficeInteractable


static func _office_of(node: Node) -> Node3D:
	var current := node
	while current:
		if current.name == "Office" and current is Node3D:
			return current
		current = current.get_parent()
	var parent := node.get_parent()
	return parent as Node3D
