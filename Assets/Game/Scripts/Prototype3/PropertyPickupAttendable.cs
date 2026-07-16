using System.Collections.Generic;
using Mossmark.Attention;
using Mossmark.Development;
using Mossmark.Inventory;
using Mossmark.Visuals;
using UnityEngine;

namespace Mossmark.Prototype3
{
    // Prototype 3's minimal single-item pickup: no inventory, no stacking, no quantity.
    // Taking the thing is the moment its one property becomes known — recorded twice in
    // PropertyKnowledge: under the item's id (the canonical discovery store) and under
    // the player-as-knower id the teach gate reads. Discovery-effort is deliberately not
    // under test here; the reveal is automatic.
    public class PropertyPickupAttendable : MonoBehaviour, IAttendable
    {
        [SerializeField] private string itemId = "p3_clay_lump";
        [SerializeField] private string playerKnowerId = "p3_player";
        [SerializeField] private string displayName = "A Lump of Clay";
        [SerializeField] private string description = "Cold, dense earth, slick where the rain has found it.";
        [SerializeField] private string propertyId = "turns_water";
        [SerializeField] private string revealLine = "Clay. Slick, dense. Water beads and runs off it.";
        [SerializeField, Min(0.1f)] private float attendDuration = 1f;

        public float AttentionDuration => attendDuration;

        // Iteration 3.5: taking the thing costs daylight, same as any wilderness yield.
        public bool RequiresDaylight => true;
        public bool ContinueAttending => false;

        public bool CanAttend() => true;

        public string GetShortName() => displayName;
        public string GetOverlayDescription() => description;
        public string GetOverlayInteractionLine() => "Hold E to take it";

        public IReadOnlyList<string> GetAppliedUpgrades() => System.Array.Empty<string>();

        public void OnAttentionComplete()
        {
            PropertyKnowledge.MarkKnown(itemId, propertyId);
            PropertyKnowledge.MarkKnown(playerKnowerId, propertyId);

            NotificationManager.Post(revealLine);
            Debug.Log($"{displayName}: taken - '{propertyId}' now known to the player.", this);

            Destroy(gameObject);
        }

        public void OnAttentionCancelled() { }
    }
}
