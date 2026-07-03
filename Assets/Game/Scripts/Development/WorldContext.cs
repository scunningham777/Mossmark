using System.Collections.Generic;
using Mossmark.Day;
using Mossmark.World;

namespace Mossmark.Development
{
    // Iteration 16.5: static read-only facade over the ambient world-state facts
    // IOutcomeModifier implementations read - one place to look rather than each
    // modifier reaching directly into five separate statics.
    public static class WorldContext
    {
        public static DayPhase CurrentDayPhase =>
            DayCycleManager.Instance != null ? DayCycleManager.Instance.CurrentPhase : DayPhase.Dawn;

        public static IReadOnlyList<PlaceArchetype> SelectedArchetypes =>
            WorldGenerator.SelectedArchetypes;

        public static bool IsSpecializationNeeded(string id) =>
            DeclaredSpecializationNeeds.Contains(id);

        public static bool IsSpecializationRealized(string id) =>
            RealizedSpecializations.Contains(id);

        public static bool GetFlag(string id) => WorldState.GetFlag(id);

        public static bool IsPropertyKnown(string itemId, string propertyId) =>
            PropertyKnowledge.IsKnown(itemId, propertyId);
    }
}
