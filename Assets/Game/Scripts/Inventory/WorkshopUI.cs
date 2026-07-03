using System.Collections.Generic;
using Mossmark.Day;
using Mossmark.Development;
using Mossmark.Visuals;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace Mossmark.Inventory
{
    // Iteration 36: The Crude Working Surface. Three placement slots + a Work action.
    // Items referenced in slots are NOT pre-removed from inventory; consumption happens
    // at Work time. Failure consumes nothing and reveals one unknown property.
    // Reuses ChestUI's code-first UIDocument + UI action map pattern.
    //
    // Iteration 39: one shared working view for any number of IWorkStation instances.
    // The open station's biasPropertyIds filter both recipe resolution and property
    // discovery (success and failure) — a property outside the station's bias can
    // neither be confirmed nor stumbled onto here. That filter is what makes station
    // choice a decision instead of a UI skin.
    [RequireComponent(typeof(UIDocument))]
    public class WorkshopUI : MonoBehaviour
    {
        private const int SlotCount = 3;

        // Row index in right column that represents the Work button.
        private const int WorkRowIndex = SlotCount;

        public static WorkshopUI Instance { get; private set; }
        public bool IsOpen { get; private set; }

        [SerializeField] private ConversionDef[] recipes = System.Array.Empty<ConversionDef>();

        private enum Column { Pack, Slots }

        private IWorkStation currentStation;
        private readonly HashSet<string> stationBias = new();
        private VisualElement root;
        private Label titleLabel;
        private VisualElement packList;
        private VisualElement slotsList;
        private FontDefinition fallbackFont;

        private string StationName => currentStation?.StationDisplayName ?? "Workshop";

        private InputAction navigateAction;
        private InputAction submitAction;
        private InputAction cancelAction;
        private Vector2 lastNavigate;

        private Column selectedColumn;
        private int selectedIndex;

        private readonly List<VisualElement> packRows = new();
        private readonly List<VisualElement> slotRows = new();

        // Items placed into slots. Null = empty.
        private readonly ItemDefinition[] placedItems = new ItemDefinition[SlotCount];

        // Items with newly-discovered properties after last Work attempt (highlighted gold).
        private readonly HashSet<string> highlightDiscoveredItemIds = new();
        private bool pendingHighlightClear;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            var ui = InputSystem.actions.FindActionMap("UI");
            ui.Enable();
            navigateAction = ui.FindAction("Navigate");
            submitAction = ui.FindAction("Submit");
            cancelAction = ui.FindAction("Cancel");
        }

        private void OnEnable()
        {
            var uiDocument = GetComponent<UIDocument>();

            if (uiDocument.panelSettings == null)
            {
                var panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
                panelSettings.themeStyleSheet = Resources.Load<ThemeStyleSheet>("OverlayTheme");
                uiDocument.panelSettings = panelSettings;
            }

            uiDocument.sortingOrder = 50;
            fallbackFont = FontDefinition.FromFont(Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"));
            BuildLayout(uiDocument.rootVisualElement);
        }

        private void Start()
        {
            if (InventoryManager.Instance != null)
                InventoryManager.Instance.InventoryChanged += HandleInventoryChanged;
        }

        private void OnDisable()
        {
            if (InventoryManager.Instance != null)
                InventoryManager.Instance.InventoryChanged -= HandleInventoryChanged;
        }

        private void BuildLayout(VisualElement uiRoot)
        {
            uiRoot.style.position = Position.Absolute;
            uiRoot.style.left = 0;
            uiRoot.style.right = 0;
            uiRoot.style.top = 0;
            uiRoot.style.bottom = 0;
            uiRoot.style.alignItems = Align.Center;
            uiRoot.style.justifyContent = Justify.Center;

            root = new VisualElement
            {
                style =
                {
                    display = DisplayStyle.None,
                    backgroundColor = new Color(0f, 0f, 0f, 0.8f),
                    paddingLeft = 16, paddingRight = 16,
                    paddingTop = 12, paddingBottom = 12,
                    alignItems = Align.Center
                }
            };

            titleLabel = MakeLabel("The Workshop", 18, FontStyle.Bold, marginBottom: 8);
            var columns = new VisualElement { style = { flexDirection = FlexDirection.Row } };

            packList = BuildColumn("Pack", columns);
            slotsList = BuildColumn("Slots", columns);

            var hint = MakeLabel("W/S select  A/D switch  Enter place/work  Esc close", 11, color: new Color(1f, 1f, 1f, 0.7f), marginTop: 8);

            root.Add(titleLabel);
            root.Add(columns);
            root.Add(hint);
            uiRoot.Add(root);
        }

        private VisualElement BuildColumn(string heading, VisualElement parent)
        {
            var column = new VisualElement
            {
                style = { flexDirection = FlexDirection.Column, minWidth = 220, marginLeft = 12, marginRight = 12 }
            };

            column.Add(MakeLabel(heading, 14, FontStyle.Bold, textAlign: TextAnchor.MiddleCenter, marginBottom: 4));

            var list = new VisualElement { style = { flexDirection = FlexDirection.Column } };
            column.Add(list);
            parent.Add(column);
            return list;
        }

        public void Open(IWorkStation station)
        {
            currentStation = station;
            stationBias.Clear();
            if (station.BiasPropertyIds != null)
                foreach (var pid in station.BiasPropertyIds)
                    if (!string.IsNullOrEmpty(pid)) stationBias.Add(pid);

            IsOpen = true;
            selectedColumn = Column.Pack;
            selectedIndex = 0;
            lastNavigate = Vector2.zero;
            titleLabel.text = $"The {StationName}";
            root.style.display = DisplayStyle.Flex;
            Refresh();
        }

        public void Close()
        {
            IsOpen = false;
            currentStation = null;
            stationBias.Clear();
            ClearSlots();
            root.style.display = DisplayStyle.None;
        }

        private void ClearSlots()
        {
            for (int i = 0; i < SlotCount; i++)
                placedItems[i] = null;
        }

        private void Update()
        {
            if (!IsOpen) return;

            Vector2 nav = navigateAction.ReadValue<Vector2>();

            if (Mathf.Abs(nav.y) > 0.5f && Mathf.Abs(lastNavigate.y) <= 0.5f)
                MoveSelection(nav.y > 0f ? -1 : 1);

            if (Mathf.Abs(nav.x) > 0.5f && Mathf.Abs(lastNavigate.x) <= 0.5f)
                SwitchColumn(nav.x > 0f ? Column.Slots : Column.Pack);

            lastNavigate = nav;

            if (submitAction.WasPerformedThisFrame())
                HandleSubmit();

            if (cancelAction.WasPerformedThisFrame())
                Close();
        }

        private void MoveSelection(int delta)
        {
            int count = RowCount(selectedColumn);
            if (count == 0) return;
            selectedIndex = ((selectedIndex + delta) % count + count) % count;
            UpdateHighlight();
        }

        private void SwitchColumn(Column column)
        {
            if (selectedColumn == column) return;
            selectedColumn = column;
            selectedIndex = 0;
            UpdateHighlight();
        }

        private int RowCount(Column column) => column == Column.Pack
            ? (InventoryManager.Instance?.Stacks.Count ?? 0)
            : SlotCount + 1; // 3 slots + Work

        private void HandleSubmit()
        {
            if (selectedColumn == Column.Pack)
            {
                PlaceSelectedPackItem();
            }
            else
            {
                if (selectedIndex == WorkRowIndex)
                    AttemptWork();
                else
                    RemoveSlotItem(selectedIndex);
            }
        }

        private void PlaceSelectedPackItem()
        {
            var stacks = InventoryManager.Instance?.Stacks;
            if (stacks == null || selectedIndex >= stacks.Count) return;

            var item = stacks[selectedIndex].Item;

            int emptySlot = -1;
            for (int i = 0; i < SlotCount; i++)
            {
                if (placedItems[i] == null) { emptySlot = i; break; }
            }

            if (emptySlot < 0) return;

            placedItems[emptySlot] = item;
            Refresh();
        }

        private void RemoveSlotItem(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= SlotCount) return;
            placedItems[slotIndex] = null;
            Refresh();
        }

        private void AttemptWork()
        {
            if (DayCycleManager.Instance != null && !DayCycleManager.Instance.HasDaylight)
            {
                NotificationManager.Post($"{StationName}: too dark to work now.");
                return;
            }

            bool hasAnyPlaced = false;
            for (int i = 0; i < SlotCount; i++)
                if (placedItems[i] != null) { hasAnyPlaced = true; break; }

            if (!hasAnyPlaced)
            {
                NotificationManager.Post($"{StationName}: place something to work with.");
                return;
            }

            DayCycleManager.Instance?.SpendDaylight();

            if (TryResolveRecipe(out var matched, out var matchedSlots))
            {
                HandleSuccess(matched, matchedSlots);
            }
            else
            {
                HandleFailure();
            }
        }

        private bool TryResolveRecipe(out ConversionDef matched, out int[] matchedSlots)
        {
            foreach (var recipe in recipes)
            {
                if (recipe == null || !IsRecipeInStationBias(recipe)) continue;
                if (recipe.TryMatch(placedItems, out matchedSlots))
                {
                    matched = recipe;
                    return true;
                }
            }

            matched = null;
            matchedSlots = null;
            return false;
        }

        // Iteration 39: recipes partition across stations by property bias. A
        // property-keyed recipe resolves only where the station can resolve every
        // property it hinges on. An item-keyed recipe hooks in through its input
        // items' latent properties — available where at least one is in the bias
        // (or everywhere, if its items carry no properties at all).
        private bool IsRecipeInStationBias(ConversionDef recipe)
        {
            bool anyPropertyInput = false;
            foreach (var input in recipe.inputs)
            {
                if (input.kind != ConversionDef.Input.Kind.Property) continue;
                anyPropertyInput = true;
                if (!stationBias.Contains(input.propertyId)) return false;
            }

            if (anyPropertyInput) return true;

            bool anyItemProperty = false;
            foreach (var input in recipe.inputs)
            {
                var ids = input.item != null ? input.item.PropertyIds : null;
                if (ids == null) continue;
                foreach (var pid in ids)
                {
                    anyItemProperty = true;
                    if (stationBias.Contains(pid)) return true;
                }
            }

            return !anyItemProperty;
        }

        private void HandleSuccess(ConversionDef recipe, int[] matchedSlots)
        {
            var discoveredItemIds = new List<string>();

            // Consume matched inputs and reveal their properties (bias-filtered).
            for (int i = 0; i < recipe.inputs.Length; i++)
            {
                var input = recipe.inputs[i];

                if (input.kind == ConversionDef.Input.Kind.Item)
                {
                    // Remove required quantity from inventory (item-keyed).
                    ConsumeFromInventory(input.item, input.quantity);
                    RevealBiasedProperties(input.item, discoveredItemIds);

                    // Clear the matching slots for item-keyed inputs.
                    int needed = input.quantity;
                    for (int j = 0; j < SlotCount && needed > 0; j++)
                    {
                        if (placedItems[j] == input.item) { placedItems[j] = null; needed--; }
                    }
                }
                else
                {
                    // Property-keyed: consume the specific item in the matched slot.
                    int slot = matchedSlots[i];
                    var item = placedItems[slot];
                    if (item != null)
                    {
                        ConsumeFromInventory(item, 1);
                        RevealBiasedProperties(item, discoveredItemIds);
                        placedItems[slot] = null;
                    }
                }
            }

            // Add output to inventory.
            if (recipe.outputItem != null)
                InventoryManager.Instance?.AddItem(recipe.outputItem, recipe.outputQuantity);

            // Post result notification.
            string flavor = !string.IsNullOrEmpty(recipe.flavorText)
                ? recipe.flavorText
                : $"yields {recipe.outputItem?.DisplayName ?? "something"}.";
            NotificationManager.Post($"{StationName}: {flavor}");

            // Trigger stage-cross tier signal on the station entity.
            currentStation?.StationObject?.GetComponent<Mossmark.Visuals.EntityFeedback>()?.TriggerPop();

            // Highlight newly discovered items then fade after 1.5s.
            foreach (var id in discoveredItemIds)
                highlightDiscoveredItemIds.Add(id);

            Refresh();
            ScheduleHighlightClear();
        }

        private void HandleFailure()
        {
            // Try to reveal one unknown property from placed items — only properties
            // this station can resolve (Iteration 39): off-bias properties can't be
            // stumbled onto here, which is what makes visiting another station matter.
            var candidates = new List<(ItemDefinition item, string propId)>();
            for (int i = 0; i < SlotCount; i++)
            {
                var item = placedItems[i];
                if (item?.PropertyIds == null) continue;
                foreach (var pid in item.PropertyIds)
                {
                    if (stationBias.Contains(pid) && !PropertyKnowledge.IsKnown(item.ItemId, pid))
                        candidates.Add((item, pid));
                }
            }

            if (candidates.Count > 0)
            {
                var pick = candidates[Random.Range(0, candidates.Count)];
                PropertyKnowledge.MarkKnown(pick.item.ItemId, pick.propId);

                var def = PropertyRegistry.GetById(pick.propId);
                string phrase = def != null ? def.Phrase : pick.propId;
                NotificationManager.Post($"{StationName}: nothing comes of it — though you notice {pick.item.DisplayName} {phrase}.");

                highlightDiscoveredItemIds.Add(pick.item.ItemId);
                Refresh();
                ScheduleHighlightClear();
            }
            else
            {
                NotificationManager.Post($"{StationName}: nothing comes of it.");
                Refresh();
            }
        }

        private static void ConsumeFromInventory(ItemDefinition item, int qty)
        {
            InventoryManager.Instance?.RemoveItem(item, qty);
        }

        private void RevealBiasedProperties(ItemDefinition item, List<string> discoveredOut)
        {
            if (item?.PropertyIds == null) return;
            foreach (var pid in item.PropertyIds)
            {
                if (stationBias.Contains(pid) && !PropertyKnowledge.IsKnown(item.ItemId, pid))
                {
                    PropertyKnowledge.MarkKnown(item.ItemId, pid);
                    discoveredOut.Add(item.ItemId);
                }
            }
        }

        private void ScheduleHighlightClear()
        {
            if (pendingHighlightClear) return;
            pendingHighlightClear = true;
            root.schedule.Execute(() =>
            {
                highlightDiscoveredItemIds.Clear();
                pendingHighlightClear = false;
                if (IsOpen) Refresh();
            }).StartingIn(1500);
        }

        private void HandleInventoryChanged()
        {
            if (IsOpen) Refresh();
        }

        private void Refresh()
        {
            RefreshPack();
            RefreshSlots();

            int count = RowCount(selectedColumn);
            if (selectedIndex >= count) selectedIndex = Mathf.Max(0, count - 1);
            UpdateHighlight();
        }

        private void RefreshPack()
        {
            packList.Clear();
            packRows.Clear();

            var stacks = InventoryManager.Instance?.Stacks;
            if (stacks == null || stacks.Count == 0)
            {
                packList.Add(BuildRow("(empty)", false));
                return;
            }

            foreach (var stack in stacks)
            {
                string text = $"{stack.Item.DisplayName} x{stack.Quantity}/{stack.Item.StackCap}";
                bool discovered = highlightDiscoveredItemIds.Contains(stack.Item.ItemId);
                var row = BuildRow(text, discovered);
                packRows.Add(row);
                packList.Add(row);
            }
        }

        private void RefreshSlots()
        {
            slotsList.Clear();
            slotRows.Clear();

            for (int i = 0; i < SlotCount; i++)
            {
                var item = placedItems[i];
                string text = item != null ? item.DisplayName : "(empty)";
                bool discovered = item != null && highlightDiscoveredItemIds.Contains(item.ItemId);
                var row = BuildRow(text, discovered);
                slotRows.Add(row);
                slotsList.Add(row);
            }

            // Work button row.
            var workRow = BuildRow("[ Work ]", false);
            slotRows.Add(workRow);
            slotsList.Add(workRow);
        }

        private VisualElement BuildRow(string text, bool highlight)
        {
            var bg = highlight
                ? new Color(0.8f, 0.7f, 0.15f, 0.35f)
                : new Color(1f, 1f, 1f, 0.08f);

            var row = new VisualElement
            {
                style =
                {
                    paddingLeft = 6, paddingRight = 6,
                    paddingTop = 2, paddingBottom = 2,
                    marginBottom = 1,
                    backgroundColor = bg
                }
            };

            row.Add(new Label(text)
            {
                style = { color = Color.white, fontSize = 12, unityFontDefinition = fallbackFont }
            });

            return row;
        }

        private void UpdateHighlight()
        {
            HighlightRows(packRows, selectedColumn == Column.Pack);
            HighlightRows(slotRows, selectedColumn == Column.Slots);
        }

        private void HighlightRows(List<VisualElement> rows, bool isActive)
        {
            for (int i = 0; i < rows.Count; i++)
            {
                bool selected = isActive && i == selectedIndex;

                // Preserve gold highlight if it's also the selected row.
                bool isGold = rows[i].style.backgroundColor.value ==
                              new Color(0.8f, 0.7f, 0.15f, 0.35f);

                rows[i].style.backgroundColor = selected
                    ? new Color(1f, 1f, 1f, 0.3f)
                    : (isGold ? new Color(0.8f, 0.7f, 0.15f, 0.35f) : new Color(1f, 1f, 1f, 0.08f));
            }
        }

        // Helpers for building labeled visual elements.

        private Label MakeLabel(string text, int fontSize, FontStyle fontStyle = FontStyle.Normal,
            TextAnchor textAlign = TextAnchor.MiddleLeft, Color? color = null,
            int marginBottom = 0, int marginTop = 0)
        {
            return new Label(text)
            {
                style =
                {
                    color = color ?? Color.white,
                    fontSize = fontSize,
                    unityFontStyleAndWeight = fontStyle,
                    unityTextAlign = textAlign,
                    unityFontDefinition = fallbackFont,
                    marginBottom = marginBottom,
                    marginTop = marginTop
                }
            };
        }
    }
}
