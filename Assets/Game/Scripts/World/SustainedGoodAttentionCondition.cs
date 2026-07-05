using System;
using Mossmark.Development;
using UnityEngine;

namespace Mossmark.World
{
    // Implemented by wilderness spots that track a per-rest "good attention" counter
    // (Iteration 43). Lets SustainedGoodAttentionCondition read the specific spot's own
    // counter through the DevelopableEntity passed into IsSatisfied — the same
    // self-reference EntityStateCondition and ArrivalCondition already rely on, just
    // narrowed to an interface instead of a concrete field/global lookup.
    public interface IGoodAttentionTracker
    {
        int GoodAttentionDays { get; }
    }

    // Satisfied once the attended entity's own goodAttentionDays counter reaches minDays.
    // Lives in Mossmark.World (like ArrivalCondition) rather than Mossmark.Development,
    // since IGoodAttentionTracker is a World-scoped concept — same cross-namespace
    // discipline noted in CLAUDE.md for KnowledgeYieldModifier.
    // [Serializable] + serialized (non-readonly) fields enable [SerializeReference]
    // storage so this gate can be authored the same way as any other condition.
    [Serializable]
    public class SustainedGoodAttentionCondition : IDependencyCondition
    {
        [SerializeField] private int minDays = 3;

        public int MinDays => minDays;

        public SustainedGoodAttentionCondition(int minDays)
        {
            this.minDays = minDays;
        }

        public bool IsSatisfied(DevelopableEntity entity) =>
            entity is IGoodAttentionTracker tracker && tracker.GoodAttentionDays >= minDays;

        public string GetNeedsDescription(DevelopableEntity entity) =>
            "needs sustained good tending across several days before it settles";
    }
}
