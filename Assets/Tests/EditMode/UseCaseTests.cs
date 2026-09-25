using System.Collections.Generic;
using EthicalLab.Application;
using EthicalLab.Domain;
using EthicalLab.Shared;
using NUnit.Framework;

namespace EthicalLab.Tests
{
    public sealed class MemoryCatalog : IMissionRepository
    {
        public IReadOnlyList<MissionDefinition> Missions { get; }
        public IReadOnlyList<ConceptCard> Concepts { get; }

        public MemoryCatalog()
        {
            Concepts = new[]
            {
                new ConceptCard(new ConceptId("recon"), "Reconocimiento", "Observación", "Mapa", "Importa", "Inventario"),
                new ConceptCard(new ConceptId("scope"), "Alcance", "Ética", "Límites", "Importa", "Permisos")
            };
            var evidence = new[]
            {
                new EvidenceSeed
                {
                    Id = "lumen", Label = "Mapa", Body = "tres piezas",
                    Question = "¿Qué afirma?", Options = new[] { "Inventario incompleto", "Compromiso" }, Answer = 0,
                    ReasonQuestion = "¿Defensa?", Reasons = new[] { "Apagar", "Completar inventario" }, ReasonAnswer = 1,
                    Concept = "recon"
                },
                new EvidenceSeed
                {
                    Id = "scope", Label = "Carta", Body = "Atlas excluido",
                    Question = "¿Atlas?", Options = new[] { "Incluir", "Fuera de alcance" }, Answer = 1,
                    ReasonQuestion = "¿Siguiente?", Reasons = new[] { "Ampliar permiso", "Ignorar" }, ReasonAnswer = 0,
                    Concept = "scope"
                }
            };
            var quiz = new[]
            {
                new QuizSeed { Question = "¿Recon?", Options = new[] { "Comprender", "Atacar" }, Answer = 0, Explanation = "Mapa primero" },
                new QuizSeed { Question = "¿Alcance?", Options = new[] { "Hereda", "Consultar" }, Answer = 1, Explanation = "Límites" },
                new QuizSeed { Question = "¿Pista?", Options = new[] { "Hecho", "Hipótesis" }, Answer = 1, Explanation = "Confirmar" }
            };
            Missions = new[]
            {
                new MissionDefinition(
                    new MissionId("recon"), "01", "TUTORIAL", "Antes de tocar nada", "Recon", "Lumen", "3 min", "cyan",
                    "Brief", "Mentor", "Alcance Lumen", "Objetivo", "scan", "Aprendido",
                    new[] { new ConceptId("recon"), new ConceptId("scope") },
                    MissionStepFactory.FromEvidence(evidence, "Cierre", new[] { "Apagar todo", "Inventario y Atlas fuera" }, 1, quiz))
            };
        }

        public MissionDefinition GetMission(MissionId id)
        {
            for (int i = 0; i < Missions.Count; i++)
                if (Missions[i].Id.Equals(id)) return Missions[i];
            return null;
        }

        public ConceptCard GetConcept(ConceptId id)
        {
            for (int i = 0; i < Concepts.Count; i++)
                if (Concepts[i].Id.Equals(id)) return Concepts[i];
            return null;
        }
    }

    public sealed class MemoryStore : IProgressStore
    {
        public PlayerProgress Data = new PlayerProgress();
        public PlayerProgress Load() => Data;
        public void Save(PlayerProgress progress) { Data = progress; }
    }

    public sealed class UseCaseTests
    {
        LabUseCases App() => new LabUseCases(new MemoryCatalog(), new MemoryStore());

        [Test]
        public void StartMission_CompleteSteps_UnlockConcept_SubmitReport_Scores()
        {
            var app = App();
            Assert.That(app.StartMission.Execute(new MissionId("recon")).Ok, Is.True);

            Assert.That(app.CompleteStep.Execute(new StepId("lumen.observe"), new StepAnswer { Choice = 0 }).Ok, Is.False, "sin inspeccionar");
            Assert.That(app.CompleteStep.Execute(new StepId("lumen.clue"), new StepAnswer()).Ok, Is.True);
            Assert.That(app.CompleteStep.Execute(new StepId("lumen.observe"), new StepAnswer { Choice = 0 }).Ok, Is.True);
            Assert.That(app.CompleteStep.Execute(new StepId("lumen.defend"), new StepAnswer { Choice = 1 }).Ok, Is.True);
            Assert.That(app.Progress.IsUnlocked(new ConceptId("recon")), Is.True);

            Assert.That(app.UnlockConcept.Execute(new ConceptId("recon")).Ok, Is.True);
            Assert.That(app.UnlockConcept.Execute(new ConceptId("mfa")).Ok, Is.False);

            Assert.That(app.CompleteStep.Execute(new StepId("scope.clue"), new StepAnswer()).Ok, Is.True);
            Assert.That(app.CompleteStep.Execute(new StepId("scope.observe"), new StepAnswer { Choice = 1 }).Ok, Is.True);
            Assert.That(app.CompleteStep.Execute(new StepId("scope.defend"), new StepAnswer { Choice = 0 }).Ok, Is.True);
            Assert.That(app.UnlockConcept.Execute(new ConceptId("scope")).Ok, Is.True);

            Assert.That(app.SubmitReport.Execute(1).Ok, Is.True);
            Assert.That(app.CompleteStep.Execute(new StepId("quiz.0"), new StepAnswer { Choice = 0 }).Ok, Is.True);
            Assert.That(app.CompleteStep.Execute(new StepId("quiz.1"), new StepAnswer { Choice = 1 }).Ok, Is.True);
            Assert.That(app.CompleteStep.Execute(new StepId("quiz.2"), new StepAnswer { Choice = 1 }).Ok, Is.True);
            Assert.That(app.SubmitReport.CloseIfReady().Ok, Is.True);
            Assert.That(app.Progress.Get("recon").Completed, Is.True);
            Assert.That(app.Progress.Get("recon").Score, Is.EqualTo(100));
        }

        [Test]
        public void ClueWorkflow_DrivesPhysicalFolderThroughObserveThenDefend()
        {
            var app = App();
            app.StartMission.Execute(new MissionId("recon"));
            var mission = app.Active;
            Assert.That(ClueWorkflow.ClueGroups(mission), Is.EqualTo(new[] { "lumen", "scope" }));
            Assert.That(ClueWorkflow.Collected(mission, app.Progress, "lumen"), Is.False);

            app.CompleteStep.Execute(new StepId("lumen.clue"), new StepAnswer());
            Assert.That(ClueWorkflow.Pending(mission, app.Progress, "lumen").Id.Value, Is.EqualTo("lumen.observe"));
            app.CompleteStep.Execute(new StepId("lumen.observe"), new StepAnswer { Choice = 0 });
            Assert.That(ClueWorkflow.Pending(mission, app.Progress, "lumen").Id.Value, Is.EqualTo("lumen.defend"));
            app.CompleteStep.Execute(new StepId("lumen.defend"), new StepAnswer { Choice = 1 });
            Assert.That(ClueWorkflow.Pending(mission, app.Progress, "lumen"), Is.Null);
            Assert.That(ClueWorkflow.Documented(mission, app.Progress, "lumen"), Is.True);
        }

        [Test]
        public void TerminalRejectsRealTools()
        {
            var app = App();
            app.StartMission.Execute(new MissionId("recon"));
            Assert.That(NarrativeTerminal.Execute(app, "curl https://example.invalid"), Does.Contain("fuera del vocabulario"));
            Assert.That(NarrativeTerminal.Execute(app, "inspect lumen"), Does.Contain("tres piezas"));
        }

        [Test]
        public void ResetDemo_ClearsProgressAndPersists()
        {
            var store = new MemoryStore();
            var app = new LabUseCases(new MemoryCatalog(), store);
            Assert.That(app.StartMission.Execute(new MissionId("recon")).Ok, Is.True);
            Assert.That(app.CompleteStep.Execute(new StepId("lumen.clue"), new StepAnswer()).Ok, Is.True);
            app.SaveLoad.Save();
            Assert.That(store.Data.Get("recon").Started, Is.True);

            app.ResetDemo();
            Assert.That(app.Progress.Get("recon").Started, Is.False);
            Assert.That(app.Progress.Get("recon").Steps.TrueForAll(s => !s.Completed), Is.True);
            Assert.That(app.Progress.Unlocked.Count, Is.EqualTo(0));
            Assert.That(store.Data.Get("recon").Started, Is.False);
            Assert.That(app.StartMission.Execute(new MissionId("recon")).Ok, Is.True);
        }
    }
}
