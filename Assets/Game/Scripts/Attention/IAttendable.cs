namespace Mossmark.Attention
{
    public interface IAttendable
    {
        float AttentionDuration { get; }

        // Whether a completed attention here draws from the day's daylight pool.
        // Checked by AttentionManager before starting a hold and by the overlay
        // to show the "too late to start that now" line at zero daylight.
        bool RequiresDaylight { get; }

        // Whether the hold continues into another tick after this one completes.
        // Read after OnAttentionComplete(), same timing as RequiresDaylight. One-shot
        // attendables (tended spots, bedroll, signal fire, the dependency-resolver
        // test entities) return false - a single attention is a complete check-in,
        // not the start of a repeating hold.
        bool ContinueAttending { get; }

        bool CanAttend();
        string GetOverlayDescription();
        string GetOverlayInteractionLine();
        void OnAttentionComplete();
        void OnAttentionCancelled();
    }
}
