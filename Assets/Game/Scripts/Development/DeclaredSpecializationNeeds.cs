using System.Collections.Generic;

namespace Mossmark.Development
{
    // Registry of specialization ids currently "needed" by buildings that have realized
    // their latent specialization (Development Application's "Building -> NPC demand").
    // Specialization ids correspond to DevelopmentStage.Id values on NPC tracks.
    public static class DeclaredSpecializationNeeds
    {
        private static readonly HashSet<string> needs = new();

        public static void Declare(string specializationId) => needs.Add(specializationId);

        public static bool Contains(string specializationId) => needs.Contains(specializationId);

        public static void Consume(string specializationId) => needs.Remove(specializationId);
    }
}
