using System;
using Mossmark.Development;
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

    // Ongoing wilderness spot (foraging, digging, etc.) — thin subclass of
    // WildernessYieldAttendable that adds the TwilightChanceModifier bias on rare drops.
    public class GenericWildernessSpotAttendable : WildernessYieldAttendable
    {
        public void Initialize(string displayName, string interactionVerb, ItemYield[] commonYields,
            ItemYield[] rareYields, float rareDropChance, float minTickInterval, float maxTickInterval,
            KnowledgeYieldEntry[] knowledgeYields = null)
        {
            InitializeBase(displayName, interactionVerb, commonYields, rareYields, rareDropChance,
                minTickInterval, maxTickInterval, knowledgeYields);
        }

        public override bool CanAttend() => true;

        public override string GetOverlayDescription() => WithTendednessSuffix(displayName);

        public override string GetOverlayInteractionLine() => $"Hold E to {interactionVerb}";

        // Dawn/Dusk lifts rare-drop odds by ×1.5 — first outcome modifier responding to
        // time-of-day rather than town development state (Iteration 16.5).
        protected override float GetEffectiveRareChance()
        {
            var request = new OutcomeRequest();
            new TwilightChanceModifier(1.5f).Apply(request);
            return rareDropChance * request.ChanceMultiplier;
        }
    }
}
