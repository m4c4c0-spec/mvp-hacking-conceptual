using System.Collections.Generic;
using EthicalLab.Shared;

namespace EthicalLab.Domain
{
    /// <summary>
    /// Convierte evidencias del JSON (datos) en pasos de misión. No conoce Unity.
    /// </summary>
    public static class MissionStepFactory
    {
        public static IReadOnlyList<MissionStep> FromEvidence(
            IReadOnlyList<EvidenceSeed> evidence,
            string reportPrompt,
            IReadOnlyList<string> reportOptions,
            int reportAnswer,
            IReadOnlyList<QuizSeed> quiz)
        {
            var steps = new List<MissionStep>();
            if (evidence != null)
            {
                for (int i = 0; i < evidence.Count; i++)
                {
                    var item = evidence[i];
                    steps.Add(new MissionStep(
                        new StepId(item.Id + ".clue"),
                        StepKind.CollectClue,
                        item.Label,
                        item.Body,
                        new string[0],
                        0,
                        new ConceptId("")));
                    steps.Add(new MissionStep(
                        new StepId(item.Id + ".observe"),
                        StepKind.ClassifyItem,
                        item.Question,
                        item.Body,
                        item.Options,
                        item.Answer,
                        new ConceptId("")));
                    steps.Add(new MissionStep(
                        new StepId(item.Id + ".defend"),
                        StepKind.ClassifyItem,
                        item.ReasonQuestion,
                        item.Feedback,
                        item.Reasons,
                        item.ReasonAnswer,
                        new ConceptId(item.Concept)));
                }
            }
            steps.Add(new MissionStep(
                new StepId("report"),
                StepKind.WriteReportSection,
                reportPrompt,
                "",
                reportOptions,
                reportAnswer,
                new ConceptId("")));
            if (quiz != null)
            {
                for (int i = 0; i < quiz.Count; i++)
                {
                    var item = quiz[i];
                    steps.Add(new MissionStep(
                        new StepId("quiz." + i),
                        StepKind.AnswerQuiz,
                        item.Question,
                        item.Explanation,
                        item.Options,
                        item.Answer,
                        new ConceptId("")));
                }
            }
            steps.Add(new MissionStep(
                new StepId("terminal"),
                StepKind.UseTerminal,
                "Terminal narrativa",
                "scan · inspect · notes · report",
                new string[0],
                0,
                new ConceptId("")));
            return steps;
        }
    }

    public sealed class EvidenceSeed
    {
        public string Id, Label, Body, Question, ReasonQuestion, Feedback, Concept;
        public IReadOnlyList<string> Options, Reasons;
        public int Answer, ReasonAnswer;
    }

    public sealed class QuizSeed
    {
        public string Question, Explanation;
        public IReadOnlyList<string> Options;
        public int Answer;
    }
}
