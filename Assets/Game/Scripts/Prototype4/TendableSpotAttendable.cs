using System.Collections.Generic;
using Mossmark.Attention;
using Mossmark.Prototype3;
using Mossmark.Visuals;
using UnityEngine;

namespace Mossmark.Prototype4
{
    // Iteration 4.12 (The Tending Thread): Sean's P2-shaped preference — attend a
    // spot repeatedly until it yields, rather than an item just lying there for a
    // single take. Styled after the pre-Iteration-44 generic wilderness spot: no
    // development stage (there's nothing to cross), attention always plays a flavor
    // line, and each attend rolls a flat chance to yield one of the site's still-
    // untaken items. TakenLedger is the only bookkeeping an entry needs — the moment
    // it's registered there it drops out of this spot's pool, so no separate
    // claimed-state is tracked here. Downstream of a yield, everything is untouched:
    // the item is registered exactly as PropertyPickupAttendable does it today, still
    // unrevealed, still worked out later at a station like the Smoking Racks. This
    // component only changes acquisition, never what a taken item does afterward.
    public class TendableSpotAttendable : MonoBehaviour, IAttendable
    {
        [SerializeField] private string displayName = "The Landing";
        [SerializeField, TextArea] private string description =
            "Whatever the work here leaves loose — driftwood, trimmings, the odd catch set aside — turns up along the bank if you take the time to look.";

        // Sean's steer (7-21-26): 0.3, so a flavor-only miss is the more common
        // outcome. Ripeness (Iteration 4.13) will make this time-aware; flat for now.
        [SerializeField, Range(0f, 1f)] private float yieldChance = 0.3f;

        [SerializeField, Min(0.1f)] private float attendDuration = 1f;

        // Fires on every miss — tending should never feel like it "failed"; a miss
        // is just the ordinary texture of the place, not an empty roll.
        [SerializeField] private string[] flavorLines =
        {
            "You turn over driftwood and old trimmings. Nothing today.",
            "The bank gives up mud and old rope-ends. You keep looking.",
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

        public void OnAttentionComplete()
        {
            var candidates = GetUnclaimedEntries();
            if (candidates.Count == 0 || Random.value >= yieldChance)
            {
                if (flavorLines.Length > 0)
                {
                    NotificationManager.Post(flavorLines[Random.Range(0, flavorLines.Length)]);
                }
                return;
            }

            var entry = candidates[Random.Range(0, candidates.Count)];
            TakenLedger.Register(entry.itemId, entry.displayName, entry.propertyIds);
            NotificationManager.Post(entry.takeLine);
            GetComponent<EntityFeedback>()?.TriggerPop();
            Debug.Log($"{displayName}: yielded '{entry.displayName}'.", this);
        }

        public void OnAttentionCancelled() { }

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
