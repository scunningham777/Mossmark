using System.Collections.Generic;
using Mossmark.Attention;
using Mossmark.Development;
using Mossmark.Inventory;
using Mossmark.Prototype3;
using UnityEngine;

namespace Mossmark.Prototype4
{
    // Iteration 4.9: the earned workshop — P3's 3.7 working-surface shape (non-modal,
    // works over the TakenLedger, bias-filtered, one deterministic reveal per held
    // tick), rebuilt as a P4 component because access is the whole point here: the
    // surface exists from cold load but CanAttend() reads a WorldState flag set by
    // another entity's acquaintance crossing. Nothing announces the change — you find
    // out the racks will have you by coming back to them.
    //
    // A fresh class rather than a subclass of WorkingSurfaceAttendable: nothing in that
    // P3 script is virtual, and making it so to serve this scene would be exactly the
    // shared-behavior edit the Reuse Discipline forbids. TakenLedger/PropertyKnowledge
    // are consumed as-is.
    public class EarnedSurfaceAttendable : MonoBehaviour, IAttendable
    {
        [SerializeField] private string displayName = "The Smoking Racks";
        [SerializeField] private string playerKnowerId = "p3_player";

        // The gate: earned elsewhere, read here. Empty = open from the start (which
        // would make this class pointless — author it).
        [SerializeField] private string requiredWorldStateFlag = "p4_smokehouse_known";
        [SerializeField, TextArea] private string lockedDescription =
            "Racks under the eaves, hung shoulder-high, worn smooth with use. Another's work, and not yours to handle.";
        [SerializeField] private string lockedInteractionLine = "You keep your hands off another's racks";
        [SerializeField, TextArea] private string openDescription =
            "The racks stand open to you now. What you've taken and carried could be worked out here, in the smoke's slow language.";
        [SerializeField] private string workVerb = "work at the racks";

        [SerializeField] private string[] biasPropertyIds = { "burns_slow", "keeps_well" };

        // The Iteration 54 additive-seam pattern, keyed on another acquaintance
        // crossing: while the bonus flag is set, these properties join the bias —
        // knowing one thing extends what another's work can teach you. Additive only,
        // never a replacement.
        [SerializeField] private string bonusBiasWorldStateFlag = "p4_osier_bed_known";
        [SerializeField] private string[] bonusBiasPropertyIds = { "binds_fast" };

        [SerializeField, Min(0.1f)] private float attendDuration = 1.5f;
        [SerializeField] private string[] fallbackFlavorLines =
        {
            "The smoke has told you what it can, for now.",
            "You turn what you've taken over in your mind; nothing more comes clear at the racks.",
        };

        private bool IsOpen => WorldState.GetFlag(requiredWorldStateFlag);

        private IEnumerable<string> EffectiveBias()
        {
            foreach (var propertyId in biasPropertyIds) yield return propertyId;

            if (!string.IsNullOrEmpty(bonusBiasWorldStateFlag) && WorldState.GetFlag(bonusBiasWorldStateFlag))
            {
                foreach (var propertyId in bonusBiasPropertyIds) yield return propertyId;
            }
        }

        private bool IsOnBias(string propertyId)
        {
            foreach (var biased in EffectiveBias())
            {
                if (biased == propertyId) return true;
            }
            return false;
        }

        public float AttentionDuration => attendDuration;

        // A discovery tick is a real daylight cost, same as everything else — that's
        // what makes it a competing use (P3's 3.5 rule, unchanged).
        public bool RequiresDaylight => true;
        public bool ContinueAttending => false;

        public bool CanAttend() => IsOpen;

        public string GetShortName() => displayName;

        public string GetOverlayDescription() => IsOpen ? openDescription : lockedDescription;

        public string GetOverlayInteractionLine()
        {
            if (!IsOpen) return lockedInteractionLine;
            return FindCandidate() != null ? $"Hold E to {workVerb}" : "Hold E to stand in the smoke-smell a while";
        }

        public IReadOnlyList<string> GetAppliedUpgrades() => System.Array.Empty<string>();

        public void OnAttentionComplete()
        {
            var candidate = FindCandidate();
            if (candidate == null)
            {
                if (fallbackFlavorLines.Length > 0)
                {
                    Visuals.NotificationManager.Post(fallbackFlavorLines[Random.Range(0, fallbackFlavorLines.Length)]);
                }
                return;
            }

            PropertyKnowledge.MarkKnown(candidate.Value.Entry.ItemId, candidate.Value.PropertyId);
            PropertyKnowledge.MarkKnown(playerKnowerId, candidate.Value.PropertyId);

            var property = PropertyRegistry.GetById(candidate.Value.PropertyId);
            var phrase = property != null ? property.Phrase : candidate.Value.PropertyId;
            Visuals.NotificationManager.Post($"Working the {candidate.Value.Entry.DisplayName}, it comes clear: it {phrase}.");
            Debug.Log($"{displayName}: revealed '{candidate.Value.PropertyId}' on {candidate.Value.Entry.DisplayName}.", this);

            GetComponent<Visuals.EntityFeedback>()?.TriggerPop();
        }

        public void OnAttentionCancelled() { }

        // Same deterministic scan as P3's bench: first-taken entry, first authored
        // property, filtered to the (current, possibly bonus-extended) bias and to
        // not-yet-known.
        private RevealCandidate? FindCandidate()
        {
            foreach (var entry in TakenLedger.All)
            {
                foreach (var propertyId in entry.PropertyIds)
                {
                    if (!IsOnBias(propertyId)) continue;
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
