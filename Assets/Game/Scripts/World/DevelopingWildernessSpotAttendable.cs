using System.Collections.Generic;
using Mossmark.Attention;
using Mossmark.Day;
using Mossmark.Development;
using Mossmark.Visuals;
using UnityEngine;

namespace Mossmark.World
{
    // Iteration 43 piloted this on Fen Bog alone; Iteration 44 made it every Generic
    // wilderness spot's class (GenericWildernessSpotAttendable, the old continuous-
    // `tendedness` implementation, is retired). Tended spots (TendedSpotAttendable) and
    // POIs (PoiAttendable, which still extends WildernessYieldAttendable and keeps the
    // continuous-tendedness model) are explicitly out of scope — this class gives
    // DevelopableEntity's stage-pool treatment (already shared by NPCs and Buildings) to
    // ongoing forage/dig spots specifically, replacing the single continuous `tendedness`
    // float with two separate mechanics that were previously conflated:
    //
    //   Exhaustion — session-scoped, resets every rest. Represents same-day fatigue, in two
    //   stacking tiers (Iteration 43.1): past exhaustionPenaltyThreshold, attending costs
    //   double daylight and takes double the hold duration (ExhaustionCostModifier, the same
    //   OutcomeRequest/IOutcomeModifier pipeline BuildingAttendable's cold-tax uses) — a felt
    //   cost the player can't miss, before yield ever changes. Only once exhaustion reaches
    //   1.0 ("overworked" for the day) does the yield penalty stack on top, reusing
    //   ItemYieldRoller's existing depleted-tendedness-band shape (qty -1, rare chance x0.7)
    //   via a synthetic value rather than duplicating the band math. Overworked also marks
    //   the day for the goodAttentionDays counter below.
    //
    //   Standing — latched, multi-day, never reverts. A SpotStagePool track (mirrors
    //   NpcStagePool/BuildingStagePool) gated by SustainedGoodAttentionCondition, which reads
    //   goodAttentionDays: a per-rest counter that only advances on a day the spot was
    //   attended AND wasn't overworked. Crossing a stage fires the Iteration 32 stage-cross
    //   feedback tier (via DevelopableEntity.OnDeveloped, which EntityFeedback already
    //   listens for) instead of the plain progress pulse.
    //
    // Every attend still yields something every tick regardless of Standing's gate state —
    // foraging is not blocked the way Building/NPC development is — so this doesn't route
    // through ResolveAttention()'s dependency-gated "no progress" short-circuit. It calls
    // AddProgress()/TryApplyStage() directly (still resolving through the same Develop-kind
    // machinery Buildings use — see AttentionOutcomeKind) and raises the progress-tick
    // signal itself via RaiseProgressMade().
    public class DevelopingWildernessSpotAttendable : DevelopableEntity, IAttendable, IGoodAttentionTracker, ITendednessSource
    {
        [SerializeField] private string displayName = "Spot";
        [SerializeField] private string interactionVerb = "forage";
        [SerializeField] private ItemYield[] commonYields;
        [SerializeField] private ItemYield[] rareYields;
        [SerializeField, Range(0f, 1f)] private float rareDropChance = 0.08f;
        [SerializeField, Min(0.1f)] private float minTickInterval = 1.5f;
        [SerializeField, Min(0.1f)] private float maxTickInterval = 2f;

        [SerializeField, Min(0f)] private float exhaustionPerTick = 0.12f;
        [SerializeField, Range(0f, 1f)] private float exhaustionPenaltyThreshold = 0.6f;

        private KnowledgeYieldEntry[] knowledgeYields;
        private HintFlavorEntry[] hintFlavors;
        private SpotStagePool stagePool;

        private DevelopmentTrack track;
        // Parallel to track.Stages — index-aligned the same way BuildingAttendable's
        // stages[CurrentStageIndex] lookup works, so the currently-applied stage's extra
        // (non-IDependencyCondition) data is always one array read away.
        private SpotStageDef[] stageDefs = System.Array.Empty<SpotStageDef>();

        private float currentTickInterval;
        private float exhaustion;
        private bool overworkedToday;
        private bool attendedThisDay;
        private int goodAttentionDays;
        // Cached so HandleDeveloped can set the stage's tint before EntityFeedback's
        // stage-cross handler bakes its circle sprite from SpriteRenderer.color.
        private SpriteRenderer spriteRenderer;

        // Iteration 47 (generalized to every archetype's Site): set only for a Site's
        // member spots (WorldGenerator wires this before activation, inactive-GO pattern).
        // Random-pool spots not pulled into any Site keep this null and fall back to their
        // own goodAttentionDays field below — zero behavior change for those.
        private WorldSite site;

        // IGoodAttentionTracker — read by SustainedGoodAttentionCondition. Delegates to
        // the shared Site counter when this spot is a Site member, so Standing rewards
        // touring the cluster instead of camping this one spot.
        public int GoodAttentionDays => site != null ? site.GoodAttentionDays : goodAttentionDays;

        // Set by HandleDeveloped() whenever this spot crosses a stage, read by WorldSite
        // right after a successful TryForceStanding() so the Site's single consolidated
        // announcement reuses this spot's own authored flavor text rather than duplicating
        // it as a separate hardcoded string on WorldSite.
        public string LastStandingFlavorText { get; private set; }

        // ITendednessSource — so WorldGenerator.GetSpot() keeps working uniformly for any
        // cross-influence consumer (NpcStageDef.passiveDriftSourceSpotId reads this for the
        // Bog Keeper's drainage stage). Synthetic but faithful to the real state: keyed off
        // overworkedToday (not just past the cost/duration threshold, per Iteration 43.1 —
        // degraded output of any kind, local or cross-system, only kicks in once truly
        // overworked) reports the old depleted band, Standing reports the old well-tended
        // band, otherwise the old resting baseline.
        public float Tendedness =>
            overworkedToday ? 0.2f :
            CurrentStageIndex >= 0 ? 0.85f :
            0.5f;

        public override string DisplayName => displayName;
        protected override DevelopmentTrack Track => track;

        // Procedural-spawn entry point (WorldGenerator), following the inactive-GO pattern —
        // call before SetActive(true) so Awake fires with the track already buildable.
        public void Initialize(string displayName, string interactionVerb, ItemYield[] commonYields,
            ItemYield[] rareYields, float rareDropChance, float minTickInterval, float maxTickInterval,
            SpotStagePool stagePool, KnowledgeYieldEntry[] knowledgeYields = null, HintFlavorEntry[] hintFlavors = null,
            WorldSite site = null)
        {
            this.displayName = displayName;
            this.interactionVerb = interactionVerb;
            this.commonYields = commonYields;
            this.rareYields = rareYields;
            this.rareDropChance = rareDropChance;
            this.minTickInterval = minTickInterval;
            this.maxTickInterval = maxTickInterval;
            this.stagePool = stagePool;
            this.knowledgeYields = knowledgeYields;
            this.hintFlavors = hintFlavors;
            this.site = site;
        }

        public float AttentionDuration => currentTickInterval;
        public bool RequiresDaylight => true;
        public bool ContinueAttending => true;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();

            var stages = new List<DevelopmentStage>();
            var defs = new List<SpotStageDef>();
            if (stagePool != null && stagePool.Stages != null)
            {
                foreach (var def in stagePool.Stages)
                {
                    if (def == null) continue;
                    stages.Add(new DevelopmentStage(def.StageId, def.DisplayName, def.ProgressCost, def.Conditions));
                    defs.Add(def);
                }
            }
            stageDefs = defs.ToArray();
            track = new DevelopmentTrack(stages.ToArray());

            OnDeveloped += HandleDeveloped;
            RollTickInterval();
        }

        private void Start()
        {
            if (DayCycleManager.Instance != null)
                DayCycleManager.Instance.DayAdvanced += OnDayAdvanced;
        }

        private void OnDestroy()
        {
            if (DayCycleManager.Instance != null)
                DayCycleManager.Instance.DayAdvanced -= OnDayAdvanced;
        }

        public bool CanAttend() => true;
        public string GetShortName() => displayName;

        public string GetOverlayDescription()
        {
            // Keyed off overworkedToday, not just past the cost/duration threshold — the
            // threshold tier is felt through cost/duration alone (Iteration 43.1); this line
            // is reserved for the tier where output has actually degraded.
            if (overworkedToday) return $"{displayName} — worked hard today";
            if (CurrentStageIndex >= 0) return $"{displayName} — feels like familiar ground";
            return displayName;
        }

        public string GetOverlayInteractionLine() => $"Hold E to {interactionVerb}";
        public IReadOnlyList<string> GetAppliedUpgrades() => GetAppliedUpgradeNames();

        public void OnAttentionComplete()
        {
            exhaustion = Mathf.Clamp01(exhaustion + exhaustionPerTick);
            attendedThisDay = true;
            if (exhaustion >= 1f) overworkedToday = true;

            // Yield penalty only stacks once fully overworked — the threshold crossing
            // itself is felt through doubled cost/duration (below), not through yield, so
            // hammering a spot reads as "this costs more" before it reads as "this gives
            // less". Reuses the exact qty/chance band shape ItemYieldRoller already applies
            // for ordinary tendedness.
            float? bandProxy = overworkedToday ? 0.2f
                : CurrentStageIndex >= 0 ? 0.75f
                : (float?)null;

            ItemYieldRoller.Roll(displayName, "foraged", commonYields, rareYields,
                GetEffectiveRareChance(), bandProxy, ItemYieldRoller.BuildKnowledgeInjectedYields(knowledgeYields));
            ItemYieldRoller.TryFireHintFlavor(displayName, hintFlavors);

            AddProgress();
            TryApplyStage();
            RaiseProgressMade();

            // Past the exhaustion threshold, this tick costs double daylight — self-spend
            // the amount beyond the base 1 AttentionManager already spent via
            // RequiresDaylight, same pattern as BuildingAttendable's cold-tax (Iteration 41).
            var request = BuildOutcomeRequest();
            int resolvedCost = Mathf.RoundToInt(1 * request.DaylightCostMultiplier);
            int extraCost = resolvedCost - 1;
            if (extraCost > 0 && DayCycleManager.Instance != null)
                DayCycleManager.Instance.SpendDaylight(extraCost);

            RollTickInterval();
        }

        public void OnAttentionCancelled() { }

        // Iteration 47: called by this spot's WorldSite once the shared GoodAttentionDays
        // counter satisfies every member's SustainedGoodAttentionCondition at the same
        // rest — reuses the exact AddProgress()/TryApplyStage() path a normal attend would
        // use, so it's idempotent (silently returns false pre-threshold or once already
        // applied) and needs no changes to DevelopableEntity, DevelopmentTrack, or the
        // stage-cross feedback wiring (tint, EntityFeedback pop, GetAppliedUpgrades()) —
        // only the trigger moves from "this spot's own next attend" to "the Site's shared
        // rest-boundary check," so every member crosses in the same pass instead of
        // staggering across separate future attends.
        public bool TryForceStanding()
        {
            AddProgress();
            return TryApplyStage();
        }

        // Standing's rare-chance multiplier stacks on top of the band proxy's own x1.2 (when
        // applicable) rather than replacing it — the point is that Familiar reads as clearly
        // bigger than anything ordinary attention could reach under the old tendedness model.
        private float GetEffectiveRareChance()
        {
            float chance = rareDropChance;
            var applied = CurrentStageIndex >= 0 && CurrentStageIndex < stageDefs.Length
                ? stageDefs[CurrentStageIndex] : null;
            if (applied != null) chance *= applied.RareChanceMultiplier;

            var request = new OutcomeRequest();
            new TwilightChanceModifier(1.5f).Apply(request);
            return chance * request.ChanceMultiplier;
        }

        // Shared by the daylight-cost self-spend and the duration roll below, so exhaustion's
        // cost/duration doubling is computed once per tick rather than twice — same
        // BuildOutcomeRequest() shape BuildingAttendable uses for its own cold tax.
        private OutcomeRequest BuildOutcomeRequest()
        {
            var request = new OutcomeRequest();
            new ExhaustionCostModifier(exhaustion, exhaustionPenaltyThreshold).Apply(request);
            return request;
        }

        private void RollTickInterval()
        {
            currentTickInterval = Random.Range(minTickInterval, maxTickInterval) * BuildOutcomeRequest().DurationMultiplier;
        }

        // Counter only advances on a rest that was both attended and not overworked —
        // overworking a day doesn't reset progress toward Standing, it just fails to add to
        // it, per the design's explicit "costs you, doesn't undo you" split.
        private void OnDayAdvanced()
        {
            if (attendedThisDay && !overworkedToday)
            {
                if (site != null) site.RegisterGoodDay(DayCycleManager.Instance.DayIndex);
                else goodAttentionDays++;
            }

            exhaustion = 0f;
            overworkedToday = false;
            attendedThisDay = false;

            // currentTickInterval was last rolled at the end of the previous attend, which
            // may have baked in the exhaustion-doubled duration — without re-rolling here,
            // that stale doubled value would sit as this spot's AttentionDuration until the
            // next attend completes, so the first hold of a new day would still take twice
            // as long even though exhaustion just reset to 0.
            RollTickInterval();
        }

        private void HandleDeveloped(DevelopmentStage stage)
        {
            var def = CurrentStageIndex >= 0 && CurrentStageIndex < stageDefs.Length
                ? stageDefs[CurrentStageIndex] : null;

            // Set before EntityFeedback's own OnDeveloped handler runs (this subscribes in
            // Awake, EntityFeedback in Start, so this fires first) — its stage-cross shape
            // swap bakes the new circle sprite's color from SpriteRenderer.color, which
            // TriangleSpriteGenerator/CircleSpriteGenerator otherwise never touch (color is
            // baked into the sprite texture instead), so without this the swap would read
            // Unity's untouched default white. Same fix BuildingAttendable.UpdateVisual()
            // already applies via BuildingStageDef.tint.
            if (def != null && spriteRenderer != null)
                spriteRenderer.color = def.Tint;

            string flavor = def != null && !string.IsNullOrEmpty(def.FlavorText)
                ? def.FlavorText
                : $"{stage.DisplayName}.";
            LastStandingFlavorText = flavor;

            // Site-scoped spots: the Site posts one consolidated "place" announcement
            // instead of every member independently posting its own near-identical line
            // (Iteration 47 fix — previously each member crossed and announced on its own
            // next attend, reading as N separate moments instead of one shared one).
            if (site == null)
            {
                NotificationManager.Post($"{displayName}: {flavor}");
                Debug.Log($"{displayName}: {flavor}", this);
            }

            // Iteration 45: same flag-bridge Buildings already use (Iteration 34 Seam 3) —
            // lets a spot's Standing crossing gate something elsewhere (e.g. a POI's
            // unlockCondition) without a direct reference to this spot.
            if (def != null && !string.IsNullOrEmpty(def.WorldStateFlag))
                WorldState.SetFlag(def.WorldStateFlag, true);
        }
    }
}
