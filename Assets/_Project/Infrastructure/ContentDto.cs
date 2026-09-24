using System;
using EthicalLab.Domain;
using EthicalLab.Shared;

namespace EthicalLab.Infrastructure
{
    [Serializable] public class ContentDto
    {
        public string title;
        public int version;
        public ConceptDto[] concepts;
        public MissionDto[] missions;
    }

    [Serializable] public class ConceptDto
    {
        public string id, name, category, definition, importance, defense;
    }

    [Serializable] public class MissionDto
    {
        public string id, number, type, title, subtitle, client, duration, accent;
        public string brief, mentor, scope, objective, commandHint, learned;
        public string[] concepts;
        public EvidenceDto[] evidence;
        public ReportDto report;
        public QuizDto[] quiz;
    }

    [Serializable] public class EvidenceDto
    {
        public string id, label, kind, icon, source, body, question, reasonQuestion, feedback, concept;
        public string[] options, reasons;
        public int answer, reasonAnswer;
    }

    [Serializable] public class ReportDto
    {
        public string prompt;
        public string[] options;
        public int answer;
    }

    [Serializable] public class QuizDto
    {
        public string question, explanation;
        public string[] options;
        public int answer;
    }

    [Serializable] public class ProgressDto
    {
        public int version = 1;
        public string activeMission;
        public string[] unlocked;
        public string[] reviewed;
    }

    public static class ContentMapper
    {
        public static ConceptCard ToConcept(ConceptDto dto)
        {
            return new ConceptCard(
                new ConceptId(dto.id),
                dto.name,
                dto.category,
                dto.definition,
                dto.importance,
                dto.defense);
        }

        public static MissionDefinition ToMission(MissionDto dto)
        {
            var concepts = new ConceptId[(dto.concepts ?? new string[0]).Length];
            for (int i = 0; i < concepts.Length; i++) concepts[i] = new ConceptId(dto.concepts[i]);
            var evidence = new EvidenceSeed[(dto.evidence ?? new EvidenceDto[0]).Length];
            for (int i = 0; i < evidence.Length; i++)
            {
                var e = dto.evidence[i];
                evidence[i] = new EvidenceSeed
                {
                    Id = e.id,
                    Label = e.label,
                    Body = e.body,
                    Question = e.question,
                    ReasonQuestion = e.reasonQuestion,
                    Feedback = e.feedback,
                    Concept = e.concept,
                    Options = e.options,
                    Reasons = e.reasons,
                    Answer = e.answer,
                    ReasonAnswer = e.reasonAnswer
                };
            }
            var quiz = new QuizSeed[(dto.quiz ?? new QuizDto[0]).Length];
            for (int i = 0; i < quiz.Length; i++)
            {
                var q = dto.quiz[i];
                quiz[i] = new QuizSeed { Question = q.question, Explanation = q.explanation, Options = q.options, Answer = q.answer };
            }
            var report = dto.report ?? new ReportDto { prompt = "", options = new string[0], answer = 0 };
            return new MissionDefinition(
                new MissionId(dto.id),
                dto.number,
                dto.type,
                dto.title,
                dto.subtitle,
                dto.client,
                dto.duration,
                dto.accent,
                dto.brief,
                dto.mentor,
                dto.scope,
                dto.objective,
                dto.commandHint,
                dto.learned,
                concepts,
                MissionStepFactory.FromEvidence(evidence, report.prompt, report.options, report.answer, quiz));
        }
    }
}
