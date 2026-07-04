namespace Mossmark.Development
{
    // Mutable context passed through IOutcomeModifier.Apply() calls. Starts at 1
    // for all dimensions (no change); each modifier scales the dimension it needs.
    // Add new fields only when a specific modifier actually requires them.
    public class OutcomeRequest
    {
        public float ChanceMultiplier = 1f;

        // Scales any extra daylight cost on bad outcomes (e.g. WoundLoreModifier sets
        // this to 0 to eliminate the additional penalty without affecting the base cost
        // the attend itself already spent via RequiresDaylight).
        public float DaylightCostMultiplier = 1f;

        // Scales a rolled hold-duration range (e.g. BuildingAttendable.RollTickInterval).
        // Duration is a continuous, organically-authored min/max range rather than a
        // single base cost, so this multiplies the roll rather than being derived from
        // DaylightCostMultiplier (Iteration 41 - the two dimensions run through the same
        // modifier pipeline but are never computed from each other).
        public float DurationMultiplier = 1f;
    }
}
