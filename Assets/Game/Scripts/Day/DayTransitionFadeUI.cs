using UnityEngine;
using UnityEngine.UIElements;

namespace Mossmark.Day
{
    [RequireComponent(typeof(UIDocument))]
    public class DayTransitionFadeUI : MonoBehaviour
    {
        private VisualElement fadeOverlay;

        private void OnEnable()
        {
            var uiDocument = GetComponent<UIDocument>();

            // Same code-first runtime PanelSettings as the other overlays, but on top:
            // a high sortingOrder so the fade covers Overlay/Inventory/DayCycle UI panels.
            if (uiDocument.panelSettings == null)
            {
                var panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
                panelSettings.themeStyleSheet = Resources.Load<ThemeStyleSheet>("OverlayTheme");
                uiDocument.panelSettings = panelSettings;
            }

            uiDocument.sortingOrder = 100;

            BuildLayout(uiDocument.rootVisualElement);
        }

        private void BuildLayout(VisualElement root)
        {
            root.style.position = Position.Absolute;
            root.style.left = 0;
            root.style.right = 0;
            root.style.top = 0;
            root.style.bottom = 0;
            root.pickingMode = PickingMode.Ignore;

            fadeOverlay = new VisualElement
            {
                style =
                {
                    position = Position.Absolute,
                    left = 0,
                    right = 0,
                    top = 0,
                    bottom = 0,
                    backgroundColor = new Color(0f, 0f, 0f, 0f)
                },
                pickingMode = PickingMode.Ignore
            };

            root.Add(fadeOverlay);
        }

        // DayCycleManager.FadeAmount01 is read every frame, same "manager owns progress,
        // UI just displays it" pattern as AttendableOverlayUI's HoldProgress01 bar.
        private void Update()
        {
            var manager = DayCycleManager.Instance;
            float fade = manager != null ? manager.FadeAmount01 : 0f;
            fadeOverlay.style.backgroundColor = new Color(0f, 0f, 0f, fade);
        }
    }
}
