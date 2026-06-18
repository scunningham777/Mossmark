using System;
using UnityEngine;

namespace Mossmark.World
{
    [Serializable]
    public class WorldStateOddsModifier
    {
        public string flagId;
        public float multiplier = 1.3f;
    }

    // Iteration 23 (G4): per-creature data extracted from WanderingThingSpawner so new
    // creature types are new assets, not new scene GameObjects or code changes.
    [CreateAssetMenu(menuName = "Mossmark/Wandering Thing Definition", fileName = "NewWanderingThing")]
    public class WanderingThingDefinition : ScriptableObject
    {
        [SerializeField] public string displayName = "Wary Traveler";
        [SerializeField] public string approachDescription = "A traveler lingers at the path's edge, watching you with wary eyes.";
        [SerializeField] public string attendVerb = "approach";
        [SerializeField] public Color color = new(0.5f, 0.45f, 0.55f, 1f);
        [SerializeField] public float colliderRadius = 0.5f;

        [SerializeField] public ItemYield[] goodYields;
        [SerializeField] public string goodFlavor = "decides you mean no harm, and presses something into your hand before moving on.";
        [SerializeField] public string badFlavor = "bolts, snatching everything from your pack as they go.";
        [SerializeField, Min(0)] public int badDaylightCost = 1;
        [SerializeField, Range(0f, 1f)] public float baseGoodChance = 0.5f;

        // (G5) Flag-keyed goodChance multipliers authored in data rather than
        // hardcoded in WanderingThingAttendable. WoundLoreModifier and
        // RealizedSpecializationChanceModifier stay hardcoded because they have
        // special logic (DaylightCostMultiplier dimension and session-specific
        // favorableSpecializationId) that can't be collapsed to a flag lookup.
        [SerializeField] public WorldStateOddsModifier[] additionalModifiers;
    }
}
