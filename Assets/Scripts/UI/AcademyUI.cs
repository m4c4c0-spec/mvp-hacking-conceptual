using System;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif
using static Academy.UIFactory;

namespace Academy
{
    public sealed class AcademyUI : MonoBehaviour
    {
        private AcademySession session;
        private RectTransform shell, body, content, navigation, brand;
        private Text status, officePrompt;
        private Text crosshair;
        private Text objectiveHUD;
        private AcademyInteractable inspectedObject;
        private ScrollRect scroll;
        private string page = "Inicio", selectedEvidence, toast = "";
        private string terminal = "ACADEMY OS  /  SESIÓN LOCAL\n\nBienvenido, analista.\nEste escritorio contiene un laboratorio narrativo.\nEscribe help para consultar el vocabulario del simulador.\n";
        private AudioSource audioSource;
        private AudioClip chime;
        private int quizIndex;
        private bool resetConfirm;
        public bool IsOffice => page == "Oficina";
        public bool ReducedMotion => session.Save.reducedMotion;
        public bool IsPaused { get; private set; }
        public event Action ResetRequested;

        public void Initialize(AcademySession value)
        {
            session = value;
            var canvas = gameObject.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = gameObject.AddComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1600, 1000); scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight; scaler.matchWidthOrHeight = .5f;
            gameObject.AddComponent<GraphicRaycaster>();
            if (FindFirstObjectByType<EventSystem>() == null)
            {
                var es = new GameObject("EventSystem"); es.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
                es.AddComponent<InputSystemUIInputModule>();
#else
                es.AddComponent<StandaloneInputModule>();
#endif
            }
            audioSource = gameObject.AddComponent<AudioSource>();
            const int count = 9000;
            var samples = new float[count];
            for (int i = 0; i < count; i++) samples[i] = Mathf.Sin(i * 2 * Mathf.PI * 660 / 44100) * Mathf.Exp(-i / 1600f) * .12f;
            chime = AudioClip.Create("Soft notification", count, 1, 44100, false); chime.SetData(samples, 0);
            var header = Column("Brand", transform, 22, 3, Ink); brand = header;
            header.anchorMin = new Vector2(0, 1); header.anchorMax = Vector2.one;
            header.offsetMin = new Vector2(0, -108); header.offsetMax = Vector2.zero;
            Label(header, "B / R     ANALYST ACADEMY", 27, White, true);
            status = Label(header, "", 17, Mint);
            navigation = Row("Navigation", transform, 6);
            navigation.anchorMin = new Vector2(0, 1); navigation.anchorMax = Vector2.one;
            navigation.offsetMin = new Vector2(22, -176); navigation.offsetMax = new Vector2(-22, -120);
            shell = Rect("Workspace", transform); Fill(shell, 22, 190, 22, 64); Background(shell, Ink);
            body = Rect("Body", shell); Fill(body);
            officePrompt = Label(transform, "", 20, Mint);
            officePrompt.rectTransform.anchorMin = Vector2.zero; officePrompt.rectTransform.anchorMax = new Vector2(1, 0);
            officePrompt.rectTransform.offsetMin = new Vector2(26, 10); officePrompt.rectTransform.offsetMax = new Vector2(-26, 62);
            crosshair = Label(transform, "+", 28, Mint);
            crosshair.rectTransform.anchorMin = new Vector2(.5f, .5f); crosshair.rectTransform.anchorMax = new Vector2(.5f, .5f);
            crosshair.rectTransform.sizeDelta = new Vector2(32, 32); crosshair.alignment = TextAnchor.MiddleCenter;
            objectiveHUD = Label(transform, "", 21, White, true);
            objectiveHUD.rectTransform.anchorMin = new Vector2(0, 1); objectiveHUD.rectTransform.anchorMax = new Vector2(1, 1);
            objectiveHUD.rectTransform.offsetMin = new Vector2(28, -96); objectiveHUD.rectTransform.offsetMax = new Vector2(-28, -24);
            Show("Inicio");
        }
        private void Update()
        {
            if (DesktopInput.Escape)
            {
                if (IsOffice) { IsPaused = true; Show("Pausa"); }
                else if (IsPaused) { IsPaused = false; Show("Oficina"); }
                else { IsPaused = true; Show("Pausa"); }
            }
        }
        private void Sound() { if (!session.Save.muted) audioSource.PlayOneShot(chime); }
        public void SetOfficePrompt(string text) { if (IsOffice) officePrompt.text = text; }
        public void SetPointerMode(bool free)
        {
            if (!IsOffice) return;
            navigation.gameObject.SetActive(free); brand.gameObject.SetActive(free);
            crosshair.gameObject.SetActive(!free); objectiveHUD.gameObject.SetActive(!free);
        }
        public void OpenTicket(string missionId)
        {
            if (!session.IsAvailable(missionId)) { toast = "Completa el caso anterior para aceptar este ticket."; Show("Misiones"); return; }
            session.Start(missionId);
            selectedEvidence = session.Active.evidence.FirstOrDefault(e => !session.Evidence(e.id).solved)?.id ?? session.Active.evidence[0].id;
            toast = "Ticket aceptado. Puedes explorar la oficina y el archivo para recoger pistas, o consultar el expediente.";
            Sound(); Show(session.Progress.completed ? "Cierre" : "Evidencia");
        }
        public void InspectObject(AcademyInteractable target)
        {
            inspectedObject = target;
            if (target.action == InteractionAction.Evidence && session.Progress.started)
            {
                int slot = Mathf.Clamp(target.evidenceSlot, 0, session.Active.evidence.Length - 1);
                selectedEvidence = string.IsNullOrEmpty(target.evidenceId) ? session.Active.evidence[slot].id : target.evidenceId;
                toast = "Pista recogida en el entorno: " + target.label;
                Show("Evidencia"); return;
            }
            Show("Inspección");
        }
        public void Interact(AcademyInteractable target)
        {
            if (!string.IsNullOrEmpty(target.missionId)) { OpenTicket(target.missionId); return; }
            if (target.action == InteractionAction.Evidence) { InspectObject(target); return; }
            if (target.action == InteractionAction.Environment && target.GetComponent<OfficeMechanism>() == null) { InspectObject(target); return; }
            Interact(target.action);
        }
        public void Interact(InteractionAction action)
        {
            Sound();
            switch (action)
            {
                case InteractionAction.Laptop: Show("Terminal"); break;
                case InteractionAction.Notebook: Show("Notas"); break;
                case InteractionAction.Environment: return;
                default: Show("Misiones"); break;
            }
        }
        private void Show(string target)
        {
            page = target;
            if (page != "Pausa") IsPaused = false;
            shell.gameObject.SetActive(!IsOffice);
            crosshair.gameObject.SetActive(IsOffice);
            brand.gameObject.SetActive(!IsOffice); navigation.gameObject.SetActive(!IsOffice);
            objectiveHUD.gameObject.SetActive(IsOffice);
            objectiveHUD.text = session.Progress.started ? "CASO " + session.Active.number + " / " + session.Active.title + "\n" + session.EvidenceCountText() + "  ·  Pistas en el archivo →" : "PRIMER DÍA / Acércate a la pizarra y acepta el ticket 01.\nE · Interactuar     F · Inspeccionar     TAB · Escritorio";
            status.text = "CONSULTORA / PROGRAMA JUNIOR     •     " + session.Completed + "/" + session.Content.missions.Length + " MISIONES     •     " + session.Save.unlocked.Count + "/" + session.Content.concepts.Length + " CONCEPTOS";
            Clear(navigation);
            foreach (string tab in new[] { "Inicio", "Oficina", "Misiones", "Terminal", "Notas", "Glosario", "Informe" })
            {
                string name = tab; Button(navigation, name, () => { toast = ""; Show(name); }, page == name, true, 54);
            }
            officePrompt.text = "LABORATORIO FICTICIO     /     Sin conexiones a sistemas externos     /     ESC · Pausa";
            if (IsOffice) return;
            Clear(body); content = Scroll(body, out scroll);
            if (!string.IsNullOrEmpty(toast)) Label(content, toast, 21, Amber, true);
            if (!string.IsNullOrEmpty(LocalStorage.LastError)) Label(content, LocalStorage.LastError, 18, Amber);
            switch (page)
            {
                case "Inicio": Home(); break;
                case "Misiones": Missions(); break;
                case "Evidencia": EvidenceView(); break;
                case "Terminal": TerminalView(); break;
                case "Notas": Notes(); break;
                case "Glosario": Glossary(); break;
                case "Informe": Report(); break;
                case "Quiz": Quiz(); break;
                case "Cierre": Debrief(); break;
                case "Pausa": Pause(); break;
                case "Inspección": ObjectView(); break;
            }
        }
        private RectTransform Card(string eyebrow, string title)
        {
            var card = Column(title, content, 24, 13, Panel);
            Label(card, eyebrow.ToUpperInvariant(), 16, Mint, true);
            Label(card, title, 29, White, true); return card;
        }
        private void Title(string eyebrow, string title, string description)
        {
            Label(content, eyebrow.ToUpperInvariant(), 17, Mint, true);
            Label(content, title, 43, White, true);
            if (!string.IsNullOrEmpty(description)) Label(content, description, 23, Muted);
        }
        private void Home()
        {
            Title("Tu primer día / Blue & Red Team Trainer", "Observa. Conecta. Protege.", "Eres el nuevo analista de la consultora. Investiga sistemas ficticios, explica tus hallazgos y ayuda a sus equipos a defenderse.");
            var hero = Card("Una pequeña oficina. Cuatro casos.", session.Completed == session.Content.missions.Length ? "Buen trabajo, analista." : "La mejor herramienta es tu criterio.");
            Label(hero, "Demo estimada: 10–15 minutos. Cada caso conecta una observación con una defensa. Puedes equivocarte, consultar el glosario y corregir tu informe.");
            var actions = Row("Start actions", hero);
            Button(actions, session.Completed == session.Content.missions.Length ? "Ver resultados" : "Abrir buzón de misiones  →", () => Show("Misiones"), true);
            Button(actions, "Entrar a la oficina  ↗", () => Show("Oficina"));
            var guide = Card("Cómo se juega", "De la pista a la recomendación");
            Label(guide, "01   Acepta un ticket y lee el alcance.\n02   Inspecciona pistas con la terminal o el expediente.\n03   Registra una observación y una defensa por evidencia.\n04   Consulta las fichas, entrega el informe y supera el quiz.");
            Label(guide, "Progreso local automático. Entra a la oficina para jugar en primera persona: WASD, mouse para mirar, E para usar, G para tomar y F para inspeccionar. TAB libera el cursor.", 20, Muted);
            if (session.Completed == session.Content.missions.Length) FinalCard();
        }
        private void Missions()
        {
            Title("Buzón de tickets", "Tu turno, analista.", "Los casos se desbloquean al cerrar la misión anterior. Elige uno para leer el alcance o retomar tu investigación.");
            foreach (var mission in session.Content.missions)
            {
                var m = mission; var p = session.Get(m.id); bool available = session.IsAvailable(m.id);
                var card = Card("CASO " + m.number + " / " + m.type + " / " + m.duration, m.title);
                Label(card, m.client + "  ·  " + m.subtitle, 21, Amber);
                Label(card, m.brief);
                Label(card, "Alcance: " + m.scope, 20, Muted);
                if (p.started) Label(card, p.completed ? "CERRADO · " + p.score + "/100" : p.evidence.Count(e => e.solved) + "/" + m.evidence.Length + " evidencias resueltas", 19, Mint);
                Button(card, p.completed ? "Consultar cierre" : available ? p.started ? "Continuar investigación  →" : "Aceptar ticket  →" : "Completa el caso anterior", () =>
                {
                    if (!session.Start(m.id)) return;
                    Sound(); selectedEvidence = m.evidence.FirstOrDefault(e => !session.Evidence(e.id).solved)?.id ?? m.evidence[0].id;
                    Show(p.completed ? "Cierre" : "Evidencia");
                }, available, available);
            }
            if (session.Completed == session.Content.missions.Length) FinalCard();
        }
        private bool RequireMission()
        {
            if (session.Progress.started) return true;
            Title("Sin ticket activo", "Tu primer caso te espera.", "Acepta una misión para comenzar a registrar evidencias.");
            Button(content, "Abrir misiones", () => Show("Misiones"), true); return false;
        }
        private void EvidenceView()
        {
            if (!RequireMission()) return;
            var m = session.Active;
            var e = m.evidence.FirstOrDefault(x => x.id == selectedEvidence) ?? m.evidence[0]; selectedEvidence = e.id;
            session.Inspect(e.id); var p = session.Evidence(e.id);
            Title("Caso " + m.number + " / " + m.client, m.title, m.objective);
            var mentor = Card("Mara / Analista senior", "Una pista antes de empezar");
            Label(mentor, m.mentor, 21); Label(mentor, "Alcance: " + m.scope, 18, Muted);
            var tabs = Row("Evidence tabs", content, 8);
            foreach (var item in m.evidence)
            {
                string id = item.id;
                Button(tabs, (session.Evidence(id).solved ? "✓ " : "○ ") + item.label, () => { selectedEvidence = id; toast = ""; Show("Evidencia"); }, id == e.id, true, 78);
            }
            var card = Card(e.kind + " / " + (Array.IndexOf(m.evidence, e) + 1) + " de " + m.evidence.Length, e.label);
            Label(card, e.source, 18, Mint); Label(card, e.body, 23);
            if (p.solved)
            {
                Label(card, "✓ Observación documentada", 23, Mint, true);
                Label(card, e.options[p.selected]); Label(card, "Defensa / siguiente paso: " + e.reasons[p.reason]);
                Label(card, e.feedback, 21, Muted);
                Button(card, "Leer la ficha de concepto  →", () => Show("Glosario"));
            }
            else
            {
                int choice = p.selected, reason = p.reason;
                Label(card, e.question, 24, White, true);
                Choices(card, e.options, choice, value => choice = value);
                Label(card, e.reasonQuestion, 24, White, true);
                Choices(card, e.reasons, reason, value => reason = value);
                var message = Label(card, "Selecciona una observación y una recomendación.", 19, Muted);
                Button(card, "Registrar hallazgo  →", () =>
                {
                    if (choice < 0 || reason < 0) { message.text = "Falta seleccionar una opción en cada grupo."; message.color = Amber; return; }
                    bool correct = session.SubmitEvidence(e.id, choice, reason);
                    if (correct) { Sound(); toast = "Hallazgo validado. Nueva ficha disponible en el glosario."; Show("Evidencia"); }
                    else { message.text = "Revisa tu decisión: " + e.feedback + " Puedes volver a intentarlo."; message.color = Amber; }
                }, true);
            }
            var next = m.evidence.FirstOrDefault(x => !session.Evidence(x.id).solved && x.id != e.id);
            if (next != null) Button(content, "Siguiente evidencia  →", () => { selectedEvidence = next.id; toast = ""; Show("Evidencia"); });
            Button(content, session.ReadyToReport ? "Preparar informe final  →" : "Ver progreso del informe", () => Show("Informe"), session.ReadyToReport);
        }
        private void Choices(Transform parent, string[] options, int selected, Action<int> onSelect, bool enabled = true)
        {
            var buttons = new Button[options.Length];
            for (int i = 0; i < options.Length; i++)
            {
                int index = i;
                buttons[i] = Button(parent, (char)('A' + i) + "   " + options[i], () =>
                {
                    foreach (var button in buttons) button.image.color = Raised;
                    buttons[index].image.color = new Color(.14f, .38f, .34f); onSelect(index);
                }, false, enabled, 72);
                buttons[i].image.color = selected == i ? new Color(.14f, .38f, .34f) : Raised;
            }
        }
        private void TerminalView()
        {
            Title("Academy OS / Terminal narrativa", "Una ventana a las pistas.", "Comandos del juego: scan, map, inspect, simulate, notes, report. help muestra la ayuda.");
            var card = Card("Sesión local / " + session.Active.client, ">_ terminal");
            var output = Label(card, terminal, 21, Mint);
            var input = Input(card, "Escribe help y pulsa Intro…", "", false, 500);
            Action send = () =>
            {
                string command = input.text.Trim(); if (command.Length == 0) return;
                if (command.Equals("clear", StringComparison.OrdinalIgnoreCase)) terminal = "ACADEMY OS / Pantalla limpia.\n";
                else terminal += "\nanalista > " + command + "\n" + FictionTerminal.Execute(session, command) + "\n";
                if (terminal.Length > 16000) terminal = terminal.Substring(terminal.Length - 16000);
                output.text = terminal; input.text = ""; input.ActivateInputField();
                Canvas.ForceUpdateCanvases(); scroll.verticalNormalizedPosition = 0;
            };
            input.onEndEdit.AddListener(_ => { if (DesktopInput.Enter) send(); });
            Button(card, "Ejecutar comando ficticio", () => send(), true);
            Button(content, "Abrir expediente de la misión  →", () => Show(session.Progress.started ? "Evidencia" : "Misiones"));
            input.ActivateInputField();
        }
        private void Notes()
        {
            if (!RequireMission()) return;
            Title("Cuaderno / " + session.Active.client, "Haz visible tu razonamiento.", "Separa lo observado de lo que falta confirmar. Tus notas se incluyen en el informe exportado.");
            var card = Card("Notas de campo", session.Active.title);
            var input = Input(card, "Observé… / Falta confirmar… / Recomiendo…", session.Progress.note, true);
            input.onValueChanged.AddListener(value => session.Progress.note = value);
            input.onEndEdit.AddListener(value => session.Note(value));
            Button(card, "Guardar notas", () => { session.Note(input.text); toast = "Notas guardadas."; Show("Notas"); }, true);
            foreach (var e in session.Active.evidence.Where(x => session.Evidence(x.id).solved))
            {
                var entry = Card("Hallazgo validado", e.label);
                Label(entry, e.options[e.answer]); Label(entry, e.reasons[e.reasonAnswer], 21, Mint);
            }
        }
        private void Glossary()
        {
            Title("Biblioteca de conceptos", "Comprender es la recompensa.", "Resuelve evidencias para desbloquear fichas. Lee cada una y marca la revisión para sumar al aprendizaje del caso.");
            foreach (var c in session.Content.concepts)
            {
                bool unlocked = session.Save.unlocked.Contains(c.id);
                var card = Card(c.category + (unlocked ? " / DESBLOQUEADO" : " / PENDIENTE"), c.name);
                if (!unlocked) { Label(card, "Esta ficha se desbloquea al validar una evidencia relacionada.", 20, Muted); continue; }
                Label(card, "QUÉ ES", 16, Mint, true); Label(card, c.definition);
                Label(card, "POR QUÉ IMPORTA", 16, Amber, true); Label(card, c.importance);
                Label(card, "CÓMO DEFENDERLO", 16, Mint, true); Label(card, c.defense);
                bool reviewed = session.Save.reviewed.Contains(c.id);
                Button(card, reviewed ? "✓ Ficha revisada" : "He leído y comprendido esta ficha", () => { session.Review(c.id); toast = "Concepto revisado: " + c.name; Show("Glosario"); }, !reviewed, !reviewed);
            }
            Button(content, "Volver a la investigación", () => Show(session.Progress.started ? "Evidencia" : "Misiones"));
        }
        private void Report()
        {
            if (!RequireMission()) return;
            if (session.Progress.completed) { Debrief(); return; }
            Title("Informe / " + session.Active.client, "La evidencia necesita contexto.", "Un informe útil describe hechos y propone defensas proporcionadas.");
            var card = Card("Control de calidad", session.EvidenceCountText());
            foreach (var e in session.Active.evidence)
                Label(card, (session.Evidence(e.id).solved ? "✓ " : "○ ") + e.label, 21, session.Evidence(e.id).solved ? Mint : Muted);
            if (!session.ReadyToReport)
            {
                Button(card, "Completar evidencias pendientes", () => { selectedEvidence = session.Active.evidence.First(e => !session.Evidence(e.id).solved).id; Show("Evidencia"); }, true); return;
            }
            var report = session.Active.report;
            var closing = Card("Conclusión", report.prompt);
            if (session.Progress.reportAccepted)
            {
                Label(closing, "✓ " + report.options[report.answer], 22, Mint);
                Button(closing, "Ir al quiz de tres preguntas  →", () => { quizIndex = 0; Show("Quiz"); }, true);
            }
            else
            {
                int selected = session.Progress.reportChoice;
                Choices(closing, report.options, selected, v => selected = v);
                var feedback = Label(closing, "Elige una conclusión sustentada por las fichas.", 19, Muted);
                Button(closing, "Entregar informe", () =>
                {
                    if (selected < 0) { feedback.text = "Selecciona una conclusión antes de entregar."; return; }
                    if (session.SubmitReport(selected)) { toast = "Informe aceptado. Falta el quiz de cierre."; Sound(); Show("Informe"); }
                    else { feedback.text = "Revisa el alcance: no declares compromisos que la evidencia no demuestra. Conecta cada observación con una defensa."; feedback.color = Amber; }
                }, true);
            }
            int reviewed = session.Active.concepts.Count(c => session.Save.reviewed.Contains(c));
            Button(content, "Revisar glosario · " + reviewed + "/" + session.Active.concepts.Length + " fichas leídas", () => Show("Glosario"));
        }
        private void Quiz()
        {
            if (!RequireMission()) return;
            if (!session.Progress.reportAccepted) { Report(); return; }
            if (session.Progress.completed) { Debrief(); return; }
            quizIndex = Mathf.Clamp(quizIndex, 0, session.Active.quiz.Length - 1);
            var q = session.Active.quiz[quizIndex]; bool passed = session.Progress.quizAnswers[quizIndex] == q.answer;
            Title("Comprobación de aprendizaje", "¿Qué te llevas del caso?", "Pregunta " + (quizIndex + 1) + " de " + session.Active.quiz.Length + ". Puedes revisar y corregir cada respuesta.");
            var card = Card("Concepto aprendido", q.question);
            int selected = session.Progress.quizAnswers[quizIndex];
            Choices(card, q.options, selected, v => selected = v, !passed);
            var feedback = Label(card, passed ? "✓ " + q.explanation : "Selecciona tu respuesta.", 22, passed ? Mint : Muted);
            if (!passed) Button(card, "Comprobar respuesta", () =>
            {
                if (selected < 0) { feedback.text = "Selecciona una respuesta."; return; }
                if (session.AnswerQuiz(quizIndex, selected)) { toast = "Respuesta correcta."; Sound(); Show("Quiz"); }
                else { feedback.text = "Todavía no. " + q.explanation + " Vuelve a intentarlo."; feedback.color = Amber; }
            }, true);
            else if (quizIndex < session.Active.quiz.Length - 1) Button(card, "Siguiente pregunta  →", () => { quizIndex++; toast = ""; Show("Quiz"); }, true);
            else if (session.QuizPassed)
            {
                Button(card, "Cerrar misión y ver resultados  →", () => { if (session.Complete()) { toast = ""; Sound(); Show("Cierre"); } }, true);
                Button(card, "Revisar glosario antes de cerrar", () => Show("Glosario"));
            }
        }
        private void Debrief()
        {
            if (!RequireMission()) return;
            if (!session.Progress.completed) { Report(); return; }
            var p = session.Progress;
            Title("Caso " + session.Active.number + " / Cerrado", "Concepto aprendido.", session.Active.learned);
            var card = Card("Informe aprobado / " + session.Active.client, p.score + " / 100   ·   " + (p.score >= 85 ? "Criterio sólido" : p.score >= 65 ? "Buen aprendizaje" : "Aprendizaje en progreso"));
            Label(card, "Evidencias: " + session.Active.evidence.Length + " verificadas   ·   Quiz: " + session.Active.quiz.Length + "/" + session.Active.quiz.Length + "   ·   Tiempo activo: " + FormatTime(p.seconds));
            Label(card, "Puntuación: precisión inicial de evidencias (45), informe (15), quiz (25), revisión del glosario (10) y tiempo razonable (5). Corregir errores permite completar siempre la misión.", 19, Muted);
            Label(card, session.Active.report.options[session.Active.report.answer], 23, Mint);
            Button(card, "Exportar mi informe (.txt)", Export, true);
            Button(card, "Consultar conceptos y defensas", () => Show("Glosario"));
            if (session.Completed < session.Content.missions.Length) Button(content, "Volver al buzón · siguiente misión  →", () => Show("Misiones"), true);
            else FinalCard();
        }
        private void Export()
        {
            try { toast = "Informe guardado en: " + LocalStorage.Export(session); }
            catch (Exception ex) { toast = "No se pudo exportar el informe: " + ex.Message; }
            Show(session.Progress.completed ? "Cierre" : "Informe");
        }
        private void FinalCard()
        {
            var card = Card("Fin de la demo", "Tu primer turno, completado.");
            Label(card, "Cuatro casos cerrados. Ocho conceptos para explicar y defender. El siguiente turno: redes, permisos, ingeniería social avanzada y forense ligera.");
            Label(card, "Esta edición es un prototipo local. La página de Steam/itch aún no está conectada.", 20, Muted);
            Button(card, "Dejar feedback local", () =>
            {
                toast = "Escribe tu feedback en las notas y guárdalo con el botón de abajo. No se envía a ningún servidor.";
                Show("Notas");
                Button(content, "Guardar feedback en un archivo", () =>
                {
                    try
                    {
                        string path = System.IO.Path.Combine(Application.persistentDataPath, "feedback-" + DateTime.Now.ToString("yyyyMMdd-HHmmss-fff") + ".txt");
                        System.IO.File.WriteAllText(path, session.Progress.note);
                        toast = "Feedback guardado: " + path;
                    }
                    catch (Exception ex) { toast = "No se pudo guardar: " + ex.Message; }
                    Show("Notas");
                }, true);
            });
        }
        private void Pause()
        {
            Title("Sesión en pausa", "Tómate tu tiempo.", "El contador está detenido. Las decisiones ya registradas se guardan automáticamente.");
            Button(content, "Continuar en primera persona", () => Show("Oficina"), true);
            Button(content, session.Save.muted ? "Activar sonidos" : "Silenciar sonidos", () => { session.Save.muted = !session.Save.muted; session.Notify(); Show("Pausa"); });
            Button(content, session.Save.reducedMotion ? "Activar balanceo suave al caminar" : "Reducir movimiento de cámara", () => { session.Save.reducedMotion = !session.Save.reducedMotion; session.Notify(); Show("Pausa"); });
            Button(content, resetConfirm ? "Confirmar nueva partida (se conserva copia del progreso anterior)" : "Nueva partida…", () =>
            {
                if (!resetConfirm) { resetConfirm = true; Show("Pausa"); return; }
                resetConfirm = false; ResetRequested?.Invoke();
            });
            Button(content, "Salir del juego", () =>
            {
                LocalStorage.Write(session.Save);
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            });
        }
        private static string FormatTime(float seconds) => ((int)seconds / 60) + ":" + ((int)seconds % 60).ToString("00");
        private void ObjectView()
        {
            if (inspectedObject == null) { Home(); return; }
            Title("Inspección de objeto", inspectedObject.label, string.IsNullOrEmpty(inspectedObject.description) ? "Un objeto de tu espacio de trabajo. Vuelve al entorno y pulsa E para usarlo." : inspectedObject.description);
            if (inspectedObject.grabbable) Label(content, "G toma o devuelve el objeto a su lugar. Mantén R y mueve el mouse para girarlo cuando esté en tu mano.", 22, Mint);
            if (inspectedObject.action == InteractionAction.Evidence && !session.Progress.started) Button(content, "Aceptar un ticket antes de leer esta evidencia", () => Show("Misiones"));
            Button(content, "Volver al entorno  →", () => { toast = ""; Show("Oficina"); }, true);
        }
    }
    internal static class SessionUIExtensions
    {
        public static string EvidenceCountText(this AcademySession session) => session.Active.evidence.Count(e => session.Evidence(e.id).solved) + " / " + session.Active.evidence.Length + " evidencias documentadas";
    }
}
