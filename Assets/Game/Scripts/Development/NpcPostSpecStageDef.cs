using System;
using UnityEngine;

namespace Mossmark.Development
{
    // Iteration 22 (G3): one post-specialization development stage for an archetype's NPC
    // specialization, authored in data on PlaceArchetype rather than hardcoded in NpcAttendable.
    // The item required is resolved from the archetype at runtime via useRareItem:
    //   false => archetype.CommonYields[0].Item
    //   true  => archetype.RareYield.Item
    [Serializable]
    public class NpcPostSpecStageDef
    {
        public string stageId;
        public string displayName;
        public int progressCost = 3;
        // false = archetype CommonYields[0].Item; true = archetype RareYield.Item
        public bool useRareItem;
        public int itemCount = 2;
        // Logged as "{specializedName}: {flavorText}" — exclude the NPC name from this string.
        public string flavorText;
        // WorldState flag key to set true on apply. Empty = no flag set.
        public string worldStateFlag;
        // ArchetypeId of the wilderness spot whose tendedness drives passive progress
        // each rest. Empty = no passive drift for this stage (attention-only).
        public string passiveDriftSourceArchetypeId;
    }
}
