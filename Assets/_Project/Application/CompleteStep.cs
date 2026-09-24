using EthicalLab.Domain;
using EthicalLab.Shared;

namespace EthicalLab.Application
{
    public sealed class CompleteStep
    {
        readonly IMissionRepository catalog;
        readonly PlayerProgress progress;

        public CompleteStep(IMissionRepository catalog, PlayerProgress progress)
        {
            this.catalog = catalog;
            this.progress = progress;
        }

        public Result Execute(StepId step, StepAnswer answer)
        {
            var mission = catalog.GetMission(new MissionId(progress.ActiveMission));
            if (mission == null) return Result.Fail("No hay misión activa.");
            return MissionCatalogRules.CompleteStep(mission, progress, step, answer != null ? answer.Choice : -1);
        }
    }
}
