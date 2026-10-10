class_name RuntimeFactory
extends RefCounted

const Desktop = preload("res://Game/Factories/desktop_runtime.gd")
const Xr = preload("res://Game/Factories/xr_runtime.gd")
const FALLBACK := "OpenXR no está activo. Iniciando modo PC para continuar la prueba."


static func create() -> RuntimeProfile:
	if not _xr_requested():
		return Desktop.new()
	var interface := XRServer.find_interface("OpenXR")
	if interface and interface.is_initialized():
		return Xr.new()
	var fallback: RuntimeProfile = Desktop.new()
	fallback.notice = FALLBACK
	return fallback


static func _xr_requested() -> bool:
	return "--xr" in OS.get_cmdline_user_args() or OS.has_feature("quest")
