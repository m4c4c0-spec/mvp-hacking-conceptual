using System;
using System.Collections.Generic;
using System.Linq;
using Academy;
using UnityEditor;
using UnityEngine;

namespace EthicalLab.Editor
{
    /// <summary>
    /// Fuente única de contenido: Assets/Resources/Content/missions.json.
    /// Los ScriptableObjects de Academy (Assets/Content/*.asset, Resources/MissionCatalog.asset)
    /// son una vista generada: se regeneran al guardar el JSON y nunca se editan a mano.
    /// Web y EthicalLab leen el mismo JSON; así las tres superficies muestran lo mismo.
    /// </summary>
    [InitializeOnLoad]
    public static class ContentSync
    {
        const string JsonPath = "Assets/Resources/Content/missions.json";
        const string CatalogPath = "Assets/Resources/MissionCatalog.asset";
        const string MissionsFolder = "Assets/Content/Missions";
        const string ConceptsFolder = "Assets/Content/Concepts";

        static ContentSync()
        {
            EditorApplication.delayCall += () =>
            {
                if (UnityEngine.Application.isBatchMode) return;
                var drift = Drift();
                if (drift.Count == 0) return;
                Debug.LogWarning("Contenido: los ScriptableObjects difieren del JSON canónico (" + drift.Count + "). " +
                                 "Edita missions.json y ejecuta EthicalLab/Contenido/Regenerar ScriptableObjects desde JSON.\n - " +
                                 string.Join("\n - ", drift));
            };
        }

        [MenuItem("EthicalLab/Contenido/Regenerar ScriptableObjects desde JSON (sobrescribe)")]
        public static void Regenerate()
        {
            var data = LoadJson();
            EnsureFolder("Assets", "Content");
            EnsureFolder("Assets/Content", "Missions");
            EnsureFolder("Assets/Content", "Concepts");

            var catalog = AssetDatabase.LoadAssetAtPath<MissionCatalog>(CatalogPath);
            bool newCatalog = catalog == null;
            if (newCatalog) catalog = ScriptableObject.CreateInstance<MissionCatalog>();

            var missions = new List<MissionDefinition>();
            foreach (var m in data.missions)
            {
                string path = MissionsFolder + "/" + m.id + ".asset";
                var asset = AssetDatabase.LoadAssetAtPath<MissionDefinition>(path);
                if (asset == null)
                {
                    asset = ScriptableObject.CreateInstance<MissionDefinition>();
                    asset.data = m;
                    AssetDatabase.CreateAsset(asset, path);
                }
                else
                {
                    asset.data = m;
                    EditorUtility.SetDirty(asset);
                }
                missions.Add(asset);
            }

            var concepts = new List<ConceptDefinition>();
            foreach (var c in data.concepts)
            {
                string path = ConceptsFolder + "/" + c.id + ".asset";
                var asset = AssetDatabase.LoadAssetAtPath<ConceptDefinition>(path);
                if (asset == null)
                {
                    asset = ScriptableObject.CreateInstance<ConceptDefinition>();
                    asset.data = c;
                    AssetDatabase.CreateAsset(asset, path);
                }
                else
                {
                    asset.data = c;
                    EditorUtility.SetDirty(asset);
                }
                concepts.Add(asset);
            }

            // Orden y membresía = JSON. Assets huérfanos se reportan, no se borran.
            catalog.missions = missions.ToArray();
            catalog.concepts = concepts.ToArray();
            if (newCatalog) AssetDatabase.CreateAsset(catalog, CatalogPath);
            else EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();

            var orphans = Orphans(data);
            Debug.Log("Contenido regenerado desde JSON: " + missions.Count + " misiones, " + concepts.Count + " conceptos." +
                      (orphans.Count > 0 ? "\nAssets sin entrada en el JSON (bórralos o agrégalos al JSON):\n - " + string.Join("\n - ", orphans) : ""));
        }

        [MenuItem("EthicalLab/Contenido/Verificar coherencia JSON ↔ ScriptableObjects")]
        public static void Verify()
        {
            var drift = Drift();
            if (drift.Count == 0) Debug.Log("Contenido coherente: los ScriptableObjects reflejan missions.json.");
            else Debug.LogError("Contenido divergente (" + drift.Count + "):\n - " + string.Join("\n - ", drift));
        }

        /// <summary>Lista de divergencias entre el JSON y los assets. Vacía = coherente.</summary>
        public static List<string> Drift()
        {
            var drift = new List<string>();
            var text = AssetDatabase.LoadAssetAtPath<TextAsset>(JsonPath);
            if (text == null) { drift.Add("Falta " + JsonPath); return drift; }
            var data = JsonUtility.FromJson<ContentData>(text.text);
            var catalog = AssetDatabase.LoadAssetAtPath<MissionCatalog>(CatalogPath);
            if (catalog == null) return drift; // Sin catálogo, Academy lee el JSON directamente: coherente por construcción.

            var catalogMissions = (catalog.missions ?? Array.Empty<MissionDefinition>()).Where(x => x != null).ToArray();
            var catalogConcepts = (catalog.concepts ?? Array.Empty<ConceptDefinition>()).Where(x => x != null).ToArray();

            if (catalogMissions.Length != data.missions.Length)
                drift.Add("Catálogo: " + catalogMissions.Length + " misiones vs " + data.missions.Length + " en JSON");
            for (int i = 0; i < Math.Min(catalogMissions.Length, data.missions.Length); i++)
            {
                if (catalogMissions[i].data.id != data.missions[i].id)
                    drift.Add("Orden de misiones: posición " + i + " es '" + catalogMissions[i].data.id + "' en catálogo y '" + data.missions[i].id + "' en JSON");
                else if (JsonUtility.ToJson(catalogMissions[i].data) != JsonUtility.ToJson(data.missions[i]))
                    drift.Add("Misión '" + data.missions[i].id + "' editada en el asset; el JSON manda");
            }

            if (catalogConcepts.Length != data.concepts.Length)
                drift.Add("Catálogo: " + catalogConcepts.Length + " conceptos vs " + data.concepts.Length + " en JSON");
            for (int i = 0; i < Math.Min(catalogConcepts.Length, data.concepts.Length); i++)
            {
                if (catalogConcepts[i].data.id != data.concepts[i].id)
                    drift.Add("Orden de conceptos: posición " + i + " es '" + catalogConcepts[i].data.id + "' en catálogo y '" + data.concepts[i].id + "' en JSON");
                else if (JsonUtility.ToJson(catalogConcepts[i].data) != JsonUtility.ToJson(data.concepts[i]))
                    drift.Add("Concepto '" + data.concepts[i].id + "' editado en el asset; el JSON manda");
            }

            drift.AddRange(Orphans(data).Select(o => "Asset huérfano: " + o));
            return drift;
        }

        static List<string> Orphans(ContentData data)
        {
            var orphans = new List<string>();
            var missionIds = new HashSet<string>(data.missions.Select(m => m.id));
            var conceptIds = new HashSet<string>(data.concepts.Select(c => c.id));
            foreach (var guid in AssetDatabase.FindAssets("t:MissionDefinition", new[] { MissionsFolder }))
            {
                var asset = AssetDatabase.LoadAssetAtPath<MissionDefinition>(AssetDatabase.GUIDToAssetPath(guid));
                if (asset != null && !missionIds.Contains(asset.data.id)) orphans.Add(AssetDatabase.GetAssetPath(asset));
            }
            foreach (var guid in AssetDatabase.FindAssets("t:ConceptDefinition", new[] { ConceptsFolder }))
            {
                var asset = AssetDatabase.LoadAssetAtPath<ConceptDefinition>(AssetDatabase.GUIDToAssetPath(guid));
                if (asset != null && !conceptIds.Contains(asset.data.id)) orphans.Add(AssetDatabase.GetAssetPath(asset));
            }
            return orphans;
        }

        static ContentData LoadJson()
        {
            var text = AssetDatabase.LoadAssetAtPath<TextAsset>(JsonPath);
            if (text == null) throw new InvalidOperationException("Falta " + JsonPath);
            return JsonUtility.FromJson<ContentData>(text.text);
        }

        static void EnsureFolder(string parent, string name)
        {
            if (!AssetDatabase.IsValidFolder(parent + "/" + name)) AssetDatabase.CreateFolder(parent, name);
        }

        /// <summary>Al guardar missions.json, los ScriptableObjects se regeneran solos.</summary>
        sealed class JsonWatcher : AssetPostprocessor
        {
            static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedFrom)
            {
                if (!imported.Contains(JsonPath)) return;
                if (AssetDatabase.LoadAssetAtPath<MissionCatalog>(CatalogPath) == null) return;
                EditorApplication.delayCall += Regenerate;
            }
        }
    }
}
