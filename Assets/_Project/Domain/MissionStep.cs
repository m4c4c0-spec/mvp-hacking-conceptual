using System.Collections.Generic;
using EthicalLab.Shared;

namespace EthicalLab.Domain
{
    public sealed class MissionStep
    {
        public StepId Id { get; }
        public StepKind Kind { get; }
        public string Prompt { get; }
        public string Body { get; }
        public IReadOnlyList<string> Options { get; }
        public int CorrectIndex { get; }
        public ConceptId Unlocks { get; }
        public bool UnlocksConcept => !string.IsNullOrEmpty(Unlocks.Value);

        public MissionStep(
            StepId id,
            StepKind kind,
            string prompt,
            string body,
            IReadOnlyList<string> options,
            int correctIndex,
            ConceptId unlocks)
        {
            Id = id;
            Kind = kind;
            Prompt = prompt ?? "";
            Body = body ?? "";
            Options = options ?? new string[0];
            CorrectIndex = correctIndex;
            Unlocks = unlocks;
        }
    }
}
