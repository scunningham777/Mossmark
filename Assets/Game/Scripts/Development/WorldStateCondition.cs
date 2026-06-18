using System;
using UnityEngine;

namespace Mossmark.Development
{
    // A flag set elsewhere (a wandering thing resolved, a curse lifted, etc.) via WorldState.
    // [Serializable] enables [SerializeReference] polymorphic storage in LandmarkAttendable.
    [Serializable]
    public class WorldStateCondition : IDependencyCondition
    {
        [SerializeField] private string flagId;
        [SerializeField] private bool requiredValue;
        [SerializeField] private string needsDescription;

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
