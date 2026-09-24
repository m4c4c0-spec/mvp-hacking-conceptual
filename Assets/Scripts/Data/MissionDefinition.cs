using UnityEngine;

namespace Academy
{
    [CreateAssetMenu(menuName = "Analyst Academy/Misión", fileName = "Mission")]
    public sealed class MissionDefinition : ScriptableObject
    {
        public MissionData data;
    }
}
