using System.Collections.Generic;

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

        // Whether AttendableOverlayUI draws anything for this attendable — the name panel
        // above it, the progress bar, and the bottom-right detail panel all go together.
        //
        // Defaulted rather than required so the eighteen existing implementers inherit
        // "yes" untouched; only something that deliberately wants to be a purely visual
        // state overrides it. P5's ambient attend is the first: attending your
        // surroundings is meant to feel like walking and noticing, and a panel pinned over
        // the player's head while they stand still is the opposite of that. Its progress
        // bar would also be actively misleading from 5.3 on, since it measures the
        // daylight tick while the thing the player cares about is per-object dwell — a
        // different clock, on a different object.
        bool ShowOverlay => true;

        // Short entity name for the world-space tooltip above the entity.
        // Should be a plain name only — no flavor prose, no drift/tendedness suffixes.
        string GetShortName();

        // Full description for the bottom-right detail panel — may include drift suffix,
        // locked state text, flavor prose, or any context that needs more screen space.
        string GetOverlayDescription();
        string GetOverlayInteractionLine();

        // Returns the display names of all development stages that have been
        // permanently applied to this entity (empty for non-DevelopableEntity types).
        IReadOnlyList<string> GetAppliedUpgrades();

        void OnAttentionComplete();
        void OnAttentionCancelled();
    }
}
