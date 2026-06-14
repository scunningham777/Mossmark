using UnityEngine;

namespace Mossmark.World
{
    // Iteration 12: a named region's pool of candidate PlaceArchetypes, from which a
    // session draws ArchetypeSelectionCount (per PROTOTYPE2.md's "2-3 archetypes").
    [CreateAssetMenu(fileName = "RegionData", menuName = "Mossmark/World/Region Data")]
    public class RegionData : ScriptableObject
    {
        [SerializeField] private string regionName;
        [SerializeField] private PlaceArchetype[] archetypePool;
        [SerializeField, Min(1)] private int archetypeSelectionCount = 3;

        public string RegionName => regionName;
        public PlaceArchetype[] ArchetypePool => archetypePool;
        public int ArchetypeSelectionCount => archetypeSelectionCount;
    }
}
