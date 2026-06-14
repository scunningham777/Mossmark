using System.Collections.Generic;

namespace Mossmark.Development
{
    // Generic flag registry for WorldStateCondition - "a flag set elsewhere (a wandering
    // thing resolved, a curse lifted)" per PROTOTYPE2.md's Generic Dependency / Response Resolver.
    public static class WorldState
    {
        private static readonly Dictionary<string, bool> flags = new();

        public static bool GetFlag(string id) => flags.TryGetValue(id, out var value) && value;

        public static void SetFlag(string id, bool value) => flags[id] = value;
    }
}
