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
    }
}
