namespace Mossmark.Attention
{
    public interface IAttendable
    {
        float AttentionDuration { get; }

        bool CanAttend();
        string GetOverlayDescription();
        string GetOverlayInteractionLine();
        void OnAttentionComplete();
        void OnAttentionCancelled();
    }
}
