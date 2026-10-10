extends RefCounted

const CONTENT_PATH := "res://Game/Features/Quests/condor.json"

func load_catalog() -> Dictionary:
	if not FileAccess.file_exists(CONTENT_PATH):
		return {}
	var parsed = JSON.parse_string(FileAccess.get_file_as_string(CONTENT_PATH))
	if not parsed is Dictionary or not parsed.get("puzzles") is Array or not parsed.get("concepts") is Array:
		return {}
	return parsed
