using EthicalLab.Domain;
using EthicalLab.Shared;

namespace EthicalLab.Application
{
    public sealed class SubmitReport
    {
        readonly IMissionRepository catalog;
        readonly PlayerProgress progress;

        public SubmitReport(IMissionRepository catalog, PlayerProgress progress)
        {
            this.catalog = catalog;
            this.progress = progress;
        }

        public Result Execute(int conclusionIndex)
        {
            var mission = catalog.GetMission(new MissionId(progress.ActiveMission));
            if (mission == null) return Result.Fail("No hay misión activa.");
            var report = MissionCatalogRules.CompleteStep(mission, progress, new StepId("report"), conclusionIndex);
            if (!report.Ok) return report;
            if (!AllQuizzesDone(mission)) return report;
            return MissionCatalogRules.CompleteMission(mission, progress);
        }

        public Result CloseIfReady()
        {
            var mission = catalog.GetMission(new MissionId(progress.ActiveMission));
            if (mission == null) return Result.Fail("No hay misión activa.");
            return MissionCatalogRules.CompleteMission(mission, progress);
        }

        bool AllQuizzesDone(MissionDefinition mission)
        {
            for (int i = 0; i < mission.Steps.Count; i++)
            {
                var step = mission.Steps[i];
                if (step.Kind == StepKind.AnswerQuiz && !progress.Step(mission.Id.Value, step.Id.Value).Completed)
                    return false;
            }
            return true;
        }
    }
}
