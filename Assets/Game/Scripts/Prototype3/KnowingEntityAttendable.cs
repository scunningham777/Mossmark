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
        [SerializeField] private string teachablePropertyId = "turns_water";
        [SerializeField] private string teachLine = "They listen, still for a moment. Something settles into place.";

        [Header("Taught-Knowledge Branch (Iteration 3.4)")]
        [SerializeField] private string stageId = "clay_lined_pit";
        [SerializeField] private string stageDisplayName = "Clay-Lined Steeping Pit";
        [SerializeField, Min(1)] private int stageProgressCost = 2;
        [SerializeField] private string stageNeedsLine = "The pit will not hold water; they seem resigned to it.";
        [SerializeField] private string developedDescription = "The steeping pit sits dark and full. Dye takes slowly now, and deep.";
        [SerializeField] private string developedInteractionLine = "Hold E to watch the colors take";
        [SerializeField] private Color developedTint = new Color(0.9f, 0.42f, 0.38f);
        [SerializeField] private Color untaughtTint = new Color(0.55f, 0.55f, 0.55f);
        [SerializeField] private Color knowingTint = new Color(0.78f, 0.5f, 0.38f);
        [SerializeField, Min(0.1f)] private float attendDuration = 2f;
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

        // Teaching resolves as a one-shot, not a hold: AttentionManager completes a
        // zero-duration attention on the same frame the hold starts, which is what makes
        // the moment read as deliberate speech rather than sustained work.
        public float AttentionDuration => TeachPending ? 0f : attendDuration;

        // Iteration 3.5: every completed attention here — visit, teach, or development
        // tick — draws from the day. Attention is the day's clock (P2's premise); a
        // constant true is safe under both of AttentionManager's reads (the pre-start
        // gate and the post-complete spend).
        public bool RequiresDaylight => true;

        // Development ticks repeat while held (the stage-crossing tick ends the hold via
        // CanMakeProgress turning false); visits and teaching stay one-shot.
        public bool ContinueAttending => lastAttentionWasDevelopment && CanMakeProgress();

        // The player knows the teachable property (from any source) and this entity
        // doesn't yet. Knowledge is the gate — nothing is carried, delivered, or consumed.
        public bool TeachPending =>
            WorldContext.IsPropertyKnown(playerKnowerId, teachablePropertyId) && !Knows(teachablePropertyId);

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

        // The one authored branch this prototype exists to test: a single stage that
        // becomes reachable only once the entity knows the taught property.
        protected virtual DevelopmentTrack BuildTrack() => new DevelopmentTrack(
            new DevelopmentStage(stageId, stageDisplayName, stageProgressCost,
                new KnownPropertyCondition(teachablePropertyId, stageNeedsLine)));

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

            if (CurrentStageIndex >= 0)
            {
                spriteRenderer.color = developedTint;
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
            var lead = CurrentStageIndex >= 0 ? developedDescription : baseDescription;

            var description = lead;
            var known = GetKnownProperties();
            if (known.Count > 0)
            {
                description = $"{lead} They speak of {JoinPhrases(known)}.";
            }

            // The resolver's "needs" surface, kept descriptive rather than instructive:
            // the blockage is stated, never the remedy.
            var nextStage = GetNextStage();
            if (nextStage != null && !nextStage.AreDependenciesSatisfied(this))
            {
                description = $"{description} {stageNeedsLine}";
            }

            return description;
        }

        public virtual string GetOverlayInteractionLine()
        {
            if (TeachPending)
            {
                var property = PropertyRegistry.GetById(teachablePropertyId);
                var phrase = property != null ? property.Phrase : teachablePropertyId;
                return $"Press E to speak of what {phrase}";
            }

            if (CurrentStageIndex >= 0) return developedInteractionLine;
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
            PropertyKnowledge.MarkKnown(entityId, teachablePropertyId);
            RefreshTint();

            var feedback = GetComponent<Visuals.EntityFeedback>();
            if (feedback != null) feedback.TriggerPop();

            Visuals.NotificationManager.Post(teachLine);
            Debug.Log($"{displayName}: taught '{teachablePropertyId}'.", this);
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
