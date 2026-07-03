using System.Collections.Generic;

namespace Mossmark.Development
{
    // Session-only store for which item properties the player has discovered.
    // No persistence yet — resets on play-mode exit. Future save integration
    // will serialize `known` and `allRevealed` to a player file.
    // Access from IOutcomeModifiers via WorldContext.IsPropertyKnown().
    public static class PropertyKnowledge
    {
        private static readonly Dictionary<string, HashSet<string>> known = new();
        private static bool allRevealed;
        private static bool showDebugTags;

        // True when the debug tag-display mode is active (shows property ids
        // instead of folk phrases, so you can see the mechanical layer directly).
        public static bool ShowDebugTags => showDebugTags;

        public static bool IsKnown(string itemId, string propertyId)
        {
            if (allRevealed) return true;
            return known.TryGetValue(itemId, out var set) && set.Contains(propertyId);
        }

        public static void MarkKnown(string itemId, string propertyId)
        {
            if (!known.ContainsKey(itemId))
                known[itemId] = new HashSet<string>();
            known[itemId].Add(propertyId);
        }

        // Debug: reveal all properties on all items without discovery grinding.
        public static void RevealAll() => allRevealed = true;

        // Debug: cycle back to normal (unknown) state for testing fresh discovery.
        public static void HideAll()
        {
            allRevealed = false;
            known.Clear();
        }

        // Debug: toggle mechanical tag display vs. folk phrases in the inventory.
        public static void ToggleShowTags() => showDebugTags = !showDebugTags;
    }
}
