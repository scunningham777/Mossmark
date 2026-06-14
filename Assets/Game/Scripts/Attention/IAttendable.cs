namespace Mossmark.Attention
{
    public interface IAttendable
    {
        float AttentionDuration { get; }

        // Whether a completed attention here draws from the day's stamina pool.
        // Checked by AttentionManager before starting a hold and by the overlay
        // to show the "too late to start that now" line at zero stamina.
        bool RequiresStamina { get; }

        bool CanAttend();
        string GetOverlayDescription();
        string GetOverlayInteractionLine();
        void OnAttentionComplete();
        void OnAttentionCancelled();
    }
}
