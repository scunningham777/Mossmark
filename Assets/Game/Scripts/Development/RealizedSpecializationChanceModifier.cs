namespace Mossmark.Development
{
    // Multiplies ChanceMultiplier when a specific NPC specialization has been
    // realized in town - the "town development shifts odds" hook from PROTOTYPE2.md's
    // Outcome Influence section, extracted from WanderingThingAttendable's prior
    // ad hoc goodChanceBonus addition.
    public class RealizedSpecializationChanceModifier : IOutcomeModifier
    {
        private readonly string specializationId;
        private readonly float multiplier;

        public RealizedSpecializationChanceModifier(string specializationId, float multiplier)
        {
            this.specializationId = specializationId;
            this.multiplier = multiplier;
        }

        public void Apply(OutcomeRequest request)
        {
            if (!string.IsNullOrEmpty(specializationId) && WorldContext.IsSpecializationRealized(specializationId))
                request.ChanceMultiplier *= multiplier;
        }
    }
}
