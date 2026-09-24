import { computeAcademyScore } from "../web/js/scoring.js";

const score = computeAcademyScore({
  evidence: [
    { solved: true, attempts: 1 },
    { solved: true, attempts: 2 },
  ],
  reportAccepted: true,
  reportAttempts: 1,
  quiz: [
    { correct: true, attempts: 1 },
    { correct: true, attempts: 1 },
    { correct: true, attempts: 1 },
  ],
  conceptsReviewed: 2,
  conceptsTotal: 2,
  seconds: 120,
});

if (score !== 23 + 15 + 25 + 10 + 5) {
  console.error("score inesperado", score);
  process.exit(1);
}
console.log("ok", score);
