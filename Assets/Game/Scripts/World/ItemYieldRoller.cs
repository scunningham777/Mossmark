using Mossmark.Inventory;
using UnityEngine;

namespace Mossmark.World
{
    // Shared weighted-yield-roll logic (Iteration 13 extraction): a weighted common
    // yield plus an independent rare-drop roll, both into the inventory. Used by both
    // GenericWildernessSpotAttendable and PoiAttendable, which share this exact
    // "one common roll + one rare roll" shape from Iteration 4/8.5.
    public static class ItemYieldRoller
    {
        // foundVerb is the past-tense verb used in the common-yield log line (e.g.
        // "foraged" for wilderness spots, "found" for POIs) - callers keep their own
        // existing log wording. Returns true only if the rare drop hit and there was
        // room to carry it - the "interrupt" signal callers use to end an ongoing hold.
        public static bool Roll(string sourceName, string foundVerb, ItemYield[] commonYields, ItemYield rareYield, float rareDropChance)
        {
            var inventory = InventoryManager.Instance;
            if (inventory == null) return false;

            var picked = PickWeighted(commonYields);
            if (picked != null && picked.Item != null)
            {
                int qty = Random.Range(picked.MinQuantity, picked.MaxQuantity + 1);
                int added = inventory.AddItem(picked.Item, qty);
                Debug.Log(added > 0
                    ? $"{sourceName}: {foundVerb} {added}x {picked.Item.DisplayName}."
                    : $"{sourceName}: found {picked.Item.DisplayName}, but there's no room to carry it.");
            }

            if (rareYield != null && rareYield.Item != null && Random.value < rareDropChance)
            {
                int qty = Random.Range(rareYield.MinQuantity, rareYield.MaxQuantity + 1);
                int added = inventory.AddItem(rareYield.Item, qty);
                if (added > 0)
                {
                    Debug.Log($"{sourceName}: found a rare {added}x {rareYield.Item.DisplayName}!");
                    return true;
                }
            }

            return false;
        }

        private static ItemYield PickWeighted(ItemYield[] yields)
        {
            if (yields == null || yields.Length == 0) return null;

            float total = 0f;
            foreach (var entry in yields) total += Mathf.Max(0f, entry.Weight);

            if (total <= 0f) return null;

            float roll = Random.value * total;
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
