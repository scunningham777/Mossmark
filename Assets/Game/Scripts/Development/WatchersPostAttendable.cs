using Mossmark.Attention;
using UnityEngine;

namespace Mossmark.Development
{
    // Iteration 8 resolver test entity: a single stage gated by an EntityStateCondition
    // (Old Cairn must have developed) and a TimeCondition (two attentions of progress).
    public class WatchersPostAttendable : DevelopableEntity, IAttendable
    {
        [SerializeField] private OldCairnAttendable oldCairn;

        private DevelopmentTrack track;

        public override string DisplayName => "Watcher's Post";
        protected override DevelopmentTrack Track => track;

        public float AttentionDuration => 2f;
        public bool RequiresDaylight => LastAttentionMadeProgress;

        // Iteration 8 debug-output entity, not part of the hold-to-build loop yet.
        public bool ContinueAttending => false;

        private void Awake()
        {
            track = new DevelopmentTrack(
                new DevelopmentStage("watch", "Take Up the Watch", progressCost: 2,
                    new EntityStateCondition(oldCairn, 0),
                    new TimeCondition(2)));
        }

        public bool CanAttend() => CanMakeProgress();

        public string GetOverlayDescription() => DisplayName;

        public string GetOverlayInteractionLine() => GetNeedsOrDefault("Hold E to keep watch");

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
