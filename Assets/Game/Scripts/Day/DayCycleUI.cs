using UnityEngine;
using UnityEngine.UIElements;

namespace Mossmark.Day
{
    [RequireComponent(typeof(UIDocument))]
    public class DayCycleUI : MonoBehaviour
    {
        private const int BarSegments = 20;

        private Label phaseLabel;
        private Label staminaLabel;

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
                manager.StaminaChanged += HandleStaminaChanged;
                manager.PhaseChanged += HandlePhaseChanged;

                Refresh(manager.StaminaRemaining, manager.MaxStamina);
                UpdatePhaseLabel(manager.CurrentPhase);
            }
        }

        private void OnDisable()
        {
            var manager = DayCycleManager.Instance;
            if (manager == null) return;

            manager.StaminaChanged -= HandleStaminaChanged;
            manager.PhaseChanged -= HandlePhaseChanged;
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

            staminaLabel = new Label
            {
                style =
                {
                    color = Color.white,
                    fontSize = 14,
                    unityTextAlign = TextAnchor.MiddleRight,
                    unityFontDefinition = fallbackFont
                }
            };

            container.Add(phaseLabel);
            container.Add(staminaLabel);
            root.Add(container);
        }

        private void HandleStaminaChanged(int remaining, int max) => Refresh(remaining, max);

        private void HandlePhaseChanged(DayPhase phase) => UpdatePhaseLabel(phase);

        private void Refresh(int remaining, int max)
        {
            int filled = Mathf.RoundToInt((float)remaining / max * BarSegments);
            staminaLabel.text = $"[{new string('#', filled)}{new string('.', BarSegments - filled)}]";
            staminaLabel.style.color = remaining > 0 ? Color.white : new Color(1f, 1f, 1f, 0.35f);
        }

        private void UpdatePhaseLabel(DayPhase phase) => phaseLabel.text = phase.ToString();
    }
}
