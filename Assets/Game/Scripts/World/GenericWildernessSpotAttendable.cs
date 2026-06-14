using System;
using Mossmark.Attention;
using Mossmark.Inventory;
using UnityEngine;

namespace Mossmark.World
{
    [Serializable]
    public class ItemYield
    {
        public ItemDefinition Item;
        public int MinQuantity = 1;
        public int MaxQuantity = 1;
        [Min(0f)] public float Weight = 1f;
    }

    public class GenericWildernessSpotAttendable : MonoBehaviour, IAttendable
    {
        [SerializeField] private string displayName = "Field";
        [SerializeField] private string interactionVerb = "forage";
        [SerializeField] private ItemYield[] commonYields;
        [SerializeField] private ItemYield rareYield;
        [SerializeField, Range(0f, 1f)] private float rareDropChance = 0.08f;

        // Generic wilderness spots have no state and yield instantly -
        // AttentionManager completes attention on the same frame the hold begins.
        public float AttentionDuration => 0f;

        public bool RequiresStamina => true;

        public bool CanAttend() => true;

        public string GetOverlayDescription() => displayName;

        public string GetOverlayInteractionLine() => $"Press E to {interactionVerb}";

        public void OnAttentionComplete()
        {
            RollYield();
        }

        public void OnAttentionCancelled()
        {
        }

        private void RollYield()
        {
            var inventory = InventoryManager.Instance;
            if (inventory == null) return;

            var picked = PickWeighted(commonYields);
            if (picked != null && picked.Item != null)
            {
                int qty = UnityEngine.Random.Range(picked.MinQuantity, picked.MaxQuantity + 1);
                int added = inventory.AddItem(picked.Item, qty);
                Debug.Log(added > 0
                    ? $"{displayName}: foraged {added}x {picked.Item.DisplayName}."
                    : $"{displayName}: found {picked.Item.DisplayName}, but there's no room to carry it.", this);
            }

            if (rareYield != null && rareYield.Item != null && UnityEngine.Random.value < rareDropChance)
            {
                int qty = UnityEngine.Random.Range(rareYield.MinQuantity, rareYield.MaxQuantity + 1);
                int added = inventory.AddItem(rareYield.Item, qty);
                if (added > 0)
                {
                    Debug.Log($"{displayName}: found a rare {added}x {rareYield.Item.DisplayName}!", this);
                }
            }
        }

        private static ItemYield PickWeighted(ItemYield[] yields)
        {
            if (yields == null || yields.Length == 0) return null;

            float total = 0f;
            foreach (var entry in yields) total += Mathf.Max(0f, entry.Weight);

            if (total <= 0f) return null;

            float roll = UnityEngine.Random.value * total;
            float cumulative = 0f;

            foreach (var entry in yields)
            {
                cumulative += Mathf.Max(0f, entry.Weight);
                if (roll <= cumulative) return entry;
            }

            return yields[^1];
        }
    }
}
