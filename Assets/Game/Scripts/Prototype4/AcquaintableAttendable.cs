using System.Collections.Generic;
using Mossmark.Attention;
using Mossmark.Day;
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
    // building's repair stages. CurrentStageIndex is the acquaintance depth: -1 =
    // Unfamiliar, then one authored AcquaintanceStage per crossing. Deepening has ZERO
    // effect on the entity itself — the subject block below is fixed at scene load, and
    // VerifySubjectUnchanged() checks that explicitly after every attend.
    //
    // Iteration 4.7: crossings ripen instead of counting down. Each stage has a
    // minAttends floor (exact — gates can be exact) and a ripenChance that ramps per
    // qualifying attend past the floor, so the moment of recognition arrives somewhere
    // in a band rather than on a countable tick (outcomes shouldn't be exact). The
    // crossing still runs through TryApplyStage/OnDeveloped, so tint, pop, and the
    // overlay's upgrades list behave exactly as before; each stage's ProgressCost is 1
    // and progress is only added on the tick that crosses.
    //
    // Iteration 4.8: wary entities (oneQualifyingTickPerDay) accept a single ripening
    // attend per day, tracked against DayCycleManager.DayIndex — P2's Standing shape
    // aimed at trust, per-entity so no WorldSite is needed. Same-day repeats resolve as
    // flavor visits, surfaced descriptively via todaySpentLine, never as an instruction.
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
            [Min(1)] public int minAttends = 2;
            [Range(0.05f, 1f)] public float ripenChance = 0.5f;
            public string shortName;
            [TextArea] public string description;
            public Color tint = Color.white;
            public string interactionLine;
            public string[] attendFlavors;
            // Set true on the crossing — the quiet consequence hook (Iteration 4.9's
            // earned surface reads it). Empty = the crossing changes nothing elsewhere.
            public string worldStateFlag;
        }

        // Iteration 4.8: a wary entity accepts one ripening attend per day; repeats the
        // same day are visits. todaySpentLine surfaces that state in the overlay,
        // descriptively (the condition, never the remedy).
        [SerializeField] private bool oneQualifyingTickPerDay;
        [SerializeField] private string todaySpentLine;

        [SerializeField, Min(0.1f)] private float attendDuration = 2f;
        [SerializeField] private string[] unfamiliarAttendFlavors =
        {
            "You watch a while. The work goes on; nothing gives itself away.",
        };

        private DevelopmentTrack track;
        private string subjectFingerprint;

        // Ripening state for the current (next-uncrossed) stage. Not part of the
        // subject fingerprint — it belongs to the player's knowing, not to the subject.
        private int attendsTowardNextStage;
        private int lastQualifyingDayIndex = -1;

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

        private AcquaintanceStage NextStageDef =>
            CurrentStageIndex + 1 < acquaintanceStages.Length ? acquaintanceStages[CurrentStageIndex + 1] : null;

        private bool IsFullyAcquainted => NextStageDef == null;

        private static int TodayIndex => DayCycleManager.Instance != null ? DayCycleManager.Instance.DayIndex : 0;

        private bool SpentToday => oneQualifyingTickPerDay && lastQualifyingDayIndex == TodayIndex;

        public float AttentionDuration => attendDuration;

        // Every completed attention in this scene draws from the day — attention is the
        // day's clock (P2's premise), and finding out what something is costs the same
        // daylight as anything else. A constant is safe under both of AttentionManager's
        // reads, same reasoning as P3.
        public bool RequiresDaylight => true;

        // Deepening ticks repeat while held; the crossing tick ends the hold (the
        // standard interrupt rule). Wary entities end the hold after their one daily
        // ripening tick rather than bleeding daylight into same-day visits.
        public bool ContinueAttending => lastAttentionWasDeepening;

        private void Awake()
        {
            var stages = new DevelopmentStage[acquaintanceStages.Length];
            for (int i = 0; i < acquaintanceStages.Length; i++)
            {
                var stage = acquaintanceStages[i];
                stages[i] = new DevelopmentStage(stage.stageId, stage.stageDisplayName,
                    1, new AttentionCountCondition(), new InOrderCondition(i));
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
        // progress (CurrentStageIndex, ripening ticks) is deliberately NOT part of it —
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
        // Also the single place a crossing's quiet consequence fires (both the attend
        // path and the debug advance route through TryApplyStage -> OnDeveloped).
        private void HandleDeveloped(DevelopmentStage stage)
        {
            RefreshTint();
            attendsTowardNextStage = 0;

            var def = FindStageDef(stage.Id);
            if (def != null && !string.IsNullOrEmpty(def.worldStateFlag))
            {
                WorldState.SetFlag(def.worldStateFlag, true);
                Debug.Log($"{name}: WorldState flag '{def.worldStateFlag}' set.", this);
            }
        }

        private AcquaintanceStage FindStageDef(string stageId)
        {
            foreach (var stage in acquaintanceStages)
            {
                if (stage.stageId == stageId) return stage;
            }
            return null;
        }

        public bool CanAttend() => true;

        public string GetShortName() => DisplayName;

        public string GetOverlayDescription()
        {
            var stage = CurrentStage;
            var description = stage == null ? unfamiliarDescription : stage.description;

            // What they know of their own craft only becomes sayable at full
            // acquaintance — partway, you can see the skill but not name it.
            if (IsFullyAcquainted)
            {
                var seeded = GetSeededKnownProperties();
                if (seeded.Count > 0)
                {
                    description = $"{description} {seededKnowledgeLead} {JoinPhrases(seeded)}.";
                }
            }
            else if (SpentToday && !string.IsNullOrEmpty(todaySpentLine))
            {
                description = $"{description} {todaySpentLine}";
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

            if (IsFullyAcquainted || SpentToday)
            {
                PostAttendFlavor();
                RaiseProgressMade();
                VerifySubjectUnchanged();
                return;
            }

            if (oneQualifyingTickPerDay) lastQualifyingDayIndex = TodayIndex;
            attendsTowardNextStage++;

            if (RollCrossing(NextStageDef))
            {
                Cross();
            }
            else
            {
                // A ripening tick: the pulse marks that the attend landed; no counter
                // surfaces anywhere. Wary entities end the hold here (their day's
                // sitting is done); others keep deepening while held.
                Debug.Log($"{name}: deepening (attend {attendsTowardNextStage} at this depth).", this);
                RaiseProgressMade();
                lastAttentionWasDeepening = !oneQualifyingTickPerDay;
            }

            VerifySubjectUnchanged();
        }

        // The 4.7 ripening roll: nothing before the floor, then a chance that ramps by
        // ripenChance per attend past it — the crossing arrives in a band, uncountable
        // but bounded below by authored pacing.
        private bool RollCrossing(AcquaintanceStage stage)
        {
            int pastFloor = attendsTowardNextStage - stage.minAttends;
            if (pastFloor < 0) return false;

            float chance = stage.ripenChance * (pastFloor + 1);
            return Random.value < chance;
        }

        private void Cross()
        {
            AddProgress();
            if (!TryApplyStage()) return;

            // Mirrors ResolveAttention's stage-cross surface, which this path bypasses:
            // DisplayName is read after the cross, so the name arrives with the moment.
            Debug.Log($"{DisplayName}: developed - {LastAppliedStage.DisplayName}!", this);
            Visuals.NotificationManager.Post($"{DisplayName}: {LastAppliedStage.DisplayName}");
        }

        public void OnAttentionCancelled() { }

        // Editor test driver shortcut (Prototype4Debug): force the next acquaintance
        // stage across without spending play-mode attends.
        public void DebugAdvanceAcquaintance()
        {
            if (IsFullyAcquainted)
            {
                Debug.Log($"{name}: already fully acquainted.", this);
                return;
            }

            AddProgress();
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

        // Acquaintance is strictly sequential — you can't be Known before you're
        // Acquainted. Pre-4.7 the cumulative attendsCost values enforced this by
        // accident (a deeper stage was never affordable first); with every stage at
        // ProgressCost 1, TryApplyStage's random draw among available stages needs the
        // order made structural or the first crossing can land on any stage.
        private class InOrderCondition : IDependencyCondition
        {
            private readonly int stageIndex;

            public InOrderCondition(int stageIndex)
            {
                this.stageIndex = stageIndex;
            }

            public bool IsSatisfied(DevelopableEntity entity) => entity.CurrentStageIndex == stageIndex - 1;

            public string GetNeedsDescription(DevelopableEntity entity) => "wants only more of your attention";
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
