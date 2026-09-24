using EthicalLab.Domain;
using EthicalLab.Shared;

namespace EthicalLab.Application
{
    public sealed class UnlockConcept
    {
        readonly PlayerProgress progress;

        public UnlockConcept(PlayerProgress progress)
        {
            this.progress = progress;
        }

        public Result Execute(ConceptId id)
        {
            return MissionCatalogRules.Review(progress, id);
        }
    }
}
