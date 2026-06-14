namespace Mossmark.Development
{
    // Another DevelopableEntity (any type) has reached a given stage.
    public class EntityStateCondition : IDependencyCondition
    {
        private readonly DevelopableEntity targetEntity;
        private readonly int minimumStageIndex;

        public EntityStateCondition(DevelopableEntity targetEntity, int minimumStageIndex)
        {
            this.targetEntity = targetEntity;
            this.minimumStageIndex = minimumStageIndex;
        }

        public bool IsSatisfied(DevelopableEntity entity)
            => targetEntity != null && targetEntity.CurrentStageIndex >= minimumStageIndex;

        public string GetNeedsDescription(DevelopableEntity entity)
            => $"needs {targetEntity?.DisplayName ?? "something"} to develop further";
    }
}
