using System.Collections.Generic;
using Mossmark.Attention;
using UnityEngine;

namespace Mossmark.Inventory
{
    public class ChestAttendable : MonoBehaviour, IAttendable
    {
        [SerializeField] private float openDuration = 0.5f;

        // One settlement chest per scene, so its contents can be checked from
        // ItemAvailableCondition/BuildingAttendable the same way InventoryManager.Instance is.
        public static ChestAttendable Instance { get; private set; }

        private readonly List<InventoryStack> stacks = new();

        public IReadOnlyList<InventoryStack> Stacks => stacks;

        public float AttentionDuration => openDuration;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        // Opening the chest is a free menu interaction, not an active extraction.
        public bool RequiresDaylight => false;

        // Opening is a single check-in; the menu itself (ChestUI) owns deposit/withdraw.
        public bool ContinueAttending => false;

        public bool CanAttend() => true;

        public string GetShortName() => "Settlement Chest";
        public string GetOverlayDescription() => "Settlement Chest";

        public string GetOverlayInteractionLine() => "Hold E to open the chest";

        public System.Collections.Generic.IReadOnlyList<string> GetAppliedUpgrades() =>
            System.Array.Empty<string>();

        public void OnAttentionComplete()
        {
            ChestUI.Instance?.Open(this);
        }

        public void OnAttentionCancelled()
        {
        }

        // Mirrors InventoryManager.GetQuantity - total carried count across all matching stacks.
        public int GetQuantity(ItemDefinition item)
        {
            int total = 0;
            foreach (var stack in stacks)
            {
                if (stack.Item == item) total += stack.Quantity;
            }
            return total;
        }

        // Unlimited stacks and per-stack quantity, per the design draft - a deposit never fails.
        public void Deposit(ItemDefinition item, int quantity)
        {
            if (quantity <= 0) return;

            foreach (var stack in stacks)
            {
                if (stack.Item != item) continue;

                stack.Quantity += quantity;
                return;
            }

            stacks.Add(new InventoryStack { Item = item, Quantity = quantity });
        }

        // Returns how many units were actually removed (capped at what's stored);
        // an emptied stack is removed entirely.
        public int Withdraw(ItemDefinition item, int quantity)
        {
            for (int i = 0; i < stacks.Count; i++)
            {
                var stack = stacks[i];
                if (stack.Item != item) continue;

                int removed = Mathf.Min(stack.Quantity, quantity);
                stack.Quantity -= removed;
                if (stack.Quantity <= 0) stacks.RemoveAt(i);

                return removed;
            }

            return 0;
        }
    }
}
