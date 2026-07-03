using System;
using UnityEngine;

namespace Mossmark.Development
{
    // A specialization that has actually been realized by some entity (RealizedSpecializations) -
    // the mirror of SpecializationNeededCondition, used to gate Iteration 13's POIs until
    // their corresponding NPC specialization exists in town.
    // [Serializable] + serialized (non-readonly) fields enable [SerializeReference]
    // storage so this gate can be authored in data via the CSV condition importer.
    [Serializable]
    public class SpecializationRealizedCondition : IDependencyCondition
    {
        [SerializeField] private string specializationId;
        [SerializeField] private string needsDescription;

        public string SpecializationId => specializationId;

        public SpecializationRealizedCondition(string specializationId, string needsDescription)
        {
            this.specializationId = specializationId;
            this.needsDescription = needsDescription;
        }

        public bool IsSatisfied(DevelopableEntity entity) => RealizedSpecializations.Contains(specializationId);

        public string GetNeedsDescription(DevelopableEntity entity) => needsDescription;
    }
}
