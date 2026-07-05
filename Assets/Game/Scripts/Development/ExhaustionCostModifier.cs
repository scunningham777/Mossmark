namespace Mossmark.Development
{
    // Doubles both DaylightCostMultiplier and DurationMultiplier once a wilderness spot's
    // exhaustion crosses its penalty threshold (Iteration 43.1) - the "felt, not read" cost
    // of hammering a spot in one day, applied before the yield penalty (which now only
    // stacks once fully overworked) ever kicks in. Takes raw values rather than an entity
    // reference, matching the decoupled-constructor precedent set by
    // DriftColdDaylightModifier/WorldStateChanceModifier/WoundLoreModifier.
    public class ExhaustionCostModifier : IOutcomeModifier
    {
        private readonly float exhaustion;
        private readonly float threshold;

        public ExhaustionCostModifier(float exhaustion, float threshold)
        {
            this.exhaustion = exhaustion;
            this.threshold = threshold;
        }

        public void Apply(OutcomeRequest request)
        {
            if (exhaustion <= threshold) return;
            request.DaylightCostMultiplier *= 2f;
            request.DurationMultiplier *= 2f;
        }
    }
}
