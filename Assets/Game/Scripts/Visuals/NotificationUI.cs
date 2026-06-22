using UnityEngine;
using UnityEngine.UIElements;

namespace Mossmark.Visuals
{
    [RequireComponent(typeof(UIDocument))]
    public class NotificationUI : MonoBehaviour
    {
        private const float DisplayDuration = 5f;

        private VisualElement notificationRoot;
        private Label messageLabel;
        private float timeRemaining;

        private void OnEnable()
        {
            var uiDocument = GetComponent<UIDocument>();

            if (uiDocument.panelSettings == null)
            {
                var panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
                panelSettings.themeStyleSheet = Resources.Load<ThemeStyleSheet>("OverlayTheme");
                uiDocument.panelSettings = panelSettings;
            }

            BuildLayout(uiDocument.rootVisualElement);
            NotificationManager.MessagePosted += ShowMessage;
        }

        private void OnDisable()
        {
            NotificationManager.MessagePosted -= ShowMessage;
        }

        private void BuildLayout(VisualElement root)
        {
            root.style.position = Position.Absolute;
            root.style.left = 0;
            root.style.right = 0;
            root.style.top = 0;
            root.style.bottom = 0;

            var font = FontDefinition.FromFont(Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"));

            notificationRoot = new VisualElement
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
                    backgroundColor = new Color(0f, 0f, 0f, 0.7f),
                    paddingLeft = 20,
                    paddingRight = 20,
                    paddingTop = 10,
                    paddingBottom = 10,
                    alignItems = Align.Center
                }
            };

            messageLabel = new Label
            {
                style =
                {
                    color = new Color(1f, 0.9f, 0.5f, 1f),
                    fontSize = 16,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    unityTextAlign = TextAnchor.MiddleCenter,
                    unityFontDefinition = font
                }
            };

            panel.Add(messageLabel);
            notificationRoot.Add(panel);
            root.Add(notificationRoot);
        }

        private void ShowMessage(string message)
        {
            messageLabel.text = message;
            timeRemaining = DisplayDuration;
            notificationRoot.style.display = DisplayStyle.Flex;
        }

        private void Update()
        {
            if (timeRemaining <= 0f) return;

            timeRemaining -= Time.deltaTime;
            if (timeRemaining <= 0f)
            {
                timeRemaining = 0f;
                notificationRoot.style.display = DisplayStyle.None;
            }
        }
    }
}
