using System.Collections.Generic;
using Mossmark.Attention;
using Mossmark.Development;
using Mossmark.Inventory;
using Mossmark.Visuals;
using UnityEngine;

namespace Mossmark.Prototype3
{
    // Iteration 3.7: the working surface. Non-modal and attend-only, deliberately —
    // see PROTOTYPE3_KNOWLEDGE_SPINE.md's "Discovery Thread" preamble for why a
    // slot-picking UI isn't built here (the shared modal-input guards live hardcoded in
    // PlayerController/AttentionManager, off-limits under the Reuse Discipline's hard
    // rule). Attending works over the full TakenLedger directly, filtered by an authored
    // bias — the Iteration 39 biasPropertyIds pattern reused as plain data rather than a
    // station component. One deterministic reveal per completed hold, first-taken/
    // first-in-property-order; when nothing on-bias remains unknown, attending falls
    // back to a flavor linger, same shape as a fully-taught NpcAttendable visit.
    public class WorkingSurfaceAttendable : MonoBehaviour, IAttendable
    {
        [SerializeField] private string displayName = "Scouring Bench";
        [SerializeField] private string description = "A flat stone by the water, worn smooth by scrubbing.";
        [SerializeField] private string playerKnowerId = "p3_player";
        [SerializeField] private string[] biasPropertyIds = { "binds_fast", "keeps_well" };
        [SerializeField, Min(0.1f)] private float attendDuration = 1.5f;
        [SerializeField] private string workVerb = "work at the bench";
        [SerializeField] private string[] fallbackFlavorLines =
        {
            "Nothing more comes clear here, not today.",
            "You turn what you have over in your hands, but the bench has taught you what it can.",
        };

        public float AttentionDuration => attendDuration;

        // Iteration 3.5's rule extended: a discovery tick is a real daylight cost, same
        // as everything else in the scene — that's what makes it a competing use.
        public bool RequiresDaylight => true;
        public bool ContinueAttending => false;

        public bool CanAttend() => true;

        public string GetShortName() => displayName;
        public string GetOverlayDescription() => description;

        public string GetOverlayInteractionLine() =>
            FindCandidate() != null ? $"Hold E to {workVerb}" : "Hold E to sit a while";

        public IReadOnlyList<string> GetAppliedUpgrades() => System.Array.Empty<string>();

        public void OnAttentionComplete()
        {
            var candidate = FindCandidate();
            if (candidate == null)
            {
                if (fallbackFlavorLines.Length > 0)
                {
                    Debug.Log($"{displayName}: {fallbackFlavorLines[Random.Range(0, fallbackFlavorLines.Length)]}", this);
                }
                return;
            }

            PropertyKnowledge.MarkKnown(candidate.Value.Entry.ItemId, candidate.Value.PropertyId);
            PropertyKnowledge.MarkKnown(playerKnowerId, candidate.Value.PropertyId);

            var property = PropertyRegistry.GetById(candidate.Value.PropertyId);
            var phrase = property != null ? property.Phrase : candidate.Value.PropertyId;
            NotificationManager.Post($"Working the {candidate.Value.Entry.DisplayName}, it comes clear: it {phrase}.");
            Debug.Log($"{displayName}: revealed '{candidate.Value.PropertyId}' on {candidate.Value.Entry.DisplayName}.", this);

            GetComponent<EntityFeedback>()?.TriggerPop();
        }

        public void OnAttentionCancelled() { }

        private RevealCandidate? FindCandidate()
        {
            foreach (var entry in TakenLedger.All)
            {
                foreach (var propertyId in entry.PropertyIds)
                {
                    if (System.Array.IndexOf(biasPropertyIds, propertyId) < 0) continue;
                    if (WorldContext.IsPropertyKnown(entry.ItemId, propertyId)) continue;

                    return new RevealCandidate(entry, propertyId);
                }
            }

            return null;
        }

        private readonly struct RevealCandidate
        {
            public readonly TakenLedger.Entry Entry;
            public readonly string PropertyId;

            public RevealCandidate(TakenLedger.Entry entry, string propertyId)
            {
                Entry = entry;
                PropertyId = propertyId;
            }
        }
    }
}
