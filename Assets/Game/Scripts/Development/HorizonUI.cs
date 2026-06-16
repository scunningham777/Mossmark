using Mossmark.Attention;
using Mossmark.Inventory;
using Mossmark.World;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace Mossmark.Development
{
    // Iteration 16's Settlement Horizon: a town-wide read of the same "needs" language the
    // per-attendable overlay already shows, per PROTOTYPE2.md's "Settlement Horizon
    // (Possibility Space)" - GetAvailableStages()/the development track itself is never
    // rendered as a player-selectable choice, only each entity's current
    // GetOverlayInteractionLine() (an action prompt, a "needs ..." line, or a post-
    // development "realized" message) plus the town-wide DeclaredSpecializationNeeds.
    [RequireComponent(typeof(UIDocument))]
    public class HorizonUI : MonoBehaviour
    {
        public static HorizonUI Instance { get; private set; }
        public bool IsOpen { get; private set; }

        private VisualElement root;
        private VisualElement entityList;
        private VisualElement needsList;
        private FontDefinition fallbackFont;
        private InputAction toggleAction;
        private Transform player;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            var gameplay = InputSystem.actions.FindActionMap("Gameplay");
            gameplay.Enable();
            toggleAction = gameplay.FindAction("Horizon");
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

            // Same menu-surface sorting as ChestUI - above the HUD overlays (default 0),
            // below DayTransitionFadeUI (100). The two never show at once (see Update()).
            uiDocument.sortingOrder = 50;

            fallbackFont = FontDefinition.FromFont(Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"));

            BuildLayout(uiDocument.rootVisualElement);
        }

        private void Start()
        {
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
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
                    minWidth = 360,
                    maxWidth = 440,
                    alignItems = Align.Center
                }
            };

            var title = new Label("Settlement Horizon")
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

            var hint = new Label("Tab to close")
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
            entityList = BuildSection("The Settlement", root);
            needsList = BuildSection("Town Needs", root);
            root.Add(hint);
            uiRoot.Add(root);
        }

        private VisualElement BuildSection(string heading, VisualElement parent)
        {
            var section = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Column,
                    alignItems = Align.Stretch,
                    width = Length.Percent(100),
                    marginBottom = 8
                }
            };

            var headingLabel = new Label(heading)
            {
                style =
                {
                    color = Color.white,
                    fontSize = 14,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    unityFontDefinition = fallbackFont,
                    marginBottom = 4
                }
            };

            var list = new VisualElement { style = { flexDirection = FlexDirection.Column } };

            section.Add(headingLabel);
            section.Add(list);
            parent.Add(section);
            return list;
        }

        private VisualElement BuildEntityRow(string name, string status)
        {
            var row = new VisualElement
            {
                style =
                {
                    paddingLeft = 6,
                    paddingRight = 6,
                    paddingTop = 2,
                    paddingBottom = 2,
                    marginBottom = 2,
                    backgroundColor = new Color(1f, 1f, 1f, 0.08f)
                }
            };

            row.Add(new Label(name)
            {
                style =
                {
                    color = Color.white,
                    fontSize = 13,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    unityFontDefinition = fallbackFont
                }
            });

            row.Add(new Label(status)
            {
                style =
                {
                    color = new Color(1f, 1f, 1f, 0.8f),
                    fontSize = 12,
                    unityFontDefinition = fallbackFont,
                    whiteSpace = WhiteSpace.Normal
                }
            });

            return row;
        }

        private Label BuildNeedLabel(string text)
        {
            return new Label(text)
            {
                style =
                {
                    color = Color.white,
                    fontSize = 12,
                    unityFontDefinition = fallbackFont,
                    whiteSpace = WhiteSpace.Normal,
                    marginBottom = 2
                }
            };
        }

        private void Update()
        {
            // The Horizon is town business - if the player wanders into the wilderness
            // while it's open, close it rather than leaving it stuck open out there.
            if (player != null && !WorldLayoutGenerator.IsInTown(player.position))
            {
                if (IsOpen)
                {
                    IsOpen = false;
                    root.style.display = DisplayStyle.None;
                }
                return;
            }

            if (!toggleAction.WasPerformedThisFrame()) return;

            // The chest menu (ChestUI) owns input while open; don't open the Horizon
            // underneath it. (AttentionManager separately refuses to open the chest
            // while the Horizon is open, so the reverse case can't arise.)
            if (!IsOpen && ChestUI.Instance != null && ChestUI.Instance.IsOpen) return;

            IsOpen = !IsOpen;
            root.style.display = IsOpen ? DisplayStyle.Flex : DisplayStyle.None;
            if (IsOpen) Refresh();
        }

        private void Refresh()
        {
            entityList.Clear();

            var entities = FindObjectsByType<DevelopableEntity>(FindObjectsInactive.Exclude);
            if (entities.Length == 0)
            {
                entityList.Add(BuildEntityRow("(nothing to report)", ""));
            }
            else
            {
                foreach (var entity in entities)
                {
                    string status = entity is IAttendable attendable ? attendable.GetOverlayInteractionLine() : "";
                    entityList.Add(BuildEntityRow(entity.DisplayName, status));
                }
            }

            needsList.Clear();

            if (DeclaredSpecializationNeeds.All.Count == 0)
            {
                needsList.Add(BuildNeedLabel("(nothing currently needed)"));
            }
            else
            {
                foreach (var id in DeclaredSpecializationNeeds.All)
                {
                    needsList.Add(BuildNeedLabel($"The town needs a {Humanize(id)}."));
                }
            }
        }

        private static string Humanize(string id)
        {
            var words = id.Split('_');

            for (int i = 0; i < words.Length; i++)
            {
                if (words[i].Length == 0) continue;
                words[i] = char.ToUpperInvariant(words[i][0]) + words[i][1..];
            }

            return string.Join(" ", words);
        }
    }
}
