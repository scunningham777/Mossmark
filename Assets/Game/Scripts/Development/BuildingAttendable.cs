using System.Collections.Generic;
using Mossmark.Attention;
using Mossmark.Inventory;
using UnityEngine;

namespace Mossmark.Development
{
    // Iteration 9: a dilapidated building, revived via repeated attention while carrying
    // its required material. Each productive tick consumes material and spends daylight;
    // the tick that crosses the revival threshold interrupts the hold.
    //
    // Content pass: optional stage 2 (development beyond revival). When stage2DisplayName
    // is non-empty and stage2Material is assigned, a second DevelopmentStage is added to
    // the track gated by ItemAvailableCondition(stage2Material) +, optionally,
    // SpecializationRealizedCondition(stage2RequiredSpecialization). Each stage consumes
    // its own material on productive ticks; OnAttentionComplete captures which stage was
    // active before ResolveAttention() advances CurrentStageIndex.
    public class BuildingAttendable : DevelopableEntity, IAttendable
    {
        [SerializeField] private string dilapidatedName = "Tumbledown Building";
        [SerializeField] private string revivedName = "Building";
        [SerializeField] private string repairVerb = "repair";
        [SerializeField] private ItemDefinition material;
        [SerializeField, Min(1)] private int materialCostPerTick = 2;
        [SerializeField, Min(1)] private int progressCost = 6;

        [SerializeField] private string declaredSpecialization;

        [SerializeField, Min(0.1f)] private float minTickInterval = 2f;
        [SerializeField, Min(0.1f)] private float maxTickInterval = 3f;

        [SerializeField] private Color revivedTint = new(1f, 0.85f, 0.5f, 1f);

        [Header("Stage 2 (optional)")]
        [SerializeField] private string stage2DisplayName;
        [SerializeField] private string stage2Verb = "develop";
        [SerializeField] private ItemDefinition stage2Material;
        [SerializeField, Min(1)] private int stage2MaterialCostPerTick = 2;
        [SerializeField, Min(1)] private int stage2ProgressCost = 4;
        // Specialization that must be realized before stage 2 is available. Empty = no dep.
        [SerializeField] private string stage2RequiredSpecialization;
        // Color.clear (alpha 0) means "no change from revivedTint."
        [SerializeField] private Color stage2Tint = Color.clear;

        private DevelopmentTrack track;
        private SpriteRenderer spriteRenderer;
        private float currentTickInterval;

        public void Initialize(string dilapidatedName, string revivedName, string repairVerb,
            ItemDefinition material, int materialCostPerTick, int progressCost,
            float minTickInterval, float maxTickInterval, Color revivedTint, string declaredSpecialization)
        {
            this.dilapidatedName = dilapidatedName;
            this.revivedName = revivedName;
            this.repairVerb = repairVerb;
            this.material = material;
            this.materialCostPerTick = materialCostPerTick;
            this.progressCost = progressCost;
            this.minTickInterval = minTickInterval;
            this.maxTickInterval = maxTickInterval;
            this.revivedTint = revivedTint;
            this.declaredSpecialization = declaredSpecialization;
        }

        // Called before SetActive(true), same as Initialize — only needed if stage 2 exists.
        public void InitializeStage2(string displayName, string verb, ItemDefinition mat,
            int matCost, int progCost, string requiredSpecialization, Color tint)
        {
            stage2DisplayName = displayName;
            stage2Verb = verb;
            stage2Material = mat;
            stage2MaterialCostPerTick = matCost;
            stage2ProgressCost = progCost;
            stage2RequiredSpecialization = requiredSpecialization;
            stage2Tint = tint;
        }

        public override string DisplayName => CurrentStageIndex >= 0 ? revivedName : dilapidatedName;
        protected override DevelopmentTrack Track => track;

        public float AttentionDuration => currentTickInterval;
        public bool RequiresDaylight => LastAttentionMadeProgress;
        public bool ContinueAttending => LastAttentionMadeProgress && !LastAttentionAppliedStage;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();

            var stages = new List<DevelopmentStage>
            {
                new("revive", $"Revive the {revivedName}", progressCost,
                    new ItemAvailableCondition(material, materialCostPerTick))
            };

            if (!string.IsNullOrEmpty(stage2DisplayName) && stage2Material != null)
            {
                var deps = new List<IDependencyCondition>
                {
                    new ItemAvailableCondition(stage2Material, stage2MaterialCostPerTick)
                };
                if (!string.IsNullOrEmpty(stage2RequiredSpecialization))
                    deps.Insert(0, new SpecializationRealizedCondition(stage2RequiredSpecialization,
                        $"needs a {stage2RequiredSpecialization} in town to develop further"));

                stages.Add(new DevelopmentStage("develop", stage2DisplayName, stage2ProgressCost,
                    deps.ToArray()));
            }

            track = new DevelopmentTrack(stages.ToArray());

            OnDeveloped += HandleDeveloped;
            UpdateVisual();
            RollTickInterval();
        }

        public bool CanAttend() => CanMakeProgress();

        public string GetOverlayDescription() => DisplayName;

        public string GetOverlayInteractionLine()
        {
            if (CurrentStageIndex < 0)
                return GetNeedsOrDefault($"Hold E to {repairVerb} the {dilapidatedName}");

            if (GetNextStage() != null)
                return GetNeedsOrDefault($"Hold E to {stage2Verb} the {revivedName}");

            return $"The {revivedName} stands restored.";
        }

        public void OnAttentionComplete()
        {
            // Capture which stage is active before ResolveAttention() may advance the index.
            bool onStage1 = CurrentStageIndex < 0;
            ResolveAttention();

            if (LastAttentionMadeProgress)
            {
                if (onStage1)
                    ConsumeMaterial(material, materialCostPerTick);
                else if (stage2Material != null)
                    ConsumeMaterial(stage2Material, stage2MaterialCostPerTick);
            }
            else
            {
                LogDependencyReport();
            }

            RollTickInterval();
        }

        public void OnAttentionCancelled() { }

        private void ConsumeMaterial(ItemDefinition mat, int amount)
        {
            int remaining = amount;
            if (InventoryManager.Instance != null)
                remaining -= InventoryManager.Instance.RemoveItem(mat, remaining);
            if (remaining > 0 && ChestAttendable.Instance != null)
                ChestAttendable.Instance.Withdraw(mat, remaining);
        }

        private void RollTickInterval()
        {
            currentTickInterval = UnityEngine.Random.Range(minTickInterval, maxTickInterval);
        }

        private void UpdateVisual()
        {
            if (spriteRenderer == null) return;
            if (CurrentStageIndex < 0)
                spriteRenderer.color = Color.white;
            else if (CurrentStageIndex >= 1 && stage2Tint.a > 0)
                spriteRenderer.color = stage2Tint;
            else
                spriteRenderer.color = revivedTint;
        }

        private void HandleDeveloped(DevelopmentStage stage)
        {
            UpdateVisual();

            if (stage.Id == "revive")
            {
                if (!string.IsNullOrEmpty(declaredSpecialization))
                {
                    DeclaredSpecializationNeeds.Declare(declaredSpecialization);
                    Debug.Log($"{DisplayName}: the town now needs a {declaredSpecialization}.", this);
                }
            }
            else
            {
                Debug.Log($"{DisplayName}: {stage.DisplayName} complete.", this);
            }
        }
    }
}
