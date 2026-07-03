using System.Collections.Generic;
using Mossmark.Attention;
using Mossmark.Development;
using Mossmark.Visuals;
using UnityEngine;

namespace Mossmark.Inventory
{
    // The Crude Working Surface (Iteration 36). A single-stage restoration building:
    // once restored (CurrentStageIndex >= 0) attending it opens WorkshopUI instead
    // of the normal BuildingAttendable linger behavior. Uses DevelopableEntity directly
    // rather than inheriting BuildingAttendable so it stays self-contained and
    // EntityFeedback can subscribe through the normal DevelopableEntity event path.
    public class WorkshopAttendable : DevelopableEntity, IAttendable
    {
        [SerializeField] private string dilapidatedName = "Derelict Workshop";
        [SerializeField] private ItemDefinition restorationMaterial;
        [SerializeField, Min(1)] private int restorationCostPerTick = 2;
        [SerializeField, Min(1)] private int restorationProgressCost = 4;
        [SerializeField, Min(0.1f)] private float minTickInterval = 2f;
        [SerializeField, Min(0.1f)] private float maxTickInterval = 3f;
        [SerializeField] private Color restoredTint = new(0.55f, 0.5f, 0.4f, 1f);

        private DevelopmentTrack track;
        private SpriteRenderer spriteRenderer;
        private float currentTickInterval;
        private bool lastAttentionWasOpen;

        public static WorkshopAttendable Instance { get; private set; }

        public override string DisplayName =>
            CurrentStageIndex >= 0 ? "Workshop" : dilapidatedName;

        protected override DevelopmentTrack Track => track;

        public float AttentionDuration => currentTickInterval;

        // Opening the workshop (restored state) is free — same as the chest.
        // Development ticks require daylight.
        public bool RequiresDaylight =>
            !lastAttentionWasOpen && LastAttentionMadeProgress;

        public bool ContinueAttending =>
            !lastAttentionWasOpen && LastAttentionMadeProgress && !LastAttentionAppliedStage;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            spriteRenderer = GetComponent<SpriteRenderer>();

            var restoreStage = new DevelopmentStage(
                "workshop_restoration",
                "Restore the Workshop",
                restorationProgressCost,
                new IDependencyCondition[]
                {
                    new ItemAvailableCondition(restorationMaterial, restorationCostPerTick)
                });

            track = new DevelopmentTrack(new[] { restoreStage });
            OnDeveloped += HandleDeveloped;
            RollTickInterval();
        }

        public bool CanAttend()
        {
            if (CurrentStageIndex >= 0) return true;
            return CanMakeProgress();
        }

        public string GetShortName() => DisplayName;
        public string GetOverlayDescription() => DisplayName;

        public IReadOnlyList<string> GetAppliedUpgrades() => GetAppliedUpgradeNames();

        public string GetOverlayInteractionLine()
        {
            if (CurrentStageIndex >= 0)
                return "Hold E to use the Workshop";

            return GetNeedsOrDefault($"Hold E to clear the {dilapidatedName}");
        }

        public void OnAttentionComplete()
        {
            lastAttentionWasOpen = false;

            if (CurrentStageIndex >= 0)
            {
                lastAttentionWasOpen = true;
                WorkshopUI.Instance?.Open(this);
                RollTickInterval();
                return;
            }

            ResolveAttention();

            if (LastAttentionMadeProgress)
            {
                InventoryManager.Instance?.RemoveItem(restorationMaterial, restorationCostPerTick);
            }
            else
            {
                LogDependencyReport();
            }

            RollTickInterval();
        }

        public void OnAttentionCancelled() { }

        private void HandleDeveloped(DevelopmentStage _)
        {
            if (spriteRenderer != null)
                spriteRenderer.color = restoredTint;

            Debug.Log($"Workshop: restored.", this);
        }

        private void RollTickInterval()
        {
            currentTickInterval = Random.Range(minTickInterval, maxTickInterval);
        }
    }
}
