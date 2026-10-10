extends Node
class_name Sala3

## Laboratorio de Operación Cóndor, pasado la puerta de z ≈ 20.
## Geometría de cajas. Las reglas viven en session.act.

const Builder = preload("res://Game/Levels/office_builder.gd")

const NAVY := Color("182b35")
const WOOD := Color("966746")
const PAPER := Color("d5d4c1")
const MINT := Color("55d4be")
const RED := Color("bb5150")
const PLASTER := Color("aab3ad")
const FLOOR := Color("41494b")
const INK := Color("243943")
const AMBER := Color("c9ae78")

const PHRASE := "COFRE EN EL CAJON DEL"
const CIPHER := "Q09GUkUgRU4gRUwgQ0FKT04gREVM"
const MAC := "DC:A6:32:0E:77:4F"
const OFFICIAL := "e8576173473a"
const FOUND_C := "21ea12c9e607"
const KILL_CODE := "7391"

const MANUALS := {
	"osint": "OSINT es observar lo que una persona ya dejó a la vista, sin entrar en ningún sistema. En el muro, la ficha de V. Soto muestra un gato. La palabra es el nombre, no la especie.",
	"base64": "Ofuscar es cambiar el aspecto de un texto; Base64 disfraza letras y no las protege. La nota ya trae la tabla y tres trozos alineados. La frase se reconoce; no hay que operar un descifrado.",
	"rssi": "El inventario es la lista de equipos que sí pertenecen a la casa; una MAC ausente es un extraño. La señal menos negativa está más cerca. Tres cuerdas se cruzan en una celda.",
	"hash": "La integridad se comprueba con una huella: si no coincide, el archivo cambió. A y B repiten el sello oficial. C, la hallada, es otra. Se descarta C.",
}

const ENDING_COPY := {
	"victory": "Copia detenida. La lista sigue en el edificio.",
	"timeout": "Descarga completa. La lista salió del edificio.",
	"safety": "Salida de emergencia. La operación se detiene sin castigo.",
}

const LESSONS := [
	"Anzuelo. El dominio llevaba un 1 donde iba una i, el mensaje pedía la clave y apuraba. Esas tres señales bastan para no entregar nada.",
	"Clave a la vista. El nombre del equipo y el año estaban en el escritorio. Una clave se protege cuando no se arma con el banderín ni con el calendario.",
	"Segunda prueba. La clave no alcanza. El código que cambia, situado en el minuto del último inicio, es la otra prueba. Sin ella la puerta magnética sigue cerrada.",
	"La línea del registro. A las 03:14 la cuenta de aseo tuvo ROOT en RACK-03. El registro muestra hora, cuenta y permiso. Una sospecha no se escribe como si fuera esa línea.",
	"La fuga. La misma copia grande sale de RACK-03 hacia 203.0.113.44, marcada AZUL. Contener es cortar ese camino. Esa dirección es de documentación: en el juego no apunta a un equipo real.",
	"El cable de vida. El verde sostiene a las personas. Cortarlo anuncia la evacuación y resta cinco minutos. El rojo quita energía y resta dos. El amarillo calla la voz y no abre nada. El cable de la fuga es el azul.",
	"Luz, no incendio. Dos detectores a la vez abren el laboratorio durante noventa segundos. La lámpara es LED: luz y sonido.",
	"El nombre que ya estaba escrito. La ficha de V. Soto nombra al gato Pixel. Otra mascota, aunque sea otro gato, no abre el cofre.",
	"Letras disfrazadas. Base64 cambia el aspecto. La tarjeta ya separa los trozos: COFRE EN EL CAJON DEL. La palabra que falta es Pixel. Ahí aparecen el 7391 y la MAC que termina en :4F.",
	"Lista, sello e interruptor. La MAC DC:A6:32:0E:77:4F no está en el inventario; el plano la sitúa en el carro, celda C-4. La huella C no coincide con la oficial y se descarta. El 7391 detiene la copia. El 1793 adelanta el reloj un minuto.",
]

const BADGES := [
	{"node": "BadgeRuiz", "who": "A. Ruiz", "pet": "Luna", "kind": "perro", "line": "Analista de plantilla."},
	{"node": "BadgeVSoto", "who": "V. Soto", "pet": "Pixel", "kind": "gato", "line": "Aseo, alta de hace meses. Tarjeta personal denegada a las 20:03."},
	{"node": "BadgePalma", "who": "M. Palma", "pet": "Nube", "kind": "pez", "line": "Archivo de plantilla."},
	{"node": "BadgeLeon", "who": "C. León", "pet": "Sol", "kind": "gato", "line": "Redes de plantilla. También hay un gato, y no es Pixel."},
	{"node": "BadgeDiaz", "who": "R. Díaz", "pet": "Mia", "kind": "loro", "line": "Recepción de plantilla."},
	{"node": "BadgePena", "who": "J. Peña", "pet": "Rocky", "kind": "hámster", "line": "Guardia de plantilla."},
]


static func build(office: Node3D) -> void:
	if office == null or office.get_node_or_null("LabFloor"):
		return
	_shell(office)
	_badges(office)
	_note(office)
	_radio(office)
	_cart(office)
	_seals(office)
	_switch(office)
	_closing(office)
	var align := Sala3.new()
	align.name = "LabAlign"
	office.add_child(align)


static func handle(main: Node, target: OfficeInteractable) -> bool:
	if main == null or target == null:
		return false
	_refresh(main)
	match target.action:
		"osint":
			_open_badge(main, target)
			return true
		"base64":
			_open_note(main)
			return true
		"rssi":
			if str(target.get_meta("rope", "")) != "":
				_mark_rope(main, str(target.get_meta("rope")))
			else:
				_open_radio(main)
			return true
		"hash":
			_open_seal(main, str(target.get_meta("sample", "")))
			return true
		"kill":
			_press_kill(main, target)
			return true
		"debrief":
			_open_debrief(main)
			return true
		"manual":
			var puzzle := str(target.get_meta("puzzle", ""))
			if puzzle in ["osint", "base64", "rssi", "hash"]:
				_open_manual(main, puzzle)
				return true
			return false
		_:
			return false


func _ready() -> void:
	call_deferred("_align_lab")


func _align_lab() -> void:
	var office := get_parent() as Node3D
	if office == null:
		return
	var host := office.get_parent()
	_sync(office, host.get("session") if host else null)


static func _shell(office: Node3D) -> void:
	# El suelo llega hasta el umbral de la puerta (z ≈ 20,14), no un metro más adentro.
	Builder.box(office, "LabFloor", Vector3(-3, -0.12, 25.57), Vector3(8, 0.24, 10.86), FLOOR, true, 2)
	Builder.box(office, "LabWest", Vector3(-6.94, 1.55, 25.57), Vector3(0.12, 3.1, 10.86), PLASTER)
	Builder.box(office, "LabEast", Vector3(0.94, 1.55, 25.57), Vector3(0.12, 3.1, 10.86), PLASTER)
	Builder.box(office, "LabNorth", Vector3(-3, 1.55, 30.94), Vector3(8, 3.1, 0.12), PLASTER)
	# El vano coincide con la hoja: x de -3,75 a -2,25, en z = 20.
	Builder.box(office, "LabSouthWest", Vector3(-5.375, 1.55, 20.08), Vector3(3.25, 3.1, 0.12), PLASTER)
	Builder.box(office, "LabSouthEast", Vector3(-0.625, 1.55, 20.08), Vector3(3.25, 3.1, 0.12), PLASTER)
	Builder.box(office, "LabLintel", Vector3(-3, 2.95, 20.08), Vector3(1.5, 0.5, 0.12), PLASTER)
	Builder.box(office, "LabDoorJambLeft", Vector3(-3.75, 1.35, 20.02), Vector3(0.08, 2.7, 0.16), WOOD, false)
	Builder.box(office, "LabDoorJambRight", Vector3(-2.25, 1.35, 20.02), Vector3(0.08, 2.7, 0.16), WOOD, false)
	Builder.box(office, "LabDoorHead", Vector3(-3, 2.74, 20.02), Vector3(1.58, 0.1, 0.16), WOOD, false)
	Builder.box(office, "LabCeiling", Vector3(-3, 3.22, 25.57), Vector3(8, 0.1, 10.86), PLASTER)
	var emergency := Builder.box(office, "LabEmergency", Vector3(-3, 2.45, 20.32), Vector3(0.7, 0.1, 0.08), RED)
	var emergency_glow := OmniLight3D.new()
	emergency_glow.name = "EmergencyGlow"
	emergency_glow.light_color = RED
	emergency_glow.light_energy = 0.7
	emergency_glow.omni_range = 3.2
	emergency_glow.shadow_enabled = false
	emergency_glow.light_specular = 0.0
	emergency.add_child(emergency_glow)
	_caption(emergency, "LUZ DE EMERGENCIA", 22, PAPER, "north")
	var status := Builder.box(office, "LabStatus", Vector3(-3, 2.2, 30.78), Vector3(0.46, 0.14, 0.08), AMBER)
	var glow := OmniLight3D.new()
	glow.name = "StatusGlow"
	glow.light_color = AMBER
	glow.light_energy = 0.55
	glow.omni_range = 4.2
	glow.shadow_enabled = false
	glow.light_specular = 0.0
	status.add_child(glow)
	_caption(status, "ESTADO", 22, INK, "south")
	Builder.label(office, "LABORATORIO · UCD", Vector3(-3, 2.55, 30.55), 36, PAPER, PI)
	var lamp := OmniLight3D.new()
	lamp.name = "LabLamp"
	lamp.position = Vector3(-3, 2.9, 26)
	lamp.light_color = Color("ffe0ac")
	lamp.light_energy = 0.55
	lamp.omni_range = 14
	lamp.shadow_enabled = false
	lamp.light_specular = 0.0
	office.add_child(lamp)


static func _badges(office: Node3D) -> void:
	for index in BADGES.size():
		var badge: Dictionary = BADGES[index]
		var column := index % 3
		var row := int(index / 3.0)
		var position := Vector3(-5.15 + column * 1.6, 1.26 if row == 0 else 1.0, 30.7)
		var caption := "%s\n%s %s" % [badge.who, badge.kind, badge.pet]
		var prompt := "FICHA %s · %s %s · E" % [badge.who, badge.kind, badge.pet]
		var body := Builder.prop(office, str(badge.node), "osint", 0, prompt, position, Vector3(0.7, 0.16, 0.1), PAPER)
		body.set_meta("pet", str(badge.pet))
		body.set_meta("who", str(badge.who))
		body.set_meta("kind", str(badge.kind))
		body.set_meta("line", str(badge.line))
		_caption(body, caption, 22, NAVY, "south")


static func _note(office: Node3D) -> void:
	var note := Builder.prop(office, "ConfigNote", "base64", 0, "NOTA · frase ya alineada · E", Vector3(0.78, 1.18, 29.15), Vector3(0.1, 0.32, 0.62), PAPER)
	var title := _caption(note, "Q09GUkUg\nRU4gRUwg\nQ0FKT04gREVM", 15, NAVY, "west")
	title.position.y = 0.04
	var reveal := _caption(note, "", 22, MINT, "west")
	reveal.name = "ConfigReveal"
	reveal.position.y = -0.08
	reveal.visible = false
	var manuals := [
		["ManualOsint", "osint", "OSINT", 24.55],
		["ManualBase64", "base64", "BASE64", 25.35],
		["ManualRssi", "rssi", "INVENTARIO", 26.15],
		["ManualHash", "hash", "HUELLA", 26.95],
	]
	for item in manuals:
		var plaque := Builder.prop(office, str(item[0]), "manual", 0, "MANUAL %s · E" % str(item[2]), Vector3(0.8, 1.05, float(item[3])), Vector3(0.08, 0.2, 0.36), MINT)
		plaque.set_meta("puzzle", str(item[1]))
		_caption(plaque, str(item[2]), 20, INK, "west")


static func _radio(office: Node3D) -> void:
	var plan := Builder.prop(office, "WifiMap", "rssi", 0, "PLANO RSSI · la MAC no está en el inventario · E", Vector3(-6.78, 1.15, 25.4), Vector3(0.1, 0.4, 0.95), NAVY)
	_caption(plan, "%s\nno está en inventario\nA1 -55 ≈ 3 m\nA2 -40 ≈ 1 m\nA3 -70 ≈ 6 m" % MAC, 16, PAPER, "east")
	var ropes := [["RopeA1", "A1", "-55 dBm · 3 m", 24.55], ["RopeA2", "A2", "-40 dBm · 1 m", 25.4], ["RopeA3", "A3", "-70 dBm · 6 m", 26.25]]
	for rope in ropes:
		var button := Builder.prop(office, str(rope[0]), "rssi", 0, "CUERDA %s · %s · E" % [rope[1], rope[2]], Vector3(-6.48, 1.12, float(rope[3])), Vector3(0.12, 0.1, 0.28), AMBER)
		button.set_meta("rope", str(rope[1]))
		_caption(button, "%s\n%s" % [rope[1], rope[2]], 16, INK, "east")
	Builder.box(office, "LabRopeShelf", Vector3(-6.48, 0.49, 25.4), Vector3(0.2, 0.98, 2.05), WOOD)


static func _cart(office: Node3D) -> void:
	var cart := Node3D.new()
	cart.name = "CleaningCart"
	cart.position = Vector3(-3.25, 0, 27.2)
	office.add_child(cart)
	Builder.box(cart, "CartWheelL", Vector3(-0.26, 0.12, 0.22), Vector3(0.1, 0.24, 0.1), NAVY)
	Builder.box(cart, "CartWheelR", Vector3(0.26, 0.12, -0.22), Vector3(0.1, 0.24, 0.1), NAVY)
	Builder.box(cart, "CartFrame", Vector3(0, 0.46, 0), Vector3(0.62, 0.7, 0.78), NAVY)
	Builder.box(cart, "CartRailL", Vector3(-0.32, 1.05, 0), Vector3(0.06, 0.18, 0.7), NAVY)
	Builder.box(cart, "CartRailR", Vector3(0.32, 1.05, 0), Vector3(0.06, 0.18, 0.7), NAVY)
	var drawer := Builder.box(cart, "CartDrawer", Vector3(0, 1.05, 0), Vector3(0.5, 0.08, 0.42), WOOD)
	var tablet := _caption(drawer, "TABLET", 28, MINT, "south")
	tablet.name = "TabletLabel"
	tablet.visible = false
	var cart_sign := _caption(cart, "ASEO\nDOBLE FONDO", 22, PAPER, "south")
	cart_sign.position = Vector3(0, 1.28, -0.42)


static func _seals(office: Node3D) -> void:
	Builder.box(office, "LabHashStand", Vector3(-0.2, 0.5, 28.35), Vector3(1.05, 1.0, 0.7), WOOD)
	Builder.label(office, "HUELLAS\nCON LA TABLET", Vector3(-1.16, 1.46, 28.35), 22, PAPER, -PI / 2.0)
	var sheet := Builder.prop(office, "HashSheet", "hash", 0, "HOJA OFICIAL · comparar sellos · E", Vector3(-0.9, 1.28, 28.35), Vector3(0.08, 0.18, 0.7), PAPER)
	sheet.set_meta("sample", "sheet")
	_caption(sheet, "OFICIAL\n%s" % OFFICIAL, 16, NAVY, "west")
	var samples := [["HashA", "A", 27.95], ["HashB", "B", 28.35], ["HashC", "C", 28.75]]
	for sample in samples:
		var found := OFFICIAL if str(sample[1]) != "C" else FOUND_C
		var card := Builder.prop(office, str(sample[0]), "hash", 0, "SELLO %s · E" % sample[1], Vector3(-1.16, 1.04, float(sample[2])), Vector3(0.08, 0.16, 0.26), MINT if str(sample[1]) != "C" else RED)
		card.set_meta("sample", str(sample[1]))
		_caption(card, "%s\n%s" % [sample[1], found], 14, INK, "west")
		card.visible = false
		card.collision_layer = 0
	sheet.visible = false
	sheet.collision_layer = 0


static func _switch(office: Node3D) -> void:
	Builder.box(office, "LabKillStand", Vector3(-1.2, 0.48, 22.72), Vector3(1.2, 0.96, 0.78), WOOD)
	var panel := Builder.prop(office, "KillSwitch", "kill", 0, "KILL SWITCH · cuatro dígitos en los botones · E", Vector3(-1.2, 0.78, 23.16), Vector3(1.0, 0.28, 0.1), NAVY)
	var display := _caption(panel, "----", 42, MINT, "south")
	display.name = "KillDisplay"
	var digits := ["1", "2", "3", "4", "5", "6", "7", "8", "9"]
	for index in digits.size():
		var column := index % 3
		var row := int(index / 3.0)
		_key(office, "Key%s" % digits[index], digits[index], Vector3(-1.52 + column * 0.32, 1.1, 22.98 - row * 0.22))
	_key(office, "KeyClear", "clear", Vector3(-1.36, 1.1, 22.32))
	_key(office, "Key0", "0", Vector3(-1.04, 1.1, 22.32))


static func _key(office: Node3D, node_name: String, digit: String, position: Vector3) -> void:
	var clearing := digit == "clear"
	var prompt := "BORRAR · E" if clearing else "TECLA %s · E" % digit
	var body := Builder.prop(office, node_name, "kill", 0, prompt, position, Vector3(0.26, 0.09, 0.14), RED if clearing else INK)
	body.set_meta("digit", digit)
	_caption(body, "C" if clearing else digit, 36, PAPER, "south")


static func _closing(office: Node3D) -> void:
	var board := Builder.prop(office, "Debrief", "debrief", 0, "CIERRE · diez lecciones · E", Vector3(-6.76, 1.16, 23.5), Vector3(0.1, 0.32, 0.5), MINT)
	_caption(board, "CIERRE\nDIEZ LECCIONES", 20, INK, "east")


static func _caption(parent: Node3D, text: String, size: int, color: Color, reader: String) -> Label3D:
	var position := Vector3.ZERO
	var yaw := 0.0
	match reader:
		"south":
			position = Vector3(0, 0, -0.07)
			yaw = PI
		"north":
			position = Vector3(0, 0, 0.07)
			yaw = 0.0
		"west":
			position = Vector3(-0.07, 0, 0)
			yaw = -PI / 2.0
		"east":
			position = Vector3(0.07, 0, 0)
			yaw = PI / 2.0
		_:
			position = Vector3(0, 0.08, 0)
	return Builder.label(parent, text, position, size, color, yaw)


static func _open_badge(main: Node, target: OfficeInteractable) -> void:
	var pet := str(target.get_meta("pet", ""))
	var who := str(target.get_meta("who", "Ficha"))
	var kind := str(target.get_meta("kind", "mascota"))
	var line := str(target.get_meta("line", ""))
	var body := "%s\n%s\nEn la foto hay un %s llamado %s." % [who, line, kind, pet]
	if who == "V. Soto":
		body += "\n\nLa credencial de la caja ata este nombre a la ficha. La palabra es el nombre del gato."
	else:
		body += "\n\nOtra mascota no sirve. Otro gato tampoco, si no se llama Pixel."
	var ui = main.get("ui")
	ui.show_card("FICHA · %s" % who, body)
	ui.add_button("Probar con %s" % pet, _try_pet.bind(main, pet))
	ui.add_button("Cerrar", ui.close)


static func _try_pet(main: Node, pet: String) -> void:
	var message := _act(main, "osint", {"pet": pet})
	if not message.is_empty():
		main.ui.toast(message)
		return
	var badge := _office(main).get_node_or_null("BadgeVSoto")
	if badge:
		badge.set_meta("solved", true)
	_refresh(main)
	main.ui.show_card("FICHA · V. SOTO", "El gato se llama Pixel. Esa palabra es la clave de la nota.\n\nLo público bastó: no hizo falta entrar en ninguna cuenta.")
	main.ui.add_button("Cerrar", main.ui.close)


static func _open_note(main: Node) -> void:
	var flags := _flags(main.get("session"))
	var ui = main.get("ui")
	if str(flags.get("kill_code", "")) != "" or _marked(_office(main), "ConfigNote"):
		ui.show_card("NOTA ABIERTA", "%s\n\n%s\nMAC termina en :4F" % [PHRASE, KILL_CODE])
		ui.add_button("Cerrar", ui.close)
		return
	ui.show_card("NOTA DE CONFIGURACIÓN", _note_body())
	for word in ["Pixel", "Sol", "Luna"]:
		ui.add_button(word, _try_key.bind(main, word))
	ui.add_button("Cerrar", ui.close)


static func _note_body() -> String:
	return "Cadena escrita:\n%s\n\nTabla breve. El alfabeto es A–Z, a–z, 0–9, + y /. Cuatro de esas letras son tres caracteres. Base64 cambia el aspecto: no es una cerradura y no hay que operar un descifrado.\n\nTrozos ya alineados:\nQ09GUkUg      COFRE\nRU4gRUwg      EN EL\nQ0FKT04gREVM  CAJON DEL\n\nLa frase es %s.\nElige la palabra que falta." % [CIPHER, PHRASE]


static func _try_key(main: Node, word: String) -> void:
	var message := _act(main, "base64", {"decoded": true, "key": word})
	if not message.is_empty():
		main.ui.toast(message)
		return
	var note := _office(main).get_node_or_null("ConfigNote")
	if note:
		note.set_meta("solved", true)
	_refresh(main)
	main.ui.show_card("NOTA ABIERTA", "%s\n\n%s\nMAC termina en :4F\n\nBase64 disfrazó la frase. No la protegió." % [PHRASE, KILL_CODE])
	main.ui.add_button("Cerrar", main.ui.close)


static func _mark_rope(main: Node, which: String) -> void:
	var plan := _office(main).get_node_or_null("WifiMap")
	if plan == null:
		return
	var marked: Dictionary = plan.get_meta("ropes", {})
	marked[which] = true
	plan.set_meta("ropes", marked)
	var button := _office(main).get_node_or_null("Rope%s" % which) as OfficeInteractable
	if button:
		button.tint(MINT)
	if marked.has("A1") and marked.has("A2") and marked.has("A3"):
		_open_radio(main)
		return
	main.ui.toast("Cuerda %s tensada." % which)


static func _open_radio(main: Node) -> void:
	var plan := _office(main).get_node_or_null("WifiMap")
	var marked: Dictionary = plan.get_meta("ropes", {}) if plan else {}
	var ready := marked.has("A1") and marked.has("A2") and marked.has("A3")
	var state := "Las tres cuerdas están tensas." if ready else "Tensa A1, A2 y A3 en los botones del plano. Llevas: %s." % _rope_list(marked)
	var ui = main.get("ui")
	ui.show_card("PLANO RSSI", _radio_body(state))
	if ready:
		for cell in ["C-2", "B-4", "C-4", "D-6"]:
			ui.add_button("Celda %s" % cell, _try_cell.bind(main, cell))
	ui.add_button("Cerrar", ui.close)


static func _radio_body(state: String) -> String:
	return "Reporte: el intruso es %s y no está en el inventario.\n\nA1  −55 dBm  ≈ 3 m   en C-1\nA2  −40 dBm  ≈ 1 m   en C-3\nA3  −70 dBm  ≈ 6 m   en I-4\n\n    1   2   3   4\nC  A1   ·  A2   ?\nI   ·   ·   ·  A3\n\nA1 y A2 están a 2 m. La cuerda de 3 m y la de 1 m se tocan en un solo punto: la celda siguiente a A2. A3, seis metros al norte en la misma columna, llega a ese mismo punto.\n\n%s" % [MAC, state]


static func _rope_list(marked: Dictionary) -> String:
	var names: PackedStringArray = PackedStringArray()
	for rope in ["A1", "A2", "A3"]:
		if marked.has(rope):
			names.append(rope)
	return "ninguna" if names.is_empty() else ", ".join(names)


static func _try_cell(main: Node, cell: String) -> void:
	var message := _act(main, "rssi", {"cell": cell})
	if not message.is_empty():
		main.ui.toast(message)
		return
	var cart := _office(main).get_node_or_null("CleaningCart")
	if cart:
		cart.set_meta("solved", true)
	_refresh(main)
	main.ui.show_card("CARRO DE ASEO", "Las cuerdas se cruzan en C-4.\n\nEl doble fondo, a la altura de la cintura, está abierto. La tablet está ahí. Los sellos pasan a la mesa.\n\nLa MAC ausente era el equipo extraño.")
	main.ui.add_button("Cerrar", main.ui.close)


static func _open_seal(main: Node, sample: String) -> void:
	var ui = main.get("ui")
	ui.show_card("SELLOS", _seal_body(sample))
	var choices := ["A", "B", "C"] if sample == "sheet" or sample == "" else [sample]
	for choice in choices:
		ui.add_button("Descartar %s" % choice, _try_discard.bind(main, choice))
	ui.add_button("Cerrar", ui.close)


static func _seal_body(sample: String) -> String:
	var focus := ""
	if sample == "A" or sample == "B":
		focus = "\n\n%s hallada y oficial: %s. Coinciden." % [sample, OFFICIAL]
	elif sample == "C":
		focus = "\n\nC hallada: %s.\nC oficial: %s.\nNo coinciden." % [FOUND_C, OFFICIAL]
	return "Hoja oficial: %s\n\nA hallada: %s\nB hallada: %s\nC hallada: %s%s\n\nSe descarta la que no es el sello oficial." % [OFFICIAL, OFFICIAL, OFFICIAL, FOUND_C, focus]


static func _try_discard(main: Node, sample: String) -> void:
	var message := _act(main, "hash", {"discard": sample})
	if not message.is_empty():
		main.ui.toast(message)
		return
	var card := _office(main).get_node_or_null("HashC")
	if card:
		card.set_meta("discarded", true)
	_refresh(main)
	main.ui.show_card("HUELLA DESCARTADA", "C no coincide: hallada %s, oficial %s. A y B siguen siendo el sello oficial.\n\nSi la huella no coincide, el archivo cambió. El interruptor ya puede usarse." % [FOUND_C, OFFICIAL])
	main.ui.add_button("Cerrar", main.ui.close)


static func _press_kill(main: Node, target: OfficeInteractable) -> void:
	var office := _office(main)
	var panel := office.get_node_or_null("KillSwitch") if office else null
	if panel == null:
		return
	var ending_now := _ending(main.get("session"))
	var board := office.get_node_or_null("Debrief")
	if _is_terminal(ending_now) and board and not bool(board.get_meta("auto_opened", false)):
		_open_debrief(main)
		return
	var digit := str(target.get_meta("digit", ""))
	if digit == "":
		var ui = main.get("ui")
		ui.show_card("KILL SWITCH", "Marca cuatro dígitos en los botones grandes. No hay casilla de texto.\n\nLlevas: %s" % _entry_text(panel))
		ui.add_button("Cerrar", ui.close)
		return
	if digit == "clear":
		panel.set_meta("entry", "")
		_set_display(office, "")
		main.ui.toast("Teclado limpio.")
		return
	var entry := str(panel.get_meta("entry", "")) + digit
	if entry.length() < 4:
		panel.set_meta("entry", entry)
		_set_display(office, entry)
		main.ui.toast(entry)
		return
	panel.set_meta("entry", "")
	_set_display(office, entry)
	_submit_kill(main, entry)


static func _submit_kill(main: Node, code: String) -> void:
	var before := _ending(main.get("session"))
	var message := _act(main, "kill", {"code": code})
	var after := _ending(main.get("session"))
	if message.is_empty() and after == "victory":
		message = str(ENDING_COPY.get("victory", ""))
		var status := _office(main).get_node_or_null("LabStatus")
		if status:
			status.set_meta("won", true)
	_refresh(main)
	if _maybe_auto_debrief(main, before, message):
		return
	if not message.is_empty():
		main.ui.show_card("KILL SWITCH", message)
		main.ui.add_button("Cerrar", main.ui.close)
		return
	main.ui.toast("El interruptor no acepta ese código.")


static func _maybe_auto_debrief(main: Node, before: String, lead: String) -> bool:
	var after := _ending(main.get("session"))
	if not _is_terminal(after):
		return false
	var board := _office(main).get_node_or_null("Debrief")
	var seen := board and bool(board.get_meta("auto_opened", false))
	if seen and before == after:
		return false
	_open_debrief(main, lead)
	return true


static func _open_debrief(main: Node, lead := "") -> void:
	var ending := _ending(main.get("session"))
	var ui = main.get("ui")
	if not _is_terminal(ending):
		ui.show_card("CIERRE", "El cierre se abre con la victoria, con el tiempo agotado o con la salida de emergencia. La pregunta de mañana no es un candado, y la segunda persona del parte tampoco.")
		ui.add_button("Cerrar", ui.close)
		return
	var board := _office(main).get_node_or_null("Debrief")
	if board:
		board.set_meta("auto_opened", true)
	ui.show_card("CIERRE · DIEZ LECCIONES", _debrief_body(ending, lead))
	ui.add_button("Mañana leeré el dominio entero antes de fiarme de un aviso", _keep_note.bind(main, "Mañana leeré el dominio entero antes de fiarme de un aviso."))
	ui.add_button("Mañana no armaré una clave con lo que está a la vista", _keep_note.bind(main, "Mañana no armaré una clave con lo que está a la vista."))
	ui.add_button("Mañana compararé la huella antes de confiar en un archivo", _keep_note.bind(main, "Mañana compararé la huella antes de confiar en un archivo."))
	ui.add_button("Dejar en blanco", _keep_note.bind(main, ""))


static func _debrief_body(ending: String, lead: String) -> String:
	var lines := PackedStringArray()
	var copy := str(ENDING_COPY.get(ending, ""))
	lines.append(copy)
	if lead != "" and lead != copy:
		lines.append(lead)
	if ending == "victory":
		lines.append("Dentro de la ficción, los ascensores vuelven a una parada segura. La salida de emergencia sigue abierta.")
	lines.append("V. Soto entró a la plantilla de aseo meses antes. A las 20:03 su tarjeta personal fue denegada. A las 03:14 la cuenta aseo_turno_noche ya tenía ROOT en RACK-03.")
	for index in LESSONS.size():
		lines.append("%d. %s" % [index + 1, LESSONS[index]])
	lines.append("Frase opcional. El parte de aseo de esa noche tenía dos personas. Solo una cuenta obtuvo ROOT. No tiene nombre que descubrir y no abre nada.")
	lines.append("¿Qué vas a cambiar mañana?")
	return "\n\n".join(lines)


static func _keep_note(main: Node, text: String) -> void:
	var session = main.get("session")
	if session:
		for info in session.get_property_list():
			if str(info.get("name", "")) == "note":
				session.set("note", text)
				break
	if text.is_empty():
		main.ui.toast("Queda en blanco. No se corrige y no cierra nada.")
	else:
		main.ui.toast("Anotado para mañana. No se corrige y no cierra nada.")
	main.ui.close()


static func _open_manual(main: Node, id: String) -> void:
	var session = main.get("session")
	var body := str(MANUALS[id])
	if session and session.has_method("puzzle"):
		var data = session.puzzle(id)
		if data is Dictionary and str(data.get("manual", "")) != "":
			body = str(data.manual)
	if session and session.has_method("read_manual"):
		session.read_manual(id)
		if main.has_method("_commit"):
			main._commit()
	main.ui.show_card("MANUAL · %s" % id.to_upper(), body)
	main.ui.add_button("Cerrar", main.ui.close)


static func _act(main: Node, id: String, payload: Dictionary) -> String:
	var session = main.get("session")
	if session == null or not session.has_method("act"):
		return "La sesión todavía no acepta esta acción."
	var result = session.act(id, payload)
	if main.has_method("_commit"):
		main._commit()
	if result == null:
		return ""
	return str(result)


static func _refresh(main: Node) -> void:
	_sync(_office(main), main.get("session") if main else null)


static func _sync(office: Node3D, session: Object) -> void:
	if office == null:
		return
	var flags := _flags(session)
	var reveal := office.get_node_or_null("ConfigNote/ConfigReveal") as Label3D
	if reveal:
		var known := str(flags.get("kill_code", "")) != "" or _marked(office, "ConfigNote")
		reveal.visible = known
		if known:
			reveal.text = "%s\nMAC termina en :4F" % str(flags.get("kill_code", KILL_CODE))
	var badge := office.get_node_or_null("BadgeVSoto") as OfficeInteractable
	if badge and (bool(flags.get("pixel_known", false)) or _marked(office, "BadgeVSoto")):
		badge.tint(MINT)
	var open := bool(flags.get("tablet_found", false)) or _node_meta(office, "CleaningCart", "solved")
	_sync_cart(office, open)
	_show_hashes(office, open)
	var discarded := office.get_node_or_null("HashC") as OfficeInteractable
	if discarded and (bool(flags.get("hash_discarded", false)) or bool(discarded.get_meta("discarded", false))):
		discarded.tint(RED)
	var status := office.get_node_or_null("LabStatus")
	if status and (_ending(session) == "victory" or bool(status.get_meta("won", false))):
		_paint_victory(status)


static func _sync_cart(office: Node3D, open: bool) -> void:
	var cart := office.get_node_or_null("CleaningCart") as Node3D
	if cart == null:
		return
	if cart.has_meta("open") and bool(cart.get_meta("open")) == open:
		var tablet_now := cart.get_node_or_null("CartDrawer/TabletLabel") as Node3D
		if tablet_now:
			tablet_now.visible = open
		return
	cart.set_meta("open", open)
	var drawer := cart.get_node_or_null("CartDrawer") as Node3D
	var tablet := cart.get_node_or_null("CartDrawer/TabletLabel") as Node3D
	if tablet:
		tablet.visible = open
	if drawer == null:
		return
	var dest := -0.34 if open else 0.0
	if drawer.is_inside_tree() and absf(drawer.position.z - dest) > 0.02:
		var tween := drawer.create_tween()
		tween.tween_property(drawer, "position:z", dest, 0.35)
	else:
		drawer.position.z = dest


static func _show_hashes(office: Node3D, show: bool) -> void:
	for node_name in ["HashA", "HashB", "HashC", "HashSheet"]:
		var node := office.get_node_or_null(node_name) as Node3D
		if node == null:
			continue
		node.visible = show
		if node is CollisionObject3D:
			(node as CollisionObject3D).collision_layer = 1 if show else 0


static func _paint_victory(status: Node) -> void:
	if status.get_meta("victory_lit", false):
		return
	status.set_meta("victory_lit", true)
	var mesh := status.get_node_or_null("Mesh") as MeshInstance3D
	if mesh and mesh.material_override:
		var material := mesh.material_override.duplicate() as StandardMaterial3D
		material.albedo_color = Color("3dba6e")
		mesh.material_override = material
	var glow := status.get_node_or_null("StatusGlow") as OmniLight3D
	if glow:
		glow.light_color = Color("3dba6e")
		glow.light_energy = 1.35
		glow.omni_range = 8.0


static func _set_display(office: Node3D, entry: String) -> void:
	var display := office.get_node_or_null("KillSwitch/KillDisplay") as Label3D
	if display:
		display.text = entry if entry != "" else "----"


static func _entry_text(panel: Node) -> String:
	var entry := str(panel.get_meta("entry", ""))
	return entry if entry != "" else "nada"


static func _marked(office: Node3D, node_name: String) -> bool:
	return _node_meta(office, node_name, "solved")


static func _node_meta(office: Node3D, node_name: String, key: String) -> bool:
	if office == null:
		return false
	var node := office.get_node_or_null(node_name)
	return node != null and bool(node.get_meta(key, false))


static func _flags(session: Object) -> Dictionary:
	if session == null:
		return {}
	var direct = session.get("flags")
	if direct is Dictionary:
		return direct
	var state = session.get("state")
	if state is Dictionary and state.get("flags") is Dictionary:
		return state.get("flags")
	return {}


static func _ending(session: Object) -> String:
	if session == null:
		return ""
	var direct = session.get("ending")
	if direct is String:
		return direct
	var state = session.get("state")
	if state is Dictionary:
		return str(state.get("ending", ""))
	return ""


static func _is_terminal(ending: String) -> bool:
	return ending == "victory" or ending == "timeout" or ending == "safety"


static func _office(main: Node) -> Node3D:
	if main == null:
		return null
	return main.get("office") as Node3D
