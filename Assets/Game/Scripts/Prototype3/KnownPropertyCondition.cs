using Mossmark.Development;

namespace Mossmark.Prototype3
{
    // Gates a DevelopmentStage on the entity itself knowing a property — the knowledge
    // analog of ItemAvailableCondition, with nothing carried or consumed. Satisfied the
    // moment the property enters the entity's known set (seeded or taught); this is the
    // hinge the whole Prototype 3 test turns on.
    public class KnownPropertyCondition : IDependencyCondition
    {
        private readonly string propertyId;
        private readonly string needsDescription;

        public KnownPropertyCondition(string propertyId, string needsDescription)
        {
            this.propertyId = propertyId;
            this.needsDescription = needsDescription;
        }

        public bool IsSatisfied(DevelopableEntity entity) =>
            entity is KnowingEntityAttendable knower && knower.Knows(propertyId);

        public string GetNeedsDescription(DevelopableEntity entity) => needsDescription;
    }
}
