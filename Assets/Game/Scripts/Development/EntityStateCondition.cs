using System;
using UnityEngine;

namespace Mossmark.Development
{
    // Another DevelopableEntity (any type) has reached a given stage.
    // [Serializable] enables [SerializeReference] polymorphic storage in LandmarkAttendable.
    [Serializable]
    public class EntityStateCondition : IDependencyCondition
    {
        [SerializeField] private DevelopableEntity targetEntity;
        [SerializeField] private int minimumStageIndex;

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
