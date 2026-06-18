using System.Collections.Generic;
using Mossmark.Attention;
using Mossmark.World;
using UnityEngine;

namespace Mossmark.Development
{
    // Iteration 10: an unspecialized NPC develops via repeated attention. Each productive
    // tick is "spending time" with them - TimeCondition makes every tick productive until
    // PendingProgress reaches progressCost, at which point TryApplyStage's random-among-
    // available selection draws a specialization (interrupting the hold).
    //
    // Iteration 17: two archetype specializations (hedge_witch, bog_keeper) have post-spec
    // development stages gated by item availability; pool-sealing lets GetNextStage() reach
    // those stages after the spec draws.
    //
    // Content pass: "smith" removed from universal pool (folded into Old Quarry archetype
    // as "stonemason"). progressCost raised to 8 to give the player time to set up a
    // building direction before specialization fires. Herald archetype gains post-spec stages.
    public class NpcAttendable : DevelopableEntity, IAttendable
    {
        [SerializeField] private string genericName = "Wanderer";
        [SerializeField, Min(1)] private int progressCost = 8;
        [SerializeField, Min(0.1f)] private float minTickInterval = 1f;
        [SerializeField, Min(0.1f)] private float maxTickInterval = 1.5f;

        private DevelopmentTrack track;
        private SpriteRenderer spriteRenderer;
        private string specializedName;
        private string drawnSpecializationId;
        private List<string> poolStageIds;
        private Dictionary<string, List<string>> postSpecStageIdsBySpecId;
        private Dictionary<string, (string Title, Color Tint)> specializationInfo;
        private float currentTickInterval;

        public override string DisplayName => drawnSpecializationId != null ? specializedName : genericName;
        protected override DevelopmentTrack Track => track;

        public float AttentionDuration => currentTickInterval;
        public bool RequiresDaylight => LastAttentionMadeProgress;
        public bool ContinueAttending => LastAttentionMadeProgress && !LastAttentionAppliedStage;

        private float RollTickInterval() => Random.Range(minTickInterval, maxTickInterval);

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            currentTickInterval = RollTickInterval();

            specializationInfo = new Dictionary<string, (string Title, Color Tint)>
            {
                ["forager"]   = ("Forager",   new Color(0.5f, 0.7f, 0.3f, 1f)),
                ["caretaker"] = ("Caretaker", new Color(0.8f, 0.6f, 0.8f, 1f)),
                ["tinkerer"]  = ("Tinkerer",  new Color(0.7f, 0.6f, 0.3f, 1f)),
            };

            // Universal pool — available in any session, no building dep required.
            var stages = new List<DevelopmentStage>
            {
                new("forager",   "Take up Foraging",   progressCost, new TimeCondition(progressCost)),
                new("caretaker", "Take up Caretaking", progressCost, new TimeCondition(progressCost)),
                new("tinkerer",  "Take up Tinkering",  progressCost, new TimeCondition(progressCost)),
            };

            poolStageIds = new List<string> { "forager", "caretaker", "tinkerer" };

            // Archetype-specific pool stages added for each selected archetype. Gated by
            // SpecializationNeededCondition so they only become candidates once the matching
            // building declares demand. Same progressCost as universal stages so TryApplyStage's
            // priority-filter (needed-stage wins over universal) fires at the same threshold.
            foreach (var archetype in WorldGenerator.SelectedArchetypes)
            {
                if (string.IsNullOrEmpty(archetype.SpecializationId)) continue;

                stages.Add(new DevelopmentStage(archetype.SpecializationId, archetype.StageDisplayName, progressCost,
                    new SpecializationNeededCondition(archetype.SpecializationId),
                    new TimeCondition(progressCost)));
                poolStageIds.Add(archetype.SpecializationId);
                specializationInfo[archetype.SpecializationId] = (archetype.NpcTitle, archetype.NpcTint);
            }

            // Post-spec stages appended after all pool stages so GetNextStage() still
            // resolves to "forager" pre-specialization (ticks 1–(progressCost-1) unaffected).
            postSpecStageIdsBySpecId = new Dictionary<string, List<string>>();
            foreach (var archetype in WorldGenerator.SelectedArchetypes)
            {
                var postStages = BuildPostSpecStages(archetype);
                if (postStages.Count == 0) continue;

                stages.AddRange(postStages);
                postSpecStageIdsBySpecId[archetype.SpecializationId] = postStages.ConvertAll(s => s.Id);
            }

            track = new DevelopmentTrack(stages.ToArray());

            // Pre-seal all post-spec stages so they can't fire before this NPC has
            // drawn the matching specialization. SpecializationRealizedCondition is
            // global (static), so without this seal a second NPC would have its
            // post-spec stages become candidates the moment the first NPC specializes.
            foreach (var kvp in postSpecStageIdsBySpecId)
                foreach (var stageId in kvp.Value)
                    MarkStageAsApplied(stageId);

            OnDeveloped += HandleDeveloped;
        }

        public bool CanAttend()
        {
            if (drawnSpecializationId == null) return true;
            return CanMakeProgress();
        }

        public string GetOverlayDescription() => DisplayName;

        public string GetOverlayInteractionLine()
        {
            if (drawnSpecializationId == null)
                return GetNeedsOrDefault($"Hold E to spend time with {genericName} - they need more time to find their place");

            if (GetNextStage() == null)
                return $"{specializedName} has found their place here.";

            return GetNeedsOrDefault($"Hold E to help {specializedName} develop their craft further");
        }

        public void OnAttentionComplete()
        {
            ResolveAttention();
            currentTickInterval = RollTickInterval();
        }

        public void OnAttentionCancelled() { }

        private void HandleDeveloped(DevelopmentStage stage)
        {
            // --- Specialization draw ---
            if (specializationInfo.TryGetValue(stage.Id, out var info))
            {
                drawnSpecializationId = stage.Id;
                specializedName = $"{genericName} the {info.Title}";
                if (spriteRenderer != null) spriteRenderer.color = info.Tint;
                RealizedSpecializations.Declare(stage.Id);

                foreach (var id in poolStageIds)
                    if (id != stage.Id) MarkStageAsApplied(id);

                foreach (var kvp in postSpecStageIdsBySpecId)
                    if (kvp.Key != stage.Id)
                        foreach (var stageId in kvp.Value)
                            MarkStageAsApplied(stageId);

                // Unseal this NPC's own post-spec stages now that specialization has
                // fired. They were pre-sealed in Awake to prevent cross-NPC contamination
                // (SpecializationRealizedCondition is global).
                if (postSpecStageIdsBySpecId.TryGetValue(stage.Id, out var ownPostStageIds))
                    foreach (var stageId in ownPostStageIds)
                        MarkStageAsAvailable(stageId);

                return;
            }

            // --- Post-specialization stage ---
            WorldState.SetFlag(stage.Id, true);
            LogPostSpecEffect(stage);
        }

        private void LogPostSpecEffect(DevelopmentStage stage)
        {
            var message = stage.Id switch
            {
                "hedge_witch_wound_lore" =>
                    $"{specializedName}: learns Wound Lore - healing herbs ease the hurt of bad encounters.",
                "hedge_witch_ravens_eye" =>
                    $"{specializedName}: learns Raven's Eye Reading - the intent of wandering things becomes clearer before you approach.",
                "bog_keeper_drainage" =>
                    $"{specializedName}: clears Drainage Channels - the fen's margins grow more passable.",
                "bog_keeper_iron_sense" =>
                    $"{specializedName}: develops Iron-Sense - bog iron deposits reveal themselves more readily.",
                "herald_trail_markers" =>
                    $"{specializedName}: marks key paths and crossroads - travelers find the road safer.",
                "herald_toll_records" =>
                    $"{specializedName}: establishes toll records - the settlement's standing on the road grows.",
                _ => $"{specializedName}: {stage.DisplayName}.",
            };
            Debug.Log(message, this);
        }

        private List<DevelopmentStage> BuildPostSpecStages(PlaceArchetype archetype)
        {
            var specId = archetype.SpecializationId;
            var item1 = archetype.CommonYields?.Length > 0 ? archetype.CommonYields[0].Item : null;
            var item2 = archetype.RareYield?.Item;

            var stages = new List<DevelopmentStage>();
            if (item1 == null) return stages;

            switch (specId)
            {
                case "hedge_witch":
                    stages.Add(new DevelopmentStage("hedge_witch_wound_lore", "Wound Lore", 3,
                        new SpecializationRealizedCondition(specId, $"needs a {archetype.NpcTitle} in town"),
                        new ItemAvailableCondition(item1, 2)));
                    if (item2 != null)
                        stages.Add(new DevelopmentStage("hedge_witch_ravens_eye", "Raven's Eye Reading", 4,
                            new SpecializationRealizedCondition(specId, $"needs a {archetype.NpcTitle} in town"),
                            new ItemAvailableCondition(item2, 2)));
                    break;

                case "bog_keeper":
                    stages.Add(new DevelopmentStage("bog_keeper_drainage", "Drainage Channels", 3,
                        new SpecializationRealizedCondition(specId, $"needs a {archetype.NpcTitle} in town"),
                        new ItemAvailableCondition(item1, 3)));
                    if (item2 != null)
                        stages.Add(new DevelopmentStage("bog_keeper_iron_sense", "Iron-Sense", 4,
                            new SpecializationRealizedCondition(specId, $"needs a {archetype.NpcTitle} in town"),
                            new ItemAvailableCondition(item2, 2)));
                    break;

                case "herald":
                    // Trail Markers: needs flint (wilderness common) - something gathered early.
                    stages.Add(new DevelopmentStage("herald_trail_markers", "Trail Markers", 3,
                        new SpecializationRealizedCondition(specId, $"needs a {archetype.NpcTitle} in town"),
                        new ItemAvailableCondition(item1, 3)));
                    // Toll Records: needs old coin (wilderness rare / POI common) - acquired
                    // via the Old Road Checkpoint once the POI opens.
                    if (item2 != null)
                        stages.Add(new DevelopmentStage("herald_toll_records", "Toll Records", 4,
                            new SpecializationRealizedCondition(specId, $"needs a {archetype.NpcTitle} in town"),
                            new ItemAvailableCondition(item2, 2)));
                    break;
            }

            return stages;
        }
    }
}
