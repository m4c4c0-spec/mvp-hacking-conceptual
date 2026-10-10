class_name QuestPlayer
extends XROrigin3D

signal used(target: OfficeInteractable)
signal grabbed(target: OfficeInteractable)
signal pause_requested
signal ui_pointer(position: Vector3, click: bool)
signal ui_scroll(amount: float)

const RAY_REACH := 6.0
const TIP_RADIUS := 0.016
const SNAP_STEP := PI / 6.0
const TELEPORT_SPEED := 6.0
const TELEPORT_STEPS := 24
const TELEPORT_DELTA := 0.06
const BODY_RADIUS := 0.25
const BODY_HEIGHT := 1.7
const FALL_LIMIT := -4.0
const ENTRANCE := Vector3(0, 0.1, 4.4)
const THROW_LIMIT := 6.0

var camera: XRCamera3D
var held: OfficeInteractable
var focused: OfficeInteractable
var blocked := true
var hands: Array[XRController3D] = []
var aims: Array[XRNode3D] = []
var snap_ready := true
var pointed_position := Vector3.ZERO
var pointing_ui := false
var holding_hand: XRController3D
var pointer_hand: XRController3D
var ray_visual: MeshInstance3D
var ray_tip: MeshInstance3D
var teleport_aiming := false
var teleport_valid := false
var teleport_position := Vector3.ZERO
var teleport_arc: MeshInstance3D
var teleport_marker: MeshInstance3D
var teleport_material: StandardMaterial3D
var body_radius := BODY_RADIUS
var body_height := BODY_HEIGHT
var _body_shape: CapsuleShape3D
var _fall_speed := 0.0
var _hand_velocity := Vector3.ZERO
var _last_hand_position := Vector3.ZERO
var _hand_ready := false


func _ready() -> void:
	camera = XRCamera3D.new()
	camera.near = 0.05
	camera.far = RenderBudget.CAMERA_FAR
	add_child(camera)
	for tracker_name in ["left_hand", "right_hand"]:
		var controller := XRController3D.new()
		controller.tracker = tracker_name
		controller.name = tracker_name
		controller.pose = "grip"
		controller.show_when_tracked = true
		add_child(controller)
		hands.append(controller)
		controller.button_pressed.connect(_button.bind(controller))
		controller.button_released.connect(_release_button.bind(controller))
		var mesh := MeshInstance3D.new()
		var cylinder := CylinderMesh.new()
		cylinder.top_radius = 0.02
		cylinder.bottom_radius = 0.02
		cylinder.height = 0.13
		mesh.mesh = cylinder
		mesh.cast_shadow = GeometryInstance3D.SHADOW_CASTING_SETTING_OFF
		controller.add_child(mesh)
		var aim := XRNode3D.new()
		aim.name = tracker_name + "_aim"
		aim.tracker = tracker_name
		aim.pose = "aim"
		add_child(aim)
		aims.append(aim)
	pointer_hand = hands[1]
	_body_shape = CapsuleShape3D.new()
	_body_shape.radius = body_radius
	_body_shape.height = body_height
	_build_ray()
	_build_teleport_preview()


func apply_body(radius: float, height: float) -> void:
	body_radius = radius
	body_height = height
	if _body_shape:
		_body_shape.radius = radius
		_body_shape.height = height
	if teleport_marker and teleport_marker.mesh is CylinderMesh:
		var marker := teleport_marker.mesh as CylinderMesh
		marker.top_radius = radius
		marker.bottom_radius = radius


func _build_ray() -> void:
	var box := BoxMesh.new()
	box.size = Vector3(0.012, 0.012, 1.0)
	var material := StandardMaterial3D.new()
	material.shading_mode = BaseMaterial3D.SHADING_MODE_UNSHADED
	material.albedo_color = Color("d7fff4")
	ray_visual = MeshInstance3D.new()
	ray_visual.name = "InteractionRay"
	ray_visual.mesh = box
	ray_visual.cast_shadow = GeometryInstance3D.SHADOW_CASTING_SETTING_OFF
	ray_visual.material_override = material
	ray_visual.visible = false
	aims[1].add_child(ray_visual)
	var sphere := SphereMesh.new()
	sphere.radius = TIP_RADIUS
	sphere.height = TIP_RADIUS * 2.0
	ray_tip = MeshInstance3D.new()
	ray_tip.name = "InteractionRayTip"
	ray_tip.mesh = sphere
	ray_tip.cast_shadow = GeometryInstance3D.SHADOW_CASTING_SETTING_OFF
	ray_tip.material_override = material
	ray_tip.visible = false
	aims[1].add_child(ray_tip)


func _build_teleport_preview() -> void:
	teleport_material = StandardMaterial3D.new()
	teleport_material.shading_mode = BaseMaterial3D.SHADING_MODE_UNSHADED
	teleport_arc = MeshInstance3D.new()
	teleport_arc.name = "TeleportArc"
	teleport_arc.mesh = ImmediateMesh.new()
	teleport_arc.material_override = teleport_material
	teleport_arc.cast_shadow = GeometryInstance3D.SHADOW_CASTING_SETTING_OFF
	teleport_arc.visible = false
	add_child(teleport_arc)
	teleport_marker = MeshInstance3D.new()
	teleport_marker.name = "TeleportDestination"
	var marker := CylinderMesh.new()
	marker.top_radius = body_radius
	marker.bottom_radius = body_radius
	marker.height = 0.015
	teleport_marker.mesh = marker
	teleport_marker.material_override = teleport_material
	teleport_marker.cast_shadow = GeometryInstance3D.SHADOW_CASTING_SETTING_OFF
	teleport_marker.visible = false
	add_child(teleport_marker)


func _aim_ray(length: float) -> void:
	var reach := maxf(length, 0.05)
	ray_visual.visible = true
	ray_visual.scale = Vector3(1.0, 1.0, reach)
	ray_visual.position = Vector3(0.0, 0.0, -reach * 0.5)
	ray_tip.visible = true
	ray_tip.position = Vector3(0.0, 0.0, -reach + TIP_RADIUS)


func set_blocked(value: bool) -> void:
	blocked = value
	if blocked:
		_clear_focus()
		_cancel_teleport()
		snap_ready = false


func _clear_focus() -> void:
	if is_instance_valid(focused):
		focused.focus(false)
	focused = null
	pointing_ui = false
	pointed_position = Vector3.ZERO


func _physics_process(delta: float) -> void:
	_sample_hand(delta)
	if teleport_aiming:
		_update_teleport_preview()
	var aim := aims[1]
	if not aim.get_has_tracking_data():
		ray_visual.visible = false
		ray_tip.visible = false
		_clear_focus()
	else:
		_update_pointer(aim)
	if hands[1].get_is_active():
		var axis := hands[1].get_vector2("primary")
		if blocked:
			if absf(axis.y) > 0.25:
				ui_scroll.emit(-axis.y * 600.0 * delta)
		elif absf(axis.x) < 0.3:
			# Giro por pasos de 30°, alrededor de la cabeza; exige volver a centrar el stick.
			snap_ready = true
		elif absf(axis.x) > 0.7 and snap_ready:
			_snap_turn(axis.x)
			snap_ready = false
	else:
		snap_ready = false
	_apply_body(delta)


func _update_pointer(aim: Node3D) -> void:
	var start := aim.global_position
	var end := start - aim.global_basis.z * RAY_REACH
	var query := PhysicsRayQueryParameters3D.create(start, end, 11)
	var hit := get_world_3d().direct_space_state.intersect_ray(query)
	var length := RAY_REACH
	if not hit.is_empty():
		length = start.distance_to(hit.position)
	_aim_ray(length)
	pointing_ui = not hit.is_empty() and hit.collider.has_meta("spatial_ui")
	if pointing_ui:
		pointed_position = hit.position
		ui_pointer.emit(pointed_position, false)
	var next: OfficeInteractable = hit.get("collider") as OfficeInteractable if not blocked else null
	if next != focused:
		if focused:
			focused.focus(false)
		focused = next
		if focused:
			focused.focus(true)


func _snap_turn(axis: float) -> void:
	var pivot := camera.global_position
	rotate_y(-signf(axis) * SNAP_STEP)
	global_position += pivot - camera.global_position


func _button(button: String, controller: XRController3D) -> void:
	if button == "menu_button" or (button == "by_button" and controller == hands[1]):
		pause_requested.emit()
		return
	if button == "trigger_click" and pointing_ui and controller == pointer_hand:
		ui_pointer.emit(pointed_position, true)
		return
	if blocked:
		return
	if button == "trigger_click" and controller == pointer_hand and focused:
		used.emit(focused)
	if _is_grip(button) and controller == pointer_hand:
		if held:
			drop()
			return
		if focused and focused.grabbable:
			focused.grab(controller)
			if focused.is_held:
				held = focused
				holding_hand = controller
				_hand_ready = false
				_hand_velocity = Vector3.ZERO
				_last_hand_position = controller.global_position
				grabbed.emit(held)
	if button == "primary_click" and controller == hands[0]:
		teleport_aiming = true


func _is_grip(button: String) -> bool:
	return button == "grip_click" or button == "grip" or button == "squeeze_click" or button == "squeeze"


func _release_button(button: String, controller: XRController3D) -> void:
	if _is_grip(button) and controller == holding_hand:
		drop()
	if button == "primary_click" and controller == hands[0] and teleport_aiming:
		if not blocked and aims[0].get_has_tracking_data():
			teleport(aims[0])
		_cancel_teleport()


func _teleport_target(aim: Node3D) -> Dictionary:
	var points := PackedVector3Array([aim.global_position])
	var velocity := -aim.global_basis.z * TELEPORT_SPEED
	for _step in TELEPORT_STEPS:
		var start := points[-1]
		var end := start + velocity * TELEPORT_DELTA + Vector3.DOWN * 4.9 * TELEPORT_DELTA * TELEPORT_DELTA
		velocity += Vector3.DOWN * 9.8 * TELEPORT_DELTA
		# Se detiene en muros, puertas y muebles antes de buscar suelo.
		var query := PhysicsRayQueryParameters3D.create(start, end, 3)
		var hit := get_world_3d().direct_space_state.intersect_ray(query)
		if not hit.is_empty():
			points.append(hit.position)
			var floor_hit: bool = hit.normal.y >= 0.9 and (hit.collider.collision_layer & 2) != 0
			var valid := floor_hit and _has_clearance(hit.position)
			return {"points": points, "valid": valid, "position": hit.position, "hit": true}
		points.append(end)
	return {"points": points, "valid": false, "position": points[-1], "hit": false}


func _has_clearance(point: Vector3) -> bool:
	# Solo suelo en capa 2 y una cápsula completa libre de obstáculos.
	var shape := CapsuleShape3D.new()
	shape.radius = body_radius
	shape.height = body_height
	var clearance := PhysicsShapeQueryParameters3D.new()
	clearance.shape = shape
	clearance.transform = Transform3D(Basis.IDENTITY, point + Vector3.UP * (body_height * 0.5 + 0.04))
	clearance.collision_mask = 1
	return get_world_3d().direct_space_state.intersect_shape(clearance).is_empty()


func _update_teleport_preview() -> void:
	if blocked or not aims[0].get_has_tracking_data():
		_cancel_teleport()
		return
	var target := _teleport_target(aims[0])
	teleport_valid = target.valid
	teleport_position = target.position
	teleport_material.albedo_color = Color("55d4be") if teleport_valid else Color("e66b65")
	var arc := teleport_arc.mesh as ImmediateMesh
	arc.clear_surfaces()
	arc.surface_begin(Mesh.PRIMITIVE_LINE_STRIP)
	for point in target.points:
		arc.surface_add_vertex(teleport_arc.to_local(point))
	arc.surface_end()
	teleport_arc.visible = true
	teleport_marker.visible = target.hit
	teleport_marker.global_position = teleport_position + Vector3.UP * 0.015


func _cancel_teleport() -> void:
	teleport_aiming = false
	teleport_valid = false
	if teleport_arc:
		teleport_arc.visible = false
	if teleport_marker:
		teleport_marker.visible = false


func teleport(aim: Node3D) -> bool:
	if blocked:
		return false
	var target := _teleport_target(aim)
	if not target.valid:
		return false
	move_to_floor(target.position)
	return true


func move_to_floor(point: Vector3) -> void:
	# Coloca los pies bajo la cabeza incluso si el jugador caminó dentro del área real.
	var offset := camera.global_position - global_position
	offset.y = 0
	global_position = point - offset + Vector3.UP * 0.03
	_fall_speed = 0.0


func drop() -> void:
	if held:
		held.release(_hand_velocity)
		held = null
		holding_hand = null
	_hand_velocity = Vector3.ZERO
	_hand_ready = false


func _sample_hand(delta: float) -> void:
	if holding_hand == null or not is_instance_valid(held) or delta <= 0.0:
		return
	if not holding_hand.get_has_tracking_data():
		_hand_velocity = Vector3.ZERO
		_hand_ready = false
		return
	var current := holding_hand.global_position
	if _hand_ready:
		var step := current - _last_hand_position
		if step.length() > 0.4:
			_hand_velocity = Vector3.ZERO
		else:
			_hand_velocity = step / delta
			if _hand_velocity.length() > THROW_LIMIT:
				_hand_velocity = _hand_velocity.normalized() * THROW_LIMIT
	_last_hand_position = current
	_hand_ready = true


func _apply_body(delta: float) -> void:
	if camera == null or get_world_3d() == null:
		return
	_separate_from_walls()
	_apply_gravity(delta)


func _separate_from_walls() -> void:
	if _body_shape == null:
		return
	var space := get_world_3d().direct_space_state
	var params := PhysicsShapeQueryParameters3D.new()
	params.shape = _body_shape
	params.collision_mask = 1
	params.margin = 0.02
	for _step in 4:
		params.transform = _body_transform()
		var info := space.get_rest_info(params)
		if info.is_empty():
			return
		var normal := Vector3(info.normal.x, 0.0, info.normal.z)
		if normal.length_squared() < 0.0001:
			return
		global_position += normal.normalized() * 0.05


func _body_transform() -> Transform3D:
	var head := camera.global_position
	var center := Vector3(head.x, global_position.y + body_height * 0.5, head.z)
	return Transform3D(Basis.IDENTITY, center)


func _apply_gravity(delta: float) -> void:
	var space := get_world_3d().direct_space_state
	var origin := Vector3(camera.global_position.x, global_position.y + 0.15, camera.global_position.z)
	var dest := origin + Vector3.DOWN * (0.4 + _fall_speed * delta)
	var query := PhysicsRayQueryParameters3D.create(origin, dest, 3)
	var hit := space.intersect_ray(query)
	if hit.is_empty():
		_fall_speed += 9.8 * delta
		global_position.y -= _fall_speed * delta
		if global_position.y < FALL_LIMIT:
			move_to_floor(ENTRANCE)
		return
	var floor_y := float(hit.position.y) + 0.03
	if global_position.y > floor_y + 0.08:
		_fall_speed += 9.8 * delta
		global_position.y = maxf(global_position.y - _fall_speed * delta, floor_y)
		if global_position.y <= floor_y + 0.001:
			_fall_speed = 0.0
		return
	global_position.y = floor_y
	_fall_speed = 0.0


func get_camera() -> Camera3D:
	return camera
