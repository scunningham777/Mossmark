using System.Collections.Generic;
using Mossmark.Attention;
using Mossmark.Day;
using Mossmark.Inventory;
using Mossmark.Visuals;
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
    //
    // Iteration 28.5: fully-developed buildings (GetNextStage() == null) remain
    // attendable. A short hold shows a flavor notification from restoredFlavors.
    // No daylight is spent (lastAttentionWasVisit guards RequiresDaylight).
    //
    // Iteration 29: settlement maintenance drift. Developed buildings accumulate drift
    // each rest; cold buildings (driftProgress >= driftThreshold) cost extra daylight
    // per development tick. Direct attend with the maintenance material resets drift.
    // Chest passive consumption is handled by MaintenanceManager.
    public class BuildingAttendable : DevelopableEntity, IAttendable, IMaintenanceConsumer
    {
        [SerializeField] private string dilapidatedName = "Tumbledown Building";
        [SerializeField] private string declaredSpecialization;
        [SerializeField, Min(0.1f)] private float minTickInterval = 2f;
        [SerializeField, Min(0.1f)] private float maxTickInterval = 3f;
        [SerializeField] private BuildingStageDef[] stages = System.Array.Empty<BuildingStageDef>();
        [SerializeField] private string[] restoredFlavors = System.Array.Empty<string>();
        [SerializeField] private string coldFlavor;
        [SerializeField, Min(1)] private int maintenanceCostPerReset = 2;

        private DevelopmentTrack track;
        private SpriteRenderer spriteRenderer;
        private ItemDefinition maintenanceMaterial;
        private float currentTickInterval;
        private bool lastAttentionWasVisit;
        private bool lastAttentionWasMaintenance;

        // IMaintenanceConsumer
        public int DriftThreshold => 5;
        public ItemDefinition MaintenanceMaterial => maintenanceMaterial;
        public int MaintenanceCostPerReset => maintenanceCostPerReset;

        public void Initialize(string dilapidatedName, BuildingStageDef[] stages,
            string declaredSpecialization, float minTickInterval = 2f, float maxTickInterval = 3f,
            string[] restoredFlavors = null, string coldFlavor = null, int maintenanceCostPerReset = 2)
        {
            this.dilapidatedName = dilapidatedName;
            this.stages = stages;
            this.declaredSpecialization = declaredSpecialization;
            this.minTickInterval = minTickInterval;
            this.maxTickInterval = maxTickInterval;
            this.restoredFlavors = restoredFlavors ?? System.Array.Empty<string>();
            this.coldFlavor = coldFlavor;
            this.maintenanceCostPerReset = maintenanceCostPerReset;
        }

        // Stage 0's displayName is the building's revived name; post-revival the name
        // stays the same regardless of further stage progression.
        public override string DisplayName =>
            stages.Length > 0 && CurrentStageIndex >= 0 ? stages[0].displayName : dilapidatedName;

        protected override DevelopmentTrack Track => track;

        public float AttentionDuration => currentTickInterval;

        public bool RequiresDaylight =>
            !lastAttentionWasVisit && !lastAttentionWasMaintenance && LastAttentionMadeProgress;

        public bool ContinueAttending =>
            !lastAttentionWasVisit && !lastAttentionWasMaintenance
            && LastAttentionMadeProgress && !LastAttentionAppliedStage;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();

            // Revival material doubles as the ongoing maintenance material.
            maintenanceMaterial = stages.Length > 0 ? stages[0].material : null;

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

        public bool CanAttend()
        {
            // Direct maintenance is always available when drifted and carrying enough material.
            if (DriftProgress > 0 && maintenanceMaterial != null
                && InventoryManager.Instance != null
                && InventoryManager.Instance.GetQuantity(maintenanceMaterial) >= maintenanceCostPerReset)
                return true;

            return GetNextStage() == null || CanMakeProgress();
        }

        // Drift suffix shows in the overlay name so the player sees it while approaching.
        public string GetOverlayDescription() =>
            GetDriftOverlayDescription(DisplayName, DriftThreshold, coldFlavor);

        public string GetOverlayInteractionLine()
        {
            // Maintenance takes priority — shown whenever the player is carrying material
            // and the building has drifted, even if development is available.
            if (DriftProgress > 0 && maintenanceMaterial != null
                && InventoryManager.Instance != null
                && InventoryManager.Instance.GetQuantity(maintenanceMaterial) >= maintenanceCostPerReset)
                return $"Hold E to tend to the {DisplayName}";

            if (GetNextStage() == null)
                return $"Hold E to linger near the {DisplayName}";

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
            lastAttentionWasVisit = false;
            lastAttentionWasMaintenance = false;

            // Priority 1: direct maintenance — consuming material resets drift.
            if (DriftProgress > 0 && maintenanceMaterial != null
                && InventoryManager.Instance != null
                && InventoryManager.Instance.GetQuantity(maintenanceMaterial) >= maintenanceCostPerReset)
            {
                lastAttentionWasMaintenance = true;
                ConsumeMaterial(maintenanceMaterial, maintenanceCostPerReset);
                ResetDrift();
                NotificationManager.Post($"{DisplayName}: feels tended again.");
                Debug.Log($"{DisplayName}: maintained directly by player.", this);
                RollTickInterval();
                return;
            }

            // Priority 2: linger when fully developed.
            if (GetNextStage() == null)
            {
                lastAttentionWasVisit = true;
                PostRestoredFlavor();
                RollTickInterval();
                return;
            }

            // Priority 3: development.
            // Capture the stage index before ResolveAttention() may advance it, so we
            // know which stage's material to consume.
            int stageIndexBefore = CurrentStageIndex;
            ResolveAttention();

            if (LastAttentionMadeProgress)
            {
                int materialIndex = stageIndexBefore + 1;
                if (materialIndex < stages.Length)
                    ConsumeMaterial(stages[materialIndex].material, stages[materialIndex].costPerTick);

                // Cold buildings tax each productive development tick with an extra daylight.
                if (DriftProgress >= DriftThreshold && DayCycleManager.Instance != null)
                    DayCycleManager.Instance.SpendDaylight(1);
            }
            else
            {
                LogDependencyReport();
            }

            RollTickInterval();
        }

        public void OnAttentionCancelled() { }

        private void PostRestoredFlavor()
        {
            string flavor = restoredFlavors != null && restoredFlavors.Length > 0
                ? restoredFlavors[Random.Range(0, restoredFlavors.Length)]
                : "stands quietly, doing the work it was always meant for.";
            NotificationManager.Post($"{DisplayName}: {flavor}");
            Debug.Log($"{DisplayName}: {flavor}", this);
        }

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
