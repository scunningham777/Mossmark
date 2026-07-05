using Mossmark.Attention;
using Mossmark.Day;
using Mossmark.Development;
using Mossmark.Visuals;
using UnityEngine;

namespace Mossmark.World
{
    // Shared base class for GenericWildernessSpotAttendable and PoiAttendable (G7). Both
    // had identical fields, tick logic, and IAttendable plumbing — only their overlay
    // text and gate logic differ. Subclasses implement the three abstract overlay methods
    // and may override GetEffectiveRareChance() to bias the drop chance with modifiers.
    //
    // Iteration 27: tendedness (float [0,1], initial 0.5) is per-session runtime state.
    // Each attended tick raises it (+0.04); each rest lowers it if unattended (-0.08) or
    // raises it slightly if attended that day (+0.03). Yield qty range and rare-drop chance
    // both respond at the extremes (>0.7 well-tended / <0.3 depleted).
    //
    // Iteration 28: knowledgeYields is an array of KnowledgeYieldEntry — each entry checks
    // a WorldState flag at roll time and, if true, injects an extra ItemYield into the common
    // pool for that tick only (not modifying the asset). Applied in OnAttentionComplete().
    //
    // Iteration 43: exposes the same Tendedness reading that WorldGenerator's spotsById
    // registry hands out to any cross-influence consumer (NpcStageDef.passiveDriftSourceSpotId
    // reads it via WorldGenerator.GetSpot()). DevelopingWildernessSpotAttendable (which does
    // NOT extend this class — it has its own exhaustion/Standing progress model) implements
    // this too with a synthetic value, so the registry works uniformly for both spot kinds.
    public interface ITendednessSource
    {
        float Tendedness { get; }
    }

    public abstract class WildernessYieldAttendable : MonoBehaviour, IAttendable, ITendednessSource
    {
        [SerializeField] protected string displayName = "Spot";
        [SerializeField] protected string interactionVerb = "forage";
        [SerializeField] protected ItemYield[] commonYields;
        [SerializeField] protected ItemYield[] rareYields;
        [SerializeField, Range(0f, 1f)] protected float rareDropChance = 0.08f;

        [SerializeField, Min(0.1f)] protected float minTickInterval = 1.5f;
        [SerializeField, Min(0.1f)] protected float maxTickInterval = 2f;

        protected bool continueAttending;
        protected float currentTickInterval;
        protected string foundVerb = "foraged";

        // Runtime tendedness — not serialized, per-session only like all other spot state.
        // Drifts up with each attended tick (+0.04) and adjusts at each rest.
        private float tendedness = 0.5f;
        private bool attendedThisDay = false;

        // Iteration 31: read-only access for external passive-progress sources
        // (NpcAttendable's Bog Keeper pilot) — the same continuous value the
        // overlay/yield logic already uses internally.
        public float Tendedness => tendedness;

        // Knowledge yield entries — set via Initialize(), not serialized on the base class
        // (each subclass's Initialize path copies from its definition source).
        private KnowledgeYieldEntry[] knowledgeYields;

        // Iteration 42: same "set via Initialize(), not serialized on the base class"
        // shape as knowledgeYields, for ambient hint flavor lines.
        private HintFlavorEntry[] hintFlavors;

        protected void InitializeBase(string displayName, string interactionVerb,
            ItemYield[] commonYields, ItemYield[] rareYields, float rareDropChance,
            float minTickInterval, float maxTickInterval,
            KnowledgeYieldEntry[] knowledgeYields = null, HintFlavorEntry[] hintFlavors = null)
        {
            this.displayName = displayName;
            this.interactionVerb = interactionVerb;
            this.commonYields = commonYields;
            this.rareYields = rareYields;
            this.rareDropChance = rareDropChance;
            this.minTickInterval = minTickInterval;
            this.maxTickInterval = maxTickInterval;
            this.knowledgeYields = knowledgeYields;
            this.hintFlavors = hintFlavors;
        }

        protected virtual void Awake()
        {
            RollTickInterval();
        }

        protected virtual void Start()
        {
            if (DayCycleManager.Instance != null)
                DayCycleManager.Instance.DayAdvanced += OnDayAdvanced;
        }

        protected virtual void OnDestroy()
        {
            if (DayCycleManager.Instance != null)
                DayCycleManager.Instance.DayAdvanced -= OnDayAdvanced;
        }

        public float AttentionDuration => currentTickInterval;
        public bool RequiresDaylight => true;
        public bool ContinueAttending => continueAttending;

        // Fired on every productive OnAttentionComplete() — wilderness spots always make
        // progress, so this fires every completed tick.
        public event System.Action OnProgressMade;

        public abstract bool CanAttend();
        public virtual string GetShortName() => displayName;
        public abstract string GetOverlayDescription();
        public abstract string GetOverlayInteractionLine();
        public virtual System.Collections.Generic.IReadOnlyList<string> GetAppliedUpgrades() =>
            System.Array.Empty<string>();

        public void OnAttentionComplete()
        {
            continueAttending = true;
            tendedness = Mathf.Clamp01(tendedness + 0.04f);
            attendedThisDay = true;
            ItemYieldRoller.Roll(displayName, foundVerb, commonYields, rareYields,
                GetEffectiveRareChance(), tendedness, ItemYieldRoller.BuildKnowledgeInjectedYields(knowledgeYields));
            ItemYieldRoller.TryFireHintFlavor(displayName, hintFlavors);
            RollTickInterval();
            OnProgressMade?.Invoke();
        }

        public void OnAttentionCancelled() { }

        // Subclasses that apply outcome modifiers (e.g. TwilightChanceModifier) override this.
        // PoiAttendable uses the base rareDropChance unchanged — POIs are distinctive
        // rare-access locations, not ambient foraging spots.
        protected virtual float GetEffectiveRareChance() => rareDropChance;

        protected void RollTickInterval()
        {
            currentTickInterval = Random.Range(minTickInterval, maxTickInterval);
        }

        // Appends a tendedness description to the base description string at the extremes.
        // Middle band (0.3 to 0.7) returns the base unchanged so the overlay stays clean
        // during normal foraging.
        protected string WithTendednessSuffix(string baseDescription)
        {
            if (tendedness > 0.7f)
                return $"{baseDescription} — this place feels well-known to you";
            if (tendedness < 0.3f)
                return $"{baseDescription} — the ground here is disturbed";
            return baseDescription;
        }

        // Applies daily drift to tendedness. Attended spots gain a small cultivation bonus;
        // neglected spots lose ground faster — neglect accumulates over multiple days toward
        // the depleted (<0.3) band. Values are tuning baselines per Iteration 27 spec.
        private void OnDayAdvanced()
        {
            if (attendedThisDay)
                tendedness = Mathf.Clamp01(tendedness + 0.03f);
            else
                tendedness = Mathf.Clamp01(tendedness - 0.08f);

            attendedThisDay = false;
            Debug.Log($"{displayName}: tendedness {tendedness:F2} after rest.");
        }
    }
}
