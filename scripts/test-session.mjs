import { readFileSync } from "node:fs";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";
import { createSession } from "../web/js/session.js";
import { executeTerminal } from "../web/js/terminal.js";

const data = JSON.parse(
  readFileSync(join(dirname(fileURLToPath(import.meta.url)), "../Assets/Resources/Content/missions.json"), "utf8"),
);

function session() {
  return createSession(data);
}

function solveEvidence(s) {
  for (const e of s.active().evidence) {
    s.inspect(e.id);
    if (!s.submitEvidence(e.id, e.answer, e.reasonAnswer)) throw new Error("no resolvió " + e.id);
  }
}

function finish(s) {
  solveEvidence(s);
  if (!s.submitReport(s.active().report.answer)) throw new Error("informe");
  for (let i = 0; i < s.active().quiz.length; i++) {
    if (!s.answerQuiz(i, s.active().quiz[i].answer)) throw new Error("quiz " + i);
  }
  for (const id of s.active().concepts) s.review(id);
  if (!s.complete()) throw new Error("complete");
}

function assert(cond, msg) {
  if (!cond) {
    console.error(msg);
    process.exit(1);
  }
}

{
  const s = session();
  assert(s.isAvailable("recon") && !s.start("web") && s.completedCount() === 0, "lock");
}
{
  const s = session();
  const e = s.active().evidence[0];
  assert(!s.submitEvidence(e.id, e.answer, e.reasonAnswer), "sin start");
  s.start("recon");
  assert(!s.submitEvidence(e.id, e.answer, e.reasonAnswer), "sin inspect");
  s.inspect(e.id);
  assert(s.submitEvidence(e.id, e.answer, e.reasonAnswer), "con inspect");
}
{
  const s = session();
  s.start("recon");
  const e = s.active().evidence[0];
  s.inspect(e.id);
  assert(!s.submitEvidence(e.id, e.answer, (e.reasonAnswer + 1) % e.reasons.length), "defensa mala");
  assert(s.save.unlocked.length === 0, "no desbloquea");
}
{
  const s = session();
  s.start("recon");
  const e = s.active().evidence[0];
  s.inspect(e.id);
  s.submitEvidence(e.id, -1, 99);
  assert(s.evidenceProgress(e.id).attempts === 0, "intentos inválidos");
}
{
  const s = session();
  s.start("recon");
  const e = s.active().evidence[0];
  s.inspect(e.id);
  s.submitEvidence(e.id, e.answer, e.reasonAnswer);
  s.submitEvidence(e.id, e.answer, e.reasonAnswer);
  assert(s.evidenceProgress(e.id).attempts === 1, "idempotente");
}
{
  const s = session();
  s.review("mfa");
  assert(s.save.reviewed.length === 0, "review bloqueado");
}
{
  const s = session();
  for (const m of data.missions) {
    assert(s.start(m.id), "start " + m.id);
    finish(s);
    assert(s.progress().score === 100, "perfecto " + m.id + " = " + s.progress().score);
  }
  assert(s.completedCount() === 4 && s.save.unlocked.length === 8 && s.save.reviewed.length === 8, "demo completa");
}
{
  const s = session();
  s.start("recon");
  const e = s.active().evidence[0];
  s.inspect(e.id);
  s.submitEvidence(e.id, (e.answer + 1) % e.options.length, e.reasonAnswer);
  finish(s);
  assert(s.progress().score === 85, "corrección evidencia " + s.progress().score);
}
{
  const s = session();
  s.start("recon");
  solveEvidence(s);
  s.submitReport(s.active().report.answer);
  for (let i = 0; i < 3; i++) s.answerQuiz(i, (s.active().quiz[i].answer + 1) % 3);
  assert(!s.complete(), "quiz mal");
  for (let i = 0; i < 3; i++) s.answerQuiz(i, s.active().quiz[i].answer);
  assert(s.complete() && s.progress().score === 65, "quiz corregido " + s.progress().score);
}
{
  const s = session();
  s.start("recon");
  s.tick(900);
  finish(s);
  assert(s.progress().score === 98, "lento " + s.progress().score);
}
{
  const s = session();
  s.start("recon");
  executeTerminal(s, "inspect lumen");
  assert(executeTerminal(s, "inspect https://example.invalid").toLowerCase().includes("no encontrada"), "inspect url");
  assert(executeTerminal(s, "curl https://example.invalid").includes("fuera del vocabulario"), "curl");
  assert(executeTerminal(s, "scan; arbitrary").includes("fuera del vocabulario"), "scan;");
  executeTerminal(s, "notes primer hallazgo");
  executeTerminal(s, "notes segundo hallazgo");
  assert(s.progress().note === "primer hallazgo\nsegundo hallazgo", "notes " + JSON.stringify(s.progress().note));
}
{
  const s = session();
  assert(String(executeTerminal(s, "clear")).includes("Acepta primero"), "clear sin ticket");
  s.start("recon");
  assert(executeTerminal(s, "clear").clear === true, "clear con ticket");
}

console.log("ok · session + terminal alineados a SessionTests.cs");
