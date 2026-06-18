using System;
using Mossmark.Attention;
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

    public class GenericWildernessSpotAttendable : MonoBehaviour, IAttendable
    {
        [SerializeField] private string displayName = "Field";
        [SerializeField] private string interactionVerb = "forage";
        [SerializeField] private ItemYield[] commonYields;
        [SerializeField] private ItemYield rareYield;
        [SerializeField, Range(0f, 1f)] private float rareDropChance = 0.08f;

        // Ongoing hold: each tick yields and (unless interrupted) the hold resets
        // and repeats. Each tick rerolls a fresh interval in this range so foraging
        // doesn't fall into a fixed metronomic rhythm.
        [SerializeField, Min(0.1f)] private float minTickInterval = 1.5f;
        [SerializeField, Min(0.1f)] private float maxTickInterval = 2f;

        private bool continueAttending;
        private float currentTickInterval;

        // Procedural-spawn entry point (Iteration 12's WorldGenerator) - sets the same
        // serialized fields an inspector-authored instance would carry, before SetActive(true).
        public void Initialize(string displayName, string interactionVerb, ItemYield[] commonYields,
            ItemYield rareYield, float rareDropChance, float minTickInterval, float maxTickInterval)
        {
            this.displayName = displayName;
            this.interactionVerb = interactionVerb;
            this.commonYields = commonYields;
            this.rareYield = rareYield;
            this.rareDropChance = rareDropChance;
            this.minTickInterval = minTickInterval;
            this.maxTickInterval = maxTickInterval;
        }

        private void Awake()
        {
            RollTickInterval();
        }

        public float AttentionDuration => currentTickInterval;

        public bool RequiresDaylight => true;

        // False on the tick a rare drop is rolled - the hold ends there so the
        // moment registers rather than disappearing into the next tick. A fresh
        // press resumes foraging immediately.
        public bool ContinueAttending => continueAttending;

        public bool CanAttend() => true;

        public string GetOverlayDescription() => displayName;

        public string GetOverlayInteractionLine() => $"Hold E to {interactionVerb}";

        public void OnAttentionComplete()
        {
            var request = new OutcomeRequest();
            new TwilightChanceModifier(1.5f).Apply(request);
            float effectiveRareChance = rareDropChance * request.ChanceMultiplier;
            continueAttending = true;
            ItemYieldRoller.Roll(displayName, "foraged", commonYields, rareYield, effectiveRareChance);
            RollTickInterval();
        }

        public void OnAttentionCancelled()
        {
        }

        private void RollTickInterval()
        {
            currentTickInterval = UnityEngine.Random.Range(minTickInterval, maxTickInterval);
        }
    }
}
