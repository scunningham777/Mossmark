using UnityEngine;
using UnityEngine.UIElements;

namespace Mossmark.Day
{
    [RequireComponent(typeof(UIDocument))]
    public class DayCycleUI : MonoBehaviour
    {
        private Label phaseLabel;
        private Label ambientTextLabel;

        private void OnEnable()
        {
            var uiDocument = GetComponent<UIDocument>();

            // Same code-first approach as AttendableOverlayUI/InventoryUI: a runtime
            // PanelSettings using the project's shared minimal theme.
            if (uiDocument.panelSettings == null)
            {
                var panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
                panelSettings.themeStyleSheet = Resources.Load<ThemeStyleSheet>("OverlayTheme");
                uiDocument.panelSettings = panelSettings;
            }

            BuildLayout(uiDocument.rootVisualElement);
        }

        // DayCycleManager.Instance is read in Start, same Awake-ordering reasoning as
        // InventoryUI/AttentionManager - Start always runs after every Awake.
        private void Start()
        {
            var manager = DayCycleManager.Instance;
            if (manager != null)
            {
                manager.PhaseChanged += HandlePhaseChanged;
                manager.AmbientTextChanged += HandleAmbientTextChanged;

                UpdatePhaseLabel(manager.CurrentPhase);
            }
        }

        private void OnDisable()
        {
            var manager = DayCycleManager.Instance;
            if (manager == null) return;

            manager.PhaseChanged -= HandlePhaseChanged;
            manager.AmbientTextChanged -= HandleAmbientTextChanged;
        }

        private void BuildLayout(VisualElement root)
        {
            root.style.position = Position.Absolute;
            root.style.left = 0;
            root.style.right = 0;
            root.style.top = 0;
            root.style.bottom = 0;

            var fallbackFont = FontDefinition.FromFont(Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"));

            var container = new VisualElement
            {
                style =
                {
                    position = Position.Absolute,
                    top = 16,
                    right = 16,
                    maxWidth = 260,
                    alignItems = Align.FlexEnd
                }
            };

            phaseLabel = new Label
            {
                style =
                {
                    color = Color.white,
                    fontSize = 14,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    unityTextAlign = TextAnchor.MiddleRight,
                    unityFontDefinition = fallbackFont,
                    marginBottom = 2
                }
            };

            ambientTextLabel = new Label
            {
                style =
                {
                    color = Color.white,
                    fontSize = 12,
                    unityTextAlign = TextAnchor.UpperRight,
                    unityFontDefinition = fallbackFont,
                    whiteSpace = WhiteSpace.Normal
                }
            };

            container.Add(phaseLabel);
            container.Add(ambientTextLabel);
            root.Add(container);
        }

        private void HandlePhaseChanged(DayPhase phase) => UpdatePhaseLabel(phase);

        private void HandleAmbientTextChanged(string text) => ambientTextLabel.text = text;

        private void UpdatePhaseLabel(DayPhase phase) => phaseLabel.text = phase.ToString();
    }
}
