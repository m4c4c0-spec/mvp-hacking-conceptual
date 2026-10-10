extends RefCounted

const Interactables = preload("res://Game/Factories/interactable_factory.gd")
const Budget = preload("res://Game/Shared/render_budget.gd")
const Assets = preload("res://Game/Levels/office_assets.gd")
const Finish = preload("res://Game/Shared/surface_finish.gd")
const NAVY := Color("182b35")
const WOOD := Color("966746")
const PAPER := Color("d5d4c1")
const MINT := Color("55d4be")
const RED := Color("bb5150")

static func build() -> Node3D:
	var root := Node3D.new()
	root.name = "Office"
	box(root, "Floor", Vector3(2, -0.12, 0), Vector3(16, 0.24, 12), Color("41494b"), true, 2)
	box(root, "Ceiling", Vector3(2, 3.65, 0), Vector3(16, 0.15, 12), Color("c5c7bd"))
	box(root, "BackWall", Vector3(2, 1.8, -6), Vector3(16, 3.6, 0.2), Color("aab3ad"))
	# Frente: la salida abre a un pequeño corredor real.
	box(root, "FrontLeft", Vector3(-5, 1.8, 6), Vector3(2, 3.6, 0.2), Color("aab3ad"))
	box(root, "FrontRight", Vector3(4, 1.8, 6), Vector3(12, 3.6, 0.2), Color("aab3ad"))
	box(root, "ExitLintel", Vector3(-3, 3.15, 6), Vector3(2, 0.9, 0.2), Color("aab3ad"))
	box(root, "WestWall", Vector3(-6, 1.8, 0), Vector3(0.2, 3.6, 12), Color("bbc2b6"))
	box(root, "EastWall", Vector3(10, 1.8, 0), Vector3(0.2, 3.6, 12), Color("9daaa3"))
	box(root, "PartitionNorth", Vector3(5.4, 1.8, -3.4), Vector3(0.15, 3.6, 5.2), Color("9daaa3"))
	box(root, "PartitionSouth", Vector3(5.4, 1.8, 3.6), Vector3(0.15, 3.6, 4.8), Color("9daaa3"))
	box(root, "ArchiveLintel", Vector3(5.4, 3.1, 0.2), Vector3(0.15, 1, 2), Color("9daaa3"))
	# Oficina: pizarra y equipos al fondo; archivo a la derecha.
	box(root, "Desk", Vector3(0, 0.8, -0.3), Vector3(3.6, 0.12, 1.4), WOOD)
	for x in [-1.55, 1.55]:
		box(root, "DeskLeg", Vector3(x, 0.38, -0.3), Vector3(0.12, 0.75, 1.2), NAVY)
	prop(root, "Laptop", "terminal", 0, "LAPTOP · terminal local · E", Vector3(-0.9, 0.95, -0.5), Vector3(0.85, 0.18, 0.65), NAVY)
	box(root, "LaptopScreen", Vector3(-0.9, 1.18, -0.8), Vector3(0.8, 0.55, 0.07), NAVY)
	label(root, "CONTROLADOR\nACCESO BLOQUEADO", Vector3(-0.9, 1.2, -0.75), 25, MINT)
	prop(root, "Notebook", "glossary", 0, "CUADERNO · conceptos desbloqueados · E", Vector3(0.45, 0.94, -0.65), Vector3(0.45, 0.08, 0.32), MINT)
	for i in range(3):
		prop(root, "Tray%d" % i, "tray", i, "BANDEJA %d · clasificar carpeta · E" % (i + 1), Vector3(-0.8 + i * 0.8, 0.96, 0.05), Vector3(0.65, 0.1, 0.4), Color("c9ae78"))
		label(root, str(i + 1), Vector3(-0.8 + i * 0.8, 1.08, 0.18), 28, NAVY)
	prop(root, "ReportInbox", "report", 0, "BUZÓN · expediente y diagnóstico · E", Vector3(1.5, 1.0, -0.3), Vector3(0.45, 0.25, 0.45), RED)
	prop(root, "MissionBoard", "board", 0, "PIZARRA · aceptar fase · E", Vector3(0, 1.85, -5.65), Vector3(3.8, 1.7, 0.12), NAVY)
	label(root, "PROTOCOLO SALIDA\nAcepta una fase con E\n1 Diagnóstico · 2 Contención\n3 Identidad · 4 Limpieza", Vector3(0, 1.9, -5.55), 42, PAPER)
	label(root, "ARCHIVO / EVIDENCIAS", Vector3(7.7, 2.65, -5.65), 36, PAPER)
	for i in range(4):
		var position := Vector3(6.5 + (i % 2) * 1.7, 1.13, -2.4 - floori(i / 2.0) * 1.6)
		box(root, "ArchiveTable%d" % i, position - Vector3(0, 0.2, 0), Vector3(1.3, 0.12, 0.85), WOOD)
		box(root, "ArchiveLeg%d" % i, position - Vector3(0, 0.65, 0), Vector3(0.6, 0.9, 0.5), NAVY)
		prop(root, "Folder%d" % i, "clue", i, "CARPETA %d · E leer · G tomar" % (i + 1), position, Vector3(0.65, 0.14, 0.5), PAPER, true)
		label(root, "EVIDENCIA %d" % (i + 1), position + Vector3(0, 0.28, 0.25), 25, PAPER)
	# Bisagra y puerta física del archivo.
	var archive_hinge := Node3D.new()
	archive_hinge.name = "ArchiveHinge"
	archive_hinge.position = Vector3(5.4, 0, -0.8)
	root.add_child(archive_hinge)
	prop(archive_hinge, "ArchiveDoor", "archive_door", 0, "PUERTA ARCHIVO · E abrir/cerrar", Vector3(0, 1.25, 1), Vector3(0.12, 2.5, 2), WOOD)
	# Panel visible junto a la puerta de salida.
	box(root, "RecoveryPanel", Vector3(0.6, 1.7, 5.6), Vector3(3.1, 1.6, 0.12), NAVY)
	for i in range(4):
		var position := Vector3(-0.5 + i * 0.72, 1.5, 5.45)
		prop(root, "Control%d" % i, "restore", i, "SEGURO %d · activar después de resolver fase · E" % (i + 1), position, Vector3(0.5, 0.4, 0.15), RED)
		label(root, str(i + 1), position - Vector3(0, 0, 0.1), 36, PAPER, PI)
	label(root, "APLICA LO APRENDIDO\n1 Diagnosticar · 2 Aislar · 3 Revocar · 4 Limpiar", Vector3(0.6, 2.15, 5.45), 30, PAPER, PI)
	var exit_hinge := Node3D.new()
	exit_hinge.name = "ExitHinge"
	exit_hinge.position = Vector3(-4, 0, 5.95)
	root.add_child(exit_hinge)
	prop(exit_hinge, "ExitDoor", "exit", 0, "SALIDA BLOQUEADA · E comprobar", Vector3(1, 1.3, 0), Vector3(2, 2.6, 0.12), RED)
	label(root, "SALIDA", Vector3(-3, 2.95, 5.8), 46, PAPER, PI)
	box(root, "ExitCorridorFloor", Vector3(-3, -0.1, 7.5), Vector3(2, 0.2, 3), Color("698c79"), true, 2)
	for x in [-4.1, -1.9]:
		box(root, "CorridorWall", Vector3(x, 1.5, 7.5), Vector3(0.2, 3, 3), Color("b9c5b8"))
	box(root, "CorridorEnd", Vector3(-3, 1.5, 9), Vector3(2.2, 3, 0.2), Color("b9c5b8"))
	label(root, "LOCKDOWN ACTIVO", Vector3(-3, 1.7, 8.85), 38, NAVY, PI)
	# Decoración legible, local y ligera.
	for x in [-4.8, 3.6]:
		box(root, "Shelf", Vector3(x, 1.0, -4.8), Vector3(1.1, 2, 0.55), WOOD)
		for j in range(6):
			box(root, "Book", Vector3(x - 0.4 + j * 0.16, 1.45, -4.4), Vector3(0.11, 0.45, 0.2), MINT if j % 2 else PAPER, false)
	for z in [-3.2, 1.0, 4.0]:
		box(root, "WindowFrame", Vector3(-5.87, 2.15, z), Vector3(0.12, 1.75, 2.4), WOOD)
		box(root, "WindowGlass", Vector3(-5.76, 2.15, z), Vector3(0.06, 1.5, 2.18), Color("608795"), false)
		for offset in [-0.6, 0, 0.6]:
			box(root, "WindowMullion", Vector3(-5.7, 2.15, z + offset), Vector3(0.06, 1.55, 0.045), NAVY, false)
	for x in [-3.5, 2.9]:
		box(root, "Planter", Vector3(x, 0.27, 2.5), Vector3(0.45, 0.5, 0.45), WOOD)
		for j in range(3):
			box(root, "PlantLeaf", Vector3(x + (j - 1) * 0.13, 0.7 + j * 0.15, 2.5), Vector3(0.12, 0.65, 0.14), Color("4e8861"), false)
	for x in [-1.2, 1.2]:
		box(root, "ChairSeat", Vector3(x, 0.5, 1.6), Vector3(0.55, 0.12, 0.55), NAVY)
		box(root, "ChairBack", Vector3(x, 0.88, 1.85), Vector3(0.55, 0.7, 0.1), NAVY)
		box(root, "ChairStand", Vector3(x, 0.25, 1.6), Vector3(0.25, 0.5, 0.25), WOOD)
	var env := WorldEnvironment.new()
	env.name = "Environment"
	env.environment = Environment.new()
	env.environment.background_mode = Environment.BG_COLOR
	env.environment.background_color = Color("243943")
	env.environment.ambient_light_source = Environment.AMBIENT_SOURCE_COLOR
	env.environment.ambient_light_color = Color("e2e6d6")
	env.environment.ambient_light_energy = 0.7
	root.add_child(env)
	var light := DirectionalLight3D.new()
	light.rotation_degrees = Vector3(-55, -25, 0)
	light.light_energy = 0.85
	light.light_specular = 0.0
	light.shadow_enabled = true
	light.shadow_blur = 0.0
	light.directional_shadow_max_distance = 18.0
	light.directional_shadow_mode = DirectionalLight3D.SHADOW_ORTHOGONAL
	root.add_child(light)
	for x in [-2, 3, 7.5]:
		var lamp := OmniLight3D.new()
		lamp.position = Vector3(x, 3.1, -0.5)
		lamp.omni_range = 8
		lamp.light_energy = 0.45
		lamp.light_specular = 0.0
		lamp.shadow_enabled = false
		lamp.light_color = Color("ffe0ac")
		root.add_child(lamp)
		box(root, "CeilingLight", Vector3(x, 3.45, -0.5), Vector3(1.1, 0.08, 0.6), PAPER, false)
	Assets.dress(root)
	_attach_detail(root)
	Finish.tune_lighting(root)
	RenderBudget.enforce(root)
	return root

static func box(parent: Node3D, node_name: String, position: Vector3, size: Vector3, color: Color, solid := true, layer := 1) -> Node3D:
	var body: Node3D = StaticBody3D.new() if solid else Node3D.new()
	body.name = node_name
	body.position = position
	parent.add_child(body, true)
	add_mesh(body, size, color, solid)
	if solid:
		body.collision_layer = layer
		body.collision_mask = 7
		add_shape(body, size)
	return body

static func prop(parent: Node3D, node_name: String, action: String, argument: int, prompt: String, position: Vector3, size: Vector3, color: Color, grabbable := false) -> OfficeInteractable:
	var body := Interactables.create(node_name, action, argument, prompt, grabbable)
	body.position = position
	parent.add_child(body)
	add_mesh(body, size, color)
	add_shape(body, size)
	return body

static func _attach_detail(root: Node3D) -> void:
	const detail_path := "res://Game/Levels/office_detail.glb"
	if not ResourceLoader.exists(detail_path):
		return
	var detail := (load(detail_path) as PackedScene).instantiate()
	detail.name = "OfficeDetail"
	root.add_child(detail)
	for node in detail.find_children("*", "MeshInstance3D", true, false):
		var mesh_instance := node as MeshInstance3D
		mesh_instance.material_override = Finish.for_bucket(mesh_instance.name)
		mesh_instance.cast_shadow = GeometryInstance3D.SHADOW_CASTING_SETTING_OFF
		mesh_instance.gi_mode = GeometryInstance3D.GI_MODE_DISABLED

static func surface_material(color: Color) -> StandardMaterial3D:
	return Finish.make(color)

static func add_mesh(parent: Node3D, size: Vector3, color: Color, cast_shadow := true) -> void:
	var mesh := MeshInstance3D.new()
	mesh.name = "Mesh"
	mesh.mesh = Assets.beveled_box(size)
	mesh.material_override = Finish.make(color, str(parent.name))
	mesh.cast_shadow = GeometryInstance3D.SHADOW_CASTING_SETTING_ON if cast_shadow else GeometryInstance3D.SHADOW_CASTING_SETTING_OFF
	parent.add_child(mesh)

static func add_shape(parent: Node3D, size: Vector3) -> void:
	var shape := CollisionShape3D.new()
	shape.shape = BoxShape3D.new()
	shape.shape.size = size
	parent.add_child(shape)

static func label(parent: Node3D, text: String, position: Vector3, font_size: int, color: Color, yaw := 0.0) -> Label3D:
	var sign := Label3D.new()
	sign.position = position
	sign.rotation.y = yaw
	sign.text = text
	sign.font_size = font_size
	sign.pixel_size = 0.0025
	sign.modulate = color
	sign.outline_modulate = Color("111a20")
	sign.outline_size = 5
	sign.no_depth_test = false
	parent.add_child(sign)
	return sign
