using Mossmark.Inventory;
using UnityEngine;

namespace Mossmark.Development
{
    // Relational-data migration: promoted from a [Serializable] row embedded in
    // PlaceArchetype to a standalone, ID-addressable asset — the building-side analogue
    // of NpcStageDef. The old requiredSpecialization string is replaced by the generic
    // conditions list (authored via stage_conditions.csv). The material-availability gate
    // stays implicit: BuildingAttendable derives it from material/costPerTick, which are
    // structural — material is consumed per productive tick and stage 0's material
    // doubles as the building's maintenance material.
    [CreateAssetMenu(menuName = "Mossmark/Development/Building Stage", fileName = "NewBuildingStage")]
    public class BuildingStageDef : ScriptableObject
    {
        public string stageId;
        public string displayName;
        public string verb;
        public ItemDefinition material;
        public int costPerTick = 2;
        public int progressCost = 6;
        [SerializeReference] public IDependencyCondition[] conditions = System.Array.Empty<IDependencyCondition>();
        public Color tint = new(0.5f, 0.5f, 0.45f, 1f);
        // WorldState flag set to true when this stage completes. Empty = no flag.
        public string worldStateFlag;
    }
}
