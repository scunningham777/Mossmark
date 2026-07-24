using System.Collections.Generic;
using Mossmark.Attention;
using Mossmark.Day;
using Mossmark.Prototype3;
using Mossmark.Visuals;
using UnityEngine;

namespace Mossmark.Prototype4
{
    // Iteration 4.17 (War Scars: Recovery Lean, single-site pilot): tests whether a
    // site's *resolved, branching* fate can be shaped by manner of attention alone,
    // with no player-facing choice ever presented (Tried, Not Chosen). Plain
    // MonoBehaviour, no DevelopableEntity — closer to TendableSpotAttendable's shape
    // (a pool checked against TakenLedger, no stage-cross machinery) than to a
    // building's multi-stage repair track, since there's exactly one fork here, not
    // a ladder.
    //
    // The fork reads two things the game already tracks the shape of, not a new
    // input: whether the SAME hold continues past a tick that had nothing left to
    // give. Every day allows exactly one roll against the pool. A hold that stops
    // right after a hit (grab-and-go) never sees a "nothing left" tick that day, so
    // it only ever nudges toward salvage. A hold that keeps going anyway — past the
    // day's one roll, hit or miss, or once the pool is fully claimed — nudges toward
    // repair every further tick. ContinueAttending is unconditionally true (same
    // reasoning as TendableSpotAttendable: no outcome here needs the hold to break)
    // specifically so this distinction falls naturally out of whether the player
    // releases E or keeps holding it, not out of any new mechanic.
    //
    // Deliberately NOT legible in advance: recoveryLean drives no tint, no flavor
    // banding, nothing at all before the fork resolves — description and interaction
    // line stay fixed through the entire Ruined phase regardless of the float's
    // current value. Only the moment of resolution changes anything visible. This is
    // the opposite of 4.15's `character`, which is continuously felt by design; here
    // legibility is only ever retrospective, per the success criterion this iteration
    // is testing.
    public class WreckAttendable : MonoBehaviour, IAttendable
    {
        private enum WreckState { Ruined, Salvaged, Repaired }

        [SerializeField] private string displayName = "The Wrecked Cart";

        [SerializeField, TextArea] private string ruinedDescription =
            "A cart lies overturned where the road gives out, wheel stove in, goods spilled and " +
            "half-sunk in the verge. Nobody's come back for any of it — not yet, maybe not ever.";
        [SerializeField] private string ruinedInteractionLine = "Hold E to go through what's left";

        [SerializeField, TextArea] private string salvagedDescription =
            "The cart's long since been picked clean. Someone squared away what was left and let " +
            "the rest go — not a ruin anymore, just a quiet marker of what happened here.";
        [SerializeField] private string salvagedInteractionLine = "Hold E to pay it a moment's attention";

        [SerializeField, TextArea] private string repairedDescription =
            "The cart's been set back on its wheel, the split boards braced with whatever came to " +
            "hand. It won't carry a load again, but it isn't wreckage anymore either.";
        [SerializeField] private string repairedInteractionLine = "Hold E to look it over";

        [SerializeField, Range(0f, 1f)] private float yieldChance = 0.5f;
        [SerializeField, Min(0.1f)] private float attendDuration = 1f;

        // Starts neutral; never shown to the player as a number. Salvage and repair
        // are deliberately asymmetric distances from the start, not opposite ends of
        // one meter re-centered at 0.5 by convention — see the two thresholds below.
        private float recoveryLean = 0.5f;
        [SerializeField, Min(0f)] private float leanNudgeOnClaim = 0.2f;
        [SerializeField, Min(0f)] private float leanNudgeOnLinger = 0.09f;
        [SerializeField, Range(0f, 1f)] private float salvageThreshold = 0.15f;
        [SerializeField, Range(0f, 1f)] private float repairThreshold = 0.85f;

        [SerializeField] private Color ruinedTint = new Color(0.4f, 0.36f, 0.34f);
        [SerializeField] private Color salvagedTint = new Color(0.5f, 0.5f, 0.52f);
        [SerializeField] private Color repairedTint = new Color(0.5f, 0.58f, 0.42f);

        [SerializeField] private string[] missFlavorLines =
        {
            "Mostly splinters and rust. Nothing worth carrying today.",
            "You turn over what's left. It stays where it is.",
        };
        [SerializeField] private string[] lingerFlavorLines =
        {
            "There's nothing more to take today. You stay with it anyway.",
            "Whatever's left keeps for tomorrow. You linger a while longer.",
        };
        [SerializeField] private string[] salvagedFlavorLines =
        {
            "There's nothing left to take. The wreck settles further into the verge.",
            "Whatever this place was, it's finished being that now.",
        };
        [SerializeField] private string[] repairedFlavorLines =
        {
            "It stands a little straighter than it did. Someone's kept at it.",
            "Not whole. But it isn't given up on.",
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

        private WreckState state = WreckState.Ruined;

        // -1 = no roll spent yet. The day's single roll is spent the first time this
        // is attended that day, hit or miss — every further attend the same day (and
        // every attend once the pool is fully claimed, any day) is a "nothing left to
        // give" tick regardless of RNG.
        private int lastRollDayIndex = -1;

        private static int TodayIndex => DayCycleManager.Instance != null ? DayCycleManager.Instance.DayIndex : 0;

        public float AttentionDuration => attendDuration;
        public bool RequiresDaylight => true;

        // Unconditionally true, same reasoning as TendableSpotAttendable: nothing
        // here needs the hold to break for the player to read it. This is also what
        // makes "ends there" vs. "continues past" a property of whether the player
        // releases E or keeps holding it, per the class comment above.
        public bool ContinueAttending => true;

        public bool CanAttend() => true;

        public string GetShortName() => displayName;

        public string GetOverlayDescription() => state switch
        {
            WreckState.Salvaged => salvagedDescription,
            WreckState.Repaired => repairedDescription,
            _ => ruinedDescription,
        };

        public string GetOverlayInteractionLine() => state switch
        {
            WreckState.Salvaged => salvagedInteractionLine,
            WreckState.Repaired => repairedInteractionLine,
            _ => ruinedInteractionLine,
        };

        public IReadOnlyList<string> GetAppliedUpgrades() => System.Array.Empty<string>();

        private void Start() => RefreshTint();

        public void OnAttentionComplete()
        {
            if (state != WreckState.Ruined)
            {
                NotificationManager.Post(RandomLine(
                    state == WreckState.Salvaged ? salvagedFlavorLines : repairedFlavorLines));
                return;
            }

            int today = TodayIndex;
            bool rollAvailable = today != lastRollDayIndex;
            var candidates = GetUnclaimedEntries();

            if (rollAvailable && candidates.Count > 0)
            {
                lastRollDayIndex = today;

                if (Random.value < yieldChance)
                {
                    var entry = candidates[Random.Range(0, candidates.Count)];
                    TakenLedger.Register(entry.itemId, entry.displayName, entry.propertyIds);
                    NotificationManager.Post(entry.takeLine);
                    GetComponent<EntityFeedback>()?.TriggerPop();
                    NudgeLean(-leanNudgeOnClaim);
                    Debug.Log($"{displayName}: yielded '{entry.displayName}' (recoveryLean={recoveryLean:0.00}).", this);
                }
                else
                {
                    NotificationManager.Post(RandomLine(missFlavorLines));
                    Debug.Log($"{displayName}: miss (recoveryLean={recoveryLean:0.00}).", this);
                }
            }
            else
            {
                NotificationManager.Post(RandomLine(lingerFlavorLines));
                NudgeLean(leanNudgeOnLinger);
                Debug.Log($"{displayName}: lingered with nothing to give (recoveryLean={recoveryLean:0.00}).", this);
            }

            CheckResolution();
        }

        public void OnAttentionCancelled() { }

        private void NudgeLean(float delta) => recoveryLean = Mathf.Clamp01(recoveryLean + delta);

        private void CheckResolution()
        {
            if (recoveryLean <= salvageThreshold) Resolve(WreckState.Salvaged);
            else if (recoveryLean >= repairThreshold) Resolve(WreckState.Repaired);
        }

        private void Resolve(WreckState newState)
        {
            state = newState;
            RefreshTint();
            GetComponent<EntityFeedback>()?.TriggerPop();
            Debug.Log($"{displayName}: resolved - {newState} (recoveryLean={recoveryLean:0.00}).", this);
        }

        private void RefreshTint()
        {
            var spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null) return;
            spriteRenderer.color = state switch
            {
                WreckState.Salvaged => salvagedTint,
                WreckState.Repaired => repairedTint,
                _ => ruinedTint,
            };
        }

        private static string RandomLine(string[] lines) =>
            lines.Length > 0 ? lines[Random.Range(0, lines.Length)] : string.Empty;

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

        // Editor test driver (Prototype4Debug): surfaces internal fork state for
        // verification without inferring it from console-log line text alone.
        public string DebugWreckState() =>
            $"state={state}, recoveryLean={recoveryLean:0.00}, lastRollDayIndex={lastRollDayIndex}, " +
            $"today={TodayIndex}, unclaimed={GetUnclaimedEntries().Count}";

        // Editor test driver: forces a lean nudge without waiting on the daily-roll
        // cap or RNG, then re-checks resolution — same shortcut role as
        // DebugAdvanceAcquaintance plays for AcquaintableAttendable's ladder.
        public void DebugNudgeLean(float delta)
        {
            NudgeLean(delta);
            Debug.Log($"{displayName}: debug-nudged recoveryLean to {recoveryLean:0.00}.", this);
            CheckResolution();
        }
    }
}
