using Mossmark.Attention;
using UnityEngine;

namespace Mossmark.Day
{
    public class BedrollAttendable : MonoBehaviour, IAttendable
    {
        [SerializeField] private float restDuration = 1f;

        public float AttentionDuration => restDuration;

        // Resting restores daylight rather than spending it.
        public bool RequiresDaylight => false;

        // Resting is a single check-in, not a hold.
        public bool ContinueAttending => false;

        // AttentionManager.HandleHoldStarted already locks all attention during
        // DayCycleManager.IsTransitioning, so this attendable has no additional gate.
        public bool CanAttend() => true;

        public string GetOverlayDescription() => "Bedroll";

        public string GetOverlayInteractionLine() => "Hold E to rest";

        public void OnAttentionComplete()
        {
            DayCycleManager.Instance?.Rest();
        }

        public void OnAttentionCancelled()
        {
        }
    }
}
