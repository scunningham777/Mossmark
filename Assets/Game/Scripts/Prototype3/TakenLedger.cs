using System;
using System.Collections.Generic;

namespace Mossmark.Prototype3
{
    // Iteration 3.6: a session store of everything the player has taken — identity
    // only, no counts, nothing consumable. This is the 7-14-26 IDEAS.md frame stated
    // plainly: what a taken thing gives you is acquaintance with it, not a stack of it.
    // Insertion order is preserved (not a Dictionary's enumeration order) so the working
    // surface (Iteration 3.7) can promise a deterministic first-taken-first-checked scan.
    public static class TakenLedger
    {
        public class Entry
        {
            public string ItemId;
            public string DisplayName;
            public string[] PropertyIds;
        }

        private static readonly List<Entry> taken = new();
        private static readonly HashSet<string> takenIds = new();

        public static event Action Changed;

        public static IReadOnlyList<Entry> All => taken;

        public static void Register(string itemId, string displayName, string[] propertyIds)
        {
            if (!takenIds.Add(itemId)) return;

            taken.Add(new Entry { ItemId = itemId, DisplayName = displayName, PropertyIds = propertyIds });
            Changed?.Invoke();
        }
    }
}
