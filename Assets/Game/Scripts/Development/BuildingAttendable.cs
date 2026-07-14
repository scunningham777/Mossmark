using System.Collections.Generic;
using Mossmark.Attention;
using Mossmark.Day;
using Mossmark.Inventory;
using Mossmark.Visuals;
using Mossmark.World;
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
    // No daylight is spent (lastOutcomeKind != Develop guards RequiresDaylight).
    //
    // Iteration 29: settlement maintenance drift. Developed buildings accumulate drift
    // each rest; cold buildings (driftProgress >= driftThreshold) cost extra daylight
    // per development tick. Direct attend with the maintenance material resets drift.
    // Chest passive consumption is handled by MaintenanceManager.
    //
    // Iteration 39: conversion stations. A building whose FINAL stage def carries
    // biasPropertyIds becomes a station once fully developed — attending it opens the
    // working view (WorkshopUI) instead of the linger flavor. Opening is free and
    // one-shot, same as the chest. Station-ness is pure stage data; unlocking one is
    // exactly restoring a building.
    //
    // Iteration 40: station capability is decoupled from finality. It now looks at the
    // most recently *applied* stage (searching stages[0..CurrentStageIndex] from the
    // end) rather than the literal last entry in stages[], so a station stays open
    // whether or not further stages exist or later apply. CanAttend()/OnAttentionComplete()
    // priority: maintenance -> develop (if circumstances currently allow it) -> open
    // station -> flavor linger.
    //
    // Iteration 41: PredictOutcomeKind() formalizes that priority chain into a single
    // AttentionOutcomeKind-returning method, used both by OnAttentionComplete()'s
    // dispatch and by RollTickInterval()'s duration roll - one source of truth instead
    // of two copies of the same chain. Duration and daylight cost are resolved through
    // OutcomeRequest/IOutcomeModifier, the same ambient-modifier pipeline already
    // proven for rare-drop odds. Open/Maintain get their own faster duration range
    // (minInteractInterval/maxInteractInterval) distinct from Develop/Visit's existing
    // minTickInterval/maxTickInterval. The cold-building daylight tax is now
    // DriftColdDaylightModifier instead of an inline conditional.
    public class BuildingAttendable : DevelopableEntity, IAttendable, IMaintenanceConsumer, IWorkStation
    {
        [SerializeField] private string dilapidatedName = "Tumbledown Building";
        [SerializeField] private string declaredSpecialization;
        [SerializeField, Min(0.1f)] private float minTickInterval = 2f;
        [SerializeField, Min(0.1f)] private float maxTickInterval = 3f;
        [SerializeField, Min(0.1f)] private float minInteractInterval = 0.5f;
        [SerializeField, Min(0.1f)] private float maxInteractInterval = 1f;
        [SerializeField] private BuildingStageDef[] stages = System.Array.Empty<BuildingStageDef>();
        [SerializeField] private string[] restoredFlavors = System.Array.Empty<string>();
        [SerializeField] private string coldFlavor;
        [SerializeField, Min(1)] private int maintenanceCostPerReset = 2;

        // Iteration 52 (Dominance Halo): the archetype this building belongs to, set by
        // WorldGenerator.SpawnBuilding() for every procedurally-spawned building. Reading
        // WorldGenerator.IsArchetypeDominant() with this id is a no-op (always false) for
        // every archetype outside Iteration 50's Bog/Sacred Grove pilot pair, so wiring it
        // unconditionally here needs no per-building special-casing to stay pilot-scoped.
        [SerializeField] private string dominanceArchetypeId = "";

        private DevelopmentTrack track;
        private SpriteRenderer spriteRenderer;
        private ItemDefinition maintenanceMaterial;
        private float currentTickInterval;
        private AttentionOutcomeKind lastOutcomeKind = AttentionOutcomeKind.Develop;

        // Iteration 52: fires the current dominance state (not just changes) any time it's
        // (re)checked, so a late subscriber (EntityFeedback.Start order vs. this one) always
        // gets a correct initial read rather than waiting for the next rest.
        public event System.Action<bool> OnDominanceChanged;

        // IMaintenanceConsumer
        public int DriftThreshold => 5;
        public ItemDefinition MaintenanceMaterial => maintenanceMaterial;
        public int MaintenanceCostPerReset => maintenanceCostPerReset;

        // IWorkStation — station-ness lives on the most recently applied stage def, not
        // necessarily the last entry in stages[] (Iteration 40).
        private BuildingStageDef AppliedStationStage
        {
            get
            {
                for (int i = CurrentStageIndex; i >= 0; i--)
                {
                    var def = stages[i];
                    if (def.biasPropertyIds != null && def.biasPropertyIds.Length > 0)
                        return def;
                }
                return null;
            }
        }

        private bool IsStationCapable => AppliedStationStage != null;

        public string StationDisplayName => DisplayName;
        public IReadOnlyList<string> BiasPropertyIds =>
            IsStationCapable ? AppliedStationStage.biasPropertyIds : System.Array.Empty<string>();
        public GameObject StationObject => gameObject;

        public void Initialize(string dilapidatedName, BuildingStageDef[] stages,
            string declaredSpecialization, float minTickInterval = 2f, float maxTickInterval = 3f,
            string[] restoredFlavors = null, string coldFlavor = null, int maintenanceCostPerReset = 2,
            string dominanceArchetypeId = "")
        {
            this.dilapidatedName = dilapidatedName;
            this.stages = stages;
            this.declaredSpecialization = declaredSpecialization;
            this.minTickInterval = minTickInterval;
            this.maxTickInterval = maxTickInterval;
            this.restoredFlavors = restoredFlavors ?? System.Array.Empty<string>();
            this.coldFlavor = coldFlavor;
            this.maintenanceCostPerReset = maintenanceCostPerReset;
            this.dominanceArchetypeId = dominanceArchetypeId;
        }

        // Stage 0's displayName is the building's revived name; post-revival the name
        // stays the same regardless of further stage progression — unless the station
        // stage (Iteration 40: the most recently applied one, not necessarily the last
        // in the array) has its own stationName (the building has become something
        // else: Woodland Shrine -> Consecrated Hearth).
        public override string DisplayName =>
            IsStationCapable && !string.IsNullOrEmpty(AppliedStationStage.stationName)
                ? AppliedStationStage.stationName
                : stages.Length > 0 && CurrentStageIndex >= 0 ? stages[0].displayName : dilapidatedName;

        protected override DevelopmentTrack Track => track;

        public float AttentionDuration => currentTickInterval;

        public bool RequiresDaylight =>
            lastOutcomeKind == AttentionOutcomeKind.Develop && LastAttentionMadeProgress;

        public bool ContinueAttending =>
            lastOutcomeKind == AttentionOutcomeKind.Develop
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

                // The material gate is structural (material is consumed per tick), so it
                // is derived here; all other gates come from the def's authored condition
                // list — e.g. the old requiredSpecialization is now an authored
                // SpecializationRealizedCondition in stage_conditions.csv.
                var deps = new List<IDependencyCondition>();
                if (def.conditions != null)
                    foreach (var condition in def.conditions)
                        if (condition != null) deps.Add(condition);
                deps.Add(new ItemAvailableCondition(def.material, def.costPerTick));

                // Stage 0's DevelopmentStage name uses "Revive the X" phrasing; later stages
                // use the stage def's displayName directly (which is the action/development name).
                string stageName = i == 0
                    ? $"Revive the {def.displayName}"
                    : def.displayName;

                string stageId = !string.IsNullOrEmpty(def.stageId) ? def.stageId : $"stage_{i}";
                devStages.Add(new DevelopmentStage(stageId, stageName, def.progressCost, deps.ToArray()));
            }

            track = new DevelopmentTrack(devStages.ToArray());
            OnDeveloped += HandleDeveloped;
            UpdateVisual();
            RollTickInterval();
        }

        private void Start()
        {
            if (string.IsNullOrEmpty(dominanceArchetypeId)) return;
            if (DayCycleManager.Instance != null)
                DayCycleManager.Instance.DayAdvanced += RefreshDominanceHalo;
            RefreshDominanceHalo();
        }

        private void OnDestroy()
        {
            if (DayCycleManager.Instance != null)
                DayCycleManager.Instance.DayAdvanced -= RefreshDominanceHalo;
        }

        // Iteration 52: reads Iteration 50's dominance check directly, no new tracking.
        // Fires every rest (and once at Start) rather than only on change, so a late
        // subscriber's first read is always correct.
        private void RefreshDominanceHalo() =>
            OnDominanceChanged?.Invoke(WorldGenerator.IsArchetypeDominant(dominanceArchetypeId));

        public bool CanAttend()
        {
            // Iteration 40: station capability keeps a building attendable even when it
            // has pending stages the player can't currently progress. Visit is the only
            // kind that isn't unconditionally attendable — it's the fallback default of
            // PredictOutcomeKind() below, so it must be re-checked against "fully developed".
            return PredictOutcomeKind() != AttentionOutcomeKind.Visit || GetNextStage() == null;
        }

        // Iteration 41: formalizes Iteration 40's priority chain (maintenance -> develop,
        // if currently possible -> open, if station-capable -> visit) into a single method.
        // Used both by OnAttentionComplete()'s dispatch and RollTickInterval()'s duration
        // roll, so there is one source of truth for "what would attending do right now"
        // rather than separate copies of the same chain.
        public AttentionOutcomeKind PredictOutcomeKind()
        {
            if (DriftProgress > 0 && maintenanceMaterial != null
                && InventoryManager.Instance != null
                && InventoryManager.Instance.GetQuantity(maintenanceMaterial) >= maintenanceCostPerReset)
                return AttentionOutcomeKind.Maintain;

            if (CanMakeProgress())
                return AttentionOutcomeKind.Develop;

            if (IsStationCapable)
                return AttentionOutcomeKind.Open;

            return AttentionOutcomeKind.Visit;
        }

        // Builds the shared OutcomeRequest that both the duration roll and the daylight
        // cost calculation run through — same ambient-modifier pipeline already proven
        // for rare-drop odds (TwilightChanceModifier, RealizedSpecializationChanceModifier).
        private OutcomeRequest BuildOutcomeRequest()
        {
            var request = new OutcomeRequest();
            new DriftColdDaylightModifier(DriftProgress, DriftThreshold).Apply(request);
            return request;
        }

        // Drift suffix shows in the overlay name so the player sees it while approaching.
        public string GetShortName() => DisplayName;

        public string GetOverlayDescription() =>
            GetDriftOverlayDescription(DisplayName, DriftThreshold, coldFlavor);

        public IReadOnlyList<string> GetAppliedUpgrades() => GetAppliedUpgradeNames();

        public string GetOverlayInteractionLine()
        {
            // Maintenance takes priority — shown whenever the player is carrying material
            // and the building has drifted, even if development is available.
            if (DriftProgress > 0 && maintenanceMaterial != null
                && InventoryManager.Instance != null
                && InventoryManager.Instance.GetQuantity(maintenanceMaterial) >= maintenanceCostPerReset)
                return $"Hold E to tend to the {DisplayName}";

            // Iteration 40: development text takes priority whenever there's a pending
            // stage and either it can currently progress, or there's no station to fall
            // back to (mirrors OnAttentionComplete()'s priority chain below).
            if (GetNextStage() != null && (CanMakeProgress() || !IsStationCapable))
            {
                int nextIndex = CurrentStageIndex + 1;
                var stageDef = stages[nextIndex];

                // Stage 0: "Hold E to {verb} the {dilapidatedName}" — names the object being worked on.
                // Stage 1+: "Hold E to {displayName}" — names the specific development action.
                return CurrentStageIndex < 0
                    ? GetNeedsOrDefault($"Hold E to {stageDef.verb} the {dilapidatedName}")
                    : GetNeedsOrDefault($"Hold E to {stageDef.displayName}");
            }

            if (IsStationCapable)
                return $"Hold E to work at the {DisplayName}";

            return $"Hold E to linger near the {DisplayName}";
        }

        public void OnAttentionComplete()
        {
            lastOutcomeKind = PredictOutcomeKind();

            switch (lastOutcomeKind)
            {
                case AttentionOutcomeKind.Maintain:
                    ConsumeMaterial(maintenanceMaterial, maintenanceCostPerReset);
                    ResetDrift();
                    NotificationManager.Post($"{DisplayName}: feels tended again.");
                    Debug.Log($"{DisplayName}: maintained directly by player.", this);
                    break;

                case AttentionOutcomeKind.Develop:
                {
                    // Capture the stage index before ResolveAttention() may advance it, so
                    // we know which stage's material to consume.
                    int stageIndexBefore = CurrentStageIndex;
                    ResolveAttention();

                    if (LastAttentionMadeProgress)
                    {
                        int materialIndex = stageIndexBefore + 1;
                        if (materialIndex < stages.Length)
                            ConsumeMaterial(stages[materialIndex].material, stages[materialIndex].costPerTick);

                        // Cold buildings tax each productive development tick with extra
                        // daylight, via DriftColdDaylightModifier doubling the resolved cost
                        // rather than an inline conditional (Iteration 41). The base develop
                        // cost is always 1 (already spent by AttentionManager via
                        // RequiresDaylight), so only the amount beyond that is self-spent here.
                        var request = BuildOutcomeRequest();
                        int resolvedCost = Mathf.RoundToInt(1 * request.DaylightCostMultiplier);
                        int extraCost = resolvedCost - 1;
                        if (extraCost > 0 && DayCycleManager.Instance != null)
                            DayCycleManager.Instance.SpendDaylight(extraCost);
                    }
                    else
                    {
                        LogDependencyReport();
                    }
                    break;
                }

                case AttentionOutcomeKind.Open:
                    WorkshopUI.Instance?.Open(this);
                    break;

                case AttentionOutcomeKind.Visit:
                    PostRestoredFlavor();
                    break;
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

        // Open/Maintain are low-commitment check-ins on an already-realized place and
        // get their own faster range; Develop/Visit keep the existing sustained-effort
        // range unchanged (Iteration 41). DurationMultiplier runs through the same
        // modifier pipeline as the daylight cost, though no live modifier scales
        // duration yet — the slot is here for future ambient modifiers.
        private void RollTickInterval()
        {
            var kind = PredictOutcomeKind();
            var request = BuildOutcomeRequest();

            bool interactRange = kind == AttentionOutcomeKind.Open || kind == AttentionOutcomeKind.Maintain;
            float min = interactRange ? minInteractInterval : minTickInterval;
            float max = interactRange ? maxInteractInterval : maxTickInterval;

            currentTickInterval = Random.Range(min, max) * request.DurationMultiplier;
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

            int defIndex = CurrentStageIndex;
            if (defIndex >= 0 && defIndex < stages.Length
                && !string.IsNullOrEmpty(stages[defIndex].worldStateFlag))
            {
                WorldState.SetFlag(stages[defIndex].worldStateFlag, true);
                Debug.Log($"{DisplayName}: WorldState flag '{stages[defIndex].worldStateFlag}' set.", this);
            }
        }
    }
}
