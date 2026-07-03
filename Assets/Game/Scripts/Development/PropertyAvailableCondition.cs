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

        public string PropertyId => propertyId;
        public string WantDescription => wantDescription;

        public PropertyAvailableCondition(string propertyId, string wantDescription)
        {
            this.propertyId = propertyId;
            this.wantDescription = wantDescription;
        }

        public bool IsSatisfied(DevelopableEntity entity)
        {
            if (InventoryManager.Instance == null) return false;
            foreach (var stack in InventoryManager.Instance.Stacks)
            {
                if (stack.Item == null || stack.Item.PropertyIds == null) continue;
                foreach (var pid in stack.Item.PropertyIds)
                    if (pid == propertyId) return true;
            }
            return false;
        }

        public string GetNeedsDescription(DevelopableEntity entity)
        {
            if (!string.IsNullOrEmpty(wantDescription)) return wantDescription;
            var prop = PropertyRegistry.GetById(propertyId);
            return prop != null
                ? $"needs something that {prop.Phrase}"
                : $"needs an item with the quality \"{propertyId}\"";
        }
    }
}
