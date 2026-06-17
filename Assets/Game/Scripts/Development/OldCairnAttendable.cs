using Mossmark.Attention;
using Mossmark.Inventory;
using UnityEngine;

namespace Mossmark.Development
{
    // Iteration 8 resolver test entity: a single stage gated by an ItemAvailableCondition
    // (forage Sticks from the Field) and a WorldStateCondition (light the Signal Fire).
    public class OldCairnAttendable : DevelopableEntity, IAttendable
    {
        [SerializeField] private ItemDefinition sticks;

        private DevelopmentTrack track;

        public override string DisplayName => "Old Cairn";
        protected override DevelopmentTrack Track => track;

        public float AttentionDuration => 2f;
        public bool RequiresDaylight => LastAttentionMadeProgress;

        // Iteration 8 debug-output entity, not part of the hold-to-build loop yet.
        public bool ContinueAttending => false;

        private void Awake()
        {
            track = new DevelopmentTrack(
                new DevelopmentStage("repair", "Repair the Cairn", progressCost: 1,
                    new ItemAvailableCondition(sticks, 2),
                    new WorldStateCondition("signal_lit", true, "needs the signal fire lit nearby")));
        }

        public bool CanAttend() => CanMakeProgress();

        public string GetOverlayDescription() => DisplayName;

        public string GetOverlayInteractionLine() => CurrentStageIndex >= 0
            ? "The cairn stands repaired."
            : GetNeedsOrDefault("Hold E to work on the cairn");

        public void OnAttentionComplete()
        {
            LogDependencyReport();
            ResolveAttention();
        }

        public void OnAttentionCancelled()
        {
        }
    }
}
