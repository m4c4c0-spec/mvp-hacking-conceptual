#!/usr/bin/env node
import { existsSync, readFileSync } from "node:fs";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";

const root = join(dirname(fileURLToPath(import.meta.url)), "..");
const file = join(root, "Godot/Game/Features/Quests/condor.json");
if (!existsSync(file)) {
  console.error("no se encontró condor.json");
  process.exit(1);
}
const raw = readFileSync(file, "utf8");
const data = JSON.parse(raw);
const errors = [];
const need = (cond, msg) => {
  if (!cond) errors.push(msg);
};
const puzzles = ["phish", "password", "mfa", "logs", "box", "traffic", "cable", "fire", "osint", "base64", "rssi", "hash", "kill"];
const concepts = ["phishing", "passwords", "mfa", "logs", "traffic", "containment", "failsafe", "osint", "obfuscation", "inventory", "integrity"];
const rooms = new Set(["oficina", "servidores", "laboratorio"]);
const solutions = {
  phish: { signals: ["dominio", "clave", "urgencia"] },
  password: { code: "Condores2026" },
  mfa: { token: "492017" },
  logs: { minute: "14" },
  box: { code: "4714" },
  traffic: { color: "azul" },
  cable: { color: "azul" },
  fire: { synced: true },
  osint: { pet: "Pixel" },
  base64: { key: "Pixel", decoded: true },
  rssi: { cell: "C-4" },
  hash: { discard: "C" },
  kill: { code: "7391" },
};
need(data.version === 3 && data.title && data.subtitle, "raíz v3 incompleta");
need(Array.isArray(data.concepts) && data.concepts.map((item) => item.id).join() === concepts.join(), "se esperan 11 conceptos");
need(Array.isArray(data.puzzles) && data.puzzles.map((item) => item.id).join() === puzzles.join(), "se esperan 13 pasos");
need(raw.includes("soporte-ti@ucd-ch1le.invalid"), "falta el dominio ficticio");
need(raw.includes("203.0.113.44"), "falta la IP de documentación");
need(!/nmap|metasploit|sqlmap|hydra|burp|mimikatz|hashcat/i.test(raw), "posible herramienta real");
need(Array.isArray(data.closing?.lessons) && data.closing.lessons.length === 10, "faltan las diez lecciones");
need(String(data.closing?.question || "").includes("cambiar mañana"), "falta la pregunta del debrief");
const byId = new Map(data.concepts.map((item) => [item.id, item]));
for (const puzzle of data.puzzles) {
  const concept = byId.get(puzzle.concept);
  need(concept?.id && concept.name && concept.definition, `concepto incompleto en ${puzzle.id}`);
  need(rooms.has(puzzle.room), `sala desconocida en ${puzzle.id}`);
  need(typeof puzzle.seconds === "number" && puzzle.seconds > 0, `tiempo inválido en ${puzzle.id}`);
  need(puzzle.manual && puzzle.debrief && puzzle.objective, `textos incompletos en ${puzzle.id}`);
  need(Array.isArray(puzzle.hints) && puzzle.hints.length === 3 && puzzle.hints.every((item) => typeof item === "string" && item.trim()), `se esperan 3 pistas en ${puzzle.id}`);
  if (concept && puzzle.manual) need(puzzle.manual.slice(0, puzzle.manual.indexOf(".") + 1) === concept.definition, `la primera frase no define el concepto en ${puzzle.id}`);
  need(JSON.stringify(puzzle.solution) === JSON.stringify(solutions[puzzle.id]), `solución distinta en ${puzzle.id}`);
}
if (errors.length) {
  console.error("contenido inválido:");
  for (const error of errors) console.error(" -", error);
  process.exit(1);
}
console.log(`ok · ${data.concepts.length} conceptos · ${data.puzzles.length} pasos · v${data.version}`);
