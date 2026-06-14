using System.Collections.Generic;

namespace Mossmark.Development
{
    public class DevelopmentStage
    {
        public string Id { get; }
        public string DisplayName { get; }
        public int ProgressCost { get; }

        private readonly List<IDependencyCondition> dependencies;

        public DevelopmentStage(string id, string displayName, int progressCost, params IDependencyCondition[] dependencies)
        {
            Id = id;
            DisplayName = displayName;
            ProgressCost = progressCost;
            this.dependencies = new List<IDependencyCondition>(dependencies);
        }

        public bool AreDependenciesSatisfied(DevelopableEntity entity)
        {
            foreach (var dependency in dependencies)
            {
                if (!dependency.IsSatisfied(entity)) return false;
            }

            return true;
        }

        public string GetUnsatisfiedNeedsDescription(DevelopableEntity entity)
        {
            foreach (var dependency in dependencies)
            {
                if (!dependency.IsSatisfied(entity)) return dependency.GetNeedsDescription(entity);
            }

            return null;
        }

        public IEnumerable<(IDependencyCondition Condition, bool Satisfied)> EvaluateDependencies(DevelopableEntity entity)
        {
            foreach (var dependency in dependencies)
            {
                yield return (dependency, dependency.IsSatisfied(entity));
            }
        }
    }
}
