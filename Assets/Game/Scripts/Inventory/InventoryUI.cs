using UnityEngine;
using UnityEngine.UIElements;

namespace Mossmark.Inventory
{
    [RequireComponent(typeof(UIDocument))]
    public class InventoryUI : MonoBehaviour
    {
        private const int SlotSize = 14;

        private VisualElement slotsContainer;
        private FontDefinition fallbackFont;

        private void OnEnable()
        {
            var uiDocument = GetComponent<UIDocument>();

            // Same code-first approach as AttendableOverlayUI: a runtime PanelSettings using
            // the project's shared minimal theme, avoiding asset-reference wiring via MCP.
            if (uiDocument.panelSettings == null)
            {
                var panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
                panelSettings.themeStyleSheet = Resources.Load<ThemeStyleSheet>("OverlayTheme");
                uiDocument.panelSettings = panelSettings;
            }

            fallbackFont = FontDefinition.FromFont(Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"));

            BuildLayout(uiDocument.rootVisualElement);
        }

        // InventoryManager.Instance is read here rather than OnEnable: Awake/OnEnable order
        // across different GameObjects isn't guaranteed, but Start always runs after every
        // Awake, so InventoryManager.Awake has set Instance by this point (same pattern as
        // AttentionManager's Start-time lookups).
        private void Start()
        {
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.InventoryChanged += Refresh;
            }

            Refresh();
        }

        private void OnDisable()
        {
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.InventoryChanged -= Refresh;
            }
        }

        private void BuildLayout(VisualElement root)
        {
            root.style.position = Position.Absolute;
            root.style.left = 0;
            root.style.right = 0;
            root.style.top = 0;
            root.style.bottom = 0;

            slotsContainer = new VisualElement
            {
                style =
                {
                    position = Position.Absolute,
                    top = 16,
                    left = 16,
                    flexDirection = FlexDirection.Column
                }
            };

            root.Add(slotsContainer);
        }

        private void Refresh()
        {
            var manager = InventoryManager.Instance;
            if (manager == null) return;

            slotsContainer.Clear();

            for (int i = 0; i < manager.CarryLimit; i++)
            {
                slotsContainer.Add(BuildSlot(i < manager.Stacks.Count ? manager.Stacks[i] : null));
            }
        }

        private VisualElement BuildSlot(InventoryStack stack)
        {
            var row = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                    marginBottom = 2,
                    paddingLeft = 4,
                    paddingRight = 8,
                    paddingTop = 2,
                    paddingBottom = 2,
                    backgroundColor = new Color(0f, 0f, 0f, 0.5f)
                }
            };

            var swatch = new VisualElement
            {
                style =
                {
                    width = SlotSize,
                    height = SlotSize,
                    marginRight = 6,
                    backgroundColor = stack != null ? stack.Item.Color : new Color(1f, 1f, 1f, 0.15f)
                }
            };

            var label = new Label(stack != null ? $"{stack.Item.DisplayName} x{stack.Quantity}/{stack.Item.StackCap}" : "--")
            {
                style =
                {
                    color = Color.white,
                    fontSize = 12,
                    unityFontDefinition = fallbackFont
                }
            };

            row.Add(swatch);
            row.Add(label);
            return row;
        }
    }
}
