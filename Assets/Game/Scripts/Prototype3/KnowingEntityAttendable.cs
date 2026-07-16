using System.Collections.Generic;
using Mossmark.Attention;
using Mossmark.Development;
using Mossmark.Inventory;
using UnityEngine;

namespace Mossmark.Prototype3
{
    // Prototype 3 (Knowledge Spine) test entity: a person whose appearance and behavior
    // are a function of which folk-phrase properties they know. Knowledge lives in
    // PropertyKnowledge keyed by entityId — the same (subject, property) store the
    // player's item discoveries use, aimed at a knower instead of an item. Seeding at
    // spawn is Iteration 49's mid-process-start instinct applied to knowledge instead
    // of a progress counter.
    public class KnowingEntityAttendable : DevelopableEntity, IAttendable
    {
        [SerializeField] private string entityId = "p3_dyer";
        [SerializeField] private string displayName = "The Dyer";
        [SerializeField] private string baseDescription = "A dyer, working a cold steeping pit at the fen's edge.";
        [SerializeField] private string[] seededPropertyIds = { "draws_the_eye" };
        [SerializeField] private string playerKnowerId = "p3_player";

        // Iteration 3.4/3.8: a short authored list rather than one hardcoded pairing.
        // Each want is independently gated — KnownPropertyCondition doesn't order them,
        // so if more than one becomes satisfied at once, DevelopableEntity's own
        // available-stage draw (used everywhere else in the game) picks between them.
        // That's a deliberate reuse, not an oversight: this entity earns no special
        // sequencing the rest of the game doesn't already have.
        [SerializeField] private TeachableWant[] teachableWants =
        {
            new TeachableWant
            {
                propertyId = "turns_water",
                teachLine = "They listen, still for a moment. Something settles into place.",
                stageId = "clay_lined_pit",
                stageDisplayName = "Clay-Lined Steeping Pit",
                stageProgressCost = 2,
                stageNeedsLine = "The pit will not hold water; they seem resigned to it.",
                developedDescription = "The steeping pit sits dark and full. Dye takes slowly now, and deep.",
                developedInteractionLine = "Hold E to watch the colors take",
                developedTint = new Color(0.9f, 0.42f, 0.38f),
            },
            new TeachableWant
            {
                propertyId = "binds_fast",
                teachLine = "They go still, turning the thought over — then nod, once, like something's decided.",
                stageId = "bound_colors",
                stageDisplayName = "Colors That Hold",
                stageProgressCost = 2,
                stageNeedsLine = "The color takes, but washing lifts it; something's missing to bind it fast.",
                developedDescription = "The steeping pit sits dark and full, and the color holds now — washing will not lift it.",
                developedInteractionLine = "Hold E to watch the colors hold",
                developedTint = new Color(0.55f, 0.3f, 0.62f),
            },
        };

        [System.Serializable]
        private class TeachableWant
        {
            public string propertyId;
            public string teachLine;
            public string stageId;
            public string stageDisplayName;
            [Min(1)] public int stageProgressCost = 2;
            public string stageNeedsLine;
            public string developedDescription;
            public string developedInteractionLine;
            public Color developedTint = Color.white;
        }

        [SerializeField] private Color untaughtTint = new Color(0.55f, 0.55f, 0.55f);
        [SerializeField] private Color knowingTint = new Color(0.78f, 0.5f, 0.38f);
        [SerializeField, Min(0.1f)] private float attendDuration = 2f;
        [SerializeField, Min(0.1f)] private float teachDuration = 0.6f;
        [SerializeField] private string[] visitFlavors =
        {
            "They work in silence a while, hands stained to the wrist.",
            "They straighten, glance at the pale sky, and bend back to the pit.",
            "They turn a hank of wool over, unhappy with something about it.",
        };

        private DevelopmentTrack track;

        public string EntityId => entityId;
        public override string DisplayName => displayName;
        protected override DevelopmentTrack Track => track;

        // Set in OnAttentionComplete when the tick resolved through the development
        // track (not a visit or teach); read afterwards by ContinueAttending, same
        // read-after-complete latch rule as everywhere else.
        private bool lastAttentionWasDevelopment;

        // Teaching still resolves as a one-shot (ContinueAttending stays false below),
        // but now runs the same short hold-to-complete timer as every other attendable
        // rather than firing on the press frame — a zero-duration teach read as
        // instantaneous next to the rest of the game's tending, which broke the "tried,
        // not chosen" feel of sustained attention. teachDuration is deliberately shorter
        // than attendDuration: brief, but still felt.
        public float AttentionDuration => GetPendingWant() != null ? teachDuration : attendDuration;

        // Iteration 3.5: every completed attention here — visit, teach, or development
        // tick — draws from the day. Attention is the day's clock (P2's premise); a
        // constant true is safe under both of AttentionManager's reads (the pre-start
        // gate and the post-complete spend).
        public bool RequiresDaylight => true;

        // Development ticks repeat while held (the stage-crossing tick ends the hold via
        // CanMakeProgress turning false); visits and teaching stay one-shot.
        public bool ContinueAttending => lastAttentionWasDevelopment && CanMakeProgress();

        // True if any want has the player knowing a property this entity doesn't yet.
        // Knowledge is the gate — nothing is carried, delivered, or consumed.
        public bool TeachPending => GetPendingWant() != null;

        // First pending want in authored order — deterministic, same "earliest in the
        // list wins" rule GetNextStage() already uses for the development track.
        private TeachableWant GetPendingWant()
        {
            foreach (var want in teachableWants)
            {
                if (WorldContext.IsPropertyKnown(playerKnowerId, want.propertyId) && !Knows(want.propertyId))
                    return want;
            }
            return null;
        }

        private TeachableWant FindWant(string stageId)
        {
            foreach (var want in teachableWants)
            {
                if (want.stageId == stageId) return want;
            }
            return null;
        }

        // The most recently applied stage's want, if any — drives tint/description once
        // developed. "Most recent" rather than "first in list" because TryApplyStage()
        // can draw either stage once both are simultaneously available (see the comment
        // on teachableWants above).
        private TeachableWant CurrentDevelopedWant =>
            CurrentStageIndex >= 0 && LastAppliedStage != null ? FindWant(LastAppliedStage.Id) : null;

        protected virtual void Awake()
        {
            track = BuildTrack();
            OnDeveloped += HandleDeveloped;

            foreach (var propertyId in seededPropertyIds)
            {
                PropertyKnowledge.MarkKnown(entityId, propertyId);
            }
        }

        private void OnDestroy()
        {
            OnDeveloped -= HandleDeveloped;
        }

        private void Start()
        {
            RefreshTint();
        }

        // The authored branch(es) this prototype exists to test: one stage per want,
        // each reachable only once the entity knows that want's taught property.
        protected virtual DevelopmentTrack BuildTrack()
        {
            var stages = new DevelopmentStage[teachableWants.Length];
            for (int i = 0; i < teachableWants.Length; i++)
            {
                var want = teachableWants[i];
                stages[i] = new DevelopmentStage(want.stageId, want.stageDisplayName, want.stageProgressCost,
                    new KnownPropertyCondition(want.propertyId, want.stageNeedsLine));
            }
            return new DevelopmentTrack(stages);
        }

        // Runs before EntityFeedback's stage-cross handler (Awake subscription order),
        // so the circle swap picks up the post-stage tint.
        private void HandleDeveloped(DevelopmentStage stage)
        {
            RefreshTint();
        }

        public bool Knows(string propertyId) => WorldContext.IsPropertyKnown(entityId, propertyId);

        public List<PropertyDefinition> GetKnownProperties()
        {
            var known = new List<PropertyDefinition>();
            foreach (var property in PropertyRegistry.All)
            {
                if (Knows(property.Id)) known.Add(property);
            }
            return known;
        }

        // Baked triangle stays white; the renderer tint carries the knowledge read.
        // Felt, not read: a person who knows something of their craft has color in them,
        // and one whose knowledge has changed their work carries it deeper still.
        protected void RefreshTint()
        {
            var spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null) return;

            var developedWant = CurrentDevelopedWant;
            if (developedWant != null)
            {
                spriteRenderer.color = developedWant.developedTint;
            }
            else
            {
                spriteRenderer.color = GetKnownProperties().Count > 0 ? knowingTint : untaughtTint;
            }
        }

        public virtual bool CanAttend() => true;

        public string GetShortName() => displayName;

        public virtual string GetOverlayDescription()
        {
            var developedWant = CurrentDevelopedWant;
            var lead = developedWant != null ? developedWant.developedDescription : baseDescription;

            var description = lead;
            var known = GetKnownProperties();
            if (known.Count > 0)
            {
                description = $"{lead} They speak of {JoinPhrases(known)}.";
            }

            // The resolver's "needs" surface, kept descriptive rather than instructive:
            // the blockage is stated, never the remedy. Reuses the stage's own condition
            // text rather than re-deriving it from the want list.
            var nextStage = GetNextStage();
            if (nextStage != null && !nextStage.AreDependenciesSatisfied(this))
            {
                description = $"{description} {nextStage.GetUnsatisfiedNeedsDescription(this)}";
            }

            return description;
        }

        public virtual string GetOverlayInteractionLine()
        {
            var pendingWant = GetPendingWant();
            if (pendingWant != null)
            {
                var property = PropertyRegistry.GetById(pendingWant.propertyId);
                var phrase = property != null ? property.Phrase : pendingWant.propertyId;
                return $"Hold E to speak of what {phrase}";
            }

            var developedWant = CurrentDevelopedWant;
            if (developedWant != null) return developedWant.developedInteractionLine;
            if (CanMakeProgress()) return "Hold E to work alongside them";

            return "Hold E to spend a while with them";
        }

        public IReadOnlyList<string> GetAppliedUpgrades() => GetAppliedUpgradeNames();

        public virtual void OnAttentionComplete()
        {
            lastAttentionWasDevelopment = false;

            if (TeachPending)
            {
                Teach();
                return;
            }

            if (CanMakeProgress())
            {
                ResolveAttention();
                lastAttentionWasDevelopment = LastAttentionMadeProgress && !LastAttentionAppliedStage;
                return;
            }

            Visit();
        }

        private void Teach()
        {
            var want = GetPendingWant();
            if (want == null) return;

            PropertyKnowledge.MarkKnown(entityId, want.propertyId);
            RefreshTint();

            var feedback = GetComponent<Visuals.EntityFeedback>();
            if (feedback != null) feedback.TriggerPop();

            Visuals.NotificationManager.Post(want.teachLine);
            Debug.Log($"{displayName}: taught '{want.propertyId}'.", this);
        }

        public void OnAttentionCancelled() { }

        protected void Visit()
        {
            if (visitFlavors.Length > 0)
            {
                Debug.Log($"{displayName}: {visitFlavors[Random.Range(0, visitFlavors.Length)]}", this);
            }

            RaiseProgressMade();
        }

        private static string JoinPhrases(List<PropertyDefinition> properties)
        {
            var phrases = new string[properties.Count];
            for (int i = 0; i < properties.Count; i++)
            {
                phrases[i] = $"what {properties[i].Phrase}";
            }
            return string.Join(", and of ", phrases);
        }
    }
}
