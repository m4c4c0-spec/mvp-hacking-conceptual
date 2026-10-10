class_name CondorPhysics
extends RefCounted

## Capas: 1 muros y uso, 2 suelo de pisada y teletransporte, 0 decoración que no frena.
const FLOOR_SNAP := 0.25
const FLOOR_ANGLE := 50.0
const SAFE_MARGIN := 0.08
const CAPSULE_RADIUS := 0.25
const CAPSULE_HEIGHT := 1.7

static func tune(office: Node, player: Node) -> void:
	if office:
		_tune_tree(office)
	_tune_desktop(player)
	_tune_xr(player)

static func _tune_tree(node: Node) -> void:
	_tune_body(node)
	for child in node.get_children():
		_tune_tree(child)

static func _tune_body(node: Node) -> void:
	if node is OfficeInteractable or not (node is StaticBody3D):
		return
	var body := node as StaticBody3D
	var surface := str(body.name)
	if _segment(surface, "Floor"):
		body.collision_layer = 2
	elif _segment(surface, "Wall") or _segment(surface, "Partition") or _segment(surface, "Lintel"):
		body.collision_layer = 1
	elif _segment(surface, "Book") or _segment(surface, "Leaf") or _segment(surface, "Mullion"):
		body.collision_layer = 0

static func _tune_desktop(player: Node) -> void:
	if not player is DesktopPlayer:
		return
	var body := player as DesktopPlayer
	body.collision_mask = 3
	body.floor_snap_length = FLOOR_SNAP
	body.floor_max_angle = deg_to_rad(FLOOR_ANGLE)
	body.safe_margin = SAFE_MARGIN
	body.up_direction = Vector3.UP
	for child in body.get_children():
		var shape_node := child as CollisionShape3D
		if shape_node == null:
			continue
		var capsule := shape_node.shape as CapsuleShape3D
		if capsule == null:
			continue
		capsule.radius = CAPSULE_RADIUS
		capsule.height = CAPSULE_HEIGHT

static func _tune_xr(player: Node) -> void:
	if not player is QuestPlayer:
		return
	(player as QuestPlayer).apply_body(CAPSULE_RADIUS, CAPSULE_HEIGHT)

static func _segment(surface_name: String, token: String) -> bool:
	if surface_name.begins_with(token):
		return true
	var at := surface_name.find(token)
	while at > 0:
		var previous := surface_name.unicode_at(at - 1)
		var head := token.unicode_at(0)
		var hump := head >= 65 and head <= 90 and ((previous >= 97 and previous <= 122) or (previous >= 48 and previous <= 57))
		if hump:
			return true
		at = surface_name.find(token, at + 1)
	return false
