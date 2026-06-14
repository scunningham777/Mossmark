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
        [SerializeField] private ItemYield harvestYield;
        [SerializeField, Min(1)] private int restsToHarvest = 1;
        [SerializeField, Min(1)] private int maxConcurrentMarked = 2;

        private SpotState state = SpotState.Unmarked;
        private int restsRemaining;
        private SpriteRenderer spriteRenderer;

        public float AttentionDuration => 0f;

        // AttentionManager.CompleteAttention reads RequiresStamina *after* OnAttentionComplete,
        // by which point a successful Harvest has already flipped state back to Unmarked - a
        // state-dependent answer here can't tell "just harvested" from "nothing to harvest".
        // So both marking and harvesting are free: tended spots are low-effort cultivation
        // check-ins, distinct from wilderness spots' active-extraction stamina cost.
        public bool RequiresStamina => false;

        public bool CanAttend() => state switch
        {
            SpotState.Unmarked => CanMark(),
            SpotState.Marked => false,
            SpotState.Ready => true,
            _ => false
        };

        public string GetOverlayDescription() => displayName;

        public string GetOverlayInteractionLine() => state switch
        {
            SpotState.Unmarked => CanMark()
                ? $"Press E to {tendVerb} the {displayName}"
                : $"{displayName} (too many spots already tended)",
            SpotState.Marked => $"{displayName} - ready in {restsRemaining} rest{(restsRemaining == 1 ? "" : "s")}",
            SpotState.Ready => $"Press E to harvest the {displayName}",
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
        }

        private void Harvest()
        {
            var inventory = InventoryManager.Instance;
            if (inventory == null || harvestYield?.Item == null) return;

            int qty = Random.Range(harvestYield.MinQuantity, harvestYield.MaxQuantity + 1);
            int added = inventory.AddItem(harvestYield.Item, qty);

            Debug.Log(added > 0
                ? $"{displayName}: harvested {added}x {harvestYield.Item.DisplayName}."
                : $"{displayName}: ready to harvest {harvestYield.Item.DisplayName}, but there's no room to carry it.", this);

            if (added <= 0) return;

            DecrementMarkedCount();
            state = SpotState.Unmarked;
            UpdateVisual();
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
