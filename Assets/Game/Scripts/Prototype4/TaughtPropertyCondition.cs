using Mossmark.Development;

namespace Mossmark.Prototype4
{
    // Gates a DevelopmentStage on the entity itself having been taught a property —
    // the P4 analog of P3's KnownPropertyCondition, typed to AcquaintableAttendable
    // instead of KnowingEntityAttendable. The two stay separate component families
    // (see "The Teaching Thread" in PROTOTYPE4_ACQUAINTANCE.md for why a shared base
    // isn't the shape here); this condition is the one new piece that lets the same
    // taught-gate idea apply to the other family. Satisfied the moment the property
    // enters the entity's own known set via AcquaintableAttendable.Teach().
    public class TaughtPropertyCondition : IDependencyCondition
    {
        private readonly string propertyId;

        public TaughtPropertyCondition(string propertyId)
        {
            this.propertyId = propertyId;
        }

        public bool IsSatisfied(DevelopableEntity entity) =>
            entity is AcquaintableAttendable acquaintable && acquaintable.KnowsOfSelf(propertyId);

        public string GetNeedsDescription(DevelopableEntity entity) => "wants to be taught something it doesn't know";
    }
}
