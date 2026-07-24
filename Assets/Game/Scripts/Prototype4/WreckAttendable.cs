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
    // Revised 7-24-26 (Sean's playtest): the original build nudged recoveryLean
    // immediately, per attend — a claim nudged down, and any further same-day attend
    // nudged up. That conflated two different things: "checking whether there's
    // anything left" and "deliberately settling in without expecting reward" are not
    // the same intent, but they produced the identical tick (there was no way to ask
    // without it counting as an answer), so a plain claim-then-recheck day could net
    // almost nothing, or even tip the wrong way. It also let the fork resolve
    // mid-attend, which read as a sudden surprise rather than a returning-to-find-out
    // moment.
    //
    // The fix: recoveryLean now resolves once per day, at rest, off that day's total
    // attend count alone (HandleDayAdvanced) — hit/miss no longer matters to the
    // lean at all, only how many times the player showed up. Few attends reads as
    // grab-and-go (nudges toward salvage); many attends reads as staying with the
    // place (nudges toward repair), on a continuous formula centered on
    // neutralAttendsPerDay rather than a hardcoded table, per the Organic value. This
    // can never be misread mid-visit, because nothing is decided until the day ends.
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

        // The day-count → nudge formula (revised 7-24-26): fewer attends than
        // neutralAttendsPerDay nudges down (salvage), more nudges up (repair), scaled
        // continuously by leanPerAttendStep rather than a hardcoded per-count table.
        // leanJitter adds a small amount of noise per day so the same attend count
        // doesn't produce a bit-identical nudge every time (Organic value: use
        // min/max ranges wherever the variance is felt).
        [SerializeField, Min(0f)] private float neutralAttendsPerDay = 3f;
        [SerializeField, Min(0f)] private float leanPerAttendStep = 0.12f;
        [SerializeField, Min(0f)] private float leanJitter = 0.02f;
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

        // Total attends today, regardless of hit/miss — the day's single pool-roll is
        // spent the first attend (attendsToday == 0, checked before incrementing);
        // every further attend the same day is a "nothing left to give" tick for the
        // pool. Also the sole input to HandleDayAdvanced's lean nudge. Reset to 0
        // there, not compared against a day index — the counter itself IS the
        // per-day state.
        private int attendsToday;

        public float AttentionDuration => attendDuration;
        public bool RequiresDaylight => true;

        // Unconditionally true, same reasoning as TendableSpotAttendable: nothing
        // here needs the hold to break for the player to read it, and — since the
        // 7-24-26 revision — the lean no longer cares about within-hold timing at
        // all, only the day's total attend count, so there's no reason to interrupt.
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

        private void Start()
        {
            RefreshTint();
            if (DayCycleManager.Instance != null) DayCycleManager.Instance.DayAdvanced += HandleDayAdvanced;
        }

        private void OnDestroy()
        {
            if (DayCycleManager.Instance != null) DayCycleManager.Instance.DayAdvanced -= HandleDayAdvanced;
        }

        public void OnAttentionComplete()
        {
            if (state != WreckState.Ruined)
            {
                NotificationManager.Post(RandomLine(
                    state == WreckState.Salvaged ? salvagedFlavorLines : repairedFlavorLines));
                return;
            }

            bool rollAvailable = attendsToday == 0;
            var candidates = GetUnclaimedEntries();
            attendsToday++;

            if (rollAvailable && candidates.Count > 0)
            {
                if (Random.value < yieldChance)
                {
                    var entry = candidates[Random.Range(0, candidates.Count)];
                    TakenLedger.Register(entry.itemId, entry.displayName, entry.propertyIds);
                    NotificationManager.Post(entry.takeLine);
                    GetComponent<EntityFeedback>()?.TriggerPop();
                    Debug.Log($"{displayName}: yielded '{entry.displayName}' (attendsToday={attendsToday}).", this);
                }
                else
                {
                    NotificationManager.Post(RandomLine(missFlavorLines));
                    Debug.Log($"{displayName}: miss (attendsToday={attendsToday}).", this);
                }
            }
            else
            {
                NotificationManager.Post(RandomLine(lingerFlavorLines));
                Debug.Log($"{displayName}: nothing left to give today (attendsToday={attendsToday}).", this);
            }
        }

        public void OnAttentionCancelled() { }

        // The day's whole attend count, read once, is the entire input — no message,
        // no tell, matching the "not legible in advance" rule at the top of this
        // file. Only a returning player (or a Log Wreck State check) ever sees this
        // move.
        private void HandleDayAdvanced()
        {
            if (state == WreckState.Ruined && attendsToday > 0)
            {
                float dayNudge = (attendsToday - neutralAttendsPerDay) * leanPerAttendStep
                    + Random.Range(-leanJitter, leanJitter);
                NudgeLean(dayNudge);
                Debug.Log($"{displayName}: day closed with {attendsToday} attend(s), " +
                    $"recoveryLean nudged {dayNudge:0.00} to {recoveryLean:0.00}.", this);
                CheckResolution();
            }

            attendsToday = 0;
        }

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
            $"state={state}, recoveryLean={recoveryLean:0.00}, attendsToday={attendsToday}, " +
            $"unclaimed={GetUnclaimedEntries().Count}";

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
