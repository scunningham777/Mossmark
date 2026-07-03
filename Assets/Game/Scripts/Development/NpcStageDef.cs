using UnityEngine;

namespace Mossmark.Development
{
    // Relational-data migration: promotion of the old NpcPostSpecStageDef ([Serializable]
    // row embedded in PlaceArchetype) to a standalone, ID-addressable asset — P2's analogue
    // of P1's UpgradeDefinition. A stage is no longer structurally trapped inside the one
    // archetype that embeds it: any pool can include it, and any source can grant it.
    //
    // The gate is a generic [SerializeReference] condition list (authored via
    // Tools/Data/stage_conditions.csv through ConditionCsvImporter), replacing the old
    // useRareItem/itemCount/requiredPropertyId hand-rolled union. Each condition owns its
    // own needs text. The archetype-level "specialization realized" gate is NOT authored
    // here — NpcAttendable prepends it structurally, since post-spec stages presuppose
    // their archetype's specialization.
    [CreateAssetMenu(menuName = "Mossmark/Development/NPC Stage", fileName = "NewNpcStage")]
    public class NpcStageDef : ScriptableObject
    {
        [SerializeField] private string stageId;
        [SerializeField] private string displayName;
        [SerializeField, Min(1)] private int progressCost = 6;
        [SerializeReference] private IDependencyCondition[] conditions = System.Array.Empty<IDependencyCondition>();
        // Logged as "{specializedName}: {flavorText}" — exclude the NPC name from this string.
        [SerializeField] private string flavorText;
        // WorldState flag key to set true on apply. Empty = no flag set.
        [SerializeField] private string worldStateFlag;
        // spotId of the wilderness spot whose tendedness drives passive progress each
        // rest. Empty = no passive drift for this stage (attention-only).
        [SerializeField] private string passiveDriftSourceSpotId;

        public string StageId => stageId;
        public string DisplayName => displayName;
        public int ProgressCost => progressCost;
        public IDependencyCondition[] Conditions => conditions;
        public string FlavorText => flavorText;
        public string WorldStateFlag => worldStateFlag;
        public string PassiveDriftSourceSpotId => passiveDriftSourceSpotId;
    }
}
