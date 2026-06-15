using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace Mossmark.Inventory
{
    // The one remaining menu surface, per the design draft - deposit/withdraw at the
    // settlement chest. Reads the project-wide "UI" action map's Navigate/Submit/Cancel
    // directly (same raw-InputAction approach as AttentionInput), rather than going
    // through UI Toolkit's focus/event system - no EventSystem/InputSystemUIInputModule
    // exists in the scene, consistent with the rest of this project's code-first UI.
    [RequireComponent(typeof(UIDocument))]
    public class ChestUI : MonoBehaviour
    {
        private enum Column { Pack, Chest }

        public static ChestUI Instance { get; private set; }
        public bool IsOpen { get; private set; }

        private ChestAttendable currentChest;
        private VisualElement root;
        private VisualElement packList;
        private VisualElement chestList;
        private FontDefinition fallbackFont;

        private InputAction navigateAction;
        private InputAction submitAction;
        private InputAction cancelAction;
        private Vector2 lastNavigate;

        private Column selectedColumn;
        private int selectedIndex;

        private readonly List<VisualElement> packRows = new();
        private readonly List<VisualElement> chestRows = new();

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

            // Above the other overlays (default sortingOrder 0) but below DayTransitionFadeUI
            // (100), so a day-transition fade still covers it if it's ever left open.
            uiDocument.sortingOrder = 50;

            fallbackFont = FontDefinition.FromFont(Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"));

            BuildLayout(uiDocument.rootVisualElement);
        }

        private void Start()
        {
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.InventoryChanged += HandleInventoryChanged;
            }
        }

        private void OnDisable()
        {
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.InventoryChanged -= HandleInventoryChanged;
            }
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
                    paddingLeft = 16,
                    paddingRight = 16,
                    paddingTop = 12,
                    paddingBottom = 12,
                    alignItems = Align.Center
                }
            };

            var title = new Label("Settlement Chest")
            {
                style =
                {
                    color = Color.white,
                    fontSize = 18,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    unityTextAlign = TextAnchor.MiddleCenter,
                    unityFontDefinition = fallbackFont,
                    marginBottom = 8
                }
            };

            var columns = new VisualElement { style = { flexDirection = FlexDirection.Row } };

            packList = BuildColumn("Pack", columns);
            chestList = BuildColumn("Chest", columns);

            var hint = new Label("W/S select - A/D switch - Enter transfer - Esc close")
            {
                style =
                {
                    color = new Color(1f, 1f, 1f, 0.7f),
                    fontSize = 12,
                    unityTextAlign = TextAnchor.MiddleCenter,
                    unityFontDefinition = fallbackFont,
                    marginTop = 8
                }
            };

            root.Add(title);
            root.Add(columns);
            root.Add(hint);
            uiRoot.Add(root);
        }

        private VisualElement BuildColumn(string heading, VisualElement parent)
        {
            var column = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Column,
                    minWidth = 220,
                    marginLeft = 12,
                    marginRight = 12
                }
            };

            var headingLabel = new Label(heading)
            {
                style =
                {
                    color = Color.white,
                    fontSize = 14,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    unityTextAlign = TextAnchor.MiddleCenter,
                    unityFontDefinition = fallbackFont,
                    marginBottom = 4
                }
            };

            var list = new VisualElement { style = { flexDirection = FlexDirection.Column } };

            column.Add(headingLabel);
            column.Add(list);
            parent.Add(column);
            return list;
        }

        public void Open(ChestAttendable chest)
        {
            currentChest = chest;
            IsOpen = true;
            selectedColumn = Column.Pack;
            selectedIndex = 0;
            lastNavigate = Vector2.zero;
            root.style.display = DisplayStyle.Flex;
            Refresh();
        }

        public void Close()
        {
            IsOpen = false;
            currentChest = null;
            root.style.display = DisplayStyle.None;
        }

        private void Update()
        {
            if (!IsOpen) return;

            Vector2 nav = navigateAction.ReadValue<Vector2>();

            if (Mathf.Abs(nav.y) > 0.5f && Mathf.Abs(lastNavigate.y) <= 0.5f)
            {
                MoveSelection(nav.y > 0f ? -1 : 1);
            }

            if (Mathf.Abs(nav.x) > 0.5f && Mathf.Abs(lastNavigate.x) <= 0.5f)
            {
                SwitchColumn(nav.x > 0f ? Column.Chest : Column.Pack);
            }

            lastNavigate = nav;

            if (submitAction.WasPerformedThisFrame())
            {
                HandleSubmit();
            }

            if (cancelAction.WasPerformedThisFrame())
            {
                Close();
            }
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
            ? InventoryManager.Instance.Stacks.Count
            : currentChest.Stacks.Count;

        // Pack -> Chest deposits a whole stack (the chest has no cap); Chest -> Pack
        // withdraws up to one player-stack-cap's worth, per the design draft - repeated
        // withdrawals are needed to drain a larger chest stack.
        private void HandleSubmit()
        {
            if (selectedColumn == Column.Pack)
            {
                var stacks = InventoryManager.Instance.Stacks;
                if (selectedIndex >= stacks.Count) return;

                var stack = stacks[selectedIndex];
                int removed = InventoryManager.Instance.RemoveItem(stack.Item, stack.Quantity);
                currentChest.Deposit(stack.Item, removed);
            }
            else
            {
                var stacks = currentChest.Stacks;
                if (selectedIndex >= stacks.Count) return;

                var stack = stacks[selectedIndex];
                int amount = Mathf.Min(stack.Quantity, stack.Item.StackCap);
                int added = InventoryManager.Instance.AddItem(stack.Item, amount);
                currentChest.Withdraw(stack.Item, added);
            }

            Refresh();
        }

        private void HandleInventoryChanged()
        {
            if (IsOpen) Refresh();
        }

        private void Refresh()
        {
            RefreshColumn(packList, packRows, InventoryManager.Instance.Stacks, true);
            RefreshColumn(chestList, chestRows, currentChest.Stacks, false);

            int count = RowCount(selectedColumn);
            if (selectedIndex >= count) selectedIndex = Mathf.Max(0, count - 1);

            UpdateHighlight();
        }

        private void RefreshColumn(VisualElement list, List<VisualElement> rows, IReadOnlyList<InventoryStack> stacks, bool showCap)
        {
            list.Clear();
            rows.Clear();

            if (stacks.Count == 0)
            {
                list.Add(BuildRow("(empty)"));
                return;
            }

            foreach (var stack in stacks)
            {
                string text = showCap
                    ? $"{stack.Item.DisplayName} x{stack.Quantity}/{stack.Item.StackCap}"
                    : $"{stack.Item.DisplayName} x{stack.Quantity}";

                var row = BuildRow(text);
                rows.Add(row);
                list.Add(row);
            }
        }

        private VisualElement BuildRow(string text)
        {
            var row = new VisualElement
            {
                style =
                {
                    paddingLeft = 6,
                    paddingRight = 6,
                    paddingTop = 2,
                    paddingBottom = 2,
                    marginBottom = 1,
                    backgroundColor = new Color(1f, 1f, 1f, 0.08f)
                }
            };

            row.Add(new Label(text)
            {
                style =
                {
                    color = Color.white,
                    fontSize = 12,
                    unityFontDefinition = fallbackFont
                }
            });

            return row;
        }

        private void UpdateHighlight()
        {
            HighlightColumn(packRows, selectedColumn == Column.Pack);
            HighlightColumn(chestRows, selectedColumn == Column.Chest);
        }

        private void HighlightColumn(List<VisualElement> rows, bool isActiveColumn)
        {
            for (int i = 0; i < rows.Count; i++)
            {
                bool selected = isActiveColumn && i == selectedIndex;
                rows[i].style.backgroundColor = selected
                    ? new Color(1f, 1f, 1f, 0.3f)
                    : new Color(1f, 1f, 1f, 0.08f);
            }
        }
    }
}
