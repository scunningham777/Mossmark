using System.Collections.Generic;

namespace Mossmark.Inventory
{
    // Static vocabulary of all folk-phrase item properties. Small enough that
    // a hard-coded registry is simpler than a ScriptableObject chain; CSV pipeline
    // can absorb these ids/phrases directly in a future pass if authoring needs grow.
    public static class PropertyRegistry
    {
        private static readonly PropertyDefinition[] all = new[]
        {
            new PropertyDefinition("holds_the_cold", "holds the cold"),
            new PropertyDefinition("turns_water",    "turns water"),
            new PropertyDefinition("binds_fast",     "binds fast"),
            new PropertyDefinition("split_prone",    "splits under strain"),
            new PropertyDefinition("burns_slow",     "burns slow"),
            new PropertyDefinition("keeps_well",     "keeps well"),
            new PropertyDefinition("draws_the_eye",  "draws the eye"),
            new PropertyDefinition("heavy_true",     "heavy and true"),
        };

        public static IReadOnlyList<PropertyDefinition> All => all;

        public static PropertyDefinition GetById(string id)
        {
            foreach (var p in all)
                if (p.Id == id) return p;
            return null;
        }
    }
}
