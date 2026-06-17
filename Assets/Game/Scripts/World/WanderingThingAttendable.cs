using System;
using Mossmark.Attention;
using Mossmark.Day;
using Mossmark.Development;
using Mossmark.Inventory;
using UnityEngine;

namespace Mossmark.World
{
    // Iteration 14: a static-with-lifespan encounter. A single attention resolves it
    // into a good or bad outcome (per PROTOTYPE2.md's Wandering Things taxonomy entry),
    // and either resolving it or letting its lifespan run out despawns it - WanderingThingSpawner
    // then spawns the next one elsewhere after a cooldown.
    public class WanderingThingAttendable : MonoBehaviour, IAttendable
    {
        [SerializeField] private string displayName = "Wary Traveler";
        [SerializeField] private string approachDescription = "A traveler lingers at the path's edge, watching you with wary eyes.";
        [SerializeField] private string attendVerb = "approach";

        [SerializeField] private ItemYield[] goodYields;
        [SerializeField] private string goodFlavor = "decides you mean no harm, and presses something into your hand before moving on.";
        [SerializeField] private string badFlavor = "bolts, snatching everything from your pack as they go.";
        [SerializeField, Min(0)] private int badDaylightCost = 1;

        [SerializeField, Range(0f, 1f)] private float baseGoodChance = 0.5f;
        [SerializeField] private string favorableSpecializationId = "";

        [SerializeField, Min(1f)] private float lifespanSeconds = 25f;

        private float lifespanRemaining;
        private bool resolved;
        private Action onGone;

        // Procedural-spawn entry point (WanderingThingSpawner), same inactive-GO
        // Initialize() pattern as the other spawned attendables. onGone is called once,
        // whether this thing is resolved by attention or its lifespan simply runs out,
        // so the spawner knows to start its next cooldown.
        public void Initialize(string displayName, string approachDescription, string attendVerb,
            ItemYield[] goodYields, string goodFlavor, string badFlavor, int badDaylightCost,
            float baseGoodChance, string favorableSpecializationId,
            float lifespanSeconds, Action onGone)
        {
            this.displayName = displayName;
            this.approachDescription = approachDescription;
            this.attendVerb = attendVerb;
            this.goodYields = goodYields;
            this.goodFlavor = goodFlavor;
            this.badFlavor = badFlavor;
            this.badDaylightCost = badDaylightCost;
            this.baseGoodChance = baseGoodChance;
            this.favorableSpecializationId = favorableSpecializationId;
            this.lifespanSeconds = lifespanSeconds;
            this.onGone = onGone;
        }

        private void Awake()
        {
            lifespanRemaining = lifespanSeconds;
        }

        private void Update()
        {
            if (resolved) return;

            lifespanRemaining -= Time.deltaTime;
            if (lifespanRemaining <= 0f)
            {
                Debug.Log($"{displayName}: gives up waiting and slips away.", this);
                Despawn();
            }
        }

        public float AttentionDuration => 2f;

        // The encounter itself spends the day's time regardless of outcome; a bad
        // outcome adds badDaylightCost on top of this in OnAttentionComplete.
        public bool RequiresDaylight => true;

        // A single attention fully resolves the encounter - not a repeating hold.
        public bool ContinueAttending => false;

        public bool CanAttend() => true;

        public string GetOverlayDescription() => approachDescription;

        public string GetOverlayInteractionLine() => $"Hold E to {attendVerb}";

        public void OnAttentionComplete()
        {
            var request = new OutcomeRequest();
            new RealizedSpecializationChanceModifier(favorableSpecializationId, 1.6f).Apply(request);
            new WoundLoreModifier().Apply(request);
            // Herald post-spec stages raise good-outcome odds via WorldState flags.
            new WorldStateChanceModifier("herald_trail_markers", 1.3f).Apply(request);
            new WorldStateChanceModifier("herald_toll_records", 1.2f).Apply(request);
            float goodChance = Mathf.Min(1f, baseGoodChance * request.ChanceMultiplier);

            if (UnityEngine.Random.value < goodChance)
            {
                Debug.Log($"{displayName}: {goodFlavor}", this);
                ItemYieldRoller.Roll(displayName, "received", goodYields, null, 0f);
            }
            else
            {
                Debug.Log($"{displayName}: {badFlavor}", this);

                int lost = InventoryManager.Instance != null ? InventoryManager.Instance.ClearInventory() : 0;
                if (lost > 0)
                    Debug.Log($"{displayName}: everything you were carrying is gone.", this);

                // DaylightCostMultiplier is 0 when Wound Lore is active - the Hedge Witch's
                // knowledge softens the blow, eliminating the extra daylight penalty.
                int effectiveDaylightCost = Mathf.RoundToInt(badDaylightCost * request.DaylightCostMultiplier);
                if (effectiveDaylightCost > 0)
                    DayCycleManager.Instance?.SpendDaylight(effectiveDaylightCost);
            }

            Despawn();
        }

        public void OnAttentionCancelled()
        {
        }

        private void Despawn()
        {
            if (resolved) return;
            resolved = true;

            var collider = GetComponent<Collider2D>();
            if (collider != null) collider.enabled = false;

            onGone?.Invoke();
            Destroy(gameObject);
        }
    }
}
