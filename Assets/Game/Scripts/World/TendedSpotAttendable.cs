using System.Collections.Generic;
using Mossmark.Attention;
using Mossmark.Day;
using Mossmark.Inventory;
using UnityEngine;

namespace Mossmark.World
{
    public class TendedSpotAttendable : MonoBehaviour, IAttendable
    {
        private enum SpotState { Unmarked, Marked, Ready }

        // Shared cap across every instance of a given spot type, keyed by displayName -
        // generalizes P1's TendedSpotConfig-keyed _markedCounts to P2's per-component config.
        private static readonly Dictionary<string, int> markedCounts = new();

        [SerializeField] private string displayName = "Bramble Patch";
        [SerializeField] private string tendVerb = "tend";
        [SerializeField] private ItemYield[] harvestYields;
        [SerializeField, Min(1)] private int restsToHarvest = 1;
        [SerializeField, Min(1)] private int maxConcurrentMarked = 2;

        // Fired when a mark or harvest succeeds — drives EntityFeedback's progress pulse.
        public event System.Action OnProgressMade;

        private SpotState state = SpotState.Unmarked;
        private int restsRemaining;
        private SpriteRenderer spriteRenderer;

        // Procedural-spawn entry point (Iteration 18's WorldGenerator) - mirrors
        // DevelopingWildernessSpotAttendable.Initialize(), called before SetActive(true).
        public void Initialize(string displayName, string tendVerb, ItemYield[] harvestYields,
            int restsToHarvest, int maxConcurrentMarked)
        {
            this.displayName = displayName;
            this.tendVerb = tendVerb;
            this.harvestYields = harvestYields;
            this.restsToHarvest = restsToHarvest;
            this.maxConcurrentMarked = maxConcurrentMarked;
        }

        public float AttentionDuration => 2f;

        // AttentionManager.CompleteAttention reads RequiresDaylight *after* OnAttentionComplete,
        // by which point a successful Harvest has already flipped state back to Unmarked - a
        // state-dependent answer here can't tell "just harvested" from "nothing to harvest".
        // So both marking and harvesting are free: tended spots are low-effort cultivation
        // check-ins, distinct from wilderness spots' active-extraction daylight cost.
        public bool RequiresDaylight => false;

        // Marking and harvesting are each a single 2-second hold, not a continuing loop.
        public bool ContinueAttending => false;

        public bool CanAttend() => state switch
        {
            SpotState.Unmarked => CanMark(),
            SpotState.Marked => false,
            SpotState.Ready => true,
            _ => false
        };

        public string GetShortName() => displayName;
        public string GetOverlayDescription() => displayName;

        public IReadOnlyList<string> GetAppliedUpgrades() => System.Array.Empty<string>();

        public string GetOverlayInteractionLine() => state switch
        {
            SpotState.Unmarked => CanMark()
                ? $"Hold E to {tendVerb} the {displayName}"
                : $"{displayName} (too many spots already tended)",
            SpotState.Marked => $"{displayName} - ready in {restsRemaining} rest{(restsRemaining == 1 ? "" : "s")}",
            SpotState.Ready => $"Hold E to harvest the {displayName}",
            _ => displayName
        };

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            UpdateVisual();
        }

        private void Start()
        {
            if (DayCycleManager.Instance != null)
            {
                DayCycleManager.Instance.DayAdvanced += HandleDayAdvanced;
            }
        }

        private void OnDestroy()
        {
            if (DayCycleManager.Instance != null)
            {
                DayCycleManager.Instance.DayAdvanced -= HandleDayAdvanced;
            }

            if (state == SpotState.Marked)
            {
                DecrementMarkedCount();
            }
        }

        public void OnAttentionComplete()
        {
            switch (state)
            {
                case SpotState.Unmarked:
                    Mark();
                    break;
                case SpotState.Ready:
                    Harvest();
                    break;
            }
        }

        public void OnAttentionCancelled()
        {
        }

        private void Mark()
        {
            state = SpotState.Marked;
            restsRemaining = restsToHarvest;
            IncrementMarkedCount();
            UpdateVisual();
            Debug.Log($"{displayName}: marked, ready in {restsRemaining} rest{(restsRemaining == 1 ? "" : "s")}.", this);
            OnProgressMade?.Invoke();
        }

        private void Harvest()
        {
            var inventory = InventoryManager.Instance;
            if (inventory == null) return;

            var picked = PickWeighted(harvestYields);
            if (picked?.Item == null) return;

            int qty = Random.Range(picked.MinQuantity, picked.MaxQuantity + 1);
            int added = inventory.AddItem(picked.Item, qty);

            Debug.Log(added > 0
                ? $"{displayName}: harvested {added}x {picked.Item.DisplayName}."
                : $"{displayName}: ready to harvest {picked.Item.DisplayName}, but there's no room to carry it.", this);

            if (added <= 0) return;

            DecrementMarkedCount();
            state = SpotState.Unmarked;
            UpdateVisual();
            OnProgressMade?.Invoke();
        }

        // Inline weighted pick — same logic as ItemYieldRoller.PickWeighted but kept here
        // since tended spots have no rare-drop roll and no interrupt, so the full
        // ItemYieldRoller.Roll signature doesn't fit.
        private static ItemYield PickWeighted(ItemYield[] yields)
        {
            if (yields == null || yields.Length == 0) return null;

            float total = 0f;
            foreach (var y in yields) total += Mathf.Max(0f, y.Weight);
            if (total <= 0f) return yields[0];

            float roll = Random.value * total;
            float cumulative = 0f;
            foreach (var y in yields)
            {
                cumulative += Mathf.Max(0f, y.Weight);
                if (roll <= cumulative) return y;
            }
            return yields[^1];
        }

        private void HandleDayAdvanced()
        {
            if (state != SpotState.Marked) return;

            restsRemaining--;
            if (restsRemaining <= 0)
            {
                state = SpotState.Ready;
                UpdateVisual();
            }
        }

        private bool CanMark() => GetMarkedCount() < maxConcurrentMarked;

        private int GetMarkedCount() => markedCounts.TryGetValue(displayName, out int count) ? count : 0;

        private void IncrementMarkedCount() => markedCounts[displayName] = GetMarkedCount() + 1;

        private void DecrementMarkedCount()
        {
            int current = GetMarkedCount();
            if (current > 0) markedCounts[displayName] = current - 1;
        }

        private void UpdateVisual()
        {
            if (spriteRenderer == null) return;

            spriteRenderer.color = state switch
            {
                SpotState.Marked => new Color(0.45f, 0.45f, 0.45f, 1f),
                SpotState.Ready => new Color(1f, 0.95f, 0.4f, 1f),
                _ => Color.white
            };
        }
    }
}
