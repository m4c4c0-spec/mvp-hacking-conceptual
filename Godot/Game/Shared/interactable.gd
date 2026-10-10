class_name OfficeInteractable
extends StaticBody3D

@export var action := ""
@export var argument := 0
@export var prompt := ""
@export var grabbable := false
var home_parent: Node
var home_transform: Transform3D
var is_held := false
var loose := false
var velocity := Vector3.ZERO
var base_color := Color.WHITE
var surface: MeshInstance3D
var follow: Node3D

func _ready() -> void:
	home_parent = get_parent()
	home_transform = transform
	surface = get_node_or_null("Mesh")
	if surface and surface.material_override:
		surface.material_override = surface.material_override.duplicate()
		base_color = surface.material_override.albedo_color

func tint(color: Color) -> void:
	base_color = color
	focus(false)

func focus(on: bool) -> void:
	if surface and surface.material_override:
		surface.material_override.albedo_color = base_color.lightened(0.3) if on else base_color

func grab(anchor: Node3D) -> void:
	if not grabbable or is_held or anchor == null:
		return
	if not is_instance_valid(home_parent):
		home_parent = get_parent()
		home_transform = transform
	focus(false)
	follow = anchor
	is_held = true
	loose = false
	velocity = Vector3.ZERO
	collision_layer = 0
	collision_mask = 0
	# El cuerpo estático no puede ser hijo del jugador: al moverse, el motor lo lanza por el mapa.
	var office := _find_office()
	if office and get_parent() != office:
		reparent(office, true)
	top_level = true
	global_transform = _hold_transform()
	_park_physics()
	set_physics_process(true)


func release(impulse := Vector3.ZERO) -> void:
	if not is_held:
		return
	var placed := global_transform
	is_held = false
	follow = null
	top_level = false
	var office := _find_office()
	if office and get_parent() != office:
		reparent(office, true)
	global_transform = placed
	_restore_physics()
	loose = true
	velocity = impulse
	if velocity.length() > 6.0:
		velocity = velocity.normalized() * 6.0


func _physics_process(delta: float) -> void:
	if is_held:
		if is_instance_valid(follow):
			if not (follow is XRController3D and not (follow as XRController3D).get_has_tracking_data()):
				global_transform = _hold_transform()
		return
	if not loose:
		return
	velocity.y -= 9.8 * delta
	_move_horizontal(delta)
	_move_vertical(delta)


func _park_physics() -> void:
	PhysicsServer3D.body_set_collision_layer(get_rid(), 0)
	PhysicsServer3D.body_set_collision_mask(get_rid(), 0)
	PhysicsServer3D.body_set_space(get_rid(), RID())


func _restore_physics() -> void:
	var world := get_world_3d()
	if world:
		PhysicsServer3D.body_set_space(get_rid(), world.space)
	collision_layer = 1
	collision_mask = 3


func _hold_transform() -> Transform3D:
	var pose := follow.global_transform
	if follow is XRController3D:
		return pose
	return Transform3D(pose.basis * Basis.from_euler(Vector3(deg_to_rad(12.0), deg_to_rad(-15.0), 0.0)), pose.origin)


func _move_horizontal(delta: float) -> void:
	var step := Vector3(velocity.x, 0.0, velocity.z) * delta
	if step.length_squared() < 0.0000001:
		return
	var origin := global_position
	var query := PhysicsRayQueryParameters3D.create(origin, origin + step, 3)
	query.exclude = [get_rid()]
	var hit := get_world_3d().direct_space_state.intersect_ray(query)
	if hit.is_empty():
		global_position += step
		return
	var normal := Vector3(hit.normal.x, 0.0, hit.normal.z)
	global_position = hit.position + hit.normal * 0.03
	if normal.length_squared() < 0.0001:
		velocity.x = 0.0
		velocity.z = 0.0
		return
	var flat := Vector3(velocity.x, 0.0, velocity.z).slide(normal.normalized()) * 0.35
	velocity.x = flat.x
	velocity.z = flat.z


func _move_vertical(delta: float) -> void:
	var bottom := _world_bottom()
	var origin := Vector3(global_position.x, bottom + 0.02, global_position.z)
	var dest := origin + Vector3(0, velocity.y * delta, 0)
	var query := PhysicsRayQueryParameters3D.create(origin, dest, 3)
	query.exclude = [get_rid()]
	var hit := get_world_3d().direct_space_state.intersect_ray(query)
	if not hit.is_empty() and hit.normal.y > 0.55 and velocity.y <= 0.0:
		global_position.y += float(hit.position.y) - bottom
		velocity = Vector3.ZERO
		loose = false
		return
	global_position.y += velocity.y * delta
	if global_position.y < -2.0 and is_instance_valid(home_parent):
		reparent(home_parent)
		transform = home_transform
		collision_layer = 1
		collision_mask = 1
		velocity = Vector3.ZERO
		loose = false


func _find_office() -> Node:
	var node: Node = self
	while node:
		if str(node.name) == "Office":
			return node
		node = node.get_parent()
	if is_instance_valid(home_parent):
		node = home_parent
		while node:
			if str(node.name) == "Office":
				return node
			node = node.get_parent()
	var tree := get_tree()
	if tree:
		var found := tree.root.find_child("Office", true, false)
		if found:
			return found
	return home_parent if is_instance_valid(home_parent) else null


func _world_bottom() -> float:
	var bottom := global_position.y
	var found := false
	for child in get_children():
		if not child is CollisionShape3D:
			continue
		var shape_node := child as CollisionShape3D
		var box := shape_node.shape as BoxShape3D
		if box == null:
			continue
		var edge := shape_node.global_position.y - box.size.y * 0.5
		if not found or edge < bottom:
			bottom = edge
			found = true
	return bottom if found else global_position.y - 0.05
