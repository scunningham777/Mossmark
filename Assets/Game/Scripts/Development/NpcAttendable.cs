using System.Collections.Generic;
using Mossmark.Attention;
using Mossmark.World;
using UnityEngine;

namespace Mossmark.Development
{
    // Iteration 10: an unspecialized NPC develops via repeated attention. Each productive
    // tick is "spending time" with them - TimeCondition makes every tick productive
    // (RequiresDaylight, ContinueAttending) until PendingProgress reaches progressCost,
    // at which point TryApplyStage's existing random-among-available selection draws a
    // specialization from this universal track (interrupting the hold, per Development
    // Application's threshold-crossing rule).
    public class NpcAttendable : DevelopableEntity, IAttendable
    {
        [SerializeField] private string genericName = "Wanderer";
        [SerializeField, Min(1)] private int progressCost = 4;
        [SerializeField, Min(0.1f)] private float tickInterval = 0.5f;

        private DevelopmentTrack track;
        private SpriteRenderer spriteRenderer;
        private string specializedName;
        private Dictionary<string, (string Title, Color Tint)> specializationInfo;

        public override string DisplayName => CurrentStageIndex >= 0 ? specializedName : genericName;
        protected override DevelopmentTrack Track => track;

        public float AttentionDuration => tickInterval;
        public bool RequiresDaylight => LastAttentionMadeProgress;

        // A productive, non-applying tick continues the hold; the specialization-drawing
        // tick interrupts it, same shape as BuildingAttendable's revival tick.
        public bool ContinueAttending => LastAttentionMadeProgress && !LastAttentionAppliedStage;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();

            specializationInfo = new Dictionary<string, (string Title, Color Tint)>
            {
                ["forager"] = ("Forager", new Color(0.5f, 0.7f, 0.3f, 1f)),
                ["caretaker"] = ("Caretaker", new Color(0.8f, 0.6f, 0.8f, 1f)),
                ["tinkerer"] = ("Tinkerer", new Color(0.7f, 0.6f, 0.3f, 1f)),
                ["smith"] = ("Smith", new Color(0.75f, 0.4f, 0.25f, 1f)),
            };

            var stages = new List<DevelopmentStage>
            {
                new("forager", "Take up Foraging", progressCost, new TimeCondition(progressCost)),
                new("caretaker", "Take up Caretaking", progressCost, new TimeCondition(progressCost)),
                new("tinkerer", "Take up Tinkering", progressCost, new TimeCondition(progressCost)),
                // Only a candidate once a building declares demand for it (Iteration 11);
                // placed before the archetype-derived stages so GetNextStage() still
                // resolves to "forager" pre-specialization, keeping ticks 1-3's
                // productivity/overlay text unaffected by this gate.
                new("smith", "Take up Smithing", progressCost,
                    new SpecializationNeededCondition("smith"), new TimeCondition(progressCost)),
            };

            // Iteration 12: one additional stage per archetype selected for this session,
            // gated the same way as "smith" above - each is only a candidate once a
            // building declares demand for that archetype's specialization. This is how
            // "NPC specialization pools derive from [archetype] selection".
            foreach (var archetype in WorldGenerator.SelectedArchetypes)
            {
                if (string.IsNullOrEmpty(archetype.SpecializationId)) continue;

                stages.Add(new DevelopmentStage(archetype.SpecializationId, archetype.StageDisplayName, progressCost,
                    new SpecializationNeededCondition(archetype.SpecializationId), new TimeCondition(progressCost)));
                specializationInfo[archetype.SpecializationId] = (archetype.NpcTitle, archetype.NpcTint);
            }

            track = new DevelopmentTrack(stages.ToArray());

            OnDeveloped += HandleDeveloped;
        }

        // Nothing further to do once specialized - this prototype draws once, per the
        // "Specialized NPC's own further advancement: Unresolved" open question.
        public bool CanAttend() => CurrentStageIndex < 0;

        public string GetOverlayDescription() => DisplayName;

        public string GetOverlayInteractionLine() => CurrentStageIndex >= 0
            ? $"{specializedName} has found their place here."
            : GetNeedsOrDefault($"Hold E to spend time with {genericName} - they need more time to find their place");

        public void OnAttentionComplete() => ResolveAttention();

        public void OnAttentionCancelled()
        {
        }

        private void HandleDeveloped(DevelopmentStage stage)
        {
            if (specializationInfo.TryGetValue(stage.Id, out var info))
            {
                specializedName = $"{genericName} the {info.Title}";
                if (spriteRenderer != null) spriteRenderer.color = info.Tint;
            }
            else
            {
                specializedName = genericName;
            }
        }
    }
}
