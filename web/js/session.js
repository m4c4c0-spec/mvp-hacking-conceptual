/** Espejo de Academy.AcademySession para que el prototipo web y Unity compartan reglas. */

export function createSession(content, save = null, onChange = () => {}) {
  const state = save ?? {
    version: 1,
    activeMission: content.missions[0]?.id ?? "recon",
    muted: false,
    reducedMotion: false,
    missions: [],
    unlocked: [],
    reviewed: [],
  };
  state.missions ??= [];
  state.unlocked ??= [];
  state.reviewed ??= [];
  state.muted ??= false;
  state.reducedMotion ??= false;

  for (const mission of content.missions) {
    let progress = state.missions.find((item) => item.id === mission.id);
    if (!progress) {
      progress = {
        id: mission.id,
        started: false,
        reportAccepted: false,
        completed: false,
        seconds: 0,
        reportAttempts: 0,
        reportChoice: -1,
        score: 0,
        note: "",
        evidence: [],
        quizAnswers: [],
        quizAttempts: [],
      };
      state.missions.push(progress);
    }
    progress.evidence ??= [];
    progress.quizAnswers ??= [];
    progress.quizAttempts ??= [];
    for (const evidence of mission.evidence) {
      if (!progress.evidence.some((item) => item.id === evidence.id)) {
        progress.evidence.push({
          id: evidence.id,
          inspected: false,
          solved: false,
          attempts: 0,
          selected: -1,
          reason: -1,
        });
      }
    }
    while (progress.quizAnswers.length < mission.quiz.length) progress.quizAnswers.push(-1);
    while (progress.quizAttempts.length < mission.quiz.length) progress.quizAttempts.push(0);
  }

  function notify() {
    onChange();
  }

  function active() {
    return content.missions.find((item) => item.id === state.activeMission) ?? content.missions[0];
  }

  function get(id) {
    return state.missions.find((item) => item.id === id);
  }

  function progress() {
    return get(active().id);
  }

  function evidenceProgress(id) {
    return progress().evidence.find((item) => item.id === id);
  }

  function isAvailable(id) {
    const index = content.missions.findIndex((item) => item.id === id);
    return index >= 0 && content.missions.slice(0, index).every((item) => get(item.id).completed);
  }

  if (!content.missions.some((item) => item.id === state.activeMission) || !isAvailable(state.activeMission)) {
    state.activeMission = content.missions[0].id;
  }

  return {
    content,
    save: state,
    active,
    progress,
    get,
    evidenceProgress,
    isAvailable,
    start(id) {
      if (!isAvailable(id)) return false;
      state.activeMission = id;
      progress().started = true;
      notify();
      return true;
    },
    inspect(id) {
      if (!progress().started) return;
      evidenceProgress(id).inspected = true;
      notify();
    },
    submitEvidence(id, choice, reason) {
      const mission = active();
      const evidence = mission.evidence.find((item) => item.id === id);
      const row = evidenceProgress(id);
      if (!progress().started || !row.inspected || progress().completed) return false;
      if (choice < 0 || choice >= evidence.options.length || reason < 0 || reason >= evidence.reasons.length) {
        return false;
      }
      if (row.solved) return true;
      row.attempts += 1;
      row.selected = choice;
      row.reason = reason;
      row.solved = choice === evidence.answer && reason === evidence.reasonAnswer;
      if (row.solved && !state.unlocked.includes(evidence.concept)) state.unlocked.push(evidence.concept);
      notify();
      return row.solved;
    },
    readyToReport() {
      return active().evidence.every((item) => evidenceProgress(item.id).solved);
    },
    submitReport(choice) {
      const mission = active();
      const row = progress();
      if (!this.readyToReport() || row.completed || choice < 0 || choice >= mission.report.options.length) {
        return false;
      }
      if (row.reportAccepted) return true;
      row.reportAttempts += 1;
      row.reportChoice = choice;
      row.reportAccepted = choice === mission.report.answer;
      notify();
      return row.reportAccepted;
    },
    answerQuiz(index, choice) {
      const mission = active();
      const row = progress();
      if (!row.reportAccepted || row.completed || index < 0 || index >= mission.quiz.length) return false;
      const question = mission.quiz[index];
      if (choice < 0 || choice >= question.options.length) return false;
      if (row.quizAnswers[index] === question.answer) return true;
      row.quizAttempts[index] += 1;
      row.quizAnswers[index] = choice;
      notify();
      return choice === question.answer;
    },
    quizPassed() {
      const mission = active();
      const row = progress();
      return mission.quiz.every((item, index) => row.quizAnswers[index] === item.answer);
    },
    score() {
      const mission = active();
      const row = progress();
      const first = mission.evidence.filter((item) => {
        const ev = evidenceProgress(item.id);
        return ev.solved && ev.attempts === 1;
      }).length;
      const quizFirst = mission.quiz.filter((item, index) => row.quizAnswers[index] === item.answer && row.quizAttempts[index] === 1).length;
      const concepts = mission.concepts.filter((id) => state.reviewed.includes(id)).length;
      return (
        Math.round((45 * first) / mission.evidence.length) +
        (row.reportAccepted ? (row.reportAttempts === 1 ? 15 : 8) : 0) +
        Math.round((25 * quizFirst) / mission.quiz.length) +
        Math.round((10 * concepts) / mission.concepts.length) +
        (row.seconds <= 600 ? 5 : 3)
      );
    },
    complete() {
      const row = progress();
      if (!this.readyToReport() || !row.reportAccepted || !this.quizPassed()) return false;
      if (row.completed) return true;
      row.score = this.score();
      row.completed = true;
      notify();
      return true;
    },
    review(id) {
      if (state.unlocked.includes(id) && !state.reviewed.includes(id)) {
        state.reviewed.push(id);
        notify();
      }
    },
    note(value) {
      progress().note = value ?? "";
      notify();
    },
    tick(delta) {
      const row = progress();
      if (row.started && !row.completed) row.seconds += Math.max(0, delta);
    },
    completedCount() {
      return state.missions.filter((item) => item.completed).length;
    },
  };
}

export function computeAcademyScore(session) {
  return session.score();
}
