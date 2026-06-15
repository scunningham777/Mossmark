namespace Mossmark.Development
{
    // A specialization that has actually been realized by some entity (RealizedSpecializations) -
    // the mirror of SpecializationNeededCondition, used to gate Iteration 13's POIs until
    // their corresponding NPC specialization exists in town.
    public class SpecializationRealizedCondition : IDependencyCondition
    {
        private readonly string specializationId;
        private readonly string needsDescription;

        public SpecializationRealizedCondition(string specializationId, string needsDescription)
        {
            this.specializationId = specializationId;
            this.needsDescription = needsDescription;
        }

        public bool IsSatisfied(DevelopableEntity entity) => RealizedSpecializations.Contains(specializationId);

        public string GetNeedsDescription(DevelopableEntity entity) => needsDescription;
    }
}
