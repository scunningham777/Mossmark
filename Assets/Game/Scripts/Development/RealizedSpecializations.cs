using System.Collections.Generic;

namespace Mossmark.Development
{
    // Registry of specialization ids actually realized by some entity (an NPC's
    // specialization draw applying a stage whose Id matches) - the mirror of
    // DeclaredSpecializationNeeds ("needed"), used by SpecializationRealizedCondition to
    // gate Iteration 13's POIs ("inaccessible until ... satisfied").
    public static class RealizedSpecializations
    {
        private static readonly HashSet<string> realized = new();

        public static void Declare(string specializationId) => realized.Add(specializationId);

        public static bool Contains(string specializationId) => realized.Contains(specializationId);
    }
}
