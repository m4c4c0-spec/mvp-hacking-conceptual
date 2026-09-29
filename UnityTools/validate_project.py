#!/usr/bin/env python3
"""Read-only validation. Does not substitute compiling or running Unity tests."""
import argparse
import json
import re
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]


def validate_content():
    data = json.loads((ROOT / "Assets/Resources/Content/missions.json").read_text())
    assert data["version"] == 1
    concepts = {c["id"] for c in data["concepts"]}
    assert len(concepts) == len(data["concepts"]) == 8
    missions = data["missions"]
    assert len({m["id"] for m in missions}) == len(missions) == 4
    unlocked = set()
    evidence_count = 0
    for concept in data["concepts"]:
        for key in ("name", "definition", "importance", "defense"):
            assert concept[key].strip(), (concept["id"], key)
    for mission in missions:
        assert mission["scope"] and mission["learned"]
        assert len(mission["quiz"]) == 3
        assert len({e["id"] for e in mission["evidence"]}) == len(mission["evidence"])
        assert set(mission["concepts"]) <= concepts
        rewards = set()
        for evidence in mission["evidence"]:
            assert 0 <= evidence["answer"] < len(evidence["options"])
            assert 0 <= evidence["reasonAnswer"] < len(evidence["reasons"])
            assert evidence["concept"] in mission["concepts"]
            assert evidence["feedback"] and evidence["body"]
            rewards.add(evidence["concept"])
            evidence_count += 1
        assert set(mission["concepts"]) <= rewards, "Unreachable concept"
        unlocked |= rewards
        report = mission["report"]
        assert 0 <= report["answer"] < len(report["options"])
        for quiz in mission["quiz"]:
            assert 0 <= quiz["answer"] < len(quiz["options"])
            assert quiz["explanation"]
    assert unlocked == concepts
    print(f"OK content: {len(missions)} missions, {evidence_count} evidence cards, {len(concepts)} reachable concepts, 12 quiz questions")


def validate_project():
    manifest = json.loads((ROOT / "Packages/manifest.json").read_text())
    # En Unity 6 TextMesh Pro forma parte de uGUI.
    assert {"com.unity.inputsystem", "com.unity.ugui", "com.unity.test-framework"} <= manifest["dependencies"].keys()
    assert "6000.6.3f1" in (ROOT / "ProjectSettings/ProjectVersion.txt").read_text()
    assembly_paths = list((ROOT / "Assets").rglob("*.asmdef"))
    assert len({p.parent for p in assembly_paths}) == len(assembly_paths), "Multiple assembly definitions in the same folder"
    definitions = [json.loads(p.read_text()) for p in assembly_paths]
    names = {d["name"] for d in definitions}
    external = {"Unity.ugui", "Unity.InputSystem", "Unity.TextMeshPro", "Unity.InputSystem.TestFramework"}
    assert len(names) == len(definitions)
    for assembly in definitions:
        assert set(assembly.get("references", [])) <= names | external
        if "Unity.InputSystem.TestFramework" in assembly.get("references", []):
            assert "TestAssemblies" in assembly.get("optionalUnityReferences", []), "Input fixture solo en tests"
    bootstrap_meta = (ROOT / "Assets/Scripts/Core/AcademyBootstrap.cs.meta").read_text()
    guid = re.search(r"guid: (\w+)", bootstrap_meta).group(1)
    assert guid in (ROOT / "Assets/Scenes/Academy.unity").read_text()
    scene_guid = re.search(r"guid: (\w+)", (ROOT / "Assets/Scenes/Academy.unity.meta").read_text()).group(1)
    assert scene_guid in (ROOT / "ProjectSettings/EditorBuildSettings.asset").read_text()
    build_settings = (ROOT / "ProjectSettings/EditorBuildSettings.asset").read_text()
    enabled_scenes = re.findall(r"enabled: 1\s+path: ([^\n]+)", build_settings)
    assert enabled_scenes[:2] == ["Assets/Scenes/Boot.unity", "Assets/Scenes/Hub.unity"]
    player_settings = (ROOT / "ProjectSettings/ProjectSettings.asset").read_text()
    assert re.search(r"activeInputHandler: [12]", player_settings), "El Hub requiere Input System activo"
    runtime = list((ROOT / "Assets/Scripts").rglob("*.cs"))
    forbidden = ("Process.Start", "System.Net.", "UnityWebRequest", "Application.OpenURL", "DllImport")
    for source in runtime:
        text = source.read_text()
        assert not any(pattern in text for pattern in forbidden), source
    controller = (ROOT / "Assets/Scripts/World/DesktopOfficeController.cs").read_text()
    for required in ("CharacterController", "controller.Move", "Physics.Raycast", "CursorLockMode.Locked", "DesktopInput.Grab"):
        assert required in controller, required
    hub_controller = (ROOT / "Assets/_Project/Presentation/PcInteractor.cs").read_text()
    for required in ("body.Move", "PcButtons.Move", "PcButtons.LookDelta", "ClampMagnitude", "ReturnToEntrance", "ControlInterrupted"):
        assert required in hub_controller, required
    print(f"OK project: {len(definitions)} assemblies, scene GUIDs, package pins, local-only runtime, first-person controller")


def validate_modeled_office():
    prefab = ROOT / "Assets/Art/Office/Prefabs/Office_AnalystAcademy.prefab"
    assert prefab.is_file(), "Falta generar el prefab de la oficina"
    metas = {}
    for path in (ROOT / "Assets").rglob("*.meta"):
        match = re.search(r"^guid: (\w+)", path.read_text(), re.MULTILINE)
        if match:
            metas[match.group(1)] = path
    text = prefab.read_text()
    script_guids = set(re.findall(r"m_Script: \{fileID: \d+, guid: (\w+)", text))
    assert len(script_guids) >= 5
    assert script_guids <= metas.keys(), "Prefab con scripts sin meta: " + str(script_guids - metas.keys())
    prefab_guid = re.search(r"guid: (\w+)", prefab.with_suffix(".prefab.meta").read_text()).group(1)
    for name in ("Hub", "Hub_Art"):
        assert prefab_guid in (ROOT / f"Assets/Scenes/{name}.unity").read_text(), name
    assert (ROOT / "Assets/TextMesh Pro/Resources/TMP Settings.asset").is_file()
    assert (ROOT / "Assets/Resources/WorldText.shader").is_file()
    print("OK modeled office: prefab, script references, shared Hub/Hub_Art model, UI fonts, world text shader")


def validate_csharp():
    from tree_sitter import Language, Parser
    import tree_sitter_c_sharp
    parser = Parser(Language(tree_sitter_c_sharp.language()))
    files = list((ROOT / "Assets").rglob("*.cs"))
    errors = []
    for path in files:
        tree = parser.parse(path.read_bytes())
        pending = [tree.root_node]
        while pending:
            node = pending.pop()
            if node.type == "ERROR" or node.is_missing:
                errors.append(f"{path.relative_to(ROOT)}:{node.start_point.row + 1}: {node.type}")
            pending.extend(node.children)
    assert not errors, "\n".join(errors)
    print(f"OK C# syntax: {len(files)} files parsed (not a Unity compilation)")


if __name__ == "__main__":
    args = argparse.ArgumentParser()
    args.add_argument("--csharp", action="store_true")
    options = args.parse_args()
    validate_content()
    validate_project()
    validate_modeled_office()
    if options.csharp:
        validate_csharp()
