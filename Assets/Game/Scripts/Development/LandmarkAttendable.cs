using Mossmark.Attention;
using UnityEngine;

namespace Mossmark.Development
{
    // Generic replacement for the Iteration 8 one-off debug entities (OldCairnAttendable,
    // WatchersPostAttendable). A single-stage DevelopableEntity whose display name, hold
    // duration, progress cost, and dependency conditions are all authored in data rather
    // than hardcoded. Dependencies are stored as [SerializeReference] so any
    // IDependencyCondition implementation can be wired up via the hand-edit-YAML pattern.
    // One-shot: ContinueAttending => false, same as the original debug entities.
    public class LandmarkAttendable : DevelopableEntity, IAttendable
    {
        [SerializeField] private string displayName = "Landmark";
        [SerializeField] private Color entityColor = Color.white;
        [SerializeField] private string attendVerb = "attend to it";
        [SerializeField] private string completedDescription = "Complete.";
        [SerializeField, Min(1)] private int progressCost = 1;
        [SerializeField, Min(0.1f)] private float attendDuration = 2f;

        // Polymorphic condition list — authored via hand-edit YAML since MCP-Unity
        // can't assign [SerializeReference] fields through the inspector.
        [SerializeReference] private IDependencyCondition[] conditions = System.Array.Empty<IDependencyCondition>();

        private DevelopmentTrack track;

        public override string DisplayName => displayName;
        protected override DevelopmentTrack Track => track;

        public float AttentionDuration => attendDuration;
        public bool RequiresDaylight => LastAttentionMadeProgress;
        public bool ContinueAttending => false;

        private void Awake()
        {
            track = new DevelopmentTrack(
                new DevelopmentStage("complete", $"Complete the {displayName}", progressCost, conditions));
        }

        public bool CanAttend() => CanMakeProgress();

        public string GetShortName() => displayName;
        public string GetOverlayDescription() => displayName;

        public System.Collections.Generic.IReadOnlyList<string> GetAppliedUpgrades() =>
            GetAppliedUpgradeNames();

        public string GetOverlayInteractionLine() => CurrentStageIndex >= 0
            ? completedDescription
            : GetNeedsOrDefault($"Hold E to {attendVerb}");

        public void OnAttentionComplete()
        {
            LogDependencyReport();
            ResolveAttention();
        }

        public void OnAttentionCancelled() { }
    }
}
