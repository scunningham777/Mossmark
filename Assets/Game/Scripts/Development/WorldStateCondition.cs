namespace Mossmark.Development
{
    // A flag set elsewhere (a wandering thing resolved, a curse lifted, etc.) via WorldState.
    public class WorldStateCondition : IDependencyCondition
    {
        private readonly string flagId;
        private readonly bool requiredValue;
        private readonly string needsDescription;

        public WorldStateCondition(string flagId, bool requiredValue, string needsDescription)
        {
            this.flagId = flagId;
            this.requiredValue = requiredValue;
            this.needsDescription = needsDescription;
        }

        public bool IsSatisfied(DevelopableEntity entity) => WorldState.GetFlag(flagId) == requiredValue;

        public string GetNeedsDescription(DevelopableEntity entity) => needsDescription;
    }
}
