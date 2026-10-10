extends SceneTree

var checks := 0
var failures := 0
var menu_presses := 0
var clicks := 0


func expect(condition: bool, message: String) -> void:
	checks += 1
	if not condition:
		failures += 1
		push_error(message)


func _initialize() -> void:
	call_deferred("run")


func run() -> void:
	var family := XrRuntime.new()
	var produced := family.create_player()
	expect(produced is QuestPlayer and family.uses_spatial_ui(), "La fábrica XR entrega el jugador de visor y la UI espacial")
	produced.free()
	var stage := Node3D.new()
	root.add_child(stage)
	var floor_body := box(stage, Vector3(20, 0.2, 20), Vector3(0, -0.1, 0), 2)
	var rig := QuestPlayer.new()
	stage.add_child(rig)
	rig.set_physics_process(false)
	rig.camera.position = Vector3(0.6, 1.62, 0.4)
	var aim := Node3D.new()
	aim.position = Vector3(0, 1.4, 0)
	stage.add_child(aim)
	await physics_frame
	await physics_frame

	var target := rig._teleport_target(aim)
	expect(target.valid and target.hit, "El arco encuentra suelo libre")
	var before := rig.global_position
	expect(not rig.teleport(aim) and rig.global_position == before, "Un menú bloquea teletransporte")
	rig.set_blocked(false)
	expect(rig.teleport(aim), "Un destino libre permite teletransporte")
	var feet := Vector3(rig.camera.global_position.x, target.position.y, rig.camera.global_position.z)
	expect(feet.distance_to(target.position) < 0.001, "El destino compensa el desplazamiento físico de la cabeza")
	var head_before := rig.camera.global_position
	rig._snap_turn(1.0)
	expect(rig.camera.global_position.distance_to(head_before) < 0.001, "Giro de 30° conserva posición de la cabeza")
	expect(is_equal_approx(rig.rotation.y, -PI / 6.0), "Giro por pasos de 30°")

	var wall := box(stage, Vector3(6, 3, 0.2), Vector3(0, 1.5, -1), 1)
	await physics_frame
	await physics_frame
	target = rig._teleport_target(aim)
	expect(target.hit and not target.valid, "Un muro bloquea el arco aunque haya suelo detrás")
	before = rig.global_position
	expect(not rig.teleport(aim) and rig.global_position == before, "El jugador no atraviesa muros")
	expect(not rig._has_clearance(Vector3(0, 0, -1)), "Un destino sin espacio para el cuerpo se rechaza")
	wall.free()
	floor_body.collision_layer = 0
	await physics_frame
	await physics_frame
	expect(not rig._teleport_target(aim).valid, "No se teletransporta al vacío")
	floor_body.collision_layer = 2
	rig.teleport_aiming = true
	rig.teleport_marker.show()
	rig.set_blocked(true)
	expect(not rig.teleport_aiming and not rig.teleport_marker.visible, "Pausa cancela el destino pendiente")
	rig.pause_requested.connect(func() -> void: menu_presses += 1)
	rig._button("by_button", rig.hands[1])
	rig._button("menu_button", rig.hands[0])
	expect(menu_presses == 2, "B derecho y menú izquierdo abren pausa")

	# Trackers simulados: se ejercitan los eventos reales de pulsar y soltar.
	var tracker := XRControllerTracker.new()
	tracker.name = "left_hand"
	tracker.type = XRServer.TRACKER_CONTROLLER
	tracker.set_pose("grip", aim.transform, Vector3.ZERO, Vector3.ZERO, XRPose.XR_TRACKING_CONFIDENCE_HIGH)
	tracker.set_pose("aim", aim.transform, Vector3.ZERO, Vector3.ZERO, XRPose.XR_TRACKING_CONFIDENCE_HIGH)
	XRServer.add_tracker(tracker)
	rig.transform = Transform3D.IDENTITY
	rig.set_blocked(false)
	await physics_frame
	before = rig.global_position
	tracker.set_input("primary_click", true)
	expect(rig.teleport_aiming and rig.global_position == before, "Pulsar el stick apunta sin mover al jugador")
	rig._update_teleport_preview()
	expect(rig.teleport_valid and rig.teleport_arc.visible and rig.teleport_marker.visible, "El destino y el arco se muestran antes de confirmar")
	tracker.set_input("primary_click", false)
	expect(not rig.teleport_aiming and rig.global_position.distance_to(before) > 0.1, "Soltar confirma el destino")
	rig.transform = Transform3D.IDENTITY
	tracker.set_input("primary_click", true)
	tracker.invalidate_pose("aim")
	rig._update_teleport_preview()
	before = rig.global_position
	tracker.set_input("primary_click", false)
	expect(not rig.teleport_aiming and not rig.teleport_marker.visible and rig.global_position == before, "Perder tracking cancela el salto pendiente")
	XRServer.remove_tracker(tracker)
	stage.free()
	await _expect_vr_body()
	await _expect_grab_releases()

	var main = load("res://Game/Levels/main.tscn").instantiate()
	root.add_child(main)
	main.allow_save = false
	main.session.reset()
	# La UI VR y el juego se prueban sin abrir un runtime ni escribir el guardado.
	main.player.free()
	rig = QuestPlayer.new()
	main.add_child(rig)
	rig.set_physics_process(false)
	rig.camera.position.y = 1.62
	main.player = rig
	main.xr_active = true
	main.spatial_screen = main.ui.make_spatial(main)
	main.ui.close()
	expect(not main.ui.crosshair.visible and main.ui.controls_label.text.contains("Trigger"), "VR muestra controles de los mandos")
	main._terminal()
	expect(main.ui.content.find_children("*", "LineEdit", true, false).is_empty(), "Terminal VR no exige teclado físico")
	var flags_before := JSON.stringify(main.session.flags)
	var output: Label = main.ui.content.get_child(7)
	for command in ["help", "scan", "map", "notes", "clear"]:
		var found := false
		for child in main.ui.content.get_children():
			if child is Button and child.text == command:
				child.pressed.emit()
				found = true
		expect(found and output.text == main._terminal_command(command), "Botón VR ejecuta " + command)
	expect(JSON.stringify(main.session.flags) == flags_before, "Consultar terminal VR conserva progreso")
	main.session.paused = false
	main._xr_suspended()
	var elapsed: float = main.session.elapsed
	main.session.tick(10.0)
	expect(main.session.paused and main.ui.modal_open and main.session.elapsed == elapsed, "Al quitarse el visor se pausa el reloj")
	main._close_menu()
	expect(not main.session.paused and not rig.blocked, "Continuar reanuda la operación")

	# Conversión mundo->viewport, pulsación real de un botón y scroll de tarjetas largas.
	main.ui.show_card("PRUEBA", "")
	var button: Button = main.ui.add_button("Pulsar", func() -> void: clicks += 1)
	for index in 20:
		main.ui.add_button("Opción %d" % index, func() -> void: pass)
	await process_frame
	await process_frame
	await process_frame
	var pixel := button.get_global_rect().get_center()
	var screen: Node3D = main.spatial_screen
	var local := Vector3((pixel.x / EscapeUI.SPATIAL_VIEWPORT.x - 0.5) * EscapeUI.SPATIAL_QUAD.x, (0.5 - pixel.y / EscapeUI.SPATIAL_VIEWPORT.y) * EscapeUI.SPATIAL_QUAD.y, 0)
	main.ui.pointer(screen.to_global(local), screen, true)
	await process_frame
	expect(clicks == 1, "El rayo del mando pulsa un botón de la UI espacial")
	main.ui.scroll_spatial(400)
	expect(main.ui.scroll.scroll_vertical > 0, "El stick permite recorrer tarjetas largas")
	main.ui.show_card("OTRA TARJETA", "")
	expect(main.ui.scroll.scroll_vertical == 0, "Una tarjeta nueva comienza por su título")
	main.free()
	for _frame in 4:
		await process_frame
	print("XR: %d checks / %d failures" % [checks, failures])
	quit(0 if failures == 0 else 1)


func _expect_vr_body() -> void:
	var pit := Node3D.new()
	pit.name = "Office"
	root.add_child(pit)
	box(pit, Vector3(30, 0.2, 30), Vector3(0, -0.1, 0), 2)
	var wall := box(pit, Vector3(0.2, 3, 4), Vector3(0, 1.5, 0), 1)
	var rig := QuestPlayer.new()
	pit.add_child(rig)
	rig.set_blocked(false)
	rig.camera.position = Vector3(0, 1.62, 0)
	CondorPhysics.tune(null, rig)
	expect(is_equal_approx(rig.body_radius, 0.25) and is_equal_approx(rig.body_height, 1.7), "La cápsula VR usa el mismo cuerpo que PC")
	for _frame in 4:
		await physics_frame
	expect(absf(rig.camera.global_position.x) > 0.3, "El cuerpo del visor sale del muro")
	wall.free()
	rig.global_position = Vector3(0, -6, 0)
	await physics_frame
	expect(rig.global_position.distance_to(Vector3(0, rig.global_position.y, 4.4)) < 0.2 and rig.global_position.y > -1.0, "Caer del mapa devuelve al visor a la entrada")
	rig.global_position = Vector3(-1, 0.1, 0)
	rig.camera.position = Vector3(0, 1.62, 0)
	rig.hands[1].position = Vector3(0, 1.2, 0)
	var barrier := box(pit, Vector3(0.2, 2, 4), Vector3(0.45, 1, 0), 1)
	var item := OfficeInteractable.new()
	item.grabbable = true
	item.position = Vector3(-1, 1.2, 0)
	pit.add_child(item)
	var shape := CollisionShape3D.new()
	var geometry := BoxShape3D.new()
	geometry.size = Vector3(0.2, 0.2, 0.2)
	shape.shape = geometry
	item.add_child(shape)
	await physics_frame
	item.grab(rig.hands[1])
	var released_at := item.global_position.x
	item.release(Vector3(4, 0.2, 0))
	for _frame in 8:
		await physics_frame
	expect(item.global_position.x > released_at + 0.05, "Soltar conserva el impulso de la mano")
	for _frame in 24:
		await physics_frame
	expect(item.global_position.x > 0.15 and item.global_position.x < barrier.position.x, "El objeto se detiene contra el muro y no lo atraviesa")
	pit.free()


func _expect_grab_releases() -> void:
	var office := Node3D.new()
	office.name = "Office"
	root.add_child(office)
	box(office, Vector3(30, 0.2, 30), Vector3(0, -0.1, 0), 2)
	var item := _carry(office, Vector3(1.2, 1.0, 0.4))
	var player := DesktopPlayer.new()
	root.add_child(player)
	player.set_physics_process(false)
	player.set_process_unhandled_input(false)
	player.position = Vector3(0, 0, 0)
	player.set_blocked(false)
	await physics_frame
	player.take(item)
	player.position = Vector3(6, 0, 5)
	for _frame in 3:
		await physics_frame
	expect(item.global_position.distance_to(player.hand.global_position) < 0.25, "En la mano, el objeto no se va por el mapa")
	expect(item.get_parent() == office, "Agarrar no cuelga el cuerpo del jugador")
	var dropped_at := item.global_position
	player.drop()
	expect(player.held == null and not item.is_held, "Soltar en PC despega el objeto")
	expect(item.get_parent() == office, "Soltar lo deja en la oficina")
	for _frame in 40:
		await physics_frame
	expect(not item.loose and item.global_position.distance_to(dropped_at) < 4.0, "Tras caer se queda cerca y quieto")
	var plain := StaticBody3D.new()
	plain.name = "Silla"
	plain.position = Vector3(-2, 0.6, 1)
	office.add_child(plain)
	plain.set_script(load("res://Game/Shared/interactable.gd"))
	var carried := plain as OfficeInteractable
	carried.grabbable = true
	var shape := CollisionShape3D.new()
	var geometry := BoxShape3D.new()
	geometry.size = Vector3(0.4, 0.4, 0.4)
	shape.shape = geometry
	carried.add_child(shape)
	await physics_frame
	player.take(carried)
	player.position = Vector3(-4, 0, -3)
	await physics_frame
	await physics_frame
	expect(carried.global_position.distance_to(player.hand.global_position) < 0.25, "Un prop sin _ready también se queda en la mano")
	player.drop()
	expect(not carried.is_held and carried.get_parent() == office, "Ese prop también se suelta")
	var rig := QuestPlayer.new()
	office.add_child(rig)
	rig.set_blocked(false)
	rig.camera.position = Vector3(0, 1.6, 0)
	var token := _carry(office, Vector3(0, 1, 2))
	await physics_frame
	rig._button("grip_click", rig.hands[1])
	token.grab(rig.hands[1])
	rig.held = token
	rig.holding_hand = rig.hands[1]
	rig.global_position = Vector3(8, 0, -6)
	await physics_frame
	expect(token.is_held and token.get_parent() == office, "En VR el objeto sigue suelto del cuerpo")
	rig._button("grip_click", rig.hands[1])
	expect(rig.held == null and not token.is_held, "Pulsar el grip otra vez suelta y no vuelve a tomar")
	expect(token.get_parent() == office, "El visor deja el objeto en la oficina")
	office.free()
	player.free()


func _carry(office: Node3D, at: Vector3) -> OfficeInteractable:
	var item := OfficeInteractable.new()
	item.grabbable = true
	item.position = at
	office.add_child(item)
	var shape := CollisionShape3D.new()
	var geometry := BoxShape3D.new()
	geometry.size = Vector3(0.2, 0.12, 0.28)
	shape.shape = geometry
	item.add_child(shape)
	return item


func box(parent: Node3D, size: Vector3, position: Vector3, layer: int) -> StaticBody3D:
	var body := StaticBody3D.new()
	body.collision_layer = layer
	body.collision_mask = 0
	body.position = position
	var shape := CollisionShape3D.new()
	var geometry := BoxShape3D.new()
	geometry.size = size
	shape.shape = geometry
	body.add_child(shape)
	parent.add_child(body)
	return body
