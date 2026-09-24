using System.Collections.Generic;
using EthicalLab.Shared;

namespace EthicalLab.Domain
{
    public sealed class MissionDefinition
    {
        public MissionId Id { get; }
        public string Number { get; }
        public string Type { get; }
        public string Title { get; }
        public string Subtitle { get; }
        public string Client { get; }
        public string Duration { get; }
        public string Accent { get; }
        public string Brief { get; }
        public string Mentor { get; }
        public string Scope { get; }
        public string Objective { get; }
        public string CommandHint { get; }
        public string Learned { get; }
        public IReadOnlyList<ConceptId> Concepts { get; }
        public IReadOnlyList<MissionStep> Steps { get; }

        public MissionDefinition(
            MissionId id,
            string number,
            string type,
            string title,
            string subtitle,
            string client,
            string duration,
            string accent,
            string brief,
            string mentor,
            string scope,
            string objective,
            string commandHint,
            string learned,
            IReadOnlyList<ConceptId> concepts,
            IReadOnlyList<MissionStep> steps)
        {
            Id = id;
            Number = number ?? "";
            Type = type ?? "";
            Title = title ?? "";
            Subtitle = subtitle ?? "";
            Client = client ?? "";
            Duration = duration ?? "";
            Accent = accent ?? "";
            Brief = brief ?? "";
            Mentor = mentor ?? "";
            Scope = scope ?? "";
            Objective = objective ?? "";
            CommandHint = commandHint ?? "";
            Learned = learned ?? "";
            Concepts = concepts ?? new ConceptId[0];
            Steps = steps ?? new MissionStep[0];
        }

        public MissionStep Step(StepId id)
        {
            for (int i = 0; i < Steps.Count; i++)
                if (Steps[i].Id.Equals(id)) return Steps[i];
            return null;
        }
    }
}
