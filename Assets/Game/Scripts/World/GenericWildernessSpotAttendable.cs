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

    // Ongoing wilderness spot (foraging, digging, etc.) — thin subclass of
    // WildernessYieldAttendable that adds the TwilightChanceModifier bias on rare drops.
    public class GenericWildernessSpotAttendable : WildernessYieldAttendable
    {
        public void Initialize(string displayName, string interactionVerb, ItemYield[] commonYields,
            ItemYield rareYield, float rareDropChance, float minTickInterval, float maxTickInterval)
        {
            InitializeBase(displayName, interactionVerb, commonYields, rareYield, rareDropChance,
                minTickInterval, maxTickInterval);
        }

        public override bool CanAttend() => true;

        public override string GetOverlayDescription() => displayName;

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
