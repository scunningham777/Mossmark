using UnityEngine;

namespace Mossmark.Attention
{
    // Temporary test fixture for Iteration 2 (Attention Framework Core).
    // Real attendable types (wilderness spots, buildings, NPCs, etc.) replace this in later iterations.
    public class PlaceholderAttendable : MonoBehaviour, IAttendable
    {
        [SerializeField] private string displayName = "Placeholder";
        [SerializeField] private float attentionDuration = 1.5f;

        public float AttentionDuration => attentionDuration;

        public bool CanAttend() => true;

        public string GetOverlayDescription() => displayName;

        public string GetOverlayInteractionLine() =>
            attentionDuration > 0f ? "Hold E to attend" : "Press E to attend";

        public void OnAttentionComplete()
        {
            Debug.Log($"{displayName}: attention complete.", this);
        }

        public void OnAttentionCancelled()
        {
            Debug.Log($"{displayName}: attention cancelled.", this);
        }
    }
}
