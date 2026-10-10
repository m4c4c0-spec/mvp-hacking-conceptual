extends Node3D

const Session = preload("res://Game/Features/Quests/session.gd")
const Terminal = preload("res://Game/Features/Quests/terminal.gd")
const UI = preload("res://Game/Shared/game_ui.gd")
const Guide = preload("res://Game/Levels/office_guide.gd")
const Runtimes = preload("res://Game/Factories/runtime_factory.gd")
const Rooms = preload("res://Game/Factories/room_factory.gd")
const GREEN := Color("55b684")
const RED := Color("bb5150")
const ENTRANCE := Vector3(0, 0.1, 4.4)

var session: EscapeSession
var ui: EscapeUI
var runtime: RuntimeProfile
var player: Node3D
var office: Node3D
var xr_active := false
var spatial_screen: Node3D
var autosave := 0.0
var allow_save := true
var archive_open := false
var clock_label: Label3D
var room_scripts: Array = []

func _ready() -> void:
	_setup_input()
	session = Session.new()
	if session.puzzles().is_empty():
		push_error(session.last_error if not session.last_error.is_empty() else "No se pudo leer el catálogo de la operación.")
		return
	session.load_save()
	allow_save = session.last_error.is_empty()
	office = $Office
	ui = UI.new()
	add_child(ui)
	QualityProfile.apply(get_tree())
	_create_player()
	# Las salas y la física se cargan solo si el archivo ya existe: un preload rompería el arranque.
	_attach_rooms()
	_tune_physics()
	_arm_props()
	_dress_visuals()
	ui.modal_changed.connect(_modal_changed)
	player.used.connect(use_object)
	player.pause_requested.connect(_pause)
	_refresh_world()
	if _fresh_operation():
		session.paused = true
		_briefing()
	elif session.ending != "":
		_show_debrief()
	elif session.paused:
		_pause(true)
	if not session.last_error.is_empty():
		ui.toast(session.last_error)
	get_tree().auto_accept_quit = false
	call_deferred("_check_render_budget")

func _check_render_budget() -> void:
	if office:
		var report := RenderBudget.measure(office)
		if not report.ok:
			push_error("Presupuesto 8 GB: %s" % str(report))
	RenderBudget.warn_if_heavy()

func _setup_input() -> void:
	var bindings := {"forward": [KEY_W, KEY_UP], "backward": [KEY_S, KEY_DOWN], "left": [KEY_A, KEY_LEFT], "right": [KEY_D, KEY_RIGHT], "use": [KEY_E], "grab": [KEY_G], "sprint": [KEY_SHIFT], "pause": [KEY_ESCAPE]}
	for action in bindings:
		if InputMap.has_action(action):
			continue
		InputMap.add_action(action)
		for code in bindings[action]:
			var event := InputEventKey.new()
			event.physical_keycode = code
			InputMap.action_add_event(action, event)
	var click := InputEventMouseButton.new()
	click.button_index = MOUSE_BUTTON_LEFT
	InputMap.action_add_event("use", click)

func _create_player() -> void:
	runtime = Runtimes.create()
	runtime.prepare(self)
	xr_active = runtime.uses_spatial_ui()
	player = runtime.create_player()
	player.position = ENTRANCE
	add_child(player)
	spatial_screen = runtime.mount(self, player, ui, _xr_suspended, _position_spatial_ui)
	if not xr_active and not runtime.notice.is_empty():
		ui.toast(runtime.notice)


func _xr_suspended() -> void:
	# El reloj se pausa al abrir el menú del visor o quitarse el casco.
	if player:
		player.drop()
	if session.ending.is_empty():
		_pause(true)
	else:
		_show_debrief()
	_save()

func _attach_rooms() -> void:
	room_scripts = Rooms.build(office)

func _tune_physics() -> void:
	Rooms.tune(office, player)

func _arm_props() -> void:
	Rooms.arm(office)

func _dress_visuals() -> void:
	Rooms.dress(office)

func _modal_changed(open: bool) -> void:
	if player:
		player.set_blocked(open)
	if not open and session:
		session.manual_open = false
	if xr_active and spatial_screen:
		spatial_screen.get_node("PointerTarget").collision_layer = 8 if open else 0
	if xr_active and open:
		_position_spatial_ui()

func _position_spatial_ui() -> void:
	var camera: Camera3D = player.get_camera()
	var forward := -camera.global_basis.z
	forward.y = 0
	if forward.length_squared() < 0.01:
		forward = Vector3.FORWARD
	forward = forward.normalized()
	spatial_screen.global_position = camera.global_position + forward * 1.7
	spatial_screen.look_at(camera.global_position, Vector3.UP, true)

func _process(delta: float) -> void:
	if session == null or ui == null or player == null:
		return
	var event := session.tick(delta)
	ui.hint.text = _clock() + "\n" + session.objective()
	_paint_clock()
	ui.focus_label.text = player.focused.prompt if player.focused else ""
	if event != "":
		ui.toast(event)
		if session.ending == "timeout":
			_show_debrief()
	elif session.pending_radio != "":
		ui.toast(session.pending_radio)
		session.pending_radio = ""
	autosave += delta
	if autosave > 20.0:
		autosave = 0.0
		_save()
	if xr_active and not ui.modal_open:
		_position_spatial_ui()

func _notification(what: int) -> void:
	if session == null:
		return
	if what == NOTIFICATION_WM_CLOSE_REQUEST:
		_save()
		get_tree().quit()
	if what == NOTIFICATION_APPLICATION_FOCUS_OUT:
		_save()
		if ui and player and not xr_active:
			_pause(true)

func _save() -> void:
	if allow_save and session and not session.save():
		if ui:
			ui.toast(session.last_error)

func _commit() -> void:
	_refresh_world()
	_save()

func _clock() -> String:
	var left := int(floor(max(0.0, 3600.0 - session.elapsed)))
	return "%02d:%02d" % [int(left / 60.0), left % 60]

func _paint_clock() -> void:
	if office == null:
		return
	if clock_label == null or not is_instance_valid(clock_label):
		var found := office.find_child("ClockLabel", true, false)
		clock_label = found if found is Label3D else null
	if clock_label:
		clock_label.text = _clock()

func _fresh_operation() -> bool:
	return session.elapsed == 0.0 and session.flags.is_empty() and session.ending.is_empty() and session.note.is_empty() and session.manuals_read.is_empty() and session.applied.is_empty() and session.kill_attempts == 0

func _briefing() -> void:
	session.paused = true
	var title := str(session.catalog.get("title", "Operación Cóndor Silencioso"))
	var subtitle := str(session.catalog.get("subtitle", "El enemigo ya tiene la llave."))
	var body := subtitle + "\n\nUnidad de Ciberdelitos, Santiago. A las 22:00 una falsa prueba de emergencia cerró el edificio. Alguien con root copia la lista de informantes.\n\nEn Operación tienes 60 minutos. En Aprendizaje el reloj se ve y no hay derrota. La salida de emergencia siempre está abierta: la palabra es CÓNDOR LIBRE.\n\nNo hay examen. Cada sala enseña un concepto y deja un cambio en el lugar."
	ui.show_card(title, body)
	if not session.migration_notice.is_empty():
		ui.add_text(session.migration_notice)
	ui.add_button("Entrar a la oficina", _close_menu)
	ui.add_button("Controles", _help)
	ui.add_button("Créditos y licencias", _credits)

func _close_menu() -> void:
	if session.ending.is_empty():
		session.paused = false
	ui.close()

func _pause(force := false) -> void:
	if ui.modal_open and not force:
		ui.close()
		if session.ending.is_empty():
			session.paused = false
		return
	session.paused = true
	var mode_name := "Operación" if session.mode == "operacion" else "Aprendizaje"
	ui.show_card("PAUSA", "Modo actual: %s.\nEn Operación, a los 60 minutos la lista sale del edificio. En Aprendizaje el reloj se ve y no hay derrota.\nCÓNDOR LIBRE detiene la operación sin castigo." % mode_name)
	ui.add_button("Continuar", _close_menu)
	ui.add_button("CÓNDOR LIBRE", _safety)
	ui.add_button("Modo Operación / Aprendizaje", _toggle_mode)
	ui.add_button("Cuaderno", func() -> void: _glossary(true))
	ui.add_button("Volver a la entrada", _return_to_entrance)
	ui.add_button("Reiniciar partida…", _confirm_reset)
	ui.add_button("Guardar y salir", _save_and_quit)

func _toggle_mode() -> void:
	session.set_mode("aprendizaje" if session.mode == "operacion" else "operacion")
	_pause(true)

func _return_to_entrance() -> void:
	if player:
		player.drop()
		_return_player_to_entrance()
	_close_menu()


func _return_player_to_entrance() -> void:
	if player is QuestPlayer:
		player.move_to_floor(ENTRANCE - Vector3.UP * 0.03)
	else:
		player.position = ENTRANCE

func _save_and_quit() -> void:
	_save()
	get_tree().quit()

func _confirm_reset() -> void:
	ui.show_card("¿REINICIAR LA OPERACIÓN?", "Se borra el avance de esta operación. El guardado anterior se conserva como .bak.")
	ui.add_button("Cancelar", func() -> void: _pause(true))
	ui.add_button("Sí, empezar de nuevo", _do_reset)

func _do_reset() -> void:
	if player:
		player.drop()
		_return_player_to_entrance()
	session.reset()
	allow_save = true
	session.paused = true
	archive_open = false
	var hinge := office.get_node_or_null("ArchiveHinge")
	if hinge:
		hinge.rotation.y = 0
	_commit()
	_briefing()

func _help() -> void:
	ui.show_card("CONTROLES", "PC: WASD caminar, mouse mirar, E o clic usar, G tomar y soltar, Shift correr, Esc menú.\n\nG toma el objeto que miras. Al soltarlo, cae y se queda donde toca el suelo o un mueble. Puertas, muros y suelo no se mueven.\n\nVR experimental: trigger derecho usar, grip derecho sostener y soltar. Al soltar, el objeto sigue el movimiento de la mano y se queda donde cae. Mantén pulsado el click del stick izquierdo para apuntar el arco; verde indica suelo libre y rojo un destino bloqueado. Suelta para teletransportarte. Stick derecho: giro de 30°; arriba/abajo desplaza los menús. B derecho o menú izquierdo abre pausa.\n\nLa radio pide una pista. El cuaderno muestra solo conceptos ya aplicados. La terminal acepta help, scan, map, notes y clear, con botones en VR.\nCÓNDOR LIBRE detiene la operación sin castigo.")
	ui.add_button("Volver", _briefing)

func _credits() -> void:
	ui.show_card("CRÉDITOS Y LICENCIAS", "Operación Cóndor Silencioso · práctica de respuesta a incidentes.\n\nEsta versión utiliza Godot Engine bajo licencia MIT. Avisos completos incluidos en ThirdParty/godot/GODOT_COPYRIGHT.txt.\nhttps://godotengine.org/license/")
	ui.add_button("Volver", _briefing)

func _safety() -> void:
	session.safety_stop()
	_show_debrief()

func _show_debrief() -> void:
	var pack: Dictionary = session.catalog.get("closing", {})
	var lines: PackedStringArray = [session.objective()]
	var twist := str(pack.get("twist", ""))
	if not twist.is_empty():
		lines.append(twist)
	var lessons: Array = pack.get("lessons", [])
	for index in range(lessons.size()):
		lines.append("%d. %s" % [index + 1, str(lessons[index])])
	var optional := str(pack.get("optional", ""))
	if not optional.is_empty():
		lines.append(optional)
	lines.append(str(pack.get("question", "¿Qué vas a cambiar mañana?")))
	var title := "CIERRE"
	if session.ending == "safety":
		title = "CÓNDOR LIBRE"
	elif session.ending == "timeout":
		title = "TIEMPO AGOTADO"
	elif session.ending == "victory":
		title = "COPIA DETENIDA"
	ui.show_card(title, "\n\n".join(lines))
	ui.add_button("Cerrar", func() -> void:
		if session.ending != "":
			session.paused = false
		ui.close())

func use_object(target: OfficeInteractable) -> void:
	if _handled_by_room(target):
		return
	match target.action:
		"guide":
			_consult_guide(int(target.argument))
		"terminal":
			_terminal()
		"glossary":
			_glossary()
		"radio":
			_radio()
		"safety":
			_safety()
		"manual":
			_manual(target)
		"archive_door":
			_toggle_archive()
		"board", "clue", "tray", "report", "restore":
			ui.toast("Este panel ya no forma parte de la operación.")
		"exit":
			ui.toast("La puerta magnética no responde todavía.")
		_:
			pass

func _handled_by_room(target: OfficeInteractable) -> bool:
	return Rooms.handle(room_scripts, self, target)

func _consult_guide(index: int) -> void:
	if index < 0 or index >= Guide.ROLES.size():
		return
	var role: Dictionary = Guide.ROLES[index]
	var body := "ASISTENCIA GRABADA · %s\n\n%s\n\nTU SIGUIENTE PASO\n%s" % [role.get("action", ""), role.get("lesson", ""), session.objective()]
	ui.show_card("%s · %s" % [role.get("name", "Guía"), role.get("zone", "")], body)
	ui.add_button("Volver a la oficina", ui.close)

func _radio() -> void:
	ui.show_card("RADIO", session.request_hint())
	ui.add_button("Cerrar", ui.close)

func _manual(target: OfficeInteractable) -> void:
	var id := str(target.get_meta("puzzle", ""))
	ui.show_card("MANUAL", session.read_manual(id))
	ui.add_button("Cerrar", func() -> void:
		session.manual_open = false
		ui.close())

func _glossary(from_pause := false) -> void:
	ui.show_card("CUADERNO · CONCEPTOS", "Aquí solo aparecen los conceptos que ya aplicaste al resolver un paso. Leerlos no resuelve la sala.")
	var any := false
	for concept in session.catalog.get("concepts", []):
		if not concept is Dictionary:
			continue
		var id := str(concept.get("id", ""))
		if not session.concept_applied(id):
			continue
		any = true
		ui.add_button(str(concept.get("name", id)), _show_concept.bind(concept, from_pause))
	if not any:
		ui.add_text("Aún no hay conceptos aplicados. Resolver un acertijo los anota.")
	if from_pause:
		ui.add_button("Volver al menú", func() -> void: _pause(true))
	else:
		ui.add_button("Cerrar", _close_menu)

func _show_concept(concept: Dictionary, from_pause: bool) -> void:
	ui.show_card(str(concept.get("name", "")), str(concept.get("definition", "")) + "\n\n" + str(concept.get("defense", "")))
	ui.add_button("Volver al cuaderno", _glossary.bind(from_pause))
	if from_pause:
		ui.add_button("Volver al menú", func() -> void: _pause(true))
	else:
		ui.add_button("Cerrar", _close_menu)

func _terminal() -> void:
	ui.show_card("TERMINAL LOCAL", "Comandos: help, scan, map, notes y clear. No ejecuta shell ni se conecta a redes. No hace falta usarla para terminar.")
	var output := Label.new()
	output.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
	output.add_theme_font_size_override("font_size", EscapeUI.BODY_PX)
	if xr_active:
		for command in ["help", "scan", "map", "notes", "clear"]:
			ui.add_button(command, _run_terminal_button.bind(output, command))
		output.text = _terminal_command("help")
		ui.content.add_child(output)
		ui.add_button("Cerrar", ui.close)
		return
	ui.content.add_child(output)
	var input := LineEdit.new()
	input.placeholder_text = "help"
	input.custom_minimum_size.y = 48
	ui.content.add_child(input)
	var execute := func() -> void:
		output.text = _terminal_command(input.text)
		input.clear()
	input.text_submitted.connect(func(_text: String) -> void: execute.call())
	ui.add_button("Ejecutar", execute)
	ui.add_button("Cerrar", ui.close)
	input.grab_focus()


func _run_terminal_button(output: Label, command: String) -> void:
	output.text = _terminal_command(command)

func _terminal_command(command: String) -> String:
	var outcome: Dictionary = Terminal.execute(session, command)
	if bool(outcome.get("save", false)):
		_save()
	if bool(outcome.get("commit", false)):
		_commit()
	return str(outcome.get("text", ""))

func _toggle_archive() -> void:
	var hinge := office.get_node_or_null("ArchiveHinge")
	if hinge == null:
		return
	archive_open = not archive_open
	create_tween().tween_property(hinge, "rotation:y", -PI / 2.0 if archive_open else 0.0, 0.45)

func _refresh_world() -> void:
	if office == null or session == null:
		return
	_paint_clock()
	var door := office.find_child("ExitDoor", true, false)
	if door is OfficeInteractable:
		door.tint(GREEN if session.flags.get("mfa_open", false) == true else RED)
	var status := office.find_child("LabStatus", true, false)
	if status is OfficeInteractable and session.ending == "victory":
		status.tint(GREEN)
