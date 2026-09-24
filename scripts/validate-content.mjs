#!/usr/bin/env node
import { existsSync, readFileSync } from "node:fs";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";

const root = join(dirname(fileURLToPath(import.meta.url)), "..");
const candidates = [
  join(root, "Assets/Resources/Content/missions.json"),
  join(root, "content/missions.json"),
];
const file = candidates.find((path) => existsSync(path));
if (!file) {
  console.error("no se encontró missions.json");
  process.exit(1);
}
const data = JSON.parse(readFileSync(file, "utf8"));
const errors = [];

function need(cond, msg) {
  if (!cond) errors.push(msg);
}

need(data.title && Array.isArray(data.concepts) && Array.isArray(data.missions), "raíz incompleta");
need(data.concepts.length >= 6, "se esperan 6–8 conceptos");
need(data.missions.length >= 4, "se esperan tutorial + 3 misiones");

const conceptIds = new Set(data.concepts.map((c) => c.id));
for (const c of data.concepts) {
  need(c.id && c.name && c.definition && c.importance && c.defense, `concepto incompleto: ${c.id}`);
}

const icons = new Set(["map", "shield", "note", "mail", "key", "globe", "archive"]);
for (const m of data.missions) {
  need(m.id && m.brief && m.scope && m.report && Array.isArray(m.quiz) && m.quiz.length === 3, `misión incompleta: ${m.id}`);
  need(!/nmap|metasploit|sqlmap|hydra|burp/i.test(JSON.stringify(m)), `posible herramienta real en ${m.id}`);
  for (const e of m.evidence ?? []) {
    need(icons.has(e.icon), `icono desconocido ${e.icon} en ${m.id}/${e.id}`);
    need(conceptIds.has(e.concept), `concepto desconocido ${e.concept} en ${m.id}/${e.id}`);
    need(e.options?.length >= 2 && e.reasons?.length >= 2, `opciones cortas en ${m.id}/${e.id}`);
    need(e.answer >= 0 && e.answer < e.options.length, `answer fuera de rango ${m.id}/${e.id}`);
  }
}

if (errors.length) {
  console.error("contenido inválido:");
  for (const e of errors) console.error(" -", e);
  process.exit(1);
}
console.log(`ok · ${data.concepts.length} conceptos · ${data.missions.length} misiones · v${data.version}`);
