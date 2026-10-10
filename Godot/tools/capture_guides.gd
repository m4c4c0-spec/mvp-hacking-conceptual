extends SceneTree

func _initialize() -> void:
	call_deferred("capture")

func capture() -> void:
	var main = load("res://Game/Levels/main.tscn").instantiate()
	root.add_child(main)
	main.allow_save = false
	main.session.reset()
	main.session.start(0)
	main._refresh_world()
	main.ui.close()
	main.player.set_blocked(true)
	# Cámara de captura independiente: no cambia spawn, partidas ni controladores.
	var camera := Camera3D.new()
	main.add_child(camera)
	camera.current = true
	for i in range(5):
		var guide: OfficeGuide = main.office.get_node("Guide%d" % i)
		var distance := 1.75 if i == 4 else 2.4
		var sideways := 0.0 if i == 4 else (1.8 if i == 3 else 0.4)
		camera.global_position = guide.global_position + guide.global_basis.z * distance + guide.global_basis.x * sideways + Vector3.UP * 1.55
		camera.look_at(guide.global_position + Vector3.UP * (1.2 if i == 4 else 1.05))
		await create_timer(0.35).timeout
		await RenderingServer.frame_post_draw
		var result := root.get_texture().get_image().save_png("res://docs/previews/guide-%d.png" % i)
		if result != OK:
			quit(1)
			return
	main.use_object(main.office.get_node("Guide4/Assistance"))
	await create_timer(0.3).timeout
	await RenderingServer.frame_post_draw
	root.get_texture().get_image().save_png("res://docs/previews/guide-help-pc.png")
	print("GUIDE PREVIEWS: OK")
	quit()
