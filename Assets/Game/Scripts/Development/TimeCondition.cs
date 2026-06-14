namespace Mossmark.Development
{
    // "N attentions of accumulated progress with no other requirement" - the default
    // early dependency for NPCs ("needs more time") per PROTOTYPE2.md. Always satisfied:
    // it never blocks a tick from being productive (LastAttentionMadeProgress), so an
    // ongoing hold spends daylight and continues every tick (Section 6's "dependencies
    // satisfied by construction"). The paired stage's ProgressCost - checked separately
    // by DevelopableEntity.TryApplyStage - is what actually gates when the stage applies.
    // requiredProgress is unused but kept as a constructor parameter purely so call sites
    // self-document the threshold they're pairing with.
    public class TimeCondition : IDependencyCondition
    {
        public TimeCondition(int requiredProgress)
        {
        }

        public bool IsSatisfied(DevelopableEntity entity) => true;

        public string GetNeedsDescription(DevelopableEntity entity) => "needs more time";
    }
}
