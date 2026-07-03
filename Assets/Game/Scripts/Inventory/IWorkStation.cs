using System.Collections.Generic;
using UnityEngine;

namespace Mossmark.Inventory
{
    // Contract between a conversion station entity and WorkshopUI (Iteration 39).
    // Defined here rather than in Mossmark.Development so the UI's dependency points
    // the same way as the rest of the Inventory namespace — Development already
    // depends on Inventory, never the reverse.
    public interface IWorkStation
    {
        string StationDisplayName { get; }

        // Property ids this station can resolve. Recipe matching and property
        // discovery (success and failure) are both filtered to this set.
        IReadOnlyList<string> BiasPropertyIds { get; }

        // The station's GameObject, for feedback signals (EntityFeedback pop).
        GameObject StationObject { get; }
    }
}
