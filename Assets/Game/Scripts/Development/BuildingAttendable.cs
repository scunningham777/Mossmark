using Mossmark.Attention;
using Mossmark.Inventory;
using UnityEngine;

namespace Mossmark.Development
{
    // Iteration 9: a dilapidated building, revived via repeated attention while carrying
    // its required material. ContinueAttending/RequiresDaylight drive an ongoing hold via
    // DevelopableEntity's split LastAttentionMadeProgress/LastAttentionAppliedStage flags -
    // each productive tick consumes one unit of material and spends daylight; the tick that
    // crosses the revival threshold interrupts the hold (Development Application's rule).
    public class BuildingAttendable : DevelopableEntity, IAttendable
    {
        [SerializeField] private string dilapidatedName = "Tumbledown Smithy";
        [SerializeField] private string revivedName = "Smithy";
        [SerializeField] private string repairVerb = "repair";
        [SerializeField] private ItemDefinition material;
        [SerializeField, Min(1)] private int materialCostPerTick = 1;
        [SerializeField, Min(1)] private int progressCost = 3;

        // Specialization id declared as "needed" (DeclaredSpecializationNeeds) once this
        // building revives - drives Iteration 11's Building -> NPC demand loop. Empty = no demand.
        [SerializeField] private string declaredSpecialization = "smith";

        // Ongoing hold: each tick consumes material and adds progress, refilling the
        // [####....] bar at this rate. Each tick rerolls a fresh interval in this range,
        // same approach as GenericWildernessSpotAttendable's tick interval.
        [SerializeField, Min(0.1f)] private float minTickInterval = 2f;
        [SerializeField, Min(0.1f)] private float maxTickInterval = 3f;

        [SerializeField] private Color revivedTint = new(1f, 0.85f, 0.5f, 1f);

        private DevelopmentTrack track;
        private SpriteRenderer spriteRenderer;
        private float currentTickInterval;

        // Procedural-spawn entry point (Iteration 12's WorldGenerator) - sets the same
        // serialized fields an inspector-authored instance would carry, before SetActive(true).
        public void Initialize(string dilapidatedName, string revivedName, string repairVerb, ItemDefinition material,
            int materialCostPerTick, int progressCost, float minTickInterval, float maxTickInterval, Color revivedTint, string declaredSpecialization)
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

        public override string DisplayName => CurrentStageIndex >= 0 ? revivedName : dilapidatedName;
        protected override DevelopmentTrack Track => track;

        public float AttentionDuration => currentTickInterval;
        public bool RequiresDaylight => LastAttentionMadeProgress;

        // True only on a productive tick that didn't also cross the revival threshold -
        // the threshold-crossing tick is the hold's last (an interrupt, per
        // Development Application). A blocked ("needs") tick also ends the hold here.
        public bool ContinueAttending => LastAttentionMadeProgress && !LastAttentionAppliedStage;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();

            track = new DevelopmentTrack(
                new DevelopmentStage("revive", $"Revive the {revivedName}", progressCost,
                    new ItemAvailableCondition(material, materialCostPerTick)));

            OnDeveloped += HandleDeveloped;
            UpdateVisual();
            RollTickInterval();
        }

        // Also covers "nothing left to do once revived" - GetNextStage() returns null
        // once the single revival stage has been applied, so CanMakeProgress() is false.
        public bool CanAttend() => CanMakeProgress();

        public string GetOverlayDescription() => DisplayName;

        public string GetOverlayInteractionLine() => CurrentStageIndex >= 0
            ? $"The {revivedName} stands restored."
            : GetNeedsOrDefault($"Hold E to {repairVerb} the {dilapidatedName}");

        public void OnAttentionComplete()
        {
            ResolveAttention();

            if (LastAttentionMadeProgress)
            {
                InventoryManager.Instance?.RemoveItem(material, materialCostPerTick);
            }
            else
            {
                LogDependencyReport();
            }

            RollTickInterval();
        }

        public void OnAttentionCancelled()
        {
        }

        private void RollTickInterval()
        {
            currentTickInterval = UnityEngine.Random.Range(minTickInterval, maxTickInterval);
        }

        private void UpdateVisual()
        {
            if (spriteRenderer == null) return;
            spriteRenderer.color = CurrentStageIndex >= 0 ? revivedTint : Color.white;
        }

        private void HandleDeveloped(DevelopmentStage stage)
        {
            UpdateVisual();

            if (string.IsNullOrEmpty(declaredSpecialization)) return;

            DeclaredSpecializationNeeds.Declare(declaredSpecialization);
            Debug.Log($"{DisplayName}: the town now needs a {declaredSpecialization}.", this);
        }
    }
}
