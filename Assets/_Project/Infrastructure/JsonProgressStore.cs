using System;
using System.Collections.Generic;
using System.IO;
using EthicalLab.Application;
using EthicalLab.Domain;
using UnityEngine;

namespace EthicalLab.Infrastructure
{
    [Serializable] class SavedProgress
    {
        public int version = 1;
        public string activeMission;
        public string[] unlocked;
        public string[] reviewed;
        public SavedMission[] missions;
    }

    [Serializable] class SavedMission
    {
        public string id, note;
        public bool started, reportAccepted, completed;
        public float seconds;
        public int reportAttempts, reportChoice, score;
        public SavedStep[] steps;
    }

    [Serializable] class SavedStep
    {
        public string id;
        public bool completed;
        public int attempts, selected;
    }

    public sealed class JsonProgressStore : IProgressStore
    {
        readonly string directory;

        // El directorio opcional permite probar persistencia sin tocar la partida del jugador.
        public JsonProgressStore(string directory = null)
        {
            this.directory = directory ?? UnityEngine.Application.persistentDataPath;
        }

        public string Path => System.IO.Path.Combine(directory, "ethicallab-progress-v1.json");

        public PlayerProgress Load()
        {
            try
            {
                if (!File.Exists(Path)) return new PlayerProgress();
                var dto = JsonUtility.FromJson<SavedProgress>(File.ReadAllText(Path));
                var progress = new PlayerProgress
                {
                    Version = dto.version,
                    ActiveMission = dto.activeMission ?? "",
                    Unlocked = ToList(dto.unlocked),
                    Reviewed = ToList(dto.reviewed),
                    Missions = new List<MissionProgress>()
                };
                if (dto.missions != null)
                {
                    for (int i = 0; i < dto.missions.Length; i++)
                    {
                        var m = dto.missions[i];
                        var row = new MissionProgress
                        {
                            Id = m.id,
                            Note = m.note ?? "",
                            Started = m.started,
                            ReportAccepted = m.reportAccepted,
                            Completed = m.completed,
                            Seconds = m.seconds,
                            ReportAttempts = m.reportAttempts,
                            ReportChoice = m.reportChoice,
                            Score = m.score,
                            Steps = new List<StepProgress>()
                        };
                        if (m.steps != null)
                        {
                            for (int s = 0; s < m.steps.Length; s++)
                            {
                                var st = m.steps[s];
                                row.Steps.Add(new StepProgress { Id = st.id, Completed = st.completed, Attempts = st.attempts, Selected = st.selected });
                            }
                        }
                        progress.Missions.Add(row);
                    }
                }
                return progress;
            }
            catch (Exception ex)
            {
                Debug.LogWarning("No se pudo leer el progreso EthicalLab: " + ex.Message);
                try { File.Copy(Path, Path + ".unreadable-" + DateTime.UtcNow.Ticks, false); }
                catch (Exception backupError) { Debug.LogWarning(backupError.Message); }
                return new PlayerProgress();
            }
        }

        public void Save(PlayerProgress progress)
        {
            var missions = new SavedMission[progress.Missions.Count];
            for (int i = 0; i < progress.Missions.Count; i++)
            {
                var m = progress.Missions[i];
                var steps = new SavedStep[m.Steps.Count];
                for (int s = 0; s < m.Steps.Count; s++)
                {
                    var st = m.Steps[s];
                    steps[s] = new SavedStep { id = st.Id, completed = st.Completed, attempts = st.Attempts, selected = st.Selected };
                }
                missions[i] = new SavedMission
                {
                    id = m.Id,
                    note = m.Note,
                    started = m.Started,
                    reportAccepted = m.ReportAccepted,
                    completed = m.Completed,
                    seconds = m.Seconds,
                    reportAttempts = m.ReportAttempts,
                    reportChoice = m.ReportChoice,
                    score = m.Score,
                    steps = steps
                };
            }
            var dto = new SavedProgress
            {
                version = progress.Version,
                activeMission = progress.ActiveMission,
                unlocked = progress.Unlocked.ToArray(),
                reviewed = progress.Reviewed.ToArray(),
                missions = missions
            };
            Directory.CreateDirectory(directory);
            string tmp = Path + ".tmp";
            File.WriteAllText(tmp, JsonUtility.ToJson(dto, true));
            if (File.Exists(Path)) File.Replace(tmp, Path, Path + ".bak");
            else File.Move(tmp, Path);
        }

        static List<string> ToList(string[] items)
        {
            return items != null ? new List<string>(items) : new List<string>();
        }
    }
}
