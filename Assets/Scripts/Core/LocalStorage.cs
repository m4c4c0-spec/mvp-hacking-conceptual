using System;
using System.IO;
using UnityEngine;

namespace Academy
{
    public static class LocalStorage
    {
        public static string LastError { get; private set; }
        public static string SavePath => Path.Combine(Application.persistentDataPath, "progress-v1.json");
        public static SaveData Read()
        {
            try
            {
                if (!File.Exists(SavePath)) return null;
                var save = JsonUtility.FromJson<SaveData>(File.ReadAllText(SavePath));
                if (save == null || save.version != 1) throw new InvalidDataException("Versión de guardado incompatible");
                return save;
            }
            catch (Exception ex)
            {
                LastError = "No se pudo recuperar el progreso. Se iniciará una sesión nueva. " + ex.Message;
                // Preserve the original for recovery before the next autosave.
                try { File.Copy(SavePath, SavePath + ".unreadable-" + DateTime.UtcNow.Ticks, false); } catch (Exception backupError) { Debug.LogWarning(backupError.Message); }
                Debug.LogWarning(LastError); return null;
            }
        }
        public static bool Write(SaveData save)
        {
            try
            {
                Directory.CreateDirectory(Application.persistentDataPath);
                File.WriteAllText(SavePath + ".tmp", JsonUtility.ToJson(save, true));
                if (File.Exists(SavePath)) File.Replace(SavePath + ".tmp", SavePath, SavePath + ".bak");
                else File.Move(SavePath + ".tmp", SavePath);
                LastError = null; return true;
            }
            catch (Exception ex) { LastError = "No se pudo guardar el progreso: " + ex.Message; Debug.LogWarning(LastError); return false; }
        }
        public static string Export(AcademySession session)
        {
            var m = session.Active; var p = session.Progress;
            var text = new System.Text.StringBuilder();
            text.AppendLine("BLUE / RED · INFORME DE LABORATORIO FICTICIO\n");
            text.AppendLine(m.title + " · " + m.client);
            text.AppendLine("Alcance: " + m.scope + "\n");
            foreach (var e in m.evidence)
            {
                var ep = session.Evidence(e.id);
                text.AppendLine("## " + e.label);
                text.AppendLine("Evidencia: " + e.body);
                text.AppendLine(ep.solved ? "Observación: " + e.options[ep.selected] + "\nDefensa: " + e.reasons[ep.reason] : "Pendiente de resolver.");
                text.AppendLine();
            }
            text.AppendLine("Notas del analista:\n" + p.note + "\n");
            if (p.reportAccepted) text.AppendLine("Conclusión: " + m.report.options[p.reportChoice]);
            text.AppendLine(p.completed ? "Misión completada · " + p.score + "/100" : "Borrador de misión en curso");
            string directory = Path.Combine(Application.persistentDataPath, "Reports");
            Directory.CreateDirectory(directory);
            string path = Path.Combine(directory, m.id + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmss-fff") + ".txt");
            File.WriteAllText(path, text.ToString()); return path;
        }
    }
}
