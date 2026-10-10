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
	main.player.rotation = Vector3.ZERO
	main.player.camera.rotation = Vector3.ZERO
	await create_timer(1.0).timeout
	await RenderingServer.frame_post_draw
	var image := root.get_texture().get_image()
	var result := image.save_png("res://docs/previews/office-pc.png")
	print("PREVIEW: ", error_string(result))
	main._welcome()
	await create_timer(0.3).timeout
	await RenderingServer.frame_post_draw
	root.get_texture().get_image().save_png("res://docs/previews/welcome-pc.png")
	main.ui.close()
	main.player.enabled = false
	main.player.position = Vector3(2.5, 0.1, 2.7)
	main.player.camera.look_at(Vector3(0, 0.9, -0.3))
	await create_timer(0.3).timeout
	await RenderingServer.frame_post_draw
	root.get_texture().get_image().save_png("res://docs/previews/%s.png" % "materials-pc")
	# Llegar al panel mediante las reglas reales, sin marcar fases a mano.
	for i in range(main.session.active().evidence.size()):
		var clue: Dictionary = main.session.clue(i)
		main.session.collect(i)
		main.session.classify(i, int(clue.answer))
		main.session.classify(i, int(clue.reasonAnswer))
	main.session.report(int(main.session.active().report.answer))
	for i in range(main.session.active().quiz.size()):
		main.session.answer_quiz(i, int(main.session.active().quiz[i].answer))
	main._refresh_world()
	main._application(0)
	await create_timer(0.3).timeout
	await RenderingServer.frame_post_draw
	root.get_texture().get_image().save_png("res://docs/previews/%s.png" % "application-pc")
	quit(0 if result == OK else 1)
