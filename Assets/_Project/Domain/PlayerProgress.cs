using System.Collections.Generic;
using EthicalLab.Shared;

namespace EthicalLab.Domain
{
    public sealed class StepProgress
    {
        public string Id;
        public bool Completed;
        public int Attempts;
        public int Selected = -1;
    }

    public sealed class MissionProgress
    {
        public string Id;
        public bool Started;
        public bool ReportAccepted;
        public bool Completed;
        public float Seconds;
        public int ReportAttempts;
        public int ReportChoice = -1;
        public int Score;
        public string Note = "";
        public List<StepProgress> Steps = new List<StepProgress>();
    }

    public sealed class PlayerProgress
    {
        public int Version = 1;
        public string ActiveMission = "";
        public List<MissionProgress> Missions = new List<MissionProgress>();
        public List<string> Unlocked = new List<string>();
        public List<string> Reviewed = new List<string>();

        public MissionProgress Get(string missionId)
        {
            for (int i = 0; i < Missions.Count; i++)
                if (Missions[i].Id == missionId) return Missions[i];
            var created = new MissionProgress { Id = missionId };
            Missions.Add(created);
            return created;
        }

        public StepProgress Step(string missionId, string stepId)
        {
            var mission = Get(missionId);
            for (int i = 0; i < mission.Steps.Count; i++)
                if (mission.Steps[i].Id == stepId) return mission.Steps[i];
            var created = new StepProgress { Id = stepId };
            mission.Steps.Add(created);
            return created;
        }

        public bool IsReviewed(ConceptId id)
        {
            return Reviewed.Contains(id.Value);
        }

        public bool IsUnlocked(ConceptId id)
        {
            return Unlocked.Contains(id.Value);
        }

        public int CompletedCount
        {
            get
            {
                int count = 0;
                for (int i = 0; i < Missions.Count; i++)
                    if (Missions[i].Completed) count++;
                return count;
            }
        }
    }
}
