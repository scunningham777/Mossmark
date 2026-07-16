using System.Collections.Generic;
using Mossmark.Attention;
using Mossmark.Development;
using Mossmark.Inventory;
using Mossmark.Visuals;
using UnityEngine;

namespace Mossmark.Prototype3
{
    // Prototype 3's minimal single-item pickup: no inventory, no stacking, no quantity.
    // Every take registers with the TakenLedger (Iteration 3.6) regardless of reveal
    // state. Whether its properties become known on the spot is now authorable per
    // pickup: autoRevealOnTake true (the 3.3 control path, e.g. the Lump of Clay) marks
    // them known immediately in PropertyKnowledge — under the item's id (the canonical
    // discovery store) and under the player-as-knower id the teach gate reads. false
    // (Iteration 3.6's new pickups) leaves them unknown, to be worked out later at a
    // working surface (Iteration 3.7).
    public class PropertyPickupAttendable : MonoBehaviour, IAttendable
    {
        [SerializeField] private string itemId = "p3_clay_lump";
        [SerializeField] private string playerKnowerId = "p3_player";
        [SerializeField] private string displayName = "A Lump of Clay";
        [SerializeField] private string description = "Cold, dense earth, slick where the rain has found it.";
        [SerializeField] private string[] propertyIds = { "turns_water" };
        [SerializeField] private bool autoRevealOnTake = true;
        [SerializeField] private string revealLine = "Clay. Slick, dense. Water beads and runs off it.";
        [SerializeField] private string unrevealedTakeLine = "Taken. There's more to it than you can say yet.";
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
            TakenLedger.Register(itemId, displayName, propertyIds);

            if (autoRevealOnTake)
            {
                foreach (var propertyId in propertyIds)
                {
                    PropertyKnowledge.MarkKnown(itemId, propertyId);
                    PropertyKnowledge.MarkKnown(playerKnowerId, propertyId);
                }

                NotificationManager.Post(revealLine);
                Debug.Log($"{displayName}: taken - revealed [{string.Join(", ", propertyIds)}].", this);
            }
            else
            {
                NotificationManager.Post(unrevealedTakeLine);
                Debug.Log($"{displayName}: taken - nature not yet known.", this);
            }

            Destroy(gameObject);
        }

        public void OnAttentionCancelled() { }
    }
}
