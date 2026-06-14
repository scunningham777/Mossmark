using Mossmark.Inventory;

namespace Mossmark.Development
{
    // Item present in carry (or, once the chest exists, chest) above a quantity threshold.
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
            var inventory = InventoryManager.Instance;
            return inventory != null && inventory.GetQuantity(item) >= quantity;
        }

        public string GetNeedsDescription(DevelopableEntity entity)
            => $"needs {quantity}x {item?.DisplayName ?? "???"}";
    }
}
