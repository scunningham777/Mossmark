using System.Collections.Generic;
using Mossmark.Attention;
using Mossmark.Day;
using Mossmark.Prototype3;
using Mossmark.Visuals;
using UnityEngine;

namespace Mossmark.Prototype4
{
    // Iteration 4.12 (The Tending Thread): Sean's P2-shaped preference — attend a
    // spot repeatedly until it yields, rather than an item just lying there for a
    // single take. Styled after the pre-Iteration-44 generic wilderness spot: no
    // development stage (there's nothing to cross), attention always plays a flavor
    // line, and each attend rolls a chance to yield one of the site's still-untaken
    // items. TakenLedger is the only bookkeeping an entry needs — the moment it's
    // registered there it drops out of this spot's pool, so no separate claimed-state
    // is tracked here. Downstream of a yield, everything is untouched: the item is
    // registered exactly as PropertyPickupAttendable does it today, still unrevealed,
    // still worked out later at a station like the Smoking Racks. This component only
    // changes acquisition, never what a taken item does afterward.
    //
    // Iteration 4.13 (Ripeness): the flat yieldChance became a base — the effective
    // chance rolled each attend rises with days since the spot was last visited
    // (DayCycleManager.DayIndex, exactly 4.8's wary-gate granularity, aimed at
    // ripeness instead of trust) and falls with attends already spent here today.
    // Sitting and tending repeatedly in one visit burns the bonus down toward the
    // bare floor; returning after time away finds the spot at its fullest. The
    // ramp is capped (maxDaysAwayBonus) so a long absence makes a hit more likely,
    // never guaranteed — outcomes stay probabilistic per the Organic value. Nothing
    // about this is shown as a number anywhere the player can see; it's felt only
    // through how often tending pays off.
    //
    // Iteration 4.15 (Site Character, single-site pilot): a second, independent axis
    // alongside ripeness — persistent instead of resetting, never resolving, and never
    // feeding yield chance. See the `character` field below for the mechanism and
    // PROTOTYPE4_ACQUAINTANCE.md's Iteration 4.15 section for why it has to stay a
    // separate outlet (tint + flavor pool) rather than a further modifier on RNG.
    public class TendableSpotAttendable : MonoBehaviour, IAttendable
    {
        [SerializeField] private string displayName = "The Landing";
        [SerializeField, TextArea] private string description =
            "Whatever the work here leaves loose — driftwood, trimmings, the odd catch set aside — turns up along the bank if you take the time to look.";

        // Sean's steer (7-21-26): 0.3, so a flavor-only miss is the more common
        // outcome even at the spot's fullest. The floor this ramp rises from and
        // falls back toward.
        [SerializeField, Range(0f, 1f)] private float yieldChance = 0.3f;

        // Per day since the spot was last attended, capped at maxDaysAwayBonus days
        // so the bonus plateaus rather than eventually guaranteeing a hit. A fresh
        // spot (never yet visited) gets no bonus on its first attend — the ramp only
        // rewards an actual return.
        [SerializeField, Min(0f)] private float ripenBonusPerDayAway = 0.15f;
        [SerializeField, Min(0)] private int maxDaysAwayBonus = 3;

        // Per attend already spent here today — the "burns through fast" half of the
        // ramp. The first attend of a visit pays the full ripeness bonus; each further
        // attend the same day chips it back down.
        [SerializeField, Min(0f)] private float depletionPerAttendToday = 0.08f;

        [SerializeField, Min(0.1f)] private float attendDuration = 1f;

        // Iteration 4.15 (Site Character, single-site pilot): a persistent, never-
        // resolved record of how this site has been treated — patient vs. greedy —
        // independent of yield RNG. Unlike ripeness (resets to a base value it decays
        // back toward) this never resets and never resolves; it just drifts slowly.
        // Deliberately not fed into yieldChance/ripeness: a probability nudge riding
        // on top of ripeness's own probability nudge isn't a legible second axis, it's
        // noise on the one that already exists (see PROTOTYPE4_ACQUAINTANCE.md's
        // Iteration 4.15 revision note). Starts neutral; nudged by the same
        // daysAway/attendsToday state 4.13 already computes, so no new tracking is
        // needed — only a new read of state that already exists.
        private float character = 0.5f;

        [SerializeField, Min(0f)] private float characterNudgeUp = 0.03f;
        [SerializeField, Min(0f)] private float characterNudgeDown = 0.06f;

        private const float CharacterLowBand = 0.3f;
        private const float CharacterHighBand = 0.7f;

        // Continuous lerp, not banded — the tint is meant to read as a slow, ongoing
        // drift (Organic value: state that changes gradually should change
        // continuously), unlike the flavor-line pool below, which necessarily has to
        // pick one of a fixed set of lines and so is banded instead.
        [SerializeField] private Color wornTint = new Color(0.55f, 0.55f, 0.6f);
        [SerializeField] private Color thrivingTint = new Color(1f, 0.97f, 0.85f);

        // Fires on every miss, hit or exhausted-pool alike — tending should never feel
        // like it "failed"; a miss is just the ordinary texture of the place, not an
        // empty roll. Which pool fires is biased by character's current band, so the
        // same miss reads differently depending on how the site's been treated — and,
        // since this keeps firing after the item pool is fully claimed, it's also what
        // keeps an exhausted spot from ever going quiet.
        [SerializeField] private string[] wornFlavorLines =
        {
            "The place feels picked over, hurried through.",
            "Whatever's here goes fast when you don't let it sit.",
        };
        [SerializeField] private string[] midFlavorLines =
        {
            "You turn over driftwood and old trimmings. Nothing today.",
            "The bank gives up mud and old rope-ends. You keep looking.",
        };
        [SerializeField] private string[] thrivingFlavorLines =
        {
            "The place feels well kept, generous with what it has.",
            "Whatever you're looking for, it feels closer for the patience.",
        };

        [SerializeField] private YieldEntry[] pool;

        [System.Serializable]
        public class YieldEntry
        {
            public string itemId;
            public string displayName;
            public string[] propertyIds;
            public string takeLine;
        }

        // -1 = never yet attended. Both are read-then-updated each attend, in that
        // order, so daysAway is computed against the *previous* visit, not the one
        // currently being spent.
        private int lastVisitDayIndex = -1;
        private int attendsToday;

        private static int TodayIndex => DayCycleManager.Instance != null ? DayCycleManager.Instance.DayIndex : 0;

        public float AttentionDuration => attendDuration;

        // Same as every other attendable in this scene: attention is the day's clock.
        public bool RequiresDaylight => true;

        // Unlike a one-shot pickup or an acquaintance stage-cross, there's no moment
        // here that needs the hold to break so the player can read it — a yield and a
        // miss are the same size of event (a posted line), so holding E should just
        // keep tending, tick after tick, regardless of which one just happened.
        public bool ContinueAttending => true;

        public bool CanAttend() => true;

        public string GetShortName() => displayName;
        public string GetOverlayDescription() => description;
        public string GetOverlayInteractionLine() => "Hold E to have a look";

        public IReadOnlyList<string> GetAppliedUpgrades() => System.Array.Empty<string>();

        private void Start() => RefreshTint();

        public void OnAttentionComplete()
        {
            int today = TodayIndex;
            int daysAway = lastVisitDayIndex < 0 ? 0 : Mathf.Clamp(today - lastVisitDayIndex, 0, maxDaysAwayBonus);
            if (today != lastVisitDayIndex) attendsToday = 0;

            float effectiveChance = Mathf.Clamp01(yieldChance
                + ripenBonusPerDayAway * daysAway
                - depletionPerAttendToday * attendsToday);

            // Character nudge reads the same daysAway/attendsToday state above, before
            // this attend's own increment — daysAway > 0 means this attend is arriving
            // after a real absence (ripe); attendsToday > 0 means the spot's already
            // been worked at least once today (depleted). The two are mutually
            // exclusive by construction: daysAway > 0 only happens on a new day, which
            // is exactly when attendsToday was just reset to 0 above. A spot's very
            // first-ever attend is neither (nothing to have been patient or greedy
            // about yet), so it nudges nothing.
            if (daysAway > 0) character = Mathf.Clamp01(character + characterNudgeUp);
            else if (attendsToday > 0) character = Mathf.Clamp01(character - characterNudgeDown);

            attendsToday++;
            lastVisitDayIndex = today;
            RefreshTint();

            var candidates = GetUnclaimedEntries();
            if (candidates.Count == 0 || Random.value >= effectiveChance)
            {
                var flavorPool = CurrentFlavorPool();
                if (flavorPool.Length > 0)
                {
                    NotificationManager.Post(flavorPool[Random.Range(0, flavorPool.Length)]);
                }
                Debug.Log($"{displayName}: miss (daysAway={daysAway}, attendsToday={attendsToday}, chance={effectiveChance:0.00}, character={character:0.00}).", this);
                return;
            }

            var entry = candidates[Random.Range(0, candidates.Count)];
            TakenLedger.Register(entry.itemId, entry.displayName, entry.propertyIds);
            NotificationManager.Post(entry.takeLine);
            GetComponent<EntityFeedback>()?.TriggerPop();
            Debug.Log($"{displayName}: yielded '{entry.displayName}' (daysAway={daysAway}, attendsToday={attendsToday}, chance={effectiveChance:0.00}, character={character:0.00}).", this);
        }

        public void OnAttentionCancelled() { }

        private void RefreshTint()
        {
            var spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null) spriteRenderer.color = Color.Lerp(wornTint, thrivingTint, character);
        }

        private string[] CurrentFlavorPool()
        {
            if (character < CharacterLowBand) return wornFlavorLines;
            if (character > CharacterHighBand) return thrivingFlavorLines;
            return midFlavorLines;
        }

        // Editor test driver (Prototype4Debug): surfaces the internal ripeness state
        // for verification without relying on parsing per-attend console lines.
        public string DebugRipenessState()
        {
            int daysAway = lastVisitDayIndex < 0 ? 0 : Mathf.Clamp(TodayIndex - lastVisitDayIndex, 0, maxDaysAwayBonus);
            int attendsTodaySoFar = TodayIndex != lastVisitDayIndex ? 0 : attendsToday;
            float nextChance = Mathf.Clamp01(yieldChance
                + ripenBonusPerDayAway * daysAway
                - depletionPerAttendToday * attendsTodaySoFar);
            return $"lastVisitDayIndex={lastVisitDayIndex}, today={TodayIndex}, daysAway={daysAway}, " +
                $"attendsToday={attendsTodaySoFar}, nextEffectiveChance={nextChance:0.00}, unclaimed={GetUnclaimedEntries().Count}";
        }

        // Iteration 4.15: surfaces character's raw value, its current band, and the
        // sprite tint that band currently drives — so the pilot's legibility can be
        // checked directly rather than inferred from console-log line text alone.
        public string DebugCharacterState()
        {
            string band = character < CharacterLowBand ? "worn" : character > CharacterHighBand ? "thriving" : "mid";
            var spriteRenderer = GetComponent<SpriteRenderer>();
            return $"character={character:0.00} ({band}), tint={(spriteRenderer != null ? spriteRenderer.color.ToString() : "n/a")}";
        }

        private List<YieldEntry> GetUnclaimedEntries()
        {
            var result = new List<YieldEntry>();
            foreach (var entry in pool)
            {
                if (!IsTaken(entry.itemId)) result.Add(entry);
            }
            return result;
        }

        private static bool IsTaken(string itemId)
        {
            foreach (var taken in TakenLedger.All)
            {
                if (taken.ItemId == itemId) return true;
            }
            return false;
        }
    }
}
