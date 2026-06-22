using Mossmark.Inventory;
using Mossmark.Visuals;
using UnityEngine;

namespace Mossmark.World
{
    // Shared weighted-yield-roll logic (Iteration 13 extraction): a weighted common
    // yield plus an independent rare-drop roll, both into the inventory. Used by both
    // GenericWildernessSpotAttendable and PoiAttendable, which share this exact
    // "one common roll + one rare roll" shape from Iteration 4/8.5.
    //
    // Iteration 27: tendedness parameter (optional, null = current behavior) adjusts
    // yield at the extremes — well-tended spots (>0.7) shift qty up by 1 and boost rare
    // chance x1.2; depleted spots (<0.3) shift qty down by 1 (floor 0) and reduce rare
    // chance x0.7. Middle band produces no adjustment.
    //
    // Iteration 28: injectedYields parameter (optional, null = no injection) adds extra
    // entries to the common pool for this tick only — used by KnowledgeYieldEntry/
    // WildernessYieldAttendable to surface NPC-knowledge-gated items without permanently
    // modifying the spot's asset. Pool is merged in memory; the asset is unchanged.
    public static class ItemYieldRoller
    {
        // foundVerb is the past-tense verb used in the common-yield log line (e.g.
        // "foraged" for wilderness spots, "found" for POIs) - callers keep their own
        // existing log wording. Returns true only if the rare drop hit and there was
        // room to carry it - the "interrupt" signal callers use to end an ongoing hold.
        // tendedness is null for callers that have no tendedness state (e.g. POI pre-unlock
        // checks, or future call sites that don't participate in the system).
        // injectedYields (Iteration 28) are merged with commonYields for this tick only.
        public static bool Roll(string sourceName, string foundVerb, ItemYield[] commonYields,
            ItemYield rareYield, float rareDropChance, float? tendedness = null,
            ItemYield[] injectedYields = null)
        {
            var inventory = InventoryManager.Instance;
            if (inventory == null) return false;

            int qtyBonus = 0;
            float chanceMultiplier = 1f;
            if (tendedness.HasValue)
            {
                if (tendedness.Value > 0.7f)      { qtyBonus = 1;  chanceMultiplier = 1.2f; }
                else if (tendedness.Value < 0.3f) { qtyBonus = -1; chanceMultiplier = 0.7f; }
            }

            // Merge injected yields into the pool for this tick only — no allocation
            // when there are no injections (the common case).
            ItemYield[] effectivePool = commonYields;
            if (injectedYields != null && injectedYields.Length > 0)
            {
                int baseLen = commonYields?.Length ?? 0;
                effectivePool = new ItemYield[baseLen + injectedYields.Length];
                if (baseLen > 0) System.Array.Copy(commonYields, effectivePool, baseLen);
                System.Array.Copy(injectedYields, 0, effectivePool, baseLen, injectedYields.Length);
            }

            var picked = PickWeighted(effectivePool);
            if (picked != null && picked.Item != null)
            {
                int qty = Mathf.Max(0, Random.Range(
                    picked.MinQuantity + qtyBonus,
                    picked.MaxQuantity + 1 + qtyBonus));

                if (qty > 0)
                {
                    int added = inventory.AddItem(picked.Item, qty);
                    Debug.Log(added > 0
                        ? $"{sourceName}: {foundVerb} {added}x {picked.Item.DisplayName}."
                        : $"{sourceName}: found {picked.Item.DisplayName}, but there's no room to carry it.");
                }
                else
                {
                    Debug.Log($"{sourceName}: the spot feels thin — nothing found this time.");
                }
            }

            float effectiveRareChance = rareDropChance * chanceMultiplier;
            if (rareYield != null && rareYield.Item != null && Random.value < effectiveRareChance)
            {
                int qty = Random.Range(rareYield.MinQuantity, rareYield.MaxQuantity + 1);
                int added = inventory.AddItem(rareYield.Item, qty);
                if (added > 0)
                {
                    Debug.Log($"{sourceName}: found a rare {added}x {rareYield.Item.DisplayName}!");
                    NotificationManager.Post($"Rare find: {added}x {rareYield.Item.DisplayName}");
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
