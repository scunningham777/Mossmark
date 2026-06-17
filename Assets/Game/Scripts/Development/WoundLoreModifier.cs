namespace Mossmark.Development
{
    // Sets DaylightCostMultiplier to 0 while the Hedge Witch's Wound Lore is active
    // (WorldState flag "hedge_witch_wound_lore"), eliminating the extra daylight penalty
    // on a bad wandering-thing outcome. The base attend cost (RequiresDaylight) still
    // applies - the modifier only softens severity, not the encounter itself.
    public class WoundLoreModifier : IOutcomeModifier
    {
        public void Apply(OutcomeRequest request)
        {
            if (WorldContext.GetFlag("hedge_witch_wound_lore"))
                request.DaylightCostMultiplier = 0f;
        }
    }
}
