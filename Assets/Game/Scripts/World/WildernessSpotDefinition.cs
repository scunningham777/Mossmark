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
        [SerializeField] public string displayName;
        [SerializeField] public Color color = Color.white;

        // --- Generic spot (ongoing hold, yields per tick) ---
        [SerializeField] public string interactionVerb = "forage";
        [SerializeField] public ItemYield[] commonYields;
        [SerializeField] public ItemYield rareYield;
        [SerializeField, Range(0f, 1f)] public float rareDropChance = 0.08f;
        // Tick interval range for generic spots (tended spots ignore these).
        [SerializeField, Min(0.1f)] public float minTickInterval = 1.5f;
        [SerializeField, Min(0.1f)] public float maxTickInterval = 2f;

        // --- Tended spot (mark → wait → harvest) ---
        [SerializeField] public string tendVerb = "tend";
        [SerializeField] public ItemYield[] harvestYields;
        [SerializeField, Min(1)] public int restsToHarvest = 1;
        [SerializeField, Min(1)] public int maxConcurrentMarked = 2;
    }
}
