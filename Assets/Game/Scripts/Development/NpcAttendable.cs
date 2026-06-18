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
    // Iteration 17: post-spec development stages gated by item availability; pool-sealing
    // lets GetNextStage() reach those stages after the spec draws.
    //
    // Iteration 22 (G3): BuildPostSpecStages is now a generic loop over
    // archetype.NpcPostSpecStages — the hardcoded per-archetype switch is gone.
    // New post-spec content requires only editing the PlaceArchetype asset.
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
        private Dictionary<string, NpcPostSpecStageDef> postSpecStageDefs;
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
            postSpecStageDefs = new Dictionary<string, NpcPostSpecStageDef>();
            foreach (var archetype in WorldGenerator.SelectedArchetypes)
            {
                var postStages = BuildPostSpecStages(archetype);
                if (postStages.Count == 0) continue;

                stages.AddRange(postStages);
                postSpecStageIdsBySpecId[archetype.SpecializationId] = postStages.ConvertAll(s => s.Id);

                // Build the stageId → def lookup for HandleDeveloped.
                if (archetype.NpcPostSpecStages != null)
                    foreach (var def in archetype.NpcPostSpecStages)
                        postSpecStageDefs[def.stageId] = def;
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
            if (postSpecStageDefs.TryGetValue(stage.Id, out var stageDef))
            {
                if (!string.IsNullOrEmpty(stageDef.worldStateFlag))
                    WorldState.SetFlag(stageDef.worldStateFlag, true);
                Debug.Log($"{specializedName}: {stageDef.flavorText}", this);
            }
            else
            {
                WorldState.SetFlag(stage.Id, true);
                Debug.Log($"{specializedName}: {stage.DisplayName}.", this);
            }
        }

        private List<DevelopmentStage> BuildPostSpecStages(PlaceArchetype archetype)
        {
            var stages = new List<DevelopmentStage>();
            if (archetype.NpcPostSpecStages == null || archetype.NpcPostSpecStages.Length == 0)
                return stages;

            var item1 = archetype.CommonYields?.Length > 0 ? archetype.CommonYields[0].Item : null;
            var item2 = archetype.RareYield?.Item;

            foreach (var def in archetype.NpcPostSpecStages)
            {
                var item = def.useRareItem ? item2 : item1;
                if (item == null) continue;

                stages.Add(new DevelopmentStage(def.stageId, def.displayName, def.progressCost,
                    new SpecializationRealizedCondition(archetype.SpecializationId,
                        $"needs a {archetype.NpcTitle} in town"),
                    new ItemAvailableCondition(item, def.itemCount)));
            }

            return stages;
        }
    }
}
