using System;
using Mossmark.Attention;
using Mossmark.Visuals;
using UnityEngine;

namespace Mossmark.World
{
    // Iteration 30: a settlement-growth arrival. Three ticks of unhurried attention
    // move this stranger from wary to willing, at which point ArrivalSpawner promotes
    // the GO to a permanent NpcAttendable and the arrival entity is destroyed.
    //
    // Approaching costs no daylight (it's a social act, not extraction). The hold
    // continues across ticks 1-2 and interrupts on tick 3 (the promotion tick),
    // matching the "crossing a threshold ends the hold" rule.
    public class ArrivalAttendable : MonoBehaviour, IAttendable
    {
        private string arrivalName;
        private int warnessThreshold = 3;
        private int warnessProgress;
        private bool promoted;

        // Called by ArrivalSpawner after promotion. Receives this GO so the spawner
        // can destroy it after creating the replacement NpcAttendable.
        private Action<GameObject> onPromote;

        public void Initialize(string arrivalName, int warnessThreshold, Action<GameObject> onPromote)
        {
            this.arrivalName = arrivalName;
            this.warnessThreshold = warnessThreshold;
            this.onPromote = onPromote;
        }

        public float AttentionDuration => 2f;

        public bool RequiresDaylight => false;

        // True for ticks 1 and 2; the promotion tick sets promoted = true which
        // ends the hold (AttentionManager reads ContinueAttending after OnAttentionComplete).
        public bool ContinueAttending => !promoted && warnessProgress < warnessThreshold;

        public bool CanAttend() => !promoted;

        public string GetOverlayDescription() =>
            warnessProgress > 0
                ? $"{arrivalName} — still cautious, but watching"
                : "A stranger resting at the settlement edge";

        public string GetOverlayInteractionLine() => "Hold E to approach";

        public void OnAttentionComplete()
        {
            warnessProgress++;
            Debug.Log($"{arrivalName}: wariness {warnessProgress}/{warnessThreshold}.", this);

            if (warnessProgress >= warnessThreshold)
            {
                promoted = true;
                NotificationManager.Post($"{arrivalName}: seems willing to stay.");
                Debug.Log($"{arrivalName}: willing to stay — becoming part of the settlement.", this);
                // ArrivalSpawner creates the NpcAttendable and destroys this GO.
                onPromote?.Invoke(gameObject);
            }
        }

        public void OnAttentionCancelled() { }
    }
}
