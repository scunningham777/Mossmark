using System;
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

    // Per-entry authored knowledge yield: gates an extra item injection into the spot's
    // common pool for a single tick. Either requiredFlag (WorldState flag) or
    // requiredSpecializationId (specialization realized) activates the entry — flag takes
    // priority if both are set. Authored on WildernessSpotDefinition (archetype spots
    // included, since the relational-data migration made those real spot assets).
    [Serializable]
    public class KnowledgeYieldEntry
    {
        public string requiredFlag;
        public string requiredSpecializationId;
        public ItemDefinition item;
        public int minQty = 1;
        public int maxQty = 2;
        public float injectedWeight = 0.15f;
    }

    // Iteration 42: analogous to KnowledgeYieldEntry, but delivers a flavor line instead
    // of an item — an ambient hint rather than a reward. Gated the same way (WorldState
    // flag), rolled at low weight each tick the flag holds true rather than injected into
    // the yield pool.
    [Serializable]
    public class HintFlavorEntry
    {
        public string requiredFlag;
        [Range(0f, 1f)] public float chance = 0.15f;
        public string text;
    }

    // Data asset for one type of generic or tended wilderness spot. WorldGenerator
    // draws from a pool of these at session start and places them randomly in the
    // wilderness, so new spot types are added here (as assets) without touching
    // the generator logic.
    [CreateAssetMenu(menuName = "Mossmark/Wilderness Spot Definition", fileName = "NewWildernessSpot")]
    public class WildernessSpotDefinition : ScriptableObject
    {
        public enum SpotKind { Generic, Tended }

        [SerializeField] public SpotKind kind;
        // Stable id for spots other systems reference (e.g. NpcStageDef's
        // passiveDriftSourceSpotId reads this spot's tendedness). Empty for spots nothing
        // references — most of the generic pool. WorldGenerator registers spawned Generic
        // spots with a non-empty spotId; if several instances share one id, the last
        // spawned wins the registry slot.
        [SerializeField] public string spotId;
        [SerializeField] public string displayName;
        [SerializeField] public Color color = Color.white;

        // --- Generic spot (ongoing hold, yields per tick) ---
        [SerializeField] public string interactionVerb = "forage";
        [SerializeField] public ItemYield[] commonYields;
        // Weighted pool of rare candidates — rareDropChance gates whether a rare drops,
        // the pool decides which. A single entry reproduces the old scalar behavior.
        [SerializeField] public ItemYield[] rareYields;
        [SerializeField, Range(0f, 1f)] public float rareDropChance = 0.08f;
        // Optional shared-pool reference (P1 LootTable pattern): when set, the table's
        // entries REPLACE rareYields above. Use for pools genuinely shared across spot
        // types (e.g. old_coin_finds); inline lists remain right for one-off content.
        // commonYields has no table counterpart — no shared common pool has existed yet.
        [SerializeField] public YieldTable rareYieldTable;
        // Tick interval range for generic spots (tended spots ignore these).
        [SerializeField, Min(0.1f)] public float minTickInterval = 1.5f;
        [SerializeField, Min(0.1f)] public float maxTickInterval = 2f;

        // --- Tended spot (mark → wait → harvest) ---
        [SerializeField] public string tendVerb = "tend";
        [SerializeField] public ItemYield[] harvestYields;
        [SerializeField, Min(1)] public int restsToHarvest = 1;
        [SerializeField, Min(1)] public int maxConcurrentMarked = 2;

        // Knowledge yield entries — usually empty for generic pool spots; checked at roll
        // time against WorldState flags to inject extra items into the common pool per tick.
        [SerializeField] public KnowledgeYieldEntry[] knowledgeYields = System.Array.Empty<KnowledgeYieldEntry>();

        // Iteration 42 (Site Clustering pilot): low-weight ambient flavor lines, gated on a
        // WorldState flag, fired instead of an item injection. Usually empty.
        [SerializeField] public HintFlavorEntry[] hintFlavors = System.Array.Empty<HintFlavorEntry>();

        // Iteration 48 (Fen Bog pilot): a second, ungated flavor pool — no WorldState flag,
        // no item, no progress. Fires at a fixed, higher weight than hintFlavors to test
        // whether ambient presence alone gives attending any pull independent of reward.
        // Hand-authored asset field, not CSV-wired, same discipline as hintFlavors itself.
        [SerializeField] public string[] ambientFlavors = System.Array.Empty<string>();

        // Iteration 43 (Fen Bog pilot), generalized to every Generic spot in Iteration 44:
        // the exhaustion + latched Standing stage-pool DevelopingWildernessSpotAttendable
        // uses. Wired into the CSV pipeline (wilderness_spots.csv's spotStagePool column)
        // as of Iteration 44 — every Generic spot has one; Tended spots and POIs don't use
        // this field at all.
        [SerializeField] public SpotStagePool spotStagePool;

        public ItemYield[] EffectiveRareYields =>
            rareYieldTable != null ? rareYieldTable.Entries : rareYields;
    }
}
