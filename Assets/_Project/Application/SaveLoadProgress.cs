using EthicalLab.Domain;

namespace EthicalLab.Application
{
    public sealed class SaveLoadProgress
    {
        readonly IProgressStore store;
        readonly PlayerProgress progress;

        public SaveLoadProgress(IProgressStore store, PlayerProgress progress)
        {
            this.store = store;
            this.progress = progress;
        }

        public PlayerProgress Load() => store.Load() ?? progress;

        public void Save() => store.Save(progress);

        /// <summary>Vacía progreso en memoria y persiste vía IProgressStore (demo cliente).</summary>
        public void ResetDemo()
        {
            progress.Clear();
            store.Save(progress);
        }
    }
}
