using Mossmark.Attention;
using Mossmark.Development;
using UnityEngine;

namespace Mossmark.World
{
    // Iteration 8 resolver test entity: a one-shot world-state toggle, demonstrating
    // WorldStateCondition's "a flag set elsewhere" case for the Old Cairn.
    public class SignalFireAttendable : MonoBehaviour, IAttendable
    {
        [SerializeField] private string flagId = "signal_lit";

        public float AttentionDuration => 2f;
        public bool RequiresDaylight => false;

        // Lighting the signal fire is a single one-shot toggle.
        public bool ContinueAttending => false;

        public bool CanAttend() => !WorldState.GetFlag(flagId);

        public string GetShortName() => "Signal Fire";
        public string GetOverlayDescription() => "Signal Fire";

        public string GetOverlayInteractionLine() => CanAttend()
            ? "Hold E to light the signal fire"
            : "The signal fire is lit.";

        public System.Collections.Generic.IReadOnlyList<string> GetAppliedUpgrades() =>
            System.Array.Empty<string>();

        public void OnAttentionComplete()
        {
            if (!CanAttend()) return;

            WorldState.SetFlag(flagId, true);
            Debug.Log("Signal Fire: lit! The cairn should be visible from it now.", this);
        }

        public void OnAttentionCancelled()
        {
        }
    }
}
