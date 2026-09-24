using System.Collections.Generic;
using EthicalLab.Application;
using EthicalLab.Domain;
using EthicalLab.Shared;
using UnityEngine;

namespace EthicalLab.Infrastructure
{
    public sealed class JsonMissionRepository : IMissionRepository
    {
        readonly List<MissionDefinition> missions = new List<MissionDefinition>();
        readonly List<ConceptCard> concepts = new List<ConceptCard>();

        public IReadOnlyList<MissionDefinition> Missions => missions;
        public IReadOnlyList<ConceptCard> Concepts => concepts;

        public JsonMissionRepository(string json)
        {
            var dto = JsonUtility.FromJson<ContentDto>(json);
            if (dto.concepts != null)
                for (int i = 0; i < dto.concepts.Length; i++) concepts.Add(ContentMapper.ToConcept(dto.concepts[i]));
            if (dto.missions != null)
                for (int i = 0; i < dto.missions.Length; i++) missions.Add(ContentMapper.ToMission(dto.missions[i]));
        }

        public static JsonMissionRepository FromResources()
        {
            var asset = Resources.Load<TextAsset>("Content/missions");
            if (asset == null) throw new System.InvalidOperationException("Falta Resources/Content/missions.json");
            return new JsonMissionRepository(asset.text);
        }

        public MissionDefinition GetMission(MissionId id)
        {
            for (int i = 0; i < missions.Count; i++)
                if (missions[i].Id.Equals(id)) return missions[i];
            return null;
        }

        public ConceptCard GetConcept(ConceptId id)
        {
            for (int i = 0; i < concepts.Count; i++)
                if (concepts[i].Id.Equals(id)) return concepts[i];
            return null;
        }
    }
}
