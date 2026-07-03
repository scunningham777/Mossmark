using UnityEditor;

namespace Mossmark.Editor
{
    // MCP-driven play-mode toggles: execute_menu_item can't reach Unity's play controls,
    // so these expose them as plain menu items for automated verification runs.
    public static class PlayModeMenu
    {
        [MenuItem("Mossmark/Debug/Enter Play Mode")]
        public static void EnterPlay() => EditorApplication.EnterPlaymode();

        [MenuItem("Mossmark/Debug/Exit Play Mode")]
        public static void ExitPlay() => EditorApplication.ExitPlaymode();
    }
}
