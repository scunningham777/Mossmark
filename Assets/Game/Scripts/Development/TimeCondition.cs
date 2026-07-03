using System;
using UnityEngine;

namespace Mossmark.Development
{
    // "N attentions of accumulated progress with no other requirement" - the default
    // early dependency for NPCs ("needs more time") per PROTOTYPE2.md. Always satisfied:
    // it never blocks a tick from being productive (LastAttentionMadeProgress), so an
    // ongoing hold spends daylight and continues every tick (Section 6's "dependencies
    // satisfied by construction"). The paired stage's ProgressCost - checked separately
    // by DevelopableEntity.TryApplyStage - is what actually gates when the stage applies.
    // requiredProgress is unused but kept for documentation (call sites self-document the
    // threshold they pair with) and for [SerializeReference] serialization in LandmarkAttendable.
    [Serializable]
    public class TimeCondition : IDependencyCondition
    {
        [SerializeField] private int requiredProgress;

        public int RequiredProgress => requiredProgress;

        public TimeCondition(int requiredProgress)
        {
            this.requiredProgress = requiredProgress;
        }

        public bool IsSatisfied(DevelopableEntity entity) => true;

        public string GetNeedsDescription(DevelopableEntity entity) => "needs more time";
    }
}
