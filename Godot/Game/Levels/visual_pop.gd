extends Node

## Luces y marcas de las tres salas. Sin colisión, sin mapa de normales.

const Builder = preload("res://Game/Levels/office_builder.gd")

const MINT := Color("55d4be")
const ALARM := Color("ff5a4a")
const AMBER := Color("ffc14d")
const SCREEN := Color("8af0ff")
const RACK := Color("d7ffe8")


static func attach(office: Node3D) -> void:
	if office == null or office.get_node_or_null("VisualPop"):
		return
	var node := Node.new()
	node.name = "VisualPop"
	node.set_script(load("res://Game/Levels/visual_pop.gd"))
	office.add_child(node)


func _ready() -> void:
	var office := get_parent() as Node3D
	if office == null:
		return
	_align_archive_frame(office)
	_glow_named(office, "LaptopScreen", SCREEN, 1.4)
	_glow_named(office, "DisplayGlass", SCREEN, 0.8)
	_glow_named(office, "DemoDisplay", SCREEN, 0.6)
	_office(office)
	_servers(office)
	_lab(office)


func _align_archive_frame(office: Node3D) -> void:
	# En la captura, el marco negro queda a la derecha de la puerta de madera.
	# Esa malla está 1 m hacia +Z respecto de la hoja del archivo (z −0,8 a 1,2).
	var detail := office.get_node_or_null("OfficeDetail")
	if detail == null:
		return
	_shift_cluster(detail.get_node_or_null("interior_metal") as MeshInstance3D, 5.15, 5.7, 0.0, 2.55, 0.15, 2.3)
	_shift_cluster(detail.get_node_or_null("interior_plaster") as MeshInstance3D, 5.2, 5.8, 2.3, 2.55, 0.2, 2.2)


func _shift_cluster(instance: MeshInstance3D, min_x: float, max_x: float, min_y: float, max_y: float, min_z: float, max_z: float) -> void:
	if instance == null or bool(instance.get_meta("frame_aligned", false)):
		return
	var mesh := instance.mesh as ArrayMesh
	if mesh == null or mesh.get_surface_count() != 1:
		return
	var tool := MeshDataTool.new()
	if tool.create_from_surface(mesh, 0) != OK:
		return
	var lowest := 99.0
	var count := 0
	for index in tool.get_vertex_count():
		var vertex := tool.get_vertex(index)
		if _in_cluster(vertex, min_x, max_x, min_y, max_y, min_z, max_z):
			lowest = minf(lowest, vertex.z)
			count += 1
	if count == 0 or lowest < 0.1:
		instance.set_meta("frame_aligned", true)
		return
	for index in tool.get_vertex_count():
		var vertex := tool.get_vertex(index)
		if _in_cluster(vertex, min_x, max_x, min_y, max_y, min_z, max_z):
			vertex.z -= 1.0
			tool.set_vertex(index, vertex)
	mesh.surface_remove(0)
	tool.commit_to_surface(mesh)
	instance.set_meta("frame_aligned", true)


func _in_cluster(vertex: Vector3, min_x: float, max_x: float, min_y: float, max_y: float, min_z: float, max_z: float) -> bool:
	return vertex.x > min_x and vertex.x < max_x and vertex.y >= min_y and vertex.y <= max_y and vertex.z > min_z and vertex.z < max_z


func _process(_delta: float) -> void:
	var pulse := 0.62 + 0.38 * sin(Time.get_ticks_msec() * 0.0035)
	for child in get_children():
		if child is OmniLight3D and bool(child.get_meta("pulse", false)):
			var base := float(child.get_meta("base_energy", 0.4))
			(child as OmniLight3D).light_energy = base * pulse


func _office(office: Node3D) -> void:
	_strip(office, "ExitGlowLeft", Vector3(-3.92, 1.35, 5.82), Vector3(0.04, 2.55, 0.03), ALARM, 1.6)
	_strip(office, "ExitGlowRight", Vector3(-2.08, 1.35, 5.82), Vector3(0.04, 2.55, 0.03), ALARM, 1.6)
	_strip(office, "ExitGlowHead", Vector3(-3.0, 2.62, 5.82), Vector3(1.9, 0.04, 0.03), ALARM, 1.6)
	_lamp(office, "ExitAlarm", Vector3(-3.0, 2.35, 5.4), ALARM, 0.55, 4.5, true)
	_strip(office, "DeskGlow", Vector3(-0.9, 1.18, -0.93), Vector3(0.7, 0.42, 0.02), MINT, 1.2)
	_lamp(office, "DeskPool", Vector3(-0.4, 1.7, -0.2), Color("fff1d2"), 0.45, 3.2, false)


func _servers(office: Node3D) -> void:
	var racks: Array[Array] = [["Rack01", 11.15], ["RACK-03", 12.35], ["Rack07", 13.55]]
	for index in range(racks.size()):
		var spec: Array = racks[index]
		var z := float(spec[1])
		var colors: Array[Color] = [MINT, AMBER, ALARM]
		for i in range(3):
			_strip(office, "RackLed%d%d" % [index, i], Vector3(-6.24, 1.85 - i * 0.22, z), Vector3(0.03, 0.08, 0.08), colors[i], 2.2)
		if str(spec[0]) == "RACK-03":
			_lamp(office, "RackGlow", Vector3(-6.1, 1.6, z), ALARM, 0.35, 2.4, true)
	_strip(office, "UplinkGlow", Vector3(-0.38, 1.3, 12.55), Vector3(0.03, 0.46, 0.16), Color("3aa0ff"), 2.0)
	_lamp(office, "UplinkPool", Vector3(-0.7, 1.5, 12.55), Color("3aa0ff"), 0.4, 2.8, false)


func _lab(office: Node3D) -> void:
	_strip(office, "LabFrameLeft", Vector3(-3.68, 1.35, 20.12), Vector3(0.03, 2.55, 0.04), MINT, 1.5)
	_strip(office, "LabFrameRight", Vector3(-2.32, 1.35, 20.12), Vector3(0.03, 2.55, 0.04), MINT, 1.5)
	_strip(office, "LabFrameHead", Vector3(-3.0, 2.66, 20.12), Vector3(1.4, 0.035, 0.04), MINT, 1.5)
	_lamp(office, "LabWash", Vector3(-3.0, 2.6, 25.5), SCREEN, 0.5, 7.0, false)
	_strip(office, "KillBezel", Vector3(-1.2, 0.78, 23.22), Vector3(1.08, 0.36, 0.02), ALARM, 1.3)
	_lamp(office, "KillGlow", Vector3(-1.2, 1.15, 23.4), ALARM, 0.28, 1.8, true)
	for i in range(4):
		_strip(office, "PathMark%d" % i, Vector3(-3.0, 0.02, 8.2 + i * 3.2), Vector3(0.35, 0.012, 0.12), MINT, 1.1)


func _strip(office: Node3D, node_name: String, position: Vector3, size: Vector3, color: Color, energy: float) -> void:
	var body := Builder.box(office, node_name, position, size, color, false)
	body.reparent(self)
	_glow_named(body, "Mesh", color, energy)


func _glow_named(root: Node, node_name: String, color: Color, energy: float) -> void:
	var node := root.get_node_or_null(node_name)
	if node == null:
		node = root.find_child(node_name, true, false)
	var mesh := node as MeshInstance3D
	if mesh == null and node:
		mesh = node.get_node_or_null("Mesh") as MeshInstance3D
	if mesh == null:
		return
	var material := StandardMaterial3D.new()
	material.albedo_color = color
	material.emission_enabled = true
	material.emission = color
	material.emission_energy_multiplier = energy
	material.roughness = 0.32
	material.metallic = 0.0
	material.normal_enabled = false
	mesh.material_override = material
	mesh.cast_shadow = GeometryInstance3D.SHADOW_CASTING_SETTING_OFF


func _lamp(office: Node3D, node_name: String, position: Vector3, color: Color, energy: float, reach: float, pulse: bool) -> void:
	var light := OmniLight3D.new()
	light.name = node_name
	light.position = position
	light.light_color = color
	light.light_energy = energy
	light.omni_range = reach
	light.shadow_enabled = false
	light.light_specular = 0.2
	light.set_meta("base_energy", energy)
	light.set_meta("pulse", pulse)
	add_child(light)
	light.global_position = office.to_global(position)
