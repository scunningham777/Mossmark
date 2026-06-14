namespace Mossmark.Development
{
    // A specialization currently "needed" by a revived building, via DeclaredSpecializationNeeds.
    // specializationId matches the DevelopmentStage.Id of the corresponding NPC specialization stage.
    public class SpecializationNeededCondition : IDependencyCondition
    {
        private readonly string specializationId;

        public SpecializationNeededCondition(string specializationId)
        {
            this.specializationId = specializationId;
        }

        public bool IsSatisfied(DevelopableEntity entity) => DeclaredSpecializationNeeds.Contains(specializationId);

        public string GetNeedsDescription(DevelopableEntity entity) => "needs the town to need this specialization";
    }
}
