using System.Collections.Generic;
using EthicalLab.Domain;
using EthicalLab.Shared;

namespace EthicalLab.Application
{
    /// <summary>
    /// Qué paso toca para una pista física (carpeta) del mundo 3D.
    /// La UI/mundo no decide reglas: pregunta aquí y llama CompleteStep.
    /// </summary>
    public static class ClueWorkflow
    {
        public static IReadOnlyList<string> ClueGroups(MissionDefinition mission)
        {
            var groups = new List<string>();
            if (mission == null) return groups;
            for (int i = 0; i < mission.Steps.Count; i++)
                if (mission.Steps[i].Kind == StepKind.CollectClue) groups.Add(mission.Steps[i].Id.ClueGroup);
            return groups;
        }

        public static bool Collected(MissionDefinition mission, PlayerProgress progress, string group)
        {
            var step = mission.Step(new StepId(group + ".clue"));
            return step != null && progress.Step(mission.Id.Value, step.Id.Value).Completed;
        }

        public static bool Documented(MissionDefinition mission, PlayerProgress progress, string group)
        {
            return Pending(mission, progress, group) == null && Collected(mission, progress, group);
        }

        /// <summary>Siguiente ClassifyItem sin completar del grupo (observe → defend) o null.</summary>
        public static MissionStep Pending(MissionDefinition mission, PlayerProgress progress, string group)
        {
            if (mission == null) return null;
            for (int i = 0; i < mission.Steps.Count; i++)
            {
                var step = mission.Steps[i];
                if (step.Kind != StepKind.ClassifyItem || step.Id.ClueGroup != group) continue;
                if (!progress.Step(mission.Id.Value, step.Id.Value).Completed) return step;
            }
            return null;
        }

        public static MissionStep Clue(MissionDefinition mission, string group)
        {
            return mission == null ? null : mission.Step(new StepId(group + ".clue"));
        }

        public static bool AllDocumented(MissionDefinition mission, PlayerProgress progress)
        {
            var groups = ClueGroups(mission);
            if (groups.Count == 0) return false;
            for (int i = 0; i < groups.Count; i++)
                if (!Documented(mission, progress, groups[i])) return false;
            return true;
        }
    }
}
