extends RefCounted

## Arte geométrico original. Materiales opacos; el albedo 1K lo pone SurfaceFinish.
const Guide = preload("res://Game/Levels/office_guide.gd")
const Interactables = preload("res://Game/Factories/interactable_factory.gd")
const Finish = preload("res://Game/Shared/surface_finish.gd")
const INK := Color("263943")
const METAL := Color("778b91")
const CREAM := Color("eee4cf")
const CYAN := Color("55d4be")
static var _boxes: Dictionary = {}

static func material(color: Color) -> StandardMaterial3D:
	return Finish.make(color)

static func beveled_box(size: Vector3) -> ArrayMesh:
	var key := str(size)
	if _boxes.has(key):
		return _boxes[key]
	var half := size * 0.5
	var bevel := minf(0.035, minf(size.x, minf(size.y, size.z)) * 0.18)
	var surface := SurfaceTool.new()
	surface.begin(Mesh.PRIMITIVE_TRIANGLES)
	for axis in range(3):
		for sign_value in [-1, 1]:
			var points: Array[Vector3] = []
			for pair in [Vector2(-1, -1), Vector2(1, -1), Vector2(1, 1), Vector2(-1, 1)]:
				var signs := Vector3.ZERO
				signs[axis] = sign_value
				signs[(axis + 1) % 3] = pair.x
				signs[(axis + 2) % 3] = pair.y
				points.append(_corner(half, bevel, signs, axis))
			var normal := Vector3.ZERO
			normal[axis] = sign_value
			_polygon(surface, points, normal)
	for first in range(3):
		var second := (first + 1) % 3
		var free_axis := (first + 2) % 3
		for a in [-1, 1]:
			for b in [-1, 1]:
				var signs := Vector3.ZERO
				signs[first] = a
				signs[second] = b
				var normal := signs.normalized()
				signs[free_axis] = -1
				var p0 := _corner(half, bevel, signs, first)
				var p1 := _corner(half, bevel, signs, second)
				signs[free_axis] = 1
				_polygon(surface, [p0, p1, _corner(half, bevel, signs, second), _corner(half, bevel, signs, first)], normal)
	for x in [-1, 1]:
		for y in [-1, 1]:
			for z in [-1, 1]:
				var signs := Vector3(x, y, z)
				_polygon(surface, [_corner(half, bevel, signs, 0), _corner(half, bevel, signs, 1), _corner(half, bevel, signs, 2)], signs.normalized())
	var result := surface.commit()
	_boxes[key] = result
	return result

static func _corner(half: Vector3, bevel: float, signs: Vector3, face: int) -> Vector3:
	var point := signs * (half - Vector3.ONE * bevel)
	point[face] = signs[face] * half[face]
	return point

static func _polygon(surface: SurfaceTool, points: Array[Vector3], normal: Vector3) -> void:
	for i in range(1, points.size() - 1):
		var triangle: Array[Vector3] = [points[0], points[i], points[i + 1]]
		# Godot utiliza caras frontales con orden horario.
		if (triangle[1] - triangle[0]).cross(triangle[2] - triangle[0]).dot(normal) > 0:
			triangle.reverse()
		for point in triangle:
			surface.set_normal(normal)
			surface.add_vertex(point)

static func mesh(parent: Node3D, node_name: String, position: Vector3, geometry: Mesh, color: Color) -> MeshInstance3D:
	var instance := MeshInstance3D.new()
	instance.name = node_name
	instance.position = position
	instance.mesh = geometry
	instance.material_override = Finish.make(color, node_name)
	instance.cast_shadow = GeometryInstance3D.SHADOW_CASTING_SETTING_OFF
	parent.add_child(instance)
	return instance

static func box(parent: Node3D, node_name: String, position: Vector3, size: Vector3, color: Color) -> MeshInstance3D:
	return mesh(parent, node_name, position, beveled_box(size), color)

static func cylinder(parent: Node3D, node_name: String, position: Vector3, radius: float, height: float, color: Color, top := -1.0) -> MeshInstance3D:
	var shape := CylinderMesh.new()
	shape.top_radius = radius if top < 0 else top
	shape.bottom_radius = radius
	shape.height = height
	shape.radial_segments = 12
	shape.rings = 1
	return mesh(parent, node_name, position, shape, color)

static func sphere(parent: Node3D, node_name: String, position: Vector3, size: Vector3, color: Color) -> MeshInstance3D:
	var shape := SphereMesh.new()
	shape.radius = 0.5
	shape.height = 1.0
	shape.radial_segments = 12
	shape.rings = 6
	var instance := mesh(parent, node_name, position, shape, color)
	instance.scale = size
	return instance

static func label(parent: Node3D, text: String, position: Vector3, font_size: int, color: Color, yaw := 0.0) -> Label3D:
	var sign := Label3D.new()
	sign.text = text
	sign.position = position
	sign.rotation.y = yaw
	sign.font_size = font_size
	sign.pixel_size = 0.0025
	sign.modulate = color
	sign.outline_modulate = INK
	sign.outline_size = 3
	parent.add_child(sign)
	return sign

static func dress(root: Node3D) -> void:
	_architecture(root)
	_equipment(root)
	_furniture(root)
	var placements := [Vector3(-4.55, 0, 3.1), Vector3(2.25, 0, -4.4), Vector3(8.8, 0, -0.15), Vector3(-3.2, 0, -0.9), Vector3(3.2, 0, 4.4)]
	var headings := [0.45, -0.65, 0.0, 0.0, -0.6]
	for i in range(Guide.ROLES.size()):
		_guide(root, i, placements[i], headings[i])
	label(root, "RECEPCIÓN / ACCESO", Vector3(-5.65, 2.7, 3.1), 30, CREAM, PI / 2)
	label(root, "ESTACIÓN DE DIAGNÓSTICO", Vector3(-3.2, 2.55, -2.0), 26, CREAM)
	label(root, "SIMULACIÓN LOCAL · ASISTENCIA GRABADA", Vector3(1.0, 3.05, -5.8), 26, INK)

static func _architecture(root: Node3D) -> void:
	for x in range(-5, 10):
		box(root, "FloorJoint", Vector3(x, 0.005, 0), Vector3(0.012, 0.006, 11.8), Color("333e42"))
	for z in range(-5, 6):
		box(root, "FloorJoint", Vector3(2, 0.005, z), Vector3(15.8, 0.006, 0.012), Color("333e42"))
	box(root, "BackSkirting", Vector3(2, 0.12, -5.86), Vector3(15.7, 0.24, 0.08), INK)
	box(root, "WestSkirting", Vector3(-5.86, 0.12, 0), Vector3(0.08, 0.24, 11.7), INK)
	box(root, "EastSkirting", Vector3(9.86, 0.12, 0), Vector3(0.08, 0.24, 11.7), INK)
	for i in range(8):
		box(root, "WallSlat", Vector3(-2.2 - i * 0.16, 1.6, -5.85), Vector3(0.06, 3.0, 0.08), Color("987b59"))
	for x in [-5.0, 0.0, 5.0, 9.0]:
		box(root, "CeilingRail", Vector3(x, 3.53, 0), Vector3(0.035, 0.06, 11.8), METAL)
	for z in [-4.0, 0.0, 4.0]:
		box(root, "CeilingRail", Vector3(2, 3.53, z), Vector3(15.8, 0.06, 0.035), METAL)
	# Marcos y manillas son hijos de las puertas: siguen su apertura real.
	var exit_door: Node3D = root.get_node("ExitHinge/ExitDoor")
	box(exit_door, "Inset", Vector3(0, 0, -0.08), Vector3(1.8, 2.3, 0.035), Color("803f40"))
	box(exit_door, "PushBar", Vector3(0, -0.25, -0.15), Vector3(1.45, 0.07, 0.08), METAL)
	label(exit_door, "CONTROL DE ACCESO\nCUATRO SEGUROS", Vector3(0, 0.6, -0.12), 26, CREAM, PI)
	var archive: Node3D = root.get_node("ArchiveHinge/ArchiveDoor")
	box(archive, "DoorInset", Vector3(-0.08, 0.2, 0), Vector3(0.035, 1.9, 1.6), Color("806448"))
	box(archive, "Handle", Vector3(-0.14, -0.1, 0.65), Vector3(0.08, 0.06, 0.28), METAL)
	for i in range(4):
		var control: Node3D = root.get_node("Control%d" % i)
		box(control, "Switch", Vector3(0, 0, -0.11), Vector3(0.13, 0.22, 0.035), INK)
		box(control, "Frame", Vector3(0, 0.26, 0.03), Vector3(0.55, 0.035, 0.05), METAL)

static func _equipment(root: Node3D) -> void:
	var laptop: Node3D = root.get_node("Laptop")
	# El teclado queda en la mitad trasera de la base, bajo la pantalla, no hacia quien se sienta.
	box(laptop, "Keyboard", Vector3(0, 0.097, -0.12), Vector3(0.67, 0.016, 0.22), Color("71838b"))
	for row in range(4):
		for col in range(10):
			box(laptop, "Key", Vector3(-0.29 + col * 0.063, 0.11, -0.2 + row * 0.05), Vector3(0.048, 0.01, 0.04), INK)
	box(laptop, "Trackpad", Vector3(0, 0.099, 0.2), Vector3(0.22, 0.012, 0.13), METAL)
	box(root, "DisplayGlass", Vector3(-0.9, 1.18, -0.757), Vector3(0.72, 0.47, 0.012), Color("0e252d"))
	box(root, "LaptopCamera", Vector3(-0.9, 1.437, -0.75), Vector3(0.028, 0.018, 0.014), CYAN)
	sphere(root, "Mouse", Vector3(-1.58, 0.89, -0.48), Vector3(0.13, 0.06, 0.21), METAL)
	cylinder(root, "Mug", Vector3(0.9, 0.96, -0.65), 0.075, 0.17, CREAM)
	cylinder(root, "Coffee", Vector3(0.9, 1.047, -0.65), 0.064, 0.004, Color("473528"))
	box(root, "MugHandle", Vector3(1.0, 0.96, -0.65), Vector3(0.05, 0.1, 0.05), CREAM)
	for i in range(3):
		var tray: Node3D = root.get_node("Tray%d" % i)
		box(tray, "BaseInset", Vector3(0, 0.055, 0), Vector3(0.57, 0.016, 0.34), Color("594d3b"))
		for side in [-1, 1]:
			box(tray, "Rim", Vector3(side * 0.3, 0.08, 0), Vector3(0.035, 0.08, 0.4), Color("d4ba86"))
		box(tray, "BackRim", Vector3(0, 0.08, -0.18), Vector3(0.62, 0.08, 0.035), Color("d4ba86"))
	var notebook: Node3D = root.get_node("Notebook")
	box(notebook, "Pages", Vector3(0.01, 0.045, 0), Vector3(0.4, 0.012, 0.29), CREAM)
	for i in range(5):
		box(notebook, "Binding", Vector3(-0.21, 0.05, -0.12 + i * 0.06), Vector3(0.025, 0.025, 0.02), METAL)
	var inbox: Node3D = root.get_node("ReportInbox")
	box(inbox, "DocumentSlot", Vector3(0, 0.01, 0.23), Vector3(0.33, 0.05, 0.012), INK)
	label(inbox, "INFORME", Vector3(0, 0.14, 0.235), 18, CREAM)
	for i in range(4):
		var folder: Node3D = root.get_node("Folder%d" % i)
		box(folder, "PageStack", Vector3(0, 0.075, 0), Vector3(0.6, 0.015, 0.45), CREAM)
		box(folder, "Cover", Vector3(0, 0.09, 0), Vector3(0.65, 0.015, 0.5), Color("cda873"))
		box(folder, "Tab", Vector3(-0.18, 0.08, -0.27), Vector3(0.2, 0.04, 0.08), CYAN)
		box(folder, "Spine", Vector3(-0.31, 0.02, 0), Vector3(0.028, 0.18, 0.5), Color("9a7853"))
		box(folder, "CaseLabel", Vector3(0.08, 0.103, 0.05), Vector3(0.26, 0.008, 0.16), CREAM)
		for line in range(3):
			box(folder, "PrintedLine", Vector3(0.08, 0.109, line * 0.035), Vector3(0.19, 0.004, 0.008), INK)

static func _furniture(root: Node3D) -> void:
	for node in root.get_children():
		if str(node.name).begins_with("Shelf"):
			node.get_node("Mesh").visible = false
			box(node, "Back", Vector3(0, 0, -0.23), Vector3(1.1, 2, 0.07), Color("806448"))
			for side in [-1, 1]:
				box(node, "Side", Vector3(side * 0.52, 0, 0), Vector3(0.06, 2, 0.55), Color("966746"))
			for level in [-0.96, -0.32, 0.32, 0.96]:
				box(node, "ShelfBoard", Vector3(0, level, 0), Vector3(1.1, 0.07, 0.55), Color("a47e57"))
			for i in range(5):
				box(node, "Binder", Vector3(-0.37 + i * 0.17, -0.1, 0.02), Vector3(0.13, 0.37, 0.38), CYAN if i % 2 else CREAM)
				box(node, "SpineLabel", Vector3(-0.37 + i * 0.17, -0.07, 0.22), Vector3(0.07, 0.1, 0.008), INK)
		if str(node.name).begins_with("PlantLeaf") or str(node.name).begins_with("ChairStand"):
			node.get_node("Mesh").visible = false
	for x in [-1.2, 1.2]:
		cylinder(root, "ChairPost", Vector3(x, 0.25, 1.6), 0.045, 0.42, METAL)
		for i in range(5):
			var angle := i * TAU / 5
			var wheel_pos := Vector3(x + sin(angle) * 0.34, 0.065, 1.6 + cos(angle) * 0.34)
			var spoke := box(root, "ChairSpoke", Vector3(x + sin(angle) * 0.17, 0.11, 1.6 + cos(angle) * 0.17), Vector3(0.045, 0.055, 0.36), METAL)
			spoke.rotation.y = angle
			var wheel := cylinder(root, "ChairWheel", wheel_pos, 0.055, 0.045, INK)
			wheel.rotation.z = PI / 2
		for side in [-1, 1]:
			box(root, "ArmRest", Vector3(x + side * 0.32, 0.77, 1.64), Vector3(0.08, 0.06, 0.38), INK)
			box(root, "ArmSupport", Vector3(x + side * 0.3, 0.63, 1.76), Vector3(0.04, 0.27, 0.04), METAL)
	for x in [-3.5, 2.9]:
		cylinder(root, "PlantPot", Vector3(x, 0.28, 2.5), 0.24, 0.5, Color("b27d59"), 0.29)
		cylinder(root, "PlantSoil", Vector3(x, 0.535, 2.5), 0.265, 0.015, Color("473528"))
		for i in range(7):
			var angle := i * TAU / 7
			var leaf := sphere(root, "Leaf", Vector3(x + sin(angle) * 0.16, 0.88 + (i % 3) * 0.12, 2.5 + cos(angle) * 0.16), Vector3(0.17, 0.72, 0.09), Color("538965") if i % 2 else Color("386b50"))
			leaf.rotation = Vector3(cos(angle) * 0.35, angle, sin(angle) * 0.35)

static func _guide(root: Node3D, index: int, position: Vector3, yaw: float) -> void:
	var guide := Guide.new() as OfficeGuide
	guide.name = "Guide%d" % index
	guide.role = index
	guide.position = position
	guide.rotation.y = yaw
	root.add_child(guide)
	var suit := [Color("487b86"), Color("627dab"), Color("b28452"), Color("6b8b70"), Color("9570a4")][index] as Color
	var skin := [Color("c99776"), Color("a2735d"), Color("d7aa89"), Color("b5896b"), Color("bd997f")][index] as Color
	cylinder(guide, "Projector", Vector3(0, 0.025, 0), 0.4, 0.05, INK)
	cylinder(guide, "ProjectionMarker", Vector3(0, 0.055, 0), 0.37, 0.01, CYAN)
	for side in [-1, 1]:
		box(guide, "Shoe", Vector3(side * 0.12, 0.12, 0.035), Vector3(0.16, 0.11, 0.28), INK)
		box(guide, "Trouser", Vector3(side * 0.12, 0.51, 0), Vector3(0.17, 0.69, 0.19), INK)
	box(guide, "Hip", Vector3(0, 0.89, 0), Vector3(0.39, 0.18, 0.23), suit)
	box(guide, "Torso", Vector3(0, 1.19, 0), Vector3(0.46, 0.47, 0.27), suit)
	box(guide, "Shirt", Vector3(0, 1.31, 0.145), Vector3(0.14, 0.24, 0.015), CREAM)
	box(guide, "Badge", Vector3(-0.13, 1.21, 0.151), Vector3(0.075, 0.1, 0.014), CYAN)
	cylinder(guide, "Neck", Vector3(0, 1.47, 0), 0.07, 0.11, skin)
	var head := Node3D.new()
	head.name = "Head"
	head.position = Vector3(0, 1.64, 0)
	guide.add_child(head)
	sphere(head, "Face", Vector3.ZERO, Vector3(0.28, 0.34, 0.26), skin)
	sphere(head, "Hair", Vector3(0, 0.09, -0.025), Vector3(0.29, 0.2, 0.27), INK)
	sphere(head, "Nose", Vector3(0, -0.01, 0.127), Vector3(0.055, 0.06, 0.065), skin)
	for side in [-1, 1]:
		box(head, "Eye", Vector3(side * 0.058, 0.03, 0.122), Vector3(0.036, 0.018, 0.014), INK)
	box(head, "Mouth", Vector3(0, -0.075, 0.113), Vector3(0.06, 0.009, 0.012), INK)
	for side in [-1, 1]:
		var shoulder := Node3D.new()
		shoulder.name = "LeftArm" if side == -1 else "RightArm"
		shoulder.position = Vector3(side * 0.28, 1.37, 0)
		guide.add_child(shoulder)
		sphere(shoulder, "Sleeve", Vector3(0, -0.14, 0), Vector3(0.16, 0.33, 0.16), suit)
		var elbow := Node3D.new()
		elbow.name = "Elbow"
		elbow.position.y = -0.29
		shoulder.add_child(elbow)
		sphere(elbow, "Forearm", Vector3(0, -0.13, 0), Vector3(0.11, 0.29, 0.11), skin)
		var hand := Node3D.new()
		hand.name = "Hand"
		hand.position.y = -0.28
		elbow.add_child(hand)
		sphere(hand, "Palm", Vector3.ZERO, Vector3(0.105, 0.12, 0.09), skin)
	var left_hand: Node3D = guide.get_node("LeftArm/Elbow/Hand")
	var right_hand: Node3D = guide.get_node("RightArm/Elbow/Hand")
	if index != 3:
		var carried_prop := Node3D.new()
		carried_prop.name = "CarriedProp"
		left_hand.add_child(carried_prop)
		var prop_color := INK if index == 4 else Color("bf9b69")
		box(carried_prop, "HeldProp", Vector3(0.16, -0.01, 0), Vector3(0.38, 0.055, 0.28), prop_color)
		box(carried_prop, "DocumentOrDisplay", Vector3(0.16, 0.021, 0), Vector3(0.31, 0.012, 0.22), CYAN if index == 4 else CREAM)
		for line in range(3):
			box(carried_prop, "DocumentLine", Vector3(0.16, 0.03, -0.07 + line * 0.06), Vector3(0.23, 0.005, 0.012), INK)
		if index == 0 or index == 4:
			box(right_hand, "Stylus", Vector3(0, -0.06, 0), Vector3(0.015, 0.2, 0.015), CYAN)
	if index == 1:
		box(right_hand, "PointingFinger", Vector3(0, -0.09, 0), Vector3(0.025, 0.15, 0.03), skin)
	if index == 2:
		box(guide, "SortingShelf", Vector3(0, 1.2, 0.66), Vector3(0.9, 0.09, 0.42), Color("966746"))
		for side in [-1, 1]:
			box(guide, "ShelfSupport", Vector3(side * 0.38, 0.59, 0.66), Vector3(0.06, 1.16, 0.3), INK)
		for i in range(3):
			box(guide, "SortedFolder", Vector3(0.24, 1.27 + i * 0.055, 0.66), Vector3(0.3, 0.04, 0.28), CREAM if i % 2 else suit)
	if index == 3:
		# La mesa queda delante del guía. El teclado se apoya en el tablero, no delante de él.
		box(guide, "DiagnosticTable", Vector3(0, 0.85, 0.65), Vector3(1.0, 0.08, 0.6), Color("966746"))
		for side in [-1, 1]:
			box(guide, "TableLeg", Vector3(side * 0.4, 0.42, 0.65), Vector3(0.06, 0.81, 0.4), METAL)
		box(guide, "DemoKeyboard", Vector3(0, 0.925, 0.52), Vector3(0.7, 0.07, 0.22), INK)
		for i in range(9):
			box(guide, "DemoKey", Vector3(-0.28 + i * 0.07, 0.968, 0.52), Vector3(0.05, 0.014, 0.14), METAL)
		box(guide, "DemoDisplay", Vector3(0, 1.15, 0.82), Vector3(0.75, 0.45, 0.055), INK)
		box(guide, "DisplayFace", Vector3(0, 1.15, 0.784), Vector3(0.67, 0.36, 0.015), Color("183c43"))
		label(guide, "DIAGNÓSTICO\nSIMULADO", Vector3(0, 1.15, 0.771), 18, CYAN, PI)
	var interaction := Interactables.create("Assistance", "guide", index, "%s · %s · E consultar" % [Guide.ROLES[index].name, Guide.ROLES[index].action])
	interaction.position.y = 1.18
	guide.add_child(interaction)
	box(interaction, "Mesh", Vector3(0, 0.02, 0.158), Vector3(0.09, 0.12, 0.015), CYAN)
	var collision := CollisionShape3D.new()
	var shape := BoxShape3D.new()
	shape.size = Vector3(0.85, 1.35, 0.8)
	collision.shape = shape
	interaction.add_child(collision)
	label(guide, "%s · E" % Guide.ROLES[index].name, Vector3(0, 2.14, 0), 26, CREAM)
	label(guide, "ASISTENCIA GRABADA\n" + Guide.ROLES[index].action, Vector3(0, 1.97, 0), 18, CYAN)
