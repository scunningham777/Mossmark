using Mossmark.Inventory;

namespace Mossmark.Development
{
    // IDependencyCondition that checks whether the player carries any item with the
    // given propertyId. Used by NPC post-spec stages whose gate is phrased as a
    // folk-property want rather than an exact item (Iteration 37).
    //
    // Unlike ItemAvailableCondition, this checks carry only (not chest + carry)
    // because the matched item is consumed from the player's pack when the stage fires.
    public class PropertyAvailableCondition : IDependencyCondition
    {
        private readonly string propertyId;
        private readonly string wantDescription;

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
