#!/usr/bin/env python3
"""Comprueba el catálogo de Operación Cóndor. missions.json sigue en disco y no es el progreso."""
from pathlib import Path
import argparse
import json
import re

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / "Godot/Game/Features/Quests/condor.json"
PUZZLES = [
    "phish", "password", "mfa", "logs", "box", "traffic", "cable",
    "fire", "osint", "base64", "rssi", "hash", "kill",
]
CONCEPTS = [
    "phishing", "passwords", "mfa", "logs", "traffic", "containment",
    "failsafe", "osint", "obfuscation", "inventory", "integrity",
]
ROOMS = {"oficina", "servidores", "laboratorio"}
SOLUTIONS = {
    "phish": {"signals": ["dominio", "clave", "urgencia"]},
    "password": {"code": "Condores2026"},
    "mfa": {"token": "492017"},
    "logs": {"minute": "14"},
    "box": {"code": "4714"},
    "traffic": {"color": "azul"},
    "cable": {"color": "azul"},
    "fire": {"synced": True},
    "osint": {"pet": "Pixel"},
    "base64": {"key": "Pixel", "decoded": True},
    "rssi": {"cell": "C-4"},
    "hash": {"discard": "C"},
    "kill": {"code": "7391"},
}
BANNED = re.compile(r"nmap|metasploit|sqlmap|hydra|burp|mimikatz|hashcat", re.I)


def validate(data: dict, raw: str) -> list[str]:
    errors = []

    def need(condition: bool, message: str) -> None:
        if not condition:
            errors.append(message)

    need(data.get("version") == 3, "versión distinta de 3")
    need(bool(data.get("title")) and bool(data.get("subtitle")), "título o subtítulo vacío")
    concepts = data.get("concepts")
    puzzles = data.get("puzzles")
    need(isinstance(concepts, list) and [item.get("id") for item in concepts] == CONCEPTS, "se esperan los 11 conceptos en orden")
    need(isinstance(puzzles, list) and [item.get("id") for item in puzzles] == PUZZLES, "se esperan los 13 pasos de act")
    need("soporte-ti@ucd-ch1le.invalid" in raw, "falta el dominio ficticio")
    need("203.0.113.44" in raw, "falta la IP de documentación")
    need(not BANNED.search(raw), "hay una herramienta real en el catálogo")
    closing = data.get("closing") if isinstance(data.get("closing"), dict) else {}
    lessons = closing.get("lessons")
    need(isinstance(lessons, list) and len(lessons) == 10, "el cierre debe traer diez lecciones")
    need("cambiar mañana" in str(closing.get("question", "")), "falta la pregunta del debrief")
    by_id = {item.get("id"): item for item in concepts or []}
    for puzzle in puzzles or []:
        pid = puzzle.get("id")
        concept = by_id.get(puzzle.get("concept"))
        need(concept is not None, f"concepto desconocido en {pid}")
        need(puzzle.get("room") in ROOMS, f"sala desconocida en {pid}")
        need(isinstance(puzzle.get("seconds"), (int, float)) and puzzle.get("seconds") > 0, f"tiempo previsto inválido en {pid}")
        manual = str(puzzle.get("manual", ""))
        need(bool(manual) and bool(puzzle.get("debrief")) and bool(puzzle.get("objective")), f"textos incompletos en {pid}")
        hints = puzzle.get("hints")
        need(isinstance(hints, list) and len(hints) == 3 and all(isinstance(item, str) and item.strip() for item in hints), f"se esperan 3 pistas en {pid}")
        if concept and manual:
            first = manual.split(".", 1)[0] + "."
            need(first == concept.get("definition"), f"la primera frase no define el concepto en {pid}")
        need(puzzle.get("solution") == SOLUTIONS.get(pid), f"solución distinta del contrato en {pid}")
        need(concept and concept.get("name") and concept.get("definition"), f"concepto incompleto en {pid}")
    return errors


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--check", action="store_true", help="Falla si el catálogo no es válido")
    parser.parse_args()
    if not SOURCE.exists():
        raise SystemExit(f"No está el catálogo: {SOURCE}")
    raw = SOURCE.read_text(encoding="utf-8")
    data = json.loads(raw)
    errors = validate(data, raw)
    if errors:
        print("contenido inválido:")
        for error in errors:
            print(" -", error)
        raise SystemExit(1)
    print(f"OK · catálogo en Godot/Game/Features/Quests/condor.json · {len(data['concepts'])} conceptos · {len(data['puzzles'])} pasos · v{data['version']}")


if __name__ == "__main__":
    main()
