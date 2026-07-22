using System;
using Mossmark.Inventory;
using UnityEngine;

namespace Mossmark.Development
{
    // IDependencyCondition that checks whether the player carries any item with the
    // given propertyId. Used by NPC post-spec stages whose gate is phrased as a
    // folk-property want rather than an exact item (Iteration 37).
    //
    // Unlike ItemAvailableCondition, this checks carry only (not chest + carry)
    // because the matched item is consumed from the player's pack when the stage fires.
    // [Serializable] + serialized (non-readonly) fields enable [SerializeReference]
    // storage so this gate can be authored in data via the CSV condition importer.
    [Serializable]
    public class PropertyAvailableCondition : IDependencyCondition
    {
        [SerializeField] private string propertyId;
        [SerializeField] private string wantDescription;
        // Iteration 53 pilot (Flow-Filled Reserve): alternate satisfy path via a
        // passively-filled reserve (IPassiveReserveTracker), independent of carried
        // inventory. Authored at roughly the same order of magnitude as other
        // thresholds in the system (a fresh Standing window is 3 days). Not CSV-wired.
        // Verified in play mode that [SerializeReference] managed-reference objects do
        // NOT run C# field initializers for fields absent from already-serialized data
        // (unlike ordinary MonoBehaviour/ScriptableObject fields) — an existing asset
        // like bog_keeper_drainage deserializes this to 0, not 3. EffectiveThreshold
        // below treats a non-positive value as "unset" so the gate is still meaningful
        // on old data, rather than silently satisfied by a reserve of 0.
        [SerializeField] private float requiredReserveThreshold = 3f;

        private float EffectiveThreshold => requiredReserveThreshold > 0f ? requiredReserveThreshold : 3f;

        public string PropertyId => propertyId;
        public string WantDescription => wantDescription;

        public PropertyAvailableCondition(string propertyId, string wantDescription,
            float requiredReserveThreshold = 3f)
        {
            this.propertyId = propertyId;
            this.wantDescription = wantDescription;
            this.requiredReserveThreshold = requiredReserveThreshold;
        }

        public bool IsSatisfied(DevelopableEntity entity)
        {
            if (InventoryManager.Instance != null)
            {
                foreach (var stack in InventoryManager.Instance.Stacks)
                {
                    if (stack.Item == null || stack.Item.PropertyIds == null) continue;
                    foreach (var pid in stack.Item.PropertyIds)
                        if (pid == propertyId) return true;
                }
            }

            // Carrying a match (checked above) stays the faster/immediate path when
            // available — this reserve check is an additional "or", not a replacement.
            if (entity is IPassiveReserveTracker tracker
                && tracker.GetPassiveReserve(propertyId) >= EffectiveThreshold)
                return true;

            return false;
        }

        public string GetNeedsDescription(DevelopableEntity entity)
        {
            if (!string.IsNullOrEmpty(wantDescription)) return wantDescription;
            var prop = PropertyRegistry.GetById(propertyId);
            return prop != null
                ? $"needs something that {prop.Clause}"
                : $"needs an item with the quality \"{propertyId}\"";
        }
    }
}
