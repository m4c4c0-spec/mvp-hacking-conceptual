using System.Collections.Generic;
using EthicalLab.Domain;
using EthicalLab.Shared;

namespace EthicalLab.Application
{
    public interface IMissionRepository
    {
        IReadOnlyList<MissionDefinition> Missions { get; }
        IReadOnlyList<ConceptCard> Concepts { get; }
        MissionDefinition GetMission(MissionId id);
        ConceptCard GetConcept(ConceptId id);
    }

    public interface IProgressStore
    {
        PlayerProgress Load();
        void Save(PlayerProgress progress);
    }

    public sealed class StepAnswer
    {
        public int Choice = -1;
        public string Text = "";
    }
}
