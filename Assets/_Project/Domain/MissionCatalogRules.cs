using System;
using System.Collections.Generic;
using EthicalLab.Shared;

namespace EthicalLab.Domain
{
    public static class MissionCatalogRules
    {
        public static void Ensure(IReadOnlyList<MissionDefinition> missions, IReadOnlyList<ConceptCard> concepts, PlayerProgress progress)
        {
            if (progress.Missions == null) progress.Missions = new List<MissionProgress>();
            if (progress.Unlocked == null) progress.Unlocked = new List<string>();
            if (progress.Reviewed == null) progress.Reviewed = new List<string>();
            for (int i = 0; i < missions.Count; i++)
            {
                var mission = missions[i];
                var row = progress.Get(mission.Id.Value);
                if (row.Steps == null) row.Steps = new List<StepProgress>();
                for (int s = 0; s < mission.Steps.Count; s++)
                    progress.Step(mission.Id.Value, mission.Steps[s].Id.Value);
            }
            if (string.IsNullOrEmpty(progress.ActiveMission) || Find(missions, progress.ActiveMission) == null)
                progress.ActiveMission = missions.Count > 0 ? missions[0].Id.Value : "";
            if (!IsAvailable(missions, progress, new MissionId(progress.ActiveMission)) && missions.Count > 0)
                progress.ActiveMission = missions[0].Id.Value;
        }

        public static MissionDefinition Find(IReadOnlyList<MissionDefinition> missions, string id)
        {
            for (int i = 0; i < missions.Count; i++)
                if (missions[i].Id.Value == id) return missions[i];
            return null;
        }

        public static bool IsAvailable(IReadOnlyList<MissionDefinition> missions, PlayerProgress progress, MissionId id)
        {
            int index = -1;
            for (int i = 0; i < missions.Count; i++)
                if (missions[i].Id.Equals(id)) { index = i; break; }
            if (index < 0) return false;
            for (int i = 0; i < index; i++)
                if (!progress.Get(missions[i].Id.Value).Completed) return false;
            return true;
        }

        public static Result Start(IReadOnlyList<MissionDefinition> missions, PlayerProgress progress, MissionId id)
        {
            if (!IsAvailable(missions, progress, id)) return Result.Fail("Completa el caso anterior.");
            progress.ActiveMission = id.Value;
            progress.Get(id.Value).Started = true;
            return Result.Success();
        }

        public static Result CompleteStep(MissionDefinition mission, PlayerProgress progress, StepId stepId, int choice)
        {
            var row = progress.Get(mission.Id.Value);
            if (!row.Started) return Result.Fail("Acepta primero un ticket.");
            if (row.Completed) return Result.Fail("Esta misión ya está cerrada.");
            var step = mission.Step(stepId);
            if (step == null) return Result.Fail("Paso desconocido.");
            var state = progress.Step(mission.Id.Value, step.Id.Value);

            switch (step.Kind)
            {
                case StepKind.CollectClue:
                    state.Completed = true;
                    if (state.Attempts == 0) state.Attempts = 1;
                    return Result.Success();
                case StepKind.ClassifyItem:
                    if (!ClueCollected(mission, progress, step.Id))
                        return Result.Fail("Inspecciona la pista antes de clasificarla.");
                    if (choice < 0 || choice >= step.Options.Count) return Result.Fail("Selecciona una opción.");
                    if (state.Completed) return Result.Success();
                    state.Attempts++;
                    state.Selected = choice;
                    state.Completed = choice == step.CorrectIndex;
                    if (!state.Completed) return Result.Fail("No encaja con la evidencia. Relee la pista.");
                    if (step.UnlocksConcept && !progress.Unlocked.Contains(step.Unlocks.Value))
                        progress.Unlocked.Add(step.Unlocks.Value);
                    return Result.Success();
                case StepKind.WriteReportSection:
                    if (!AllClassified(mission, progress)) return Result.Fail("Documenta todas las pistas.");
                    if (choice < 0 || choice >= step.Options.Count) return Result.Fail("Selecciona una conclusión.");
                    if (row.ReportAccepted) return Result.Success();
                    row.ReportAttempts++;
                    row.ReportChoice = choice;
                    row.ReportAccepted = choice == step.CorrectIndex;
                    state.Attempts = row.ReportAttempts;
                    state.Selected = choice;
                    state.Completed = row.ReportAccepted;
                    return row.ReportAccepted ? Result.Success() : Result.Fail("Ese cierre no está respaldado.");
                case StepKind.AnswerQuiz:
                    if (!row.ReportAccepted) return Result.Fail("Entrega el informe antes del quiz.");
                    if (choice < 0 || choice >= step.Options.Count) return Result.Fail("Selecciona una respuesta.");
                    if (state.Completed) return Result.Success();
                    state.Attempts++;
                    state.Selected = choice;
                    state.Completed = choice == step.CorrectIndex;
                    return state.Completed ? Result.Success() : Result.Fail("Todavía no. Revisa la ficha y responde de nuevo.");
                case StepKind.UseTerminal:
                    state.Completed = true;
                    if (state.Attempts == 0) state.Attempts = 1;
                    return Result.Success();
                default:
                    throw new ArgumentOutOfRangeException(nameof(step.Kind), step.Kind, "Tipo de paso no soportado.");
            }
        }

        public static Result Review(PlayerProgress progress, ConceptId id)
        {
            if (!progress.Unlocked.Contains(id.Value)) return Result.Fail("Esa ficha aún no está desbloqueada.");
            if (!progress.Reviewed.Contains(id.Value)) progress.Reviewed.Add(id.Value);
            return Result.Success();
        }

        public static Result CompleteMission(MissionDefinition mission, PlayerProgress progress)
        {
            var row = progress.Get(mission.Id.Value);
            if (!row.ReportAccepted || !AllQuizzes(mission, progress)) return Result.Fail("Falta el informe o el quiz.");
            if (row.Completed) return Result.Success();
            row.Score = Score(mission, progress);
            row.Completed = true;
            return Result.Success();
        }

        public static int Score(MissionDefinition mission, PlayerProgress progress)
        {
            var row = progress.Get(mission.Id.Value);
            int groups = 0;
            int first = 0;
            var seen = new List<string>();
            for (int i = 0; i < mission.Steps.Count; i++)
            {
                var step = mission.Steps[i];
                if (step.Kind != StepKind.ClassifyItem) continue;
                string group = step.Id.ClueGroup;
                if (seen.Contains(group)) continue;
                seen.Add(group);
                groups++;
                if (GroupFirstTry(mission, progress, group)) first++;
            }
            int quizTotal = 0;
            int quizFirst = 0;
            for (int i = 0; i < mission.Steps.Count; i++)
            {
                var step = mission.Steps[i];
                if (step.Kind != StepKind.AnswerQuiz) continue;
                quizTotal++;
                var state = progress.Step(mission.Id.Value, step.Id.Value);
                if (state.Completed && state.Attempts == 1) quizFirst++;
            }
            int concepts = 0;
            for (int i = 0; i < mission.Concepts.Count; i++)
                if (progress.Reviewed.Contains(mission.Concepts[i].Value)) concepts++;
            return (int)Math.Round(groups == 0 ? 0 : 45.0 * first / groups)
                + (row.ReportAccepted ? (row.ReportAttempts == 1 ? 15 : 8) : 0)
                + (int)Math.Round(quizTotal == 0 ? 0 : 25.0 * quizFirst / quizTotal)
                + (int)Math.Round(mission.Concepts.Count == 0 ? 0 : 10.0 * concepts / mission.Concepts.Count)
                + (row.Seconds <= 600 ? 5 : 3);
        }

        static bool ClueCollected(MissionDefinition mission, PlayerProgress progress, StepId classifyId)
        {
            string group = classifyId.ClueGroup;
            for (int i = 0; i < mission.Steps.Count; i++)
            {
                var step = mission.Steps[i];
                if (step.Kind == StepKind.CollectClue && step.Id.ClueGroup == group)
                    return progress.Step(mission.Id.Value, step.Id.Value).Completed;
            }
            return true;
        }

        static bool AllClassified(MissionDefinition mission, PlayerProgress progress)
        {
            for (int i = 0; i < mission.Steps.Count; i++)
            {
                var step = mission.Steps[i];
                if (step.Kind == StepKind.ClassifyItem && !progress.Step(mission.Id.Value, step.Id.Value).Completed)
                    return false;
            }
            return true;
        }

        static bool AllQuizzes(MissionDefinition mission, PlayerProgress progress)
        {
            for (int i = 0; i < mission.Steps.Count; i++)
            {
                var step = mission.Steps[i];
                if (step.Kind == StepKind.AnswerQuiz && !progress.Step(mission.Id.Value, step.Id.Value).Completed)
                    return false;
            }
            return true;
        }

        static bool GroupFirstTry(MissionDefinition mission, PlayerProgress progress, string group)
        {
            for (int i = 0; i < mission.Steps.Count; i++)
            {
                var step = mission.Steps[i];
                if (step.Kind != StepKind.ClassifyItem || step.Id.ClueGroup != group) continue;
                var state = progress.Step(mission.Id.Value, step.Id.Value);
                if (!state.Completed || state.Attempts != 1) return false;
            }
            return true;
        }
    }
}
