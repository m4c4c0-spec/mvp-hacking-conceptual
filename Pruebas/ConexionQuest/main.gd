extends Node3D

const HAND_COLORS := [Color("ff927d"), Color("a99bf5")]

@onready var lab: Node3D = $Lab

var interface: OpenXRInterface
var xr_active := false
var camera: Camera3D
var controllers: Array[XRController3D] = []
var triggers := [0, 0]
var trigger_down := [false, false]
var head_baseline: Transform3D
var baseline_set := false
var head_moved := false
var verified := false
var status_elapsed := 0.0
var hud_label: Label


func _ready() -> void:
	interface = XRServer.find_interface("OpenXR") as OpenXRInterface
	xr_active = interface != null and interface.is_initialized()
	if xr_active:
		get_viewport().use_xr = true
		DisplayServer.window_set_vsync_mode(DisplayServer.VSYNC_DISABLED)
		_build_xr_rig()
		print("XR_LAB: OpenXR inicializado. Esperando tracking y gatillos reales.")
	else:
		_build_desktop_camera()
		print("XR_LAB: Vista escritorio. No acredita conexión con un visor.")
	_build_hud()
	_update_status()


func _build_xr_rig() -> void:
	var origin := XROrigin3D.new()
	origin.position = Vector3(0, 0, 1.8)
	add_child(origin)
	var xr_camera := XRCamera3D.new()
	xr_camera.near = 0.05
	xr_camera.far = 40
	origin.add_child(xr_camera)
	camera = xr_camera
	for index in range(2):
		var controller := XRController3D.new()
		controller.tracker = "left_hand" if index == 0 else "right_hand"
		controller.pose = "grip"
		controller.show_when_tracked = true
		origin.add_child(controller)
		controllers.append(controller)
		var shape := MeshInstance3D.new()
		var mesh := CapsuleMesh.new()
		mesh.radius = 0.025
		mesh.height = 0.14
		shape.mesh = mesh
		shape.rotation_degrees.x = -30
		shape.material_override = _hand_material(HAND_COLORS[index])
		controller.add_child(shape)
		var aim := XRNode3D.new()
		aim.tracker = controller.tracker
		aim.pose = "aim"
		aim.show_when_tracked = true
		origin.add_child(aim)
		var ray := MeshInstance3D.new()
		var ray_mesh := BoxMesh.new()
		ray_mesh.size = Vector3(0.006, 0.006, 1.2)
		ray.mesh = ray_mesh
		ray.position.z = -0.6
		ray.material_override = _hand_material(HAND_COLORS[index])
		aim.add_child(ray)


func _hand_material(color: Color) -> StandardMaterial3D:
	var material := StandardMaterial3D.new()
	material.shading_mode = BaseMaterial3D.SHADING_MODE_UNSHADED
	material.albedo_color = color
	return material


func _build_desktop_camera() -> void:
	camera = Camera3D.new()
	camera.position = Vector3(0, 1.7, 3.8)
	camera.near = 0.05
	camera.far = 40
	add_child(camera)
	camera.look_at(Vector3(0, 1.4, -1))


func _build_hud() -> void:
	var layer := CanvasLayer.new()
	layer.visible = not xr_active
	add_child(layer)
	var panel := PanelContainer.new()
	panel.position = Vector2(20, 20)
	panel.mouse_filter = Control.MOUSE_FILTER_IGNORE
	layer.add_child(panel)
	var margin := MarginContainer.new()
	for side in ["left", "right", "top", "bottom"]:
		margin.add_theme_constant_override("margin_" + side, 14)
	panel.add_child(margin)
	hud_label = Label.new()
	hud_label.add_theme_font_size_override("font_size", 18)
	margin.add_child(hud_label)


func _process(delta: float) -> void:
	if xr_active:
		_read_xr_input()
	else:
		_move_desktop(delta)
	status_elapsed += delta
	if status_elapsed >= 0.15:
		status_elapsed = 0
		_update_status()


func _read_xr_input() -> void:
	var focused := interface.get_session_state() == OpenXRInterface.SESSION_STATE_FOCUSED
	if focused:
		var tracker := XRServer.get_tracker("head") as XRPositionalTracker
		if tracker and tracker.has_pose("default"):
			var pose := tracker.get_pose("default")
			if pose.has_tracking_data:
				if not baseline_set:
					head_baseline = pose.transform
					baseline_set = true
				else:
					var distance := head_baseline.origin.distance_to(pose.transform.origin)
					var angle := head_baseline.basis.get_rotation_quaternion().angle_to(pose.transform.basis.get_rotation_quaternion())
					head_moved = head_moved or distance > 0.05 or absf(angle) > deg_to_rad(8)
	for index in range(controllers.size()):
		var controller := controllers[index]
		var pressed := focused and controller.get_has_tracking_data() and (controller.get_float("trigger") > 0.6 or controller.is_button_pressed("trigger_click"))
		if pressed and not trigger_down[index]:
			triggers[index] += 1
			lab.feedback(index)
		trigger_down[index] = pressed


func _head_has_tracking() -> bool:
	var tracker := XRServer.get_tracker("head") as XRPositionalTracker
	return tracker != null and tracker.has_pose("default") and tracker.get_pose("default").has_tracking_data


func _update_status() -> void:
	if not xr_active:
		lab.set_status("VISTA DE ESCRITORIO\nConexión con visor: SIN VERIFICAR\nConecta WiVRn y vuelve a iniciar en VR", false)
		hud_label.text = "VISTA ESCRITORIO · conexión VR sin verificar\nWASD: mover · botón derecho + ratón: mirar · Esc: salir"
		return
	var focused := interface.get_session_state() == OpenXRInterface.SESSION_STATE_FOCUSED
	var head_tracking := _head_has_tracking()
	var left := controllers[0].get_has_tracking_data()
	var right := controllers[1].get_has_tracking_data()
	var now_verified: bool = focused and head_tracking and head_moved and left and right and triggers[0] > 0 and triggers[1] > 0
	if now_verified and not verified:
		print("XR_LAB: ENTRADAS VERIFICADAS · cabeza y ambos gatillos. Confirma imagen en ambos ojos dentro del visor.")
	verified = now_verified
	var text := "ENTRADAS VR VERIFICADAS" if verified else "PRUEBA VR EN CURSO"
	text += "\nOpenXR: ACTIVO · sesión: " + ("ENFOCADA" if focused else "EN ESPERA")
	text += "\nCabeza: " + ("OK" if head_tracking else "SIN TRACKING") + " · movimiento: " + ("OK" if head_moved else "mira a los lados")
	text += "\nMandos: izquierdo " + ("OK" if left else "pendiente") + " / derecho " + ("OK" if right else "pendiente")
	text += "\nGatillos recibidos: I %d / D %d" % [triggers[0], triggers[1]]
	text += "\nConfirma que ves el entorno en ambos ojos."
	lab.set_status(text, verified)


func _move_desktop(delta: float) -> void:
	var movement := Vector3.ZERO
	if Input.is_physical_key_pressed(KEY_W):
		movement.z -= 1
	if Input.is_physical_key_pressed(KEY_S):
		movement.z += 1
	if Input.is_physical_key_pressed(KEY_A):
		movement.x -= 1
	if Input.is_physical_key_pressed(KEY_D):
		movement.x += 1
	camera.position += camera.basis * movement.normalized() * delta * 2
	camera.position.x = clampf(camera.position.x, -4.4, 4.4)
	camera.position.z = clampf(camera.position.z, -4.4, 4.4)
	camera.position.y = clampf(camera.position.y, 0.8, 3.2)


func _unhandled_input(event: InputEvent) -> void:
	if event is InputEventKey and event.pressed and event.keycode == KEY_ESCAPE:
		get_tree().quit()
	if not xr_active and event is InputEventMouseMotion and Input.is_mouse_button_pressed(MOUSE_BUTTON_RIGHT):
		camera.rotation.y -= event.relative.x * 0.003
		camera.rotation.x = clampf(camera.rotation.x - event.relative.y * 0.003, -1.1, 1.1)
