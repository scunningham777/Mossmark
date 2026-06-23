using System;
using Mossmark.Development;
using UnityEngine;

namespace Mossmark.World
{
    // Iteration 30: condition that gates a new-arrival spawn event. Checks that a set
    // of WorldState flags are all true AND that a minimum number of DevelopableEntities
    // have crossed at least one development threshold. Authored inline as a [Serializable]
    // field inside ArrivalTrigger so conditions live entirely in scene/asset data.
    //
    // Failure is silent — arrivals don't surface a "needs" message, they just don't happen.
    [Serializable]
    public class ArrivalCondition : IDependencyCondition
    {
        [SerializeField] private string[] requiredFlags = Array.Empty<string>();
        [SerializeField] private int minimumDevelopedEntities;

        // IsSatisfied: all required flags must be set, AND developed-entity count must
        // meet or exceed the minimum. Entity parameter is unused (arrival conditions are
        // world-scope, not entity-scope), following the established pattern from PoiAttendable.
        public bool IsSatisfied(DevelopableEntity entity)
        {
            if (requiredFlags != null)
                foreach (var flag in requiredFlags)
                    if (!WorldContext.GetFlag(flag)) return false;

            if (minimumDevelopedEntities > 0)
            {
                var entities = UnityEngine.Object.FindObjectsByType<DevelopableEntity>(FindObjectsInactive.Exclude);
                int count = 0;
                foreach (var e in entities)
                    if (e.CurrentStageIndex >= 0) count++;
                if (count < minimumDevelopedEntities) return false;
            }

            return true;
        }

        // Arrivals don't surface needs — they appear without announcement.
        public string GetNeedsDescription(DevelopableEntity entity) => "";
    }
}
