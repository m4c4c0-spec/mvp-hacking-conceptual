extends SceneTree

var failures := 0
var checks := 0

func expect(condition: bool, message: String) -> void:
	checks += 1
	if not condition:
		failures += 1
		push_error(message)

func _initialize() -> void:
	call_deferred("run")

func run() -> void:
	var main = load("res://Game/Levels/main.tscn").instantiate()
	root.add_child(main)
	main.allow_save = false
	main.session.reset()
	main._refresh_world()
	main.ui.close()
	await physics_frame
	await physics_frame
	expect(main.player is DesktopPlayer, "PC first person por defecto")
	expect(main.runtime is DesktopRuntime and not main.runtime.uses_spatial_ui(), "La fábrica de escritorio no monta la familia XR")
	expect(main.player.get_camera().current, "Cámara primera persona activa")
	var probe_player := RuntimeFactory.create().create_player()
	expect(probe_player is DesktopPlayer, "RuntimeFactory entrega DesktopPlayer sin --xr")
	probe_player.free()
	var probe := InteractableFactory.create("Probe", "manual", 2, "probar", true)
	expect(probe is OfficeInteractable and probe.action == "manual" and probe.argument == 2 and probe.grabbable, "InteractableFactory fija identidad y agarre")
	probe.free()
	var expected_rooms := 0
	for file_name in ["sala1.gd", "sala2.gd", "sala3.gd"]:
		if script_exists(file_name):
			expected_rooms += 1
	expect(main.room_scripts.size() == expected_rooms, "RoomFactory carga las salas presentes")
	var guides: Array[OfficeGuide] = []
	for node in main.office.find_children("*", "", true, false):
		if node is OfficeGuide:
			guides.append(node)
	expect(guides.size() == 5, "Cinco guías")
	expect(RenderBudget.measure(main.office).ok, "Arte dentro del presupuesto PC/Quest")
	for path in ["Floor/Mesh", "Desk/Mesh", "BackWall/Mesh", "ChairSeat/Mesh"]:
		var material: StandardMaterial3D = main.office.get_node(path).material_override
		expect(material.albedo_texture != null, "Acabado con textura visible en " + path)
		expect(material.texture_filter == BaseMaterial3D.TEXTURE_FILTER_LINEAR_WITH_MIPMAPS_ANISOTROPIC, "Filtrado de textura en ángulo: " + path)
		expect(material.uv1_triplanar and material.uv1_triplanar_sharpness > 1, "Proyección definida sin UV ausentes: " + path)
		expect(material.albedo_texture.get_width() <= 1024, "Textura dentro del presupuesto: " + path)
	for guide in guides:
		var target: OfficeInteractable = guide.get_node("Assistance")
		var flags := JSON.stringify(main.session.flags)
		var applied := JSON.stringify(main.session.applied)
		var hints := JSON.stringify(main.session.hints)
		var ending: String = str(main.session.ending)
		main.use_object(target)
		expect(main.ui.modal_open and main.player.blocked, "Consultar guía abre ayuda y pausa movimiento")
		expect(card_text(main).contains(main.session.objective()), "La ayuda muestra el objetivo y no un quiz")
		expect(JSON.stringify(main.session.flags) == flags and JSON.stringify(main.session.applied) == applied, "La ayuda no muta banderas")
		expect(JSON.stringify(main.session.hints) == hints and main.session.ending == ending, "La ayuda no pide pista ni cierra la operación")
		expect(press_button(main, "Volver a la oficina"), "La ayuda tiene vuelta a la oficina")
		expect(not main.ui.modal_open and not main.player.blocked, "El botón regresa a primera persona")
		var before: Vector3 = guide.get_node("RightArm/Elbow/Hand").global_position
		await create_timer(0.25).timeout
		expect(before.distance_to(guide.get_node("RightArm/Elbow/Hand").global_position) > 0.001, "La acción del guía mueve la mano")
		var origin := target.global_position + guide.global_basis.z * 1.5
		var query := PhysicsRayQueryParameters3D.create(origin, target.global_position, 1)
		var hit: Dictionary = main.office.get_world_3d().direct_space_state.intersect_ray(query)
		expect(not hit.is_empty() and hit.collider == target, "Guía accesible con raycast de uso")
	var flags_after := JSON.stringify(main.session.flags)
	var curl: String = str(main._terminal_command("curl https://example.invalid"))
	expect(not curl.is_empty() and curl.contains("help"), "La terminal rechaza curl")
	expect(not main._terminal_command("curl").is_empty(), "La terminal rechaza curl a secas")
	expect(not main._terminal_command("inspect recon").is_empty(), "La terminal rechaza el vocabulario viejo")
	expect(main._terminal_command("help").contains("scan") and main._terminal_command("help").contains("notes"), "help ofrece el vocabulario local")
	expect(JSON.stringify(main.session.flags) == flags_after, "La terminal no muta banderas")
	if script_exists("sala1.gd"):
		expect(main.office.find_child("EmailPhish", true, false) != null, "Sala 1 deja el correo falso en el árbol")
		expect(has_magnetic_door(main.office), "Sala 1 deja la puerta magnética en el árbol")
	if script_exists("sala2.gd"):
		expect(floor_layer(main.office, "ServerFloor") == 2, "Sala 2 tiene suelo en la capa 2")
	if script_exists("sala3.gd"):
		expect(floor_layer(main.office, "LabFloor") == 2, "Sala 3 tiene suelo en la capa 2")
	root.remove_child(main)
	main.free()
	for _frame in 4:
		await process_frame
	print("WORLD: %d checks / %d failures" % [checks, failures])
	quit(0 if failures == 0 else 1)

func card_text(main) -> String:
	var lines := PackedStringArray()
	for child in main.ui.content.get_children():
		if child is Label:
			lines.append(child.text)
	return "\n".join(lines)

func press_button(main, caption: String) -> bool:
	for child in main.ui.content.get_children():
		if child is Button and child.text == caption:
			child.pressed.emit()
			return true
	return false

func script_exists(file_name: String) -> bool:
	return ResourceLoader.exists("res:" + "//Game/Levels/" + file_name)

func has_magnetic_door(office: Node) -> bool:
	for node in office.find_children("*", "Label3D", true, false):
		if "PUERTA MAGNÉTICA" in str(node.text):
			return true
	var door := office.find_child("ExitDoor", true, false)
	return door is OfficeInteractable and "MAGNÉTICA" in str(door.prompt)

func floor_layer(office: Node, node_name: String) -> int:
	var floor := office.find_child(node_name, true, false)
	var current: Node = floor
	while current:
		if current is CollisionObject3D:
			return current.collision_layer
		current = current.get_parent()
	return -1
