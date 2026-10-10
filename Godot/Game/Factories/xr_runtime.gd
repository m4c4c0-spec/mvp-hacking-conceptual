class_name XrRuntime
extends RuntimeProfile

## Familia OpenXR: origen del visor y pantalla espacial.


func prepare(host: Node) -> void:
	var viewport := host.get_viewport()
	if viewport:
		viewport.use_xr = true
	DisplayServer.window_set_vsync_mode(DisplayServer.VSYNC_DISABLED)


func create_player() -> Node3D:
	return QuestPlayer.new()


func uses_spatial_ui() -> bool:
	return true


func mount(host: Node, player: Node3D, ui: EscapeUI, on_suspend: Callable, on_focus: Callable) -> Node3D:
	var screen := ui.make_spatial(host)
	var quest := player as QuestPlayer
	quest.ui_pointer.connect(func(point: Vector3, click: bool) -> void: ui.pointer(point, screen, click))
	quest.ui_scroll.connect(ui.scroll_spatial)
	var interface := XRServer.find_interface("OpenXR") as OpenXRInterface
	if interface:
		interface.session_visible.connect(on_suspend)
		interface.session_stopping.connect(on_suspend)
		interface.session_loss_pending.connect(on_suspend)
		interface.session_focussed.connect(on_focus)
		interface.pose_recentered.connect(on_focus)
	return screen
