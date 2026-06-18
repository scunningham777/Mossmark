using System;
using Mossmark.Inventory;
using UnityEngine;

namespace Mossmark.Development
{
    [Serializable]
    public class BuildingStageDef
    {
        public string displayName;
        public string verb;
        public ItemDefinition material;
        public int costPerTick = 2;
        public int progressCost = 6;
        // Specialization that must be realized before this stage is available. Empty = no dep.
        public string requiredSpecialization;
        public Color tint = new(0.5f, 0.5f, 0.45f, 1f);
    }
}
