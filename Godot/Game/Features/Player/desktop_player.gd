class_name DesktopPlayer
extends CharacterBody3D

signal used(target: OfficeInteractable)
signal grabbed(target: OfficeInteractable)
signal pause_requested

var camera: Camera3D
var hand: Node3D
var held: OfficeInteractable
var focused: OfficeInteractable
var blocked := true
var sensitivity := 0.0022
var enabled := true
var pitch := 0.0

func _ready() -> void:
	name = "DesktopPlayer"
	collision_layer = 4
	collision_mask = 3
	floor_snap_length = 0.25
	floor_max_angle = deg_to_rad(50.0)
	safe_margin = 0.08
	up_direction = Vector3.UP
	var shape := CollisionShape3D.new()
	var capsule := CapsuleShape3D.new()
	capsule.radius = 0.25
	capsule.height = 1.7
	shape.shape = capsule
	shape.position.y = 0.85
	add_child(shape)
	camera = Camera3D.new()
	camera.name = "Camera"
	camera.position.y = 1.62
	camera.near = 0.05
	camera.far = RenderBudget.CAMERA_FAR
	camera.fov = 72
	add_child(camera)
	hand = Node3D.new()
	hand.position = Vector3(0.3, -0.3, -0.7)
	camera.add_child(hand)

func set_blocked(value: bool) -> void:
	blocked = value
	Input.mouse_mode = Input.MOUSE_MODE_VISIBLE if blocked else Input.MOUSE_MODE_CAPTURED
	if blocked and focused:
		focused.focus(false)
		focused = null

func _unhandled_input(event: InputEvent) -> void:
	if not enabled:
		return
	if event.is_action_pressed("pause"):
		pause_requested.emit()
		get_viewport().set_input_as_handled()
		return
	if blocked:
		return
	if event is InputEventMouseMotion:
		rotate_y(-event.relative.x * sensitivity)
		pitch = clampf(pitch - event.relative.y * sensitivity, -1.4, 1.4)
		camera.rotation.x = pitch
	if event.is_action_pressed("use") and focused:
		used.emit(focused)
	if event.is_action_pressed("grab"):
		if held:
			drop()
		elif focused and focused.grabbable:
			take(focused)

func _physics_process(delta: float) -> void:
	if not enabled:
		return
	if not is_on_floor():
		velocity.y -= 9.8 * delta
	if blocked:
		velocity.x = move_toward(velocity.x, 0, 18 * delta)
		velocity.z = move_toward(velocity.z, 0, 18 * delta)
	else:
		var axis := Input.get_vector("left", "right", "forward", "backward")
		var direction := global_basis * Vector3(axis.x, 0, axis.y)
		var speed := 4.0 if Input.is_action_pressed("sprint") else 2.6
		velocity.x = move_toward(velocity.x, direction.x * speed, 18 * delta)
		velocity.z = move_toward(velocity.z, direction.z * speed, 18 * delta)
		_update_focus()
	move_and_slide()
	# Vuelta a la entrada. La partida sigue en la sesión.
	if position.y < -4.0:
		position = Vector3(0, 0.1, 4.4)
		velocity = Vector3.ZERO

func _update_focus() -> void:
	var from := camera.global_position
	var query := PhysicsRayQueryParameters3D.create(from, from - camera.global_basis.z * 3.0, 1)
	var hit := get_world_3d().direct_space_state.intersect_ray(query)
	var next: OfficeInteractable = hit.get("collider") as OfficeInteractable
	if focused == next:
		return
	if focused:
		focused.focus(false)
	focused = next
	if focused:
		focused.focus(true)

func take(target: OfficeInteractable) -> void:
	if held:
		drop()
	target.grab(hand)
	if not target.is_held:
		return
	held = target
	grabbed.emit(target)

func drop() -> void:
	if held:
		held.release()
		held = null

func get_camera() -> Camera3D:
	return camera
