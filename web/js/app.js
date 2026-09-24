import { createInteractableBus } from "./interactables.js";
import { createSession } from "./session.js";
import { executeTerminal, parseCommand } from "./terminal.js";

const STORAGE_KEY = "academy-save-v1";
const CONTENT_URLS = [
  "../Assets/Resources/Content/missions.json",
  "/Assets/Resources/Content/missions.json",
  "../content/missions.json",
];

const bus = createInteractableBus();
window.BRT = { interactables: bus };

const el = {
  boot: document.querySelector("#boot"),
  office: document.querySelector("#office"),
  overlay: document.querySelector("#overlay"),
  glossary: document.querySelector("#glossary"),
  toast: document.querySelector("#toast"),
  board: document.querySelector("#board-tickets"),
  clock: document.querySelector("#hud-clock"),
  rank: document.querySelector("#hud-rank"),
  concepts: document.querySelector("#hud-concepts"),
};

const ui = {
  screen: "boot",
  evidenceId: null,
  step: "read",
  draftQ: -1,
  quizIndex: 0,
  lastScore: null,
  termLines: [],
  termHistory: [],
  termHistIdx: -1,
};

let session = null;
let clockTimer = 0;
let tickTimer = 0;

function persist() {
  if (!session) return;
  localStorage.setItem(STORAGE_KEY, JSON.stringify(session.save));
}

function restoreSave() {
  try {
    const raw = JSON.parse(localStorage.getItem(STORAGE_KEY) ?? "null");
    return raw && raw.version === 1 ? raw : null;
  } catch {
    return null;
  }
}

function esc(value) {
  return String(value ?? "").replace(/[&<>"']/g, (char) => {
    switch (char) {
      case "&":
        return "&amp;";
      case "<":
        return "&lt;";
      case ">":
        return "&gt;";
      case '"':
        return "&quot;";
      case "'":
        return "&#39;";
      default:
        return char;
    }
  });
}

function nl(value) {
  return esc(value).replace(/\n/g, "<br>");
}

function conceptById(id) {
  return session.content.concepts.find((item) => item.id === id);
}

function toast(message) {
  el.toast.textContent = message;
  el.toast.dataset.show = "1";
  window.setTimeout(() => {
    el.toast.dataset.show = "0";
  }, 2800);
}

function icon(name) {
  const paths = {
    map: "M3 6l6-2 6 2 6-2v14l-6 2-6-2-6 2V6zm6 0v14m6-12v14",
    shield: "M12 3l8 3v6c0 5-3.5 8.5-8 10-4.5-1.5-8-5-8-10V6l8-3z",
    note: "M6 4h9l5 5v11H6V4zm9 0v5h5",
    mail: "M3 6h18v12H3V6zm0 0l9 7 9-7",
    key: "M8 14a4 4 0 1 1 3.9-4.8L20 13v3h-3v2h-3v-4.2L11.8 12A4 4 0 0 1 8 14z",
    globe: "M12 3a9 9 0 1 0 0 18 9 9 0 0 0 0-18zm-9 9h18M12 3c3 3 4 6 4 9s-1 6-4 9c-3-3-4-6-4-9s1-6 4-9z",
    archive: "M4 6h16v4H4V6zm2 4v10h12V10M9 14h6",
  };
  const d = paths[name] ?? paths.note;
  return `<svg viewBox="0 0 24 24" aria-hidden="true"><path d="${d}" fill="none" stroke="currentColor" stroke-width="1.6" stroke-linejoin="round" stroke-linecap="round"/></svg>`;
}

function mission() {
  return session.active();
}

function currentEvidence() {
  return mission().evidence.find((item) => item.id === ui.evidenceId) ?? null;
}

function renderHud() {
  const total = session.content.concepts.length;
  el.concepts.textContent = `${session.save.unlocked.length}/${total} conceptos`;
  const done = session.completedCount();
  el.rank.textContent = done === 0 ? "Junior · escritorio 4" : `Analista · ${done} informes`;
}

function renderBoard() {
  el.board.innerHTML = session.content.missions
    .map((item) => {
      const locked = !session.isAvailable(item.id);
      const row = session.get(item.id);
      return `<button class="ticket accent-${esc(item.accent)}" data-id="ticket:${esc(item.id)}" ${
        locked ? "disabled" : ""
      }>
        <span class="ticket-num">${esc(item.number)}</span>
        <span class="ticket-body">
          <strong>${esc(item.title)}</strong>
          <em>${esc(item.client)}</em>
        </span>
        <span class="ticket-tag">${row.completed ? `OK ${row.score}` : locked ? "BLOQUEADO" : esc(item.type)}</span>
      </button>`;
    })
    .join("");
}

function openOverlay(html, { terminal = false } = {}) {
  el.overlay.hidden = false;
  el.overlay.innerHTML = html;
  if (terminal) bindTerminal();
  bindOverlay();
}

function closeOverlay() {
  el.overlay.hidden = true;
  el.overlay.innerHTML = "";
  ui.screen = "hub";
  renderBoard();
  renderHud();
}

function bindOverlay() {
  el.overlay.querySelectorAll("[data-act]").forEach((node) => {
    node.addEventListener("click", () => handleAct(node.dataset.act, node.dataset.arg));
  });
  el.overlay.querySelectorAll("[data-pick]").forEach((node) => {
    node.addEventListener("click", () => handlePick(node.dataset.pick, Number(node.dataset.i)));
  });
  const notes = el.overlay.querySelector("#analyst-note");
  if (notes) {
    notes.addEventListener("change", () => session.note(notes.value.slice(0, 4000)));
  }
}

function bindTerminal() {
  const input = el.overlay.querySelector("#term-input");
  if (!input) return;
  input.focus();
  input.addEventListener("keydown", (event) => {
    if (event.key === "Enter") {
      event.preventDefault();
      runTerminal(input.value);
      input.value = "";
      ui.termHistIdx = -1;
    } else if (event.key === "ArrowUp") {
      event.preventDefault();
      if (!ui.termHistory.length) return;
      ui.termHistIdx = Math.min(ui.termHistory.length - 1, ui.termHistIdx + 1);
      input.value = ui.termHistory[ui.termHistory.length - 1 - ui.termHistIdx];
    } else if (event.key === "ArrowDown") {
      event.preventDefault();
      if (ui.termHistIdx <= 0) {
        ui.termHistIdx = -1;
        input.value = "";
        return;
      }
      ui.termHistIdx -= 1;
      input.value = ui.termHistory[ui.termHistory.length - 1 - ui.termHistIdx];
    }
  });
}

function termPrint(text, kind = "out") {
  ui.termLines.push({ kind, text });
  if (ui.termLines.length > 80) ui.termLines.shift();
}

function termView() {
  return ui.termLines.map((line) => `<div class="term-${esc(line.kind)}">${nl(line.text)}</div>`).join("");
}

function startMission(id) {
  if (!session.start(id)) {
    toast("Todavía no te asignaron ese ticket.");
    return;
  }
  const item = mission();
  ui.evidenceId = item.evidence[0].id;
  ui.step = "read";
  ui.draftQ = -1;
  ui.quizIndex = 0;
  ui.termLines = [];
  ui.termHistory = [];
  termPrint(`sesión abierta · ${item.client}`, "sys");
  termPrint(item.mentor, "mentor");
  termPrint(executeTerminal(session, "help"), "out");
  persist();
  renderBrief();
}

function renderBrief() {
  const item = mission();
  ui.screen = "brief";
  openOverlay(`
    <article class="sheet accent-${esc(item.accent)}">
      <header class="sheet-head">
        <p>TICKET ${esc(item.number)} · ${esc(item.type)}</p>
        <h2>${esc(item.title)}</h2>
        <p class="lede">${esc(item.subtitle)} · ${esc(item.client)} · ${esc(item.duration)}</p>
      </header>
      <dl class="meta">
        <div><dt>Alcance</dt><dd>${esc(item.scope)}</dd></div>
        <div><dt>Objetivo</dt><dd>${esc(item.objective)}</dd></div>
      </dl>
      <p class="brief">${esc(item.brief)}</p>
      <blockquote>${esc(item.mentor)}</blockquote>
      <footer class="actions">
        <button type="button" data-act="hub" class="ghost">Volver al escritorio</button>
        <button type="button" data-act="lab" class="solid">Sentarse al laboratorio</button>
      </footer>
    </article>
  `);
}

function renderLab() {
  const item = mission();
  ui.screen = "lab";
  const evidence = currentEvidence();
  const list = item.evidence
    .map((row) => {
      const prog = session.evidenceProgress(row.id);
      const active = row.id === ui.evidenceId ? "active" : "";
      return `<button type="button" class="ev ${active}" data-act="inspect" data-arg="${esc(row.id)}">
        ${icon(row.icon)}
        <span><strong>${esc(row.label)}</strong><em>${esc(row.kind)}</em></span>
        <b>${prog.solved ? "DOC" : prog.inspected ? "LEÍDO" : "PEND"}</b>
      </button>`;
    })
    .join("");

  openOverlay(
    `
    <section class="lab accent-${esc(item.accent)}">
      <aside>
        <p class="kicker">${esc(item.client)}</p>
        <h3>${esc(item.title)}</h3>
        <div class="ev-list">${list}</div>
        <label class="note-box">Cuaderno
          <textarea id="analyst-note" maxlength="4000" placeholder="notes <texto> también escribe aquí">${esc(
            session.progress().note,
          )}</textarea>
        </label>
        <div class="lab-nav">
          <button type="button" data-act="glossary" class="ghost">Glosario</button>
          <button type="button" data-act="report" class="solid" ${session.readyToReport() ? "" : "disabled"}>Informe</button>
          <button type="button" data-act="hub" class="ghost">Escritorio</button>
        </div>
      </aside>
      <div class="term-wrap" data-id="terminal">
        <header><span>brt-term · laboratorio</span><span>${esc(item.commandHint)}</span></header>
        <div class="term-body" id="term-body">${termView()}</div>
        <label class="term-prompt"><span>junior@brt:~$</span>
          <input id="term-input" autocomplete="off" spellcheck="false" aria-label="Comando de laboratorio">
        </label>
      </div>
      <article class="doc">${renderDoc(evidence)}</article>
    </section>
  `,
    { terminal: true },
  );
  const body = el.overlay.querySelector("#term-body");
  if (body) body.scrollTop = body.scrollHeight;
}

function renderDoc(evidence) {
  if (!evidence) return "<p>Elige una pista.</p>";
  const prog = session.evidenceProgress(evidence.id);
  const concept = conceptById(evidence.concept);
  if (prog.solved && ui.step === "feedback") {
    return `
      <p class="kicker">${esc(evidence.kind)}</p>
      <h3>${esc(evidence.label)}</h3>
      <p class="src">${esc(evidence.source)}</p>
      <p>${nl(evidence.feedback)}</p>
      <p class="ok">Clasificación alineada con la evidencia.</p>
      ${concept ? `<div class="card-mini"><strong>${esc(concept.name)}</strong><span>${esc(concept.category)}</span><p>${esc(concept.defense)}</p></div>` : ""}
      <footer class="actions">
        <button type="button" data-act="next-ev" class="solid">Siguiente pista</button>
        <button type="button" data-act="open-concept" data-arg="${esc(evidence.concept)}" class="ghost">Leer la ficha de concepto</button>
      </footer>
    `;
  }
  if (ui.step === "reason") {
    return `
      <p class="kicker">Defensa</p>
      <h3>${esc(evidence.reasonQuestion)}</h3>
      <div class="choices">${evidence.reasons.map((opt, i) => `<button type="button" data-pick="reason" data-i="${i}">${esc(opt)}</button>`).join("")}</div>
    `;
  }
  if (ui.step === "question") {
    return `
      <p class="kicker">Clasificación</p>
      <h3>${esc(evidence.question)}</h3>
      <div class="choices">${evidence.options.map((opt, i) => `<button type="button" data-pick="question" data-i="${i}">${esc(opt)}</button>`).join("")}</div>
    `;
  }
  return `
    <p class="kicker">${esc(evidence.kind)}</p>
    <h3>${esc(evidence.label)}</h3>
    <p class="src">${esc(evidence.source)}</p>
    <div class="paper">${nl(evidence.body)}</div>
    <footer class="actions">
      <button type="button" data-act="ask" class="solid" ${prog.inspected ? "" : "disabled"}>Documentar hallazgo</button>
    </footer>
  `;
}

function inspectEvidence(id) {
  const found = mission().evidence.find(
    (item) => item.id === id || item.id.toLowerCase() === String(id).toLowerCase(),
  );
  if (!found) {
    toast("Esa pista no está en alcance.");
    return;
  }
  session.inspect(found.id);
  ui.evidenceId = found.id;
  ui.step = session.evidenceProgress(found.id).solved ? "feedback" : "read";
  ui.draftQ = -1;
  persist();
  renderLab();
}

function runTerminal(line) {
  const raw = String(line ?? "").trim();
  if (!raw) return;
  ui.termHistory.push(raw);
  termPrint(`junior@brt:~$ ${raw}`, "in");
  const { cmd, arg } = parseCommand(raw);
  const result = executeTerminal(session, raw);
  if (result && typeof result === "object" && result.clear) {
    ui.termLines = [];
    renderLab();
    return;
  }
  termPrint(String(result), "out");
  if (cmd === "inspect") {
    const found = mission().evidence.find((item) => item.id.toLowerCase() === arg.trim().toLowerCase());
    if (found) {
      ui.evidenceId = found.id;
      ui.step = session.evidenceProgress(found.id).solved ? "feedback" : "read";
    }
  }
  persist();
  renderLab();
}

function handlePick(kind, index) {
  const evidence = currentEvidence();
  switch (kind) {
    case "question":
      ui.draftQ = index;
      ui.step = "reason";
      renderLab();
      return;
    case "reason": {
      if (!evidence) return;
      const ok = session.submitEvidence(evidence.id, ui.draftQ, index);
      persist();
      if (!ok) {
        toast("No encaja con la evidencia. Relee la pista y documenta de nuevo.");
        ui.step = "read";
        ui.draftQ = -1;
        renderLab();
        return;
      }
      persist();
      ui.step = "feedback";
      renderLab();
      return;
    }
    case "report": {
      const ok = session.submitReport(index);
      persist();
      if (!ok) {
        toast("Ese cierre no está respaldado. Elige el informe que sí describe la evidencia.");
        renderReport();
        return;
      }
      ui.quizIndex = 0;
      renderQuiz();
      return;
    }
    case "quiz": {
      const ok = session.answerQuiz(ui.quizIndex, index);
      persist();
      if (!ok) {
        toast("Todavía no. Revisa la ficha del concepto y responde de nuevo.");
        renderQuiz();
        return;
      }
      if (ui.quizIndex + 1 < mission().quiz.length) {
        ui.quizIndex += 1;
        renderQuiz();
        return;
      }
      finishMission();
      return;
    }
    default:
      console.warn("pick no cableado", kind);
  }
}

function handleAct(act, arg) {
  switch (act) {
    case "hub":
      closeOverlay();
      return;
    case "lab":
      renderLab();
      return;
    case "inspect":
      inspectEvidence(arg);
      return;
    case "ask": {
      const evidence = currentEvidence();
      if (!evidence || !session.evidenceProgress(evidence.id).inspected) {
        toast("Primero inspecciona la pista (clic o inspect <id>).");
        return;
      }
      ui.step = "question";
      renderLab();
      return;
    }
    case "next-ev": {
      const ids = mission().evidence.map((item) => item.id);
      const idx = ids.indexOf(ui.evidenceId);
      const next = ids[idx + 1];
      if (next) inspectEvidence(next);
      else if (session.readyToReport()) renderReport();
      else toast("Quedan pistas por documentar.");
      return;
    }
    case "report":
      if (!session.readyToReport()) {
        toast("Documenta todas las pistas antes del informe.");
        renderLab();
        return;
      }
      renderReport();
      return;
    case "glossary":
      openGlossary(arg);
      return;
    case "open-concept":
      openGlossary(arg);
      return;
    case "review":
      session.review(arg);
      persist();
      openGlossary(arg);
      return;
    case "close-glossary":
      el.glossary.hidden = true;
      return;
    default:
      console.warn("acción no cableada", act);
  }
}

function renderReport() {
  const item = mission();
  ui.screen = "report";
  openOverlay(`
    <article class="sheet accent-${esc(item.accent)}">
      <header class="sheet-head">
        <p>INFORME · ${esc(item.client)}</p>
        <h2>Cierre respaldado por evidencia</h2>
        <p class="lede">${esc(item.report.prompt)}</p>
      </header>
      <div class="choices">${item.report.options.map((opt, i) => `<button type="button" data-pick="report" data-i="${i}">${esc(opt)}</button>`).join("")}</div>
    </article>
  `);
}

function renderQuiz() {
  const item = mission();
  const q = item.quiz[ui.quizIndex];
  ui.screen = "quiz";
  openOverlay(`
    <article class="sheet accent-${esc(item.accent)}">
      <header class="sheet-head">
        <p>QUIZ ${ui.quizIndex + 1}/${item.quiz.length}</p>
        <h2>${esc(q.question)}</h2>
      </header>
      <div class="choices">${q.options.map((opt, i) => `<button type="button" data-pick="quiz" data-i="${i}">${esc(opt)}</button>`).join("")}</div>
    </article>
  `);
}

function finishMission() {
  session.complete();
  persist();
  ui.lastScore = session.progress().score;
  const item = mission();
  ui.screen = "learned";
  const cards = item.concepts
    .map((id) => {
      const concept = conceptById(id);
      return `<div class="card-mini"><strong>${esc(concept.name)}</strong><span>${esc(concept.category)}</span><p>${esc(concept.definition)}</p><p class="def">${esc(concept.defense)}</p></div>`;
    })
    .join("");
  openOverlay(`
    <article class="sheet accent-${esc(item.accent)} learned">
      <header class="sheet-head">
        <p>CONCEPTO APRENDIDO</p>
        <h2>${esc(item.title)}</h2>
        <p class="lede">${esc(item.learned)}</p>
      </header>
      <div class="scoreline">
        <b>${ui.lastScore}</b>
        <ul>
          <li>Misma fórmula que Unity: primer intento, informe, quiz, fichas revisadas, tiempo.</li>
          <li>Abre el glosario para que las fichas cuenten en el score.</li>
        </ul>
      </div>
      <div class="cards">${cards}</div>
      <footer class="actions">
        <button type="button" data-act="glossary" class="ghost">Abrir glosario</button>
        <button type="button" data-act="hub" class="solid">Volver al escritorio</button>
      </footer>
    </article>
  `);
  renderHud();
  renderBoard();
}

function openGlossary(focusId) {
  const items = session.save.unlocked.map((id) => conceptById(id)).filter(Boolean);
  el.glossary.hidden = false;
  el.glossary.innerHTML = `
    <div class="glossary-panel">
      <header>
        <h2>Fichas de concepto</h2>
        <button type="button" data-act="close-glossary" class="ghost">Cerrar</button>
      </header>
      <div class="glossary-list">
        ${
          items.length
            ? items
                .map((concept) => {
                  const open = concept.id === focusId ? "open" : "";
                  const reviewed = session.save.reviewed.includes(concept.id);
                  return `<details ${open} class="g-card">
                    <summary>${esc(concept.name)} <em>${esc(concept.category)}</em></summary>
                    <p><strong>Qué es.</strong> ${esc(concept.definition)}</p>
                    <p><strong>Por qué importa.</strong> ${esc(concept.importance)}</p>
                    <p><strong>Defensa.</strong> ${esc(concept.defense)}</p>
                    ${
                      reviewed
                        ? "<p class=\"ok\">Ficha revisada.</p>"
                        : `<button type="button" data-act="review" data-arg="${esc(concept.id)}" class="solid">He leído y comprendido esta ficha</button>`
                    }
                  </details>`;
                })
                .join("")
            : "<p>Todavía no hay fichas. Documenta un hallazgo en el laboratorio.</p>"
        }
      </div>
    </div>
  `;
  persist();
  el.glossary.querySelectorAll("[data-act]").forEach((node) => {
    node.addEventListener("click", () => handleAct(node.dataset.act, node.dataset.arg));
  });
}

function useHub(id) {
  switch (id) {
    case "laptop":
      if (session.progress().started && !session.progress().completed) {
        renderLab();
        return;
      }
      toast("Toma un ticket de la pizarra o del buzón.");
      return;
    case "board":
      toast("WASD en Unity; aquí los tickets de la pizarra abren la misión.");
      return;
    case "mailbox": {
      const next = session.content.missions.find((item) => session.isAvailable(item.id) && !session.get(item.id).completed);
      if (next) startMission(next.id);
      else toast("No hay tickets pendientes.");
      return;
    }
    case "notes":
      openGlossary();
      return;
    case "usb":
      toast("Evidencia de mentira. En Unity el cuaderno y el buzón sí se agarran.");
      return;
    default:
      console.warn("interactable sin cablear", id);
  }
}

function wireHub() {
  const defs = [
    { id: "laptop", kind: "Laptop", label: "LAPTOP · Abrir terminal" },
    { id: "board", kind: "Board", label: "PIZARRA · Misiones y progreso" },
    { id: "mailbox", kind: "Tickets", label: "BUZÓN · Elegir misión" },
    { id: "notes", kind: "Notebook", label: "CUADERNO · Notas y hallazgos" },
    { id: "usb", kind: "prop", label: "USB de evidencia" },
  ];
  for (const def of defs) {
    bus.register({
      ...def,
      onUse() {
        useHub(def.id);
      },
    });
  }
  document.querySelectorAll(".interactable").forEach((node) => {
    const id = node.dataset.id;
    node.addEventListener("click", (event) => {
      if (id === "board" && event.target.closest("[data-id^='ticket:']")) return;
      bus.use(id);
    });
    node.addEventListener("mouseenter", () => bus.hover(id, true));
    node.addEventListener("mouseleave", () => bus.hover(id, false));
  });
  el.board.addEventListener("click", (event) => {
    const btn = event.target.closest("[data-id]");
    if (!btn || btn.disabled) return;
    bus.use(btn.dataset.id);
  });
  bus.on((event) => {
    if (event.action === "use" && event.id.startsWith("ticket:")) startMission(event.id.slice(7));
  });
}

function tickClock() {
  el.clock.textContent = new Date().toLocaleTimeString("es-CL", { hour: "2-digit", minute: "2-digit" });
}

async function loadData() {
  let lastError = null;
  for (const url of CONTENT_URLS) {
    try {
      const response = await fetch(url);
      if (response.ok) return await response.json();
      lastError = new Error(`HTTP ${response.status} ${url}`);
    } catch (error) {
      lastError = error;
    }
  }
  throw lastError ?? new Error("sin contenido");
}

function enterOffice() {
  el.boot.hidden = true;
  el.office.hidden = false;
  ui.screen = "hub";
  renderHud();
  renderBoard();
  tickClock();
  window.clearInterval(clockTimer);
  window.clearInterval(tickTimer);
  clockTimer = window.setInterval(tickClock, 1000);
  tickTimer = window.setInterval(() => {
    if (session.progress().started && !session.progress().completed) {
      session.tick(1);
      persist();
    }
  }, 1000);
}

async function init() {
  try {
    const content = await loadData();
    session = createSession(content, restoreSave(), persist);
  } catch (error) {
    document.querySelector("#boot-status").textContent =
      "No se pudo leer Assets/Resources/Content/missions.json. Sirve el repo desde la raíz.";
    console.error(error);
    return;
  }
  document.querySelector("#boot-title").textContent = session.content.title;
  document.querySelector("#boot-status").textContent = "sesión lista · laboratorio narrativo";
  document.querySelector("#enter").disabled = false;
  document.querySelector("#enter").addEventListener("click", enterOffice);
  document.addEventListener("keydown", (event) => {
    if (ui.screen === "boot" && event.key === "Enter") enterOffice();
    if (event.key === "Escape") el.glossary.hidden = true;
    if (event.key === "g" && event.altKey) {
      event.preventDefault();
      openGlossary();
    }
  });
  wireHub();
}

init();
