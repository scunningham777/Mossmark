using UnityEngine;

namespace Mossmark.World
{
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

        public ItemYield[] EffectiveRareYields =>
            rareYieldTable != null ? rareYieldTable.Entries : rareYields;
    }
}
