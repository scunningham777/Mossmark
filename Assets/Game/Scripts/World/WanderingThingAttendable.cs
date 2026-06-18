using System;
using Mossmark.Attention;
using Mossmark.Day;
using Mossmark.Development;
using Mossmark.Inventory;
using Mossmark.Visuals;
using UnityEngine;

namespace Mossmark.World
{
    // Iteration 14: a static-with-lifespan encounter. A single attention resolves it
    // into a good or bad outcome (per PROTOTYPE2.md's Wandering Things taxonomy entry),
    // and either resolving it or letting its lifespan run out despawns it - WanderingThingSpawner
    // then spawns the next one elsewhere after a cooldown.
    // Iteration 23 (G4+G5): accepts a WanderingThingDefinition instead of individual
    // per-creature fields; OnAttentionComplete loops def.additionalModifiers rather than
    // hardcoding Herald WorldStateChanceModifier calls.
    public class WanderingThingAttendable : MonoBehaviour, IAttendable
    {
        [SerializeField, Min(1f)] private float lifespanSeconds = 25f;

        private WanderingThingDefinition def;
        private string favorableSpecializationId;
        private float lifespanRemaining;
        private bool resolved;
        private Action onGone;

        // Procedural-spawn entry point (WanderingThingSpawner), same inactive-GO
        // Initialize() pattern as the other spawned attendables. onGone is called once,
        // whether this thing is resolved by attention or its lifespan simply runs out,
        // so the spawner knows to start its next cooldown.
        public void Initialize(WanderingThingDefinition def, string favorableSpecializationId,
            float lifespanSeconds, Action onGone)
        {
            this.def = def;
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
                Debug.Log($"{def.displayName}: gives up waiting and slips away.", this);
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

        public string GetOverlayDescription() => def.approachDescription;

        public string GetOverlayInteractionLine() => $"Hold E to {def.attendVerb}";

        public void OnAttentionComplete()
        {
            var request = new OutcomeRequest();
            new RealizedSpecializationChanceModifier(favorableSpecializationId, 1.6f).Apply(request);
            new WoundLoreModifier().Apply(request);
            if (def.additionalModifiers != null)
                foreach (var m in def.additionalModifiers)
                    new WorldStateChanceModifier(m.flagId, m.multiplier).Apply(request);
            float goodChance = Mathf.Min(1f, def.baseGoodChance * request.ChanceMultiplier);

            if (UnityEngine.Random.value < goodChance)
            {
                NotificationManager.Post($"{def.displayName}: {def.goodFlavor}");
                Debug.Log($"{def.displayName}: {def.goodFlavor}", this);
                ItemYieldRoller.Roll(def.displayName, "received", def.goodYields, null, 0f);
            }
            else
            {
                NotificationManager.Post($"{def.displayName}: {def.badFlavor}");
                Debug.Log($"{def.displayName}: {def.badFlavor}", this);

                int lost = InventoryManager.Instance != null ? InventoryManager.Instance.ClearInventory() : 0;
                if (lost > 0)
                    Debug.Log($"{def.displayName}: everything you were carrying is gone.", this);

                // DaylightCostMultiplier is 0 when Wound Lore is active - the Hedge Witch's
                // knowledge softens the blow, eliminating the extra daylight penalty.
                int effectiveDaylightCost = Mathf.RoundToInt(def.badDaylightCost * request.DaylightCostMultiplier);
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
