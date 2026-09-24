/** Fórmula de Academy.AcademySession.Score (precisión al primer intento, glosario revisado, tiempo razonable). */

export function computeAcademyScore({
  evidence = [],
  reportAccepted = false,
  reportAttempts = 0,
  quiz = [],
  conceptsReviewed = 0,
  conceptsTotal = 1,
  seconds = 0,
}) {
  const first = evidence.filter((item) => item.solved && item.attempts === 1).length;
  const quizFirst = quiz.filter((item) => item.correct && item.attempts === 1).length;
  return (
    Math.round((45 * first) / Math.max(1, evidence.length)) +
    (reportAccepted ? (reportAttempts === 1 ? 15 : 8) : 0) +
    Math.round((25 * quizFirst) / Math.max(1, quiz.length)) +
    Math.round((10 * conceptsReviewed) / Math.max(1, conceptsTotal)) +
    (seconds <= 600 ? 5 : 3)
  );
}
