using System.Linq;
using UnityEngine;

namespace Academy
{
    [CreateAssetMenu(menuName = "Analyst Academy/Catálogo", fileName = "MissionCatalog")]
    public sealed class MissionCatalog : ScriptableObject
    {
        public MissionDefinition[] missions;
        public ConceptDefinition[] concepts;
        public ContentData ToContent()
        {
            return new ContentData {
                title = "Blue / Red · Analyst Academy", version = 1,
                missions = missions.Where(x => x != null).Select(x => x.data).ToArray(),
                concepts = concepts.Where(x => x != null).Select(x => x.data).ToArray()
            };
        }
    }
}
