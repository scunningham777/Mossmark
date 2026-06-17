namespace Mossmark.Development
{
    // Multiplies ChanceMultiplier when a WorldState flag is set - the generic
    // "a development stage set a flag, an encounter reads it and biases outcome"
    // hook. Companion to WoundLoreModifier for effects that shift probability rather
    // than severity (e.g. Charm Against the Wraith once that stage is authored).
    public class WorldStateChanceModifier : IOutcomeModifier
    {
        private readonly string flagId;
        private readonly float multiplier;

        public WorldStateChanceModifier(string flagId, float multiplier)
        {
            this.flagId = flagId;
            this.multiplier = multiplier;
        }

        public void Apply(OutcomeRequest request)
        {
            if (WorldContext.GetFlag(flagId))
                request.ChanceMultiplier *= multiplier;
        }
    }
}
