using UnityEngine;
using UnityEngine.UIElements;

namespace Mossmark.Attention
{
    [RequireComponent(typeof(UIDocument))]
    public class AttendableOverlayUI : MonoBehaviour
    {
        private const int ProgressBarSegments = 10;

        private Label descriptionLabel;
        private Label interactionLabel;
        private VisualElement overlayRoot;

        private void OnEnable()
        {
            var uiDocument = GetComponent<UIDocument>();

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
            // to zero height within the panel's layout. overlayRoot's absolute "bottom: 64"
            // then resolves against that zero-height containing block and lands off-screen.
            // Stretch root to fill the panel so descendants position correctly.
            root.style.position = Position.Absolute;
            root.style.left = 0;
            root.style.right = 0;
            root.style.top = 0;
            root.style.bottom = 0;

            // The minimal theme above defines no default font, so set one explicitly —
            // otherwise label text renders invisibly even though layout/background show up.
            var fallbackFont = FontDefinition.FromFont(Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"));

            overlayRoot = new VisualElement
            {
                style =
                {
                    position = Position.Absolute,
                    left = 0,
                    right = 0,
                    bottom = 64,
                    alignItems = Align.Center,
                    display = DisplayStyle.None
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
        }

        private void Update()
        {
            var manager = AttentionManager.Instance;

            var target = manager == null ? null
                : manager.State == AttentionState.Attending ? manager.AttendingTarget
                : manager.CurrentTarget;

            if (target == null)
            {
                overlayRoot.style.display = DisplayStyle.None;
                return;
            }

            overlayRoot.style.display = DisplayStyle.Flex;
            descriptionLabel.text = target.GetOverlayDescription();
            interactionLabel.text = manager.State == AttentionState.Attending
                ? BuildProgressBar(manager.HoldProgress01)
                : target.GetOverlayInteractionLine();
        }

        private static string BuildProgressBar(float progress01)
        {
            int filled = Mathf.RoundToInt(Mathf.Clamp01(progress01) * ProgressBarSegments);
            return $"[{new string('#', filled)}{new string('.', ProgressBarSegments - filled)}]";
        }
    }
}
