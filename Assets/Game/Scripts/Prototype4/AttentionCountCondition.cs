using Mossmark.Development;

namespace Mossmark.Prototype4
{
    // "N completed attends at this entity, no other requirement" — the acquaintance
    // analog of TimeCondition, scoped to attends-at-this-entity rather than global time.
    // Always satisfied: it never blocks a tick from being productive, so every attend
    // deepens acquaintance; the paired stage's ProgressCost (checked separately by
    // DevelopableEntity.TryApplyStage) is what actually gates the stage crossing.
    // AcquaintableAttendable adds 1 progress per completed attend, so ProgressCost is
    // literally an attend count.
    [System.Serializable]
    public class AttentionCountCondition : IDependencyCondition
    {
        public bool IsSatisfied(DevelopableEntity entity) => true;

        public string GetNeedsDescription(DevelopableEntity entity) => "wants only more of your attention";
    }
}
