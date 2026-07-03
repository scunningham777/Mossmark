using System;
using Mossmark.Inventory;
using UnityEngine;

namespace Mossmark.Development
{
    // Item present in carry plus the settlement chest, combined, above a quantity threshold.
    // [Serializable] enables [SerializeReference] polymorphic storage in LandmarkAttendable.
    [Serializable]
    public class ItemAvailableCondition : IDependencyCondition
    {
        [SerializeField] private ItemDefinition item;
        [SerializeField] private int quantity = 1;

        public ItemDefinition Item => item;
        public int Quantity => quantity;

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
