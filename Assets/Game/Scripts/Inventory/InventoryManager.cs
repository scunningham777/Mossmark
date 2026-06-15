using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mossmark.Inventory
{
    public class InventoryStack
    {
        public ItemDefinition Item;
        public int Quantity;
    }

    public class InventoryManager : MonoBehaviour
    {
        public static InventoryManager Instance { get; private set; }

        [SerializeField] private int carryLimit = 8;

        private readonly List<InventoryStack> stacks = new();

        public event Action InventoryChanged;

        public IReadOnlyList<InventoryStack> Stacks => stacks;
        public int CarryLimit => carryLimit;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        public int GetQuantity(ItemDefinition item)
        {
            int total = 0;
            foreach (var stack in stacks)
            {
                if (stack.Item == item) total += stack.Quantity;
            }
            return total;
        }

        // Returns how many units were actually added; any remainder is refused
        // (stack cap and carry limit both reached) rather than dropped on the ground.
        public int AddItem(ItemDefinition item, int quantity)
        {
            int remaining = quantity;

            foreach (var stack in stacks)
            {
                if (stack.Item != item || stack.Quantity >= item.StackCap) continue;

                int added = Mathf.Min(item.StackCap - stack.Quantity, remaining);
                stack.Quantity += added;
                remaining -= added;

                if (remaining <= 0) break;
            }

            while (remaining > 0 && stacks.Count < carryLimit)
            {
                int added = Mathf.Min(item.StackCap, remaining);
                stacks.Add(new InventoryStack { Item = item, Quantity = added });
                remaining -= added;
            }

            int totalAdded = quantity - remaining;
            if (totalAdded > 0) InventoryChanged?.Invoke();

            return totalAdded;
        }

        // Returns how many units were carried away. Used by Wandering Things' negative
        // outcome ("lose all carried items") - a full wipe rather than a per-item removal.
        public int ClearInventory()
        {
            int total = 0;
            foreach (var stack in stacks) total += stack.Quantity;

            if (total <= 0) return 0;

            stacks.Clear();
            InventoryChanged?.Invoke();
            return total;
        }

        // Returns how many units were actually removed (capped at what's carried);
        // emptied stacks are removed entirely.
        public int RemoveItem(ItemDefinition item, int quantity)
        {
            int remaining = quantity;

            for (int i = stacks.Count - 1; i >= 0 && remaining > 0; i--)
            {
                var stack = stacks[i];
                if (stack.Item != item) continue;

                int removed = Mathf.Min(stack.Quantity, remaining);
                stack.Quantity -= removed;
                remaining -= removed;

                if (stack.Quantity <= 0) stacks.RemoveAt(i);
            }

            int totalRemoved = quantity - remaining;
            if (totalRemoved > 0) InventoryChanged?.Invoke();

            return totalRemoved;
        }
    }
}
