using System.Collections.Generic;
using Mossmark.Attention;
using Mossmark.Inventory;
using UnityEngine;

namespace Mossmark.Development
{
    // Iteration 9: a dilapidated building, revived and further developed via repeated
    // attention while carrying its required material. Each productive tick consumes
    // material and spends daylight; the tick that crosses a stage threshold interrupts
    // the hold.
    //
    // Iteration 21: replaced parallel per-stage field groups with BuildingStageDef[],
    // so any number of stages can be authored without code changes. stage[0] is always
    // the revival stage; stage[0].displayName is the building's revived name.
    public class BuildingAttendable : DevelopableEntity, IAttendable
    {
        [SerializeField] private string dilapidatedName = "Tumbledown Building";
        [SerializeField] private string declaredSpecialization;
        [SerializeField, Min(0.1f)] private float minTickInterval = 2f;
        [SerializeField, Min(0.1f)] private float maxTickInterval = 3f;
        [SerializeField] private BuildingStageDef[] stages = System.Array.Empty<BuildingStageDef>();

        private DevelopmentTrack track;
        private SpriteRenderer spriteRenderer;
        private float currentTickInterval;

        public void Initialize(string dilapidatedName, BuildingStageDef[] stages,
            string declaredSpecialization, float minTickInterval = 2f, float maxTickInterval = 3f)
        {
            this.dilapidatedName = dilapidatedName;
            this.stages = stages;
            this.declaredSpecialization = declaredSpecialization;
            this.minTickInterval = minTickInterval;
            this.maxTickInterval = maxTickInterval;
        }

        // Stage 0's displayName is the building's revived name; post-revival the name
        // stays the same regardless of further stage progression.
        public override string DisplayName =>
            stages.Length > 0 && CurrentStageIndex >= 0 ? stages[0].displayName : dilapidatedName;

        protected override DevelopmentTrack Track => track;

        public float AttentionDuration => currentTickInterval;
        public bool RequiresDaylight => LastAttentionMadeProgress;
        public bool ContinueAttending => LastAttentionMadeProgress && !LastAttentionAppliedStage;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();

            var devStages = new List<DevelopmentStage>();
            for (int i = 0; i < stages.Length; i++)
            {
                var def = stages[i];
                var deps = new List<IDependencyCondition>
                {
                    new ItemAvailableCondition(def.material, def.costPerTick)
                };
                if (!string.IsNullOrEmpty(def.requiredSpecialization))
                    deps.Insert(0, new SpecializationRealizedCondition(def.requiredSpecialization,
                        $"needs a {def.requiredSpecialization} in town to develop further"));

                // Stage 0's DevelopmentStage name uses "Revive the X" phrasing; later stages
                // use the stage def's displayName directly (which is the action/development name).
                string stageName = i == 0
                    ? $"Revive the {def.displayName}"
                    : def.displayName;

                devStages.Add(new DevelopmentStage($"stage_{i}", stageName, def.progressCost, deps.ToArray()));
            }

            track = new DevelopmentTrack(devStages.ToArray());
            OnDeveloped += HandleDeveloped;
            UpdateVisual();
            RollTickInterval();
        }

        public bool CanAttend() => CanMakeProgress();

        public string GetOverlayDescription() => DisplayName;

        public string GetOverlayInteractionLine()
        {
            if (GetNextStage() == null)
                return stages.Length > 0
                    ? $"The {stages[0].displayName} stands restored."
                    : $"The {dilapidatedName} stands restored.";

            int nextIndex = CurrentStageIndex + 1;
            var stageDef = stages[nextIndex];

            // Stage 0: "Hold E to {verb} the {dilapidatedName}" — names the object being worked on.
            // Stage 1+: "Hold E to {displayName}" — names the specific development action.
            if (CurrentStageIndex < 0)
                return GetNeedsOrDefault($"Hold E to {stageDef.verb} the {dilapidatedName}");
            else
                return GetNeedsOrDefault($"Hold E to {stageDef.displayName}");
        }

        public void OnAttentionComplete()
        {
            // Capture the stage index before ResolveAttention() may advance it, so we
            // know which stage's material to consume.
            int stageIndexBefore = CurrentStageIndex;
            ResolveAttention();

            if (LastAttentionMadeProgress)
            {
                int materialIndex = stageIndexBefore + 1;
                if (materialIndex < stages.Length)
                    ConsumeMaterial(stages[materialIndex].material, stages[materialIndex].costPerTick);
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
            currentTickInterval = Random.Range(minTickInterval, maxTickInterval);
        }

        private void UpdateVisual()
        {
            if (spriteRenderer == null || stages == null || stages.Length == 0) return;
            if (CurrentStageIndex < 0)
            {
                spriteRenderer.color = Color.white;
                return;
            }
            int idx = Mathf.Clamp(CurrentStageIndex, 0, stages.Length - 1);
            spriteRenderer.color = stages[idx].tint;
        }

        private void HandleDeveloped(DevelopmentStage stage)
        {
            UpdateVisual();

            // CurrentStageIndex was just incremented; 0 means stage 0 (revival) just fired.
            if (CurrentStageIndex == 0)
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
