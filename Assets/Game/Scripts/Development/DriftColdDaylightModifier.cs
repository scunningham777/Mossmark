namespace Mossmark.Development
{
    // Doubles DaylightCostMultiplier once drift has crossed the cold threshold.
    // Replaces the inline "if (DriftProgress >= DriftThreshold) SpendDaylight(1)"
    // tax that used to live in BuildingAttendable.OnAttentionComplete() - since the
    // base develop cost is always 1, x2 and +1 land on the same number, so this is
    // a pure refactor of existing behavior into the modifier pattern, not a balance
    // change. Takes raw values rather than an entity reference, matching the
    // decoupled-constructor precedent set by WorldStateChanceModifier/WoundLoreModifier.
    public class DriftColdDaylightModifier : IOutcomeModifier
    {
        private readonly int driftProgress;
        private readonly int driftThreshold;

        public DriftColdDaylightModifier(int driftProgress, int driftThreshold)
        {
            this.driftProgress = driftProgress;
            this.driftThreshold = driftThreshold;
        }

        public void Apply(OutcomeRequest request)
        {
            if (driftProgress >= driftThreshold)
                request.DaylightCostMultiplier *= 2f;
        }
    }
}
