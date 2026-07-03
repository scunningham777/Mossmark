using Mossmark.Development;

namespace Mossmark.World
{
    // Archetype POI (Point of Interest) — thin subclass of WildernessYieldAttendable that
    // adds a gate (IDependencyCondition) and a locked overlay description. While the gate is
    // unsatisfied the POI is inaccessible; once satisfied it behaves like a wilderness spot.
    // POIs intentionally do NOT apply TwilightChanceModifier — they are distinctive rare-access
    // locations, not ambient foraging spots, so their base rareDropChance is used unchanged.
    public class PoiAttendable : WildernessYieldAttendable
    {
        private string lockedDescription = "Something here feels just out of reach.";
        private IDependencyCondition gate;

        public void Initialize(string displayName, string lockedDescription, string interactionVerb,
            ItemYield[] commonYields, ItemYield[] rareYields, float rareDropChance,
            float minTickInterval, float maxTickInterval, IDependencyCondition gate)
        {
            InitializeBase(displayName, interactionVerb, commonYields, rareYields, rareDropChance,
                minTickInterval, maxTickInterval);
            this.lockedDescription = lockedDescription;
            this.gate = gate;
            foundVerb = "found";
        }

        public override bool CanAttend() => gate == null || gate.IsSatisfied(null);

        public override string GetOverlayDescription() => CanAttend() ? WithTendednessSuffix(displayName) : lockedDescription;

        public override string GetOverlayInteractionLine() => CanAttend()
            ? $"Hold E to {interactionVerb}"
            : gate.GetNeedsDescription(null);
    }
}
