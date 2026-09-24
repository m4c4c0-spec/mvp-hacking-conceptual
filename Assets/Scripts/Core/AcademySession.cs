using System;
using System.Collections.Generic;
using System.Linq;

namespace Academy
{
    [Serializable] public class EvidenceProgress
    {
        public string id;
        public bool inspected, solved;
        public int attempts, selected = -1, reason = -1;
    }

    [Serializable] public class MissionProgress
    {
        public string id;
        public bool started, reportAccepted, completed;
        public float seconds;
        public int reportAttempts, reportChoice = -1, score;
        public string note = "";
        public List<EvidenceProgress> evidence = new List<EvidenceProgress>();
        public List<int> quizAnswers = new List<int>();
        public List<int> quizAttempts = new List<int>();
    }

    [Serializable] public class SaveData
    {
        public int version = 1;
        public string activeMission = "recon";
        public bool muted;
        public bool reducedMotion;
        public List<MissionProgress> missions = new List<MissionProgress>();
        public List<string> unlocked = new List<string>();
        public List<string> reviewed = new List<string>();
    }

    // Pure C# domain: UI, desktop input and future XR all share these rules.
    public sealed class AcademySession
    {
        public ContentData Content { get; }
        public SaveData Save { get; }
        public MissionData Active => Content.missions.First(m => m.id == Save.activeMission);
        public MissionProgress Progress => Get(Active.id);
        public int Completed => Save.missions.Count(m => m.completed);
        public event Action Changed;

        public AcademySession(ContentData content, SaveData save = null)
        {
            Content = content;
            Save = save ?? new SaveData();
            Save.missions = Save.missions ?? new List<MissionProgress>();
            Save.unlocked = Save.unlocked ?? new List<string>();
            Save.reviewed = Save.reviewed ?? new List<string>();
            foreach (var m in content.missions)
            {
                var p = Save.missions.FirstOrDefault(x => x.id == m.id);
                if (p == null) { p = new MissionProgress { id = m.id }; Save.missions.Add(p); }
                p.evidence = p.evidence ?? new List<EvidenceProgress>();
                p.quizAnswers = p.quizAnswers ?? new List<int>();
                p.quizAttempts = p.quizAttempts ?? new List<int>();
                foreach (var e in m.evidence)
                    if (!p.evidence.Any(x => x.id == e.id)) p.evidence.Add(new EvidenceProgress { id = e.id });
                while (p.quizAnswers.Count < m.quiz.Length) p.quizAnswers.Add(-1);
                while (p.quizAttempts.Count < m.quiz.Length) p.quizAttempts.Add(0);
            }
            if (!content.missions.Any(x => x.id == Save.activeMission)) Save.activeMission = content.missions[0].id;
            if (!IsAvailable(Save.activeMission)) Save.activeMission = content.missions[0].id;
        }

        public MissionProgress Get(string id) => Save.missions.First(m => m.id == id);
        public EvidenceProgress Evidence(string id) => Progress.evidence.First(e => e.id == id);
        public bool IsAvailable(string id)
        {
            int index = Array.FindIndex(Content.missions, m => m.id == id);
            return index >= 0 && (index == 0 || Content.missions.Take(index).All(m => Get(m.id).completed));
        }
        public bool Start(string id)
        {
            if (!IsAvailable(id)) return false;
            Save.activeMission = id;
            Progress.started = true;
            Notify();
            return true;
        }
        public void Inspect(string id)
        {
            if (!Progress.started) return;
            Evidence(id).inspected = true;
            Notify();
        }
        public bool SubmitEvidence(string id, int choice, int reason)
        {
            var e = Active.evidence.First(x => x.id == id);
            var p = Evidence(id);
            if (!Progress.started || !p.inspected || Progress.completed) return false;
            if (choice < 0 || choice >= e.options.Length || reason < 0 || reason >= e.reasons.Length) return false;
            if (p.solved) return true;
            p.attempts++; p.selected = choice; p.reason = reason;
            p.solved = choice == e.answer && reason == e.reasonAnswer;
            if (p.solved && !Save.unlocked.Contains(e.concept)) Save.unlocked.Add(e.concept);
            Notify(); return p.solved;
        }
        public bool ReadyToReport => Active.evidence.All(e => Evidence(e.id).solved);
        public bool SubmitReport(int choice)
        {
            if (!ReadyToReport || Progress.completed || choice < 0 || choice >= Active.report.options.Length) return false;
            if (Progress.reportAccepted) return true;
            Progress.reportAttempts++; Progress.reportChoice = choice;
            Progress.reportAccepted = choice == Active.report.answer;
            Notify(); return Progress.reportAccepted;
        }
        public bool AnswerQuiz(int index, int choice)
        {
            if (!Progress.reportAccepted || Progress.completed || index < 0 || index >= Active.quiz.Length) return false;
            var q = Active.quiz[index];
            if (choice < 0 || choice >= q.options.Length) return false;
            if (Progress.quizAnswers[index] == q.answer) return true;
            Progress.quizAttempts[index]++; Progress.quizAnswers[index] = choice;
            Notify(); return choice == q.answer;
        }
        public bool QuizPassed => Enumerable.Range(0, Active.quiz.Length).All(i => Progress.quizAnswers[i] == Active.quiz[i].answer);
        public int Score()
        {
            int first = Active.evidence.Count(e => Evidence(e.id).solved && Evidence(e.id).attempts == 1);
            int quizFirst = Enumerable.Range(0, Active.quiz.Length).Count(i => Progress.quizAnswers[i] == Active.quiz[i].answer && Progress.quizAttempts[i] == 1);
            int concepts = Active.concepts.Count(id => Save.reviewed.Contains(id));
            return (int)Math.Round(45.0 * first / Active.evidence.Length)
                + (Progress.reportAccepted ? (Progress.reportAttempts == 1 ? 15 : 8) : 0)
                + (int)Math.Round(25.0 * quizFirst / Active.quiz.Length)
                + (int)Math.Round(10.0 * concepts / Active.concepts.Length)
                + (Progress.seconds <= 600 ? 5 : 3);
        }
        public bool Complete()
        {
            if (!ReadyToReport || !Progress.reportAccepted || !QuizPassed) return false;
            if (Progress.completed) return true;
            Progress.score = Score(); Progress.completed = true; Notify(); return true;
        }
        public void Review(string id)
        {
            if (Save.unlocked.Contains(id) && !Save.reviewed.Contains(id)) { Save.reviewed.Add(id); Notify(); }
        }
        public void Note(string value) { Progress.note = value ?? ""; Notify(); }
        public void Tick(float delta) { if (Progress.started && !Progress.completed) Progress.seconds += Math.Max(0, delta); }
        public void Notify() => Changed?.Invoke();
    }
}
