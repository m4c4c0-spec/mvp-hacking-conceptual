extends Node3D

const CYAN := Color("48ded2")
const CORAL := Color("ff927d")
const INK := Color("111d30")

var status_label: Label3D
var indicator: MeshInstance3D
var target: MeshInstance3D
var target_material: StandardMaterial3D
var animation_time := 0.0
var pulse := 0.0


func _ready() -> void:
	var world := WorldEnvironment.new()
	var environment := Environment.new()
	environment.background_mode = Environment.BG_COLOR
	environment.background_color = Color("17243b")
	environment.ambient_light_source = Environment.AMBIENT_SOURCE_COLOR
	environment.ambient_light_color = Color("b6cadf")
	environment.ambient_light_energy = 0.8
	world.environment = environment
	add_child(world)
	var sun := DirectionalLight3D.new()
	sun.rotation_degrees = Vector3(-55, -25, 0)
	sun.light_color = Color("fff0da")
	sun.light_energy = 1.4
	add_child(sun)

	_box(Vector3(10, 0.12, 10), Vector3(0, -0.08, 0), INK)
	for index in range(-5, 6):
		_box(Vector3(0.015, 0.008, 10), Vector3(index, -0.012, 0), Color("33465c"), true)
		_box(Vector3(10, 0.008, 0.015), Vector3(0, -0.012, index), Color("33465c"), true)
	_box(Vector3(10, 0.015, 0.03), Vector3(0, 0, -4.95), CYAN, true)
	_box(Vector3(10, 0.015, 0.03), Vector3(0, 0, 4.95), CYAN, true)
	_box(Vector3(0.03, 0.015, 10), Vector3(-4.95, 0, 0), CYAN, true)
	_box(Vector3(0.03, 0.015, 10), Vector3(4.95, 0, 0), CYAN, true)
	_box(Vector3(10, 4.5, 0.15), Vector3(0, 2.2, -4.8), Color("21314a"))
	_box(Vector3(0.15, 0.8, 9.8), Vector3(-4.8, 0.35, 0), Color("21314a"))
	_box(Vector3(0.15, 0.8, 9.8), Vector3(4.8, 0.35, 0), Color("21314a"))
	_label("QUEST / CONNECTION LAB", Vector3(0, 3.55, -4.68), 76, Color("eef6ff"))
	_label("LINUX   ·   GODOT   ·   OPENXR", Vector3(0, 3.18, -4.67), 30, CYAN)
	for x in [-4.0, 4.0]:
		_box(Vector3(0.45, 2.2, 0.45), Vector3(x, 1.1, -3.7), Color("30425b"))
		_box(Vector3(0.08, 1.9, 0.47), Vector3(x, 1.15, -3.69), CYAN, true)

	_box(Vector3(3.1, 1.14, 0.08), Vector3(0, 1.96, -1.3), INK, true)
	_box(Vector3(3.1, 0.025, 0.1), Vector3(0, 2.54, -1.29), CYAN, true)
	indicator = _box(Vector3(0.07, 1.14, 0.1), Vector3(-1.58, 1.96, -1.29), CORAL, true)
	status_label = _label("PREPARANDO PRUEBA…", Vector3(0, 1.98, -1.24), 34, Color.WHITE)
	_label("Mira a los lados y pulsa el gatillo de cada mando", Vector3(0, 1.25, -1.22), 28, Color("c6d4e5"))

	for x in [-1.9, 0.0, 1.9]:
		_box(Vector3(0.75, 0.6, 0.75), Vector3(x, 0.3, -0.5), Color("30425b"))
		_box(Vector3(0.8, 0.06, 0.8), Vector3(x, 0.64, -0.5), Color("ccd9e5"))
	var cube := _box(Vector3(0.4, 0.4, 0.4), Vector3(-1.9, 1, -0.5), CORAL)
	cube.rotation_degrees = Vector3(15, 30, 12)
	var sphere := SphereMesh.new()
	sphere.radius = 0.23
	sphere.height = 0.46
	_mesh(sphere, Vector3(1.9, 1, -0.5), Color("a99bf5"))
	var torus := TorusMesh.new()
	torus.inner_radius = 0.16
	torus.outer_radius = 0.3
	target = _mesh(torus, Vector3(0, 1.0, -0.5), CYAN)
	target_material = target.material_override as StandardMaterial3D
	_label("IZQUIERDO", Vector3(-1.9, 0.37, -0.10), 23, CORAL)
	_label("GATILLOS → CAMBIA EL ARO", Vector3(0, 0.37, -0.10), 15, CYAN)
	_label("DERECHO", Vector3(1.9, 0.37, -0.10), 23, Color("a99bf5"))
	_label("CUADRÍCULA: 1 METRO", Vector3(0, 0.02, 2.2), 25, Color("adbed1")).rotation_degrees.x = -90
	_label("01  /  ESCENA DE PRUEBA", Vector3(-3.1, 0.05, -3.5), 27, CYAN).rotation_degrees.x = -90


func _process(delta: float) -> void:
	animation_time += delta
	target.rotation = Vector3(PI / 2.0, animation_time * 0.4, animation_time * 0.25)
	pulse = maxf(0, pulse - delta * 2)
	target.scale = Vector3.ONE * (1.0 + pulse * 0.25)


func feedback(hand_index: int) -> void:
	target_material.albedo_color = CORAL if hand_index == 0 else Color("a99bf5")
	pulse = 1.0


func set_status(text: String, verified: bool) -> void:
	status_label.text = text
	(indicator.material_override as StandardMaterial3D).albedo_color = CYAN if verified else CORAL


func _box(size: Vector3, location: Vector3, color: Color, unshaded: bool = false) -> MeshInstance3D:
	var mesh := BoxMesh.new()
	mesh.size = size
	return _mesh(mesh, location, color, unshaded)


func _mesh(mesh: Mesh, location: Vector3, color: Color, unshaded: bool = false) -> MeshInstance3D:
	var instance := MeshInstance3D.new()
	instance.mesh = mesh
	instance.position = location
	var material := StandardMaterial3D.new()
	material.albedo_color = color
	material.roughness = 0.7
	if unshaded:
		material.shading_mode = BaseMaterial3D.SHADING_MODE_UNSHADED
	instance.material_override = material
	instance.cast_shadow = GeometryInstance3D.SHADOW_CASTING_SETTING_OFF
	add_child(instance)
	return instance


func _label(text: String, location: Vector3, font_size: int, color: Color) -> Label3D:
	var label := Label3D.new()
	label.text = text
	label.position = location
	label.font_size = font_size
	label.pixel_size = 0.003
	label.modulate = color
	label.outline_size = 0
	label.no_depth_test = false
	add_child(label)
	return label
