using Mossmark.Day;
using Mossmark.Development;
using Mossmark.Inventory;
using UnityEngine;
using UnityEngine.UIElements;

namespace Mossmark.Attention
{
    [RequireComponent(typeof(UIDocument))]
    public class AttendableOverlayUI : MonoBehaviour
    {
        private const int ProgressBarSegments = 10;
        // World-space units to offset the label above the entity's pivot.
        private const float WorldLabelOffset = 1.5f;

        private Label descriptionLabel;
        private Label interactionLabel;
        private VisualElement overlayRoot;
        private UIDocument uiDocument;

        // Bottom-right detail panel
        private VisualElement detailRoot;
        private Label detailDescriptionLabel;
        private VisualElement upgradesContainer;
        private IAttendable cachedDetailTarget;
        private FontDefinition fallbackFont;

        private void OnEnable()
        {
            uiDocument = GetComponent<UIDocument>();

            // Created at runtime rather than referencing a PanelSettings asset, keeping this
            // overlay fully code-first with no asset wiring required. A theme style sheet is
            // mandatory though — without one, UIDocument logs a warning and the panel doesn't
            // render at all — so load the minimal one in Assets/Game/UI/Resources/.
            if (uiDocument.panelSettings == null)
            {
                var panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
                panelSettings.themeStyleSheet = Resources.Load<ThemeStyleSheet>("OverlayTheme");
                uiDocument.panelSettings = panelSettings;
            }

            BuildLayout(uiDocument.rootVisualElement);
        }

        private void BuildLayout(VisualElement root)
        {
            // UIDocument's rootVisualElement has no explicit size by default, so it collapses
            // to zero height within the panel's layout. overlayRoot's absolute position then
            // resolves against that zero-height containing block and lands off-screen.
            // Stretch root to fill the panel so descendants position correctly.
            root.style.position = Position.Absolute;
            root.style.left = 0;
            root.style.right = 0;
            root.style.top = 0;
            root.style.bottom = 0;

            // The minimal theme above defines no default font, so set one explicitly —
            // otherwise label text renders invisibly even though layout/background show up.
            fallbackFont = FontDefinition.FromFont(Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"));

            overlayRoot = new VisualElement
            {
                style =
                {
                    position = Position.Absolute,
                    display = DisplayStyle.None,
                    // Shift left by half its own width so it centers on the anchor point
                    // set each frame in Update() via style.left.
                    translate = new StyleTranslate(new Translate(Length.Percent(-50), 0))
                }
            };

            var panel = new VisualElement
            {
                style =
                {
                    backgroundColor = new Color(0f, 0f, 0f, 0.6f),
                    paddingLeft = 16,
                    paddingRight = 16,
                    paddingTop = 8,
                    paddingBottom = 8,
                    alignItems = Align.Center
                }
            };

            descriptionLabel = new Label
            {
                style =
                {
                    color = Color.white,
                    fontSize = 18,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    unityTextAlign = TextAnchor.MiddleCenter,
                    unityFontDefinition = fallbackFont
                }
            };

            interactionLabel = new Label
            {
                style =
                {
                    color = Color.white,
                    fontSize = 14,
                    unityTextAlign = TextAnchor.MiddleCenter,
                    marginTop = 4,
                    unityFontDefinition = fallbackFont
                }
            };

            panel.Add(descriptionLabel);
            panel.Add(interactionLabel);
            overlayRoot.Add(panel);
            root.Add(overlayRoot);

            BuildDetailPanel(root);
        }

        private void BuildDetailPanel(VisualElement root)
        {
            detailRoot = new VisualElement
            {
                style =
                {
                    position = Position.Absolute,
                    right = 20,
                    bottom = 20,
                    maxWidth = Length.Percent(33),
                    display = DisplayStyle.None,
                }
            };

            var detailPanel = new VisualElement
            {
                style =
                {
                    backgroundColor = new Color(0f, 0f, 0f, 0.6f),
                    paddingLeft = 16,
                    paddingRight = 16,
                    paddingTop = 8,
                    paddingBottom = 8,
                }
            };

            detailDescriptionLabel = new Label
            {
                style =
                {
                    color = Color.white,
                    fontSize = 16,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    unityFontDefinition = fallbackFont,
                }
            };

            upgradesContainer = new VisualElement
            {
                style = { display = DisplayStyle.None }
            };

            detailPanel.Add(detailDescriptionLabel);
            detailPanel.Add(upgradesContainer);
            detailRoot.Add(detailPanel);
            root.Add(detailRoot);
        }

        private void Update()
        {
            // The chest menu (ChestUI) covers the screen while open; don't show the
            // attendable overlay underneath it.
            if (ChestUI.Instance != null && ChestUI.Instance.IsOpen)
            {
                overlayRoot.style.display = DisplayStyle.None;
                detailRoot.style.display = DisplayStyle.None;
                return;
            }

            // The Horizon panel (HorizonUI) covers the screen while open; same reasoning.
            if (HorizonUI.Instance != null && HorizonUI.Instance.IsOpen)
            {
                overlayRoot.style.display = DisplayStyle.None;
                detailRoot.style.display = DisplayStyle.None;
                return;
            }

            // The Workshop menu (WorkshopUI) covers the screen while open; same reasoning.
            if (WorkshopUI.Instance != null && WorkshopUI.Instance.IsOpen)
            {
                overlayRoot.style.display = DisplayStyle.None;
                detailRoot.style.display = DisplayStyle.None;
                return;
            }

            var manager = AttentionManager.Instance;

            var target = manager == null ? null
                : manager.State == AttentionState.Attending ? manager.AttendingTarget
                : manager.CurrentTarget;

            if (target == null)
            {
                overlayRoot.style.display = DisplayStyle.None;
                detailRoot.style.display = DisplayStyle.None;
                cachedDetailTarget = null;
                return;
            }

            // Position the panel above the target in world space each frame.
            if (target is Component c && Camera.main != null && uiDocument.rootVisualElement.panel != null)
            {
                var worldAbove = c.transform.position + new Vector3(0f, WorldLabelOffset, 0f);
                var panelPos = RuntimePanelUtils.CameraTransformWorldToPanel(
                    uiDocument.rootVisualElement.panel, worldAbove, Camera.main);
                overlayRoot.style.left = panelPos.x;
                overlayRoot.style.top = panelPos.y;
            }

            overlayRoot.style.display = DisplayStyle.Flex;
            descriptionLabel.text = target.GetShortName();
            interactionLabel.text = manager.State == AttentionState.Attending
                ? BuildProgressBar(manager.HoldProgress01)
                : GetInteractionLine(target);

            UpdateDetailPanel(target);
        }

        private void UpdateDetailPanel(IAttendable target)
        {
            detailDescriptionLabel.text = target.GetOverlayDescription();

            var upgrades = target.GetAppliedUpgrades();
            if (upgrades.Count > 0)
            {
                if (cachedDetailTarget != target || upgradesContainer.childCount != upgrades.Count)
                {
                    upgradesContainer.Clear();
                    foreach (var upgrade in upgrades)
                    {
                        upgradesContainer.Add(new Label($"• {upgrade}")
                        {
                            style =
                            {
                                color = new Color(0.85f, 0.85f, 0.85f, 1f),
                                fontSize = 13,
                                marginTop = 3,
                                unityFontDefinition = fallbackFont,
                            }
                        });
                    }
                }
                upgradesContainer.style.marginTop = 6;
                upgradesContainer.style.display = DisplayStyle.Flex;
            }
            else
            {
                upgradesContainer.style.display = DisplayStyle.None;
            }

            cachedDetailTarget = target;
            detailRoot.style.display = DisplayStyle.Flex;
        }

        private static string GetInteractionLine(IAttendable target)
        {
            var dayCycle = DayCycleManager.Instance;
            if (target.RequiresDaylight && dayCycle != null && !dayCycle.HasDaylight)
            {
                return "Too late to start that now.";
            }

            return target.GetOverlayInteractionLine();
        }

        private static string BuildProgressBar(float progress01)
        {
            int filled = Mathf.RoundToInt(Mathf.Clamp01(progress01) * ProgressBarSegments);
            return $"[{new string('#', filled)}{new string('.', ProgressBarSegments - filled)}]";
        }
    }
}
