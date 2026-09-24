using EthicalLab.Domain;
using EthicalLab.Shared;

namespace EthicalLab.Application
{
    public sealed class StartMission
    {
        readonly IMissionRepository catalog;
        readonly PlayerProgress progress;

        public StartMission(IMissionRepository catalog, PlayerProgress progress)
        {
            this.catalog = catalog;
            this.progress = progress;
        }

        public Result Execute(MissionId id)
        {
            MissionCatalogRules.Ensure(catalog.Missions, catalog.Concepts, progress);
            return MissionCatalogRules.Start(catalog.Missions, progress, id);
        }
    }
}
