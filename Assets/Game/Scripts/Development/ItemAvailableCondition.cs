using Mossmark.Inventory;

namespace Mossmark.Development
{
    // Item present in carry plus the settlement chest, combined, above a quantity threshold.
    public class ItemAvailableCondition : IDependencyCondition
    {
        private readonly ItemDefinition item;
        private readonly int quantity;

        public ItemAvailableCondition(ItemDefinition item, int quantity = 1)
        {
            this.item = item;
            this.quantity = quantity;
        }

        public bool IsSatisfied(DevelopableEntity entity)
        {
            int carried = InventoryManager.Instance != null ? InventoryManager.Instance.GetQuantity(item) : 0;
            int chested = ChestAttendable.Instance != null ? ChestAttendable.Instance.GetQuantity(item) : 0;
            return carried + chested >= quantity;
        }

        public string GetNeedsDescription(DevelopableEntity entity)
            => $"needs {item?.DisplayName ?? "???"}";
    }
}
