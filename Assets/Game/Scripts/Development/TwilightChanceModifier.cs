using Mossmark.Day;

namespace Mossmark.Development
{
    // Multiplies ChanceMultiplier during Dawn and Dusk - rare finds surface more
    // often at the edges of the day than at its heart. The first modifier that
    // responds to time-of-day rather than town development state, proving the
    // same IOutcomeModifier approach works across different WorldContext reads.
    public class TwilightChanceModifier : IOutcomeModifier
    {
        private readonly float multiplier;

        public TwilightChanceModifier(float multiplier)
        {
            this.multiplier = multiplier;
        }

        public void Apply(OutcomeRequest request)
        {
            var phase = WorldContext.CurrentDayPhase;
            if (phase == DayPhase.Dawn || phase == DayPhase.Dusk)
                request.ChanceMultiplier *= multiplier;
        }
    }
}
