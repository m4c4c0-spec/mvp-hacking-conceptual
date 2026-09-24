using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Academy.Tests
{
    public sealed class SessionTests
    {
        private ContentData data;
        private AcademySession session;
        [SetUp] public void Setup()
        {
            data = JsonUtility.FromJson<ContentData>(Resources.Load<TextAsset>("Content/missions").text);
            session = new AcademySession(data);
        }
        private void SolveEvidence()
        {
            foreach (var e in session.Active.evidence)
            {
                session.Inspect(e.id);
                Assert.That(session.SubmitEvidence(e.id, e.answer, e.reasonAnswer), Is.True);
            }
        }
        private void Finish()
        {
            SolveEvidence();
            Assert.That(session.SubmitReport(session.Active.report.answer), Is.True);
            for (int i = 0; i < session.Active.quiz.Length; i++) Assert.That(session.AnswerQuiz(i, session.Active.quiz[i].answer), Is.True);
            foreach (var id in session.Active.concepts) session.Review(id);
            Assert.That(session.Complete(), Is.True);
        }
        [Test] public void InitialStateLocksLaterCases()
        {
            Assert.That(session.IsAvailable("recon"), Is.True);
            Assert.That(session.Start("web"), Is.False);
            Assert.That(session.Completed, Is.Zero);
        }
        [Test] public void CannotSolveWithoutStartingOrInspecting()
        {
            var e = session.Active.evidence[0];
            Assert.That(session.SubmitEvidence(e.id, e.answer, e.reasonAnswer), Is.False);
            session.Start("recon");
            Assert.That(session.SubmitEvidence(e.id, e.answer, e.reasonAnswer), Is.False);
            session.Inspect(e.id);
            Assert.That(session.SubmitEvidence(e.id, e.answer, e.reasonAnswer), Is.True);
        }
        [Test] public void CorrectObservationWithoutCorrectDefenseDoesNotUnlockConcept()
        {
            session.Start("recon"); var e = session.Active.evidence[0]; session.Inspect(e.id);
            Assert.That(session.SubmitEvidence(e.id, e.answer, (e.reasonAnswer + 1) % e.reasons.Length), Is.False);
            Assert.That(session.Save.unlocked, Is.Empty);
        }
        [Test] public void InvalidSelectionsDoNotCountAsAttempts()
        {
            session.Start("recon"); var e = session.Active.evidence[0]; session.Inspect(e.id);
            Assert.That(session.SubmitEvidence(e.id, -1, 99), Is.False);
            Assert.That(session.Evidence(e.id).attempts, Is.Zero);
        }
        [Test] public void ReportsAndQuizzesCannotSkipEvidence()
        {
            session.Start("recon");
            Assert.That(session.SubmitReport(session.Active.report.answer), Is.False);
            Assert.That(session.AnswerQuiz(0, session.Active.quiz[0].answer), Is.False);
            Assert.That(session.Complete(), Is.False);
        }
        [Test] public void SolvedEvidenceIsIdempotent()
        {
            session.Start("recon"); var e = session.Active.evidence[0]; session.Inspect(e.id);
            session.SubmitEvidence(e.id, e.answer, e.reasonAnswer); session.SubmitEvidence(e.id, e.answer, e.reasonAnswer);
            Assert.That(session.Evidence(e.id).attempts, Is.EqualTo(1));
            Assert.That(session.Save.unlocked.Count(x => x == e.concept), Is.EqualTo(1));
        }
        [Test] public void ReviewingLockedCardsDoesNotGrantPoints()
        {
            session.Review("mfa"); Assert.That(session.Save.reviewed, Is.Empty);
        }
        [Test] public void PerfectRunCompletesAllFourAndUnlocksEightConcepts()
        {
            foreach (var m in data.missions)
            {
                Assert.That(session.Start(m.id), Is.True); Finish(); Assert.That(session.Progress.score, Is.EqualTo(100));
            }
            Assert.That(session.Completed, Is.EqualTo(4));
            Assert.That(session.Save.unlocked.Count, Is.EqualTo(8));
            Assert.That(session.Save.reviewed.Count, Is.EqualTo(8));
        }
        [Test] public void CorrectionsAllowProgressButDoNotRestoreFirstAttemptCredit()
        {
            session.Start("recon"); var e = session.Active.evidence[0]; session.Inspect(e.id);
            session.SubmitEvidence(e.id, (e.answer + 1) % e.options.Length, e.reasonAnswer);
            Finish(); Assert.That(session.Progress.completed, Is.True); Assert.That(session.Progress.score, Is.EqualTo(85));
        }
        [Test] public void IncorrectQuizMustBeCorrectedBeforeCompletion()
        {
            session.Start("recon"); SolveEvidence(); session.SubmitReport(session.Active.report.answer);
            for (int i = 0; i < 3; i++) session.AnswerQuiz(i, (session.Active.quiz[i].answer + 1) % 3);
            Assert.That(session.Complete(), Is.False);
            for (int i = 0; i < 3; i++) session.AnswerQuiz(i, session.Active.quiz[i].answer);
            Assert.That(session.Complete(), Is.True);
            Assert.That(session.Progress.score, Is.EqualTo(65));
        }
        [Test] public void SlowPlayNeverBlocksCompletion()
        {
            session.Start("recon"); session.Tick(900); Finish();
            Assert.That(session.Progress.score, Is.EqualTo(98));
        }
        [Test] public void CompletedMissionScoreIsFrozen()
        {
            session.Start("recon"); Finish(); int score = session.Progress.score;
            session.Tick(5000); session.Complete();
            Assert.That(session.Progress.score, Is.EqualTo(score));
        }
        [Test] public void SaveRoundTripPreservesUnlocksAnswersNotesAndClock()
        {
            session.Start("recon"); session.Note("Evidencia y defensa."); session.Tick(135); Finish();
            var restored = new AcademySession(data, JsonUtility.FromJson<SaveData>(JsonUtility.ToJson(session.Save)));
            Assert.That(restored.Progress.completed, Is.True);
            Assert.That(restored.Progress.note, Is.EqualTo("Evidencia y defensa."));
            Assert.That(restored.Progress.seconds, Is.EqualTo(135));
            Assert.That(restored.IsAvailable("social"), Is.True);
            Assert.That(restored.Save.reviewed.Count, Is.EqualTo(2));
        }
        [Test] public void InvalidActiveMissionFallsBackToTutorial()
        {
            var loaded = new AcademySession(data, new SaveData { activeMission = "not-a-mission" });
            Assert.That(loaded.Active.id, Is.EqualTo("recon"));
        }
        [Test] public void TerminalReadsOnlyKnownLocalEvidence()
        {
            session.Start("recon");
            Assert.That(FictionTerminal.Execute(session, "inspect lumen"), Does.Contain("tres piezas"));
            Assert.That(session.Evidence("lumen").inspected, Is.True);
            Assert.That(FictionTerminal.Execute(session, "inspect https://example.invalid"), Does.Contain("no encontrada"));
            Assert.That(FictionTerminal.Execute(session, "curl https://example.invalid"), Does.Contain("fuera del vocabulario"));
            Assert.That(FictionTerminal.Execute(session, "scan; arbitrary"), Does.Contain("fuera del vocabulario"));
        }
        [Test] public void NotesCommandAppendsLiteralText()
        {
            session.Start("recon"); FictionTerminal.Execute(session, "notes primer hallazgo");
            FictionTerminal.Execute(session, "notes segundo hallazgo");
            Assert.That(session.Progress.note, Is.EqualTo("primer hallazgo\nsegundo hallazgo"));
        }
    }
}
