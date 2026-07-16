using Mossmark.Development;
using Mossmark.Inventory;
using UnityEngine;
using UnityEngine.UIElements;

namespace Mossmark.Prototype3
{
    // Iteration 3.9: a glanceable, non-modal HUD strip answering "what am I carrying
    // knowledge of?" — the InventoryUI layout pattern (persistent, top-left, captures
    // no input), reading TakenLedger instead of InventoryManager.Stacks since P3 has no
    // pack. Property-line rendering is written fresh here rather than reusing the shared
    // ItemPropertyDisplay helper (Mossmark.Inventory), which is keyed off ItemDefinition;
    // TakenLedger.Entry carries only itemId/displayName/propertyIds, and the Reuse
    // Discipline's precedent (WorkingSurfaceAttendable inlines its own phrase lookup
    // rather than touching a Greybox-shared display path) applies here too.
    [RequireComponent(typeof(UIDocument))]
    public class TakenLedgerUI : MonoBehaviour
    {
        private VisualElement entriesContainer;
        private FontDefinition fallbackFont;

        private void OnEnable()
        {
            var uiDocument = GetComponent<UIDocument>();

            if (uiDocument.panelSettings == null)
            {
                var panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
                panelSettings.themeStyleSheet = Resources.Load<ThemeStyleSheet>("OverlayTheme");
                uiDocument.panelSettings = panelSettings;
            }

            fallbackFont = FontDefinition.FromFont(Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"));

            BuildLayout(uiDocument.rootVisualElement);
        }

        private void Start()
        {
            TakenLedger.Changed += Refresh;
            PropertyKnowledge.PropertyRevealed += Refresh;

            Refresh();
        }

        private void OnDisable()
        {
            TakenLedger.Changed -= Refresh;
            PropertyKnowledge.PropertyRevealed -= Refresh;
        }

        private void BuildLayout(VisualElement root)
        {
            root.style.position = Position.Absolute;
            root.style.left = 0;
            root.style.right = 0;
            root.style.top = 0;
            root.style.bottom = 0;

            var column = new VisualElement
            {
                style =
                {
                    position = Position.Absolute,
                    top = 16,
                    left = 16,
                    maxWidth = 220,
                    flexDirection = FlexDirection.Column
                }
            };

            var heading = new Label("What You've Taken")
            {
                style =
                {
                    color = new Color(1f, 1f, 1f, 0.6f),
                    fontSize = 11,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    unityFontDefinition = fallbackFont,
                    marginBottom = 4
                }
            };
            column.Add(heading);

            entriesContainer = new VisualElement { style = { flexDirection = FlexDirection.Column } };
            column.Add(entriesContainer);

            root.Add(column);
        }

        private void Refresh()
        {
            entriesContainer.Clear();

            foreach (var entry in TakenLedger.All)
            {
                entriesContainer.Add(BuildEntry(entry));
            }
        }

        private VisualElement BuildEntry(TakenLedger.Entry entry)
        {
            var block = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Column,
                    marginBottom = 6,
                    paddingLeft = 4,
                    paddingRight = 6,
                    paddingTop = 2,
                    paddingBottom = 2,
                    backgroundColor = new Color(0f, 0f, 0f, 0.5f)
                }
            };

            block.Add(new Label(entry.DisplayName)
            {
                style =
                {
                    color = Color.white,
                    fontSize = 12,
                    unityFontDefinition = fallbackFont
                }
            });

            bool anyUnknown = false;
            foreach (var propertyId in entry.PropertyIds)
            {
                if (PropertyKnowledge.IsKnown(entry.ItemId, propertyId))
                {
                    string text;
                    if (PropertyKnowledge.ShowDebugTags)
                        text = $"[{propertyId}]";
                    else
                    {
                        var def = PropertyRegistry.GetById(propertyId);
                        text = def != null ? def.Phrase : propertyId;
                    }
                    block.Add(BuildPropertyLabel(text));
                }
                else
                {
                    anyUnknown = true;
                }
            }

            if (anyUnknown)
                block.Add(BuildPropertyLabel("There's more to it."));

            return block;
        }

        private Label BuildPropertyLabel(string text) => new Label(text)
        {
            style =
            {
                color = new Color(0.7f, 0.82f, 0.7f, 0.85f),
                fontSize = 10,
                unityFontDefinition = fallbackFont,
                marginTop = 1,
                marginLeft = 2
            }
        };
    }
}
