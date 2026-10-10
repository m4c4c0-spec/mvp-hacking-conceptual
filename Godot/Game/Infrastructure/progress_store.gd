extends RefCounted

const SAVE_PATH := "user://escape-progress-v1.json"
var last_error := ""

func write(state: Dictionary, path: String = SAVE_PATH) -> String:
	var temp := path + ".tmp"
	var file := FileAccess.open(temp, FileAccess.WRITE)
	if file == null:
		return "No se pudo guardar la partida: " + error_string(FileAccess.get_open_error())
	file.store_string(JSON.stringify(state, "\t"))
	file.close()
	if FileAccess.file_exists(path):
		var backup_error := DirAccess.copy_absolute(path, path + ".bak")
		if backup_error != OK:
			return "No se pudo conservar la copia del guardado."
	var result := DirAccess.rename_absolute(temp, path)
	if result != OK:
		return "No se pudo reemplazar el guardado: " + error_string(result)
	return ""

func read(path: String = SAVE_PATH) -> Variant:
	last_error = ""
	if not FileAccess.file_exists(path):
		return null
	var file := FileAccess.open(path, FileAccess.READ)
	if file == null:
		last_error = "No se pudo leer el guardado; se conserva hasta que decidas reiniciar."
		return null
	var json := JSON.new()
	var result := json.parse(file.get_as_text())
	file.close()
	if result != OK or json.data == null:
		last_error = "Guardado incompatible o dañado; se conserva y puedes iniciar una partida nueva."
		return null
	return json.data
