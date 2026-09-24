using System;
using UnityEngine;

namespace Academy
{
    [Serializable] public class ContentData
    {
        public string title;
        public int version;
        public ConceptData[] concepts;
        public MissionData[] missions;
    }

    [Serializable] public class ConceptData
    {
        public string id, name, category, definition, importance, defense;
    }

    [Serializable] public class MissionData
    {
        public string id, number, type, title, subtitle, client, duration, accent;
        public string brief, mentor, scope, objective, commandHint, learned;
        public string[] concepts;
        public EvidenceData[] evidence;
        public ReportData report;
        public QuizData[] quiz;
    }

    [Serializable] public class EvidenceData
    {
        public string id, label, kind, icon, source, body, question, reasonQuestion, feedback, concept;
        public string[] options, reasons;
        public int answer, reasonAnswer;
    }

    [Serializable] public class ReportData
    {
        public string prompt;
        public string[] options;
        public int answer;
    }

    [Serializable] public class QuizData
    {
        public string question, explanation;
        public string[] options;
        public int answer;
    }

    public static class ContentLoader
    {
        public static ContentData Load()
        {
            // ScriptableObjects can override JSON without changing the game logic.
            var catalog = Resources.Load<MissionCatalog>("MissionCatalog");
            if (catalog != null && catalog.missions != null && catalog.missions.Length > 0)
                return catalog.ToContent();
            var json = Resources.Load<TextAsset>("Content/missions");
            if (json == null) throw new InvalidOperationException("Falta Resources/Content/missions.json");
            return JsonUtility.FromJson<ContentData>(json.text);
        }
    }
}
