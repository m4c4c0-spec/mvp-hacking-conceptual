using EthicalLab.Domain;
using EthicalLab.Shared;

namespace EthicalLab.Application
{
    /// <summary>
    /// Fachada de presentación: la UI solo llama casos de uso.
    /// </summary>
    public sealed class LabUseCases
    {
        public IMissionRepository Catalog { get; }
        public PlayerProgress Progress { get; }
        public StartMission StartMission { get; }
        public CompleteStep CompleteStep { get; }
        public UnlockConcept UnlockConcept { get; }
        public SubmitReport SubmitReport { get; }
        public SaveLoadProgress SaveLoad { get; }

        public LabUseCases(IMissionRepository catalog, IProgressStore store)
        {
            Catalog = catalog;
            Progress = store.Load() ?? new PlayerProgress();
            MissionCatalogRules.Ensure(catalog.Missions, catalog.Concepts, Progress);
            StartMission = new StartMission(catalog, Progress);
            CompleteStep = new CompleteStep(catalog, Progress);
            UnlockConcept = new UnlockConcept(Progress);
            SubmitReport = new SubmitReport(catalog, Progress);
            SaveLoad = new SaveLoadProgress(store, Progress);
        }

        public MissionDefinition Active => Catalog.GetMission(new MissionId(Progress.ActiveMission));

        public void Tick(float seconds)
        {
            var row = Progress.Get(Progress.ActiveMission);
            if (row.Started && !row.Completed) row.Seconds += seconds < 0 ? 0 : seconds;
        }
    }
}
