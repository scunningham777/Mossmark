using System.Collections.Generic;
using Mossmark.Development;
using Mossmark.Inventory;
using Mossmark.Visuals;
using UnityEngine;

namespace Mossmark.World
{
    // Shared weighted-yield-roll logic (Iteration 13 extraction): a weighted common
    // yield plus an independent rare-drop roll, both into the inventory. Used by
    // DevelopingWildernessSpotAttendable and PoiAttendable, which share this exact
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
        // rareYields (relational-data migration) is a weighted pool: rareDropChance still
        // gates whether a rare drops at all; the pool decides which — a single-entry pool
        // behaves exactly like the old scalar rareYield.
        public static bool Roll(string sourceName, string foundVerb, ItemYield[] commonYields,
            ItemYield[] rareYields, float rareDropChance, float? tendedness = null,
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
            if (rareYields != null && rareYields.Length > 0 && Random.value < effectiveRareChance)
            {
                var rare = PickWeighted(rareYields);
                if (rare?.Item != null)
                {
                    int qty = Random.Range(rare.MinQuantity, rare.MaxQuantity + 1);
                    int added = inventory.AddItem(rare.Item, qty);
                    if (added > 0)
                    {
                        Debug.Log($"{sourceName}: found a rare {added}x {rare.Item.DisplayName}!");
                        NotificationManager.Post($"Rare find: {added}x {rare.Item.DisplayName}");
                        return true;
                    }
                }
            }

            return false;
        }

        // Extracted from WildernessYieldAttendable (Iteration 43) so DevelopingWildernessSpotAttendable
        // can share the exact same knowledge-injection and hint-flavor behavior without
        // duplicating it — both classes roll yields for a spot, just with a different
        // progress model underneath.
        //
        // Checks each knowledge entry and collects items to inject into the common pool for
        // this tick only. Entries activate via requiredFlag (WorldState flag, checked first)
        // or requiredSpecializationId (NPC specialization realized) — flag takes priority if
        // both are set. Returns null when no entries are active (avoids allocation each tick).
        public static ItemYield[] BuildKnowledgeInjectedYields(KnowledgeYieldEntry[] knowledgeYields)
        {
            if (knowledgeYields == null || knowledgeYields.Length == 0) return null;
            List<ItemYield> result = null;
            foreach (var entry in knowledgeYields)
            {
                if (entry.item == null) continue;
                bool conditionMet = !string.IsNullOrEmpty(entry.requiredFlag)
                    ? WorldContext.GetFlag(entry.requiredFlag)
                    : !string.IsNullOrEmpty(entry.requiredSpecializationId)
                        && WorldContext.IsSpecializationRealized(entry.requiredSpecializationId);
                if (!conditionMet) continue;
                result ??= new List<ItemYield>();
                result.Add(new ItemYield
                {
                    Item = entry.item,
                    MinQuantity = entry.minQty,
                    MaxQuantity = entry.maxQty,
                    Weight = entry.injectedWeight
                });
            }
            return result?.ToArray();
        }

        // Low-weight ambient hint line, gated on a WorldState flag exactly like
        // KnowledgeYieldEntry's item injection, but delivered as flavor text via
        // notification rather than added to the yield pool. At most one per tick.
        // Returns true if a line fired, so callers (Iteration 48) can fall through to an
        // unconditional ambient pool only when this gated, rarer content didn't fire.
        public static bool TryFireHintFlavor(string sourceName, HintFlavorEntry[] hintFlavors)
        {
            if (hintFlavors == null || hintFlavors.Length == 0) return false;
            foreach (var hint in hintFlavors)
            {
                if (string.IsNullOrEmpty(hint.requiredFlag) || !WorldContext.GetFlag(hint.requiredFlag)) continue;
                if (Random.value >= hint.chance) continue;
                NotificationManager.Post(hint.text);
                Debug.Log($"{sourceName}: {hint.text}");
                return true;
            }
            return false;
        }

        // Iteration 48: mirrors TryFireHintFlavor's shape, but with no WorldState.GetFlag
        // check at all — that's the entire point of "unconditional." Fires at a fixed,
        // notably higher weight (call site passes 0.35 vs. hintFlavors' typical 0.15)
        // since this needs to be felt regularly, not discovered rarely, to test whether
        // ambient presence alone gives attending any pull independent of reward. No item,
        // no progress, no flag — a fired line has zero mechanical effect.
        public static void TryFireAmbientFlavor(string sourceName, string[] ambientFlavors, float chance)
        {
            if (ambientFlavors == null || ambientFlavors.Length == 0) return;
            if (Random.value >= chance) return;
            string line = ambientFlavors[Random.Range(0, ambientFlavors.Length)];
            NotificationManager.Post(line);
            Debug.Log($"{sourceName}: {line}");
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
