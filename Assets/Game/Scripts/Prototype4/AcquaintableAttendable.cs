using System.Collections.Generic;
using Mossmark.Attention;
using Mossmark.Development;
using Mossmark.Inventory;
using UnityEngine;

namespace Mossmark.Prototype4
{
    // Prototype 4 (Acquaintance) test entity: something that already has a history and a
    // current state, seeded at scene load (Iteration 49's mid-process-start instinct, same
    // as P3's Dyer) — but the player starts ignorant of it. What the overlay shows is a
    // function of how well the player knows the thing, not of the thing itself; nothing
    // the player does here changes the entity's own state (P4 is one-directional:
    // world -> player).
    //
    // Iteration 4.2: acquaintance IS a DevelopmentTrack — the same machinery as a
    // building's repair stages, gated on AttentionCountCondition (always satisfied; each
    // stage's attendsCost is the threshold). CurrentStageIndex is the acquaintance depth:
    // -1 = Unfamiliar, then one authored AcquaintanceStage per crossing. Deepening has
    // ZERO effect on the entity itself — the subject block below is fixed at scene load,
    // and VerifySubjectUnchanged() checks that explicitly after every attend rather than
    // assuming it.
    public class AcquaintableAttendable : DevelopableEntity, IAttendable
    {
        [SerializeField] private string entityId = "p4_netmender";

        // The veil (acquaintance depth -1): what an arriving stranger can see from the
        // path. Must stay silhouette-level — no craft, no name, no want.
        [SerializeField] private string unfamiliarShortName = "Someone";
        [SerializeField, TextArea] private string unfamiliarDescription =
            "Someone bent over work you can't make out, their back half to the path.";
        [SerializeField] private Color unfamiliarTint = new Color(0.45f, 0.45f, 0.45f);
        [SerializeField] private string unfamiliarInteractionLine = "Hold E to watch a while";

        // The subject's true current state, fixed at scene load and never modified by
        // anything in this prototype. Seeded knowledge lives in PropertyKnowledge keyed
        // by entityId (exactly like P3's Dyer); trueStateNote is the functional-state
        // fingerprint the zero-effect check logs against — it describes what the entity
        // is up to independent of the player, and acquaintance only reveals it.
        [SerializeField] private string[] seededPropertyIds = { "binds_fast" };

        // Iteration 4.4's re-skin finding, applied: the mechanism is one meter for every
        // entity type; the only thing that genuinely needed re-skinning is language, and
        // all of it is authored data. This lead phrase is the one line code used to
        // hardcode person-shaped ("They speak of what binds fast" doesn't fit a
        // smokehouse; "The place is given to" does).
        [SerializeField] private string seededKnowledgeLead = "They speak of";
        [SerializeField, TextArea] private string trueStateNote =
            "mends eel traps for three households; hands slowing; teaching no one";

        // One entry per acquaintance depth past Unfamiliar. Each is a strictly richer
        // read of the same fixed truth, not new truth invented per stage. The last
        // entry is full acquaintance — its read is the entity's actual current state.
        [SerializeField] private AcquaintanceStage[] acquaintanceStages;

        [System.Serializable]
        public class AcquaintanceStage
        {
            public string stageId;
            public string stageDisplayName;
            [Min(1)] public int attendsCost = 3;
            public string shortName;
            [TextArea] public string description;
            public Color tint = Color.white;
            public string interactionLine;
            public string[] attendFlavors;
        }

        [SerializeField, Min(0.1f)] private float attendDuration = 2f;
        [SerializeField] private string[] unfamiliarAttendFlavors =
        {
            "You watch a while. The work goes on; nothing gives itself away.",
        };

        private DevelopmentTrack track;
        private string subjectFingerprint;

        // Read-after-complete latch, same rule as everywhere else: set in
        // OnAttentionComplete, read by ContinueAttending.
        private bool lastAttentionWasDeepening;

        public string EntityId => entityId;
        public override string DisplayName => CurrentStage != null ? CurrentStage.shortName : unfamiliarShortName;
        protected override DevelopmentTrack Track => track;

        private AcquaintanceStage CurrentStage =>
            CurrentStageIndex >= 0 && CurrentStageIndex < acquaintanceStages.Length
                ? acquaintanceStages[CurrentStageIndex]
                : null;

        public float AttentionDuration => attendDuration;

        // Every completed attention in this scene draws from the day — attention is the
        // day's clock (P2's premise), and finding out what something is costs the same
        // daylight as anything else. A constant is safe under both of AttentionManager's
        // reads, same reasoning as P3.
        public bool RequiresDaylight => true;

        // Deepening ticks repeat while held (the stage-crossing tick ends the hold, same
        // interrupt rule as development everywhere); visits at full acquaintance are
        // one-shot check-ins.
        public bool ContinueAttending => lastAttentionWasDeepening && CanMakeProgress();

        private void Awake()
        {
            var stages = new DevelopmentStage[acquaintanceStages.Length];
            for (int i = 0; i < acquaintanceStages.Length; i++)
            {
                var stage = acquaintanceStages[i];
                stages[i] = new DevelopmentStage(stage.stageId, stage.stageDisplayName,
                    stage.attendsCost, new AttentionCountCondition());
            }
            track = new DevelopmentTrack(stages);
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
            subjectFingerprint = ComputeSubjectFingerprint();
        }

        public bool KnowsOfSelf(string propertyId) => WorldContext.IsPropertyKnown(entityId, propertyId);

        private List<PropertyDefinition> GetSeededKnownProperties()
        {
            var result = new List<PropertyDefinition>();
            foreach (var property in PropertyRegistry.All)
            {
                if (KnowsOfSelf(property.Id)) result.Add(property);
            }
            return result;
        }

        // The 4.2 zero-effect check, made explicit rather than assumed: everything that
        // constitutes the subject's own state, in one comparable string. Acquaintance
        // progress (CurrentStageIndex/PendingProgress) is deliberately NOT part of it —
        // that state belongs to the player's knowing, not to the subject.
        public string ComputeSubjectFingerprint()
        {
            var knownIds = new List<string>();
            foreach (var property in PropertyRegistry.All)
            {
                if (KnowsOfSelf(property.Id)) knownIds.Add(property.Id);
            }
            return $"trueState=\"{trueStateNote}\" knows=[{string.Join(", ", knownIds)}]";
        }

        private void VerifySubjectUnchanged()
        {
            var current = ComputeSubjectFingerprint();
            if (current != subjectFingerprint)
            {
                Debug.LogError($"{name}: subject state CHANGED as a side effect of attention — " +
                    $"was ({subjectFingerprint}), now ({current}). Acquaintance must be zero-effect.", this);
            }
        }

        // Baked triangle stays white; the renderer tint carries the read. Unfamiliar is
        // deliberately drab — color is something you only see in a thing once you know it.
        private void RefreshTint()
        {
            var spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null) return;
            spriteRenderer.color = CurrentStage != null ? CurrentStage.tint : unfamiliarTint;
        }

        // Runs before EntityFeedback's stage-cross handler (Awake subscription order), so
        // the circle swap picks up the post-stage tint — same ordering note as P3.
        private void HandleDeveloped(DevelopmentStage stage)
        {
            RefreshTint();
        }

        public bool CanAttend() => true;

        public string GetShortName() => DisplayName;

        public string GetOverlayDescription()
        {
            var stage = CurrentStage;
            if (stage == null) return unfamiliarDescription;

            var description = stage.description;

            // What they know of their own craft only becomes sayable at full
            // acquaintance — partway, you can see the skill but not name it.
            if (!CanMakeProgress())
            {
                var seeded = GetSeededKnownProperties();
                if (seeded.Count > 0)
                {
                    description = $"{description} {seededKnowledgeLead} {JoinPhrases(seeded)}.";
                }
            }

            return description;
        }

        public string GetOverlayInteractionLine()
        {
            var stage = CurrentStage;
            if (stage != null && !string.IsNullOrEmpty(stage.interactionLine)) return stage.interactionLine;
            return unfamiliarInteractionLine;
        }

        public IReadOnlyList<string> GetAppliedUpgrades() => GetAppliedUpgradeNames();

        public void OnAttentionComplete()
        {
            lastAttentionWasDeepening = false;

            if (CanMakeProgress())
            {
                // The whole mechanic: one attend = one progress on the acquaintance
                // track, and nothing else. ResolveAttention's own stage-cross
                // notification uses the post-cross DisplayName, so the moment reads
                // "The Netmender: Acquainted" — the name arriving with the crossing.
                ResolveAttention();
                lastAttentionWasDeepening = LastAttentionMadeProgress && !LastAttentionAppliedStage;
            }
            else
            {
                // Fully acquainted: attending is a visit. Flavor keeps the entity alive
                // to be around; the pulse marks that the attend landed.
                PostAttendFlavor();
                RaiseProgressMade();
            }

            VerifySubjectUnchanged();
        }

        public void OnAttentionCancelled() { }

        // Editor test driver shortcut (Prototype4Debug): force the next acquaintance
        // stage across without spending play-mode attends.
        public void DebugAdvanceAcquaintance()
        {
            if (!CanMakeProgress())
            {
                Debug.Log($"{name}: already fully acquainted.", this);
                return;
            }

            var next = GetNextStage();
            int needed = Mathf.Max(0, next.ProgressCost - PendingProgress);
            if (needed > 0) AddProgress(needed);
            TryApplyStage();
            VerifySubjectUnchanged();
        }

        private void PostAttendFlavor()
        {
            var stage = CurrentStage;
            var flavors = stage != null && stage.attendFlavors != null && stage.attendFlavors.Length > 0
                ? stage.attendFlavors
                : unfamiliarAttendFlavors;
            if (flavors.Length == 0) return;
            Visuals.NotificationManager.Post(flavors[Random.Range(0, flavors.Length)]);
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
