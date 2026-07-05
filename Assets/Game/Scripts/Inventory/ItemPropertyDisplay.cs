using Mossmark.Development;
using UnityEngine;
using UnityEngine.UIElements;

namespace Mossmark.Inventory
{
    // Shared property-line rendering, used by both InventoryUI and ChestUI so
    // property discovery is visible wherever an item stack is listed.
    internal static class ItemPropertyDisplay
    {
        public static void AppendPropertyLines(VisualElement container, ItemDefinition item, FontDefinition font)
        {
            var ids = item.PropertyIds;
            if (ids == null || ids.Length == 0) return;

            bool anyUnknown = false;
            foreach (var pid in ids)
            {
                if (PropertyKnowledge.IsKnown(item.ItemId, pid))
                {
                    string text;
                    if (PropertyKnowledge.ShowDebugTags)
                        text = $"[{pid}]";
                    else
                    {
                        var def = PropertyRegistry.GetById(pid);
                        text = def != null ? def.Phrase : pid;
                    }
                    container.Add(MakeLabel(text, font));
                }
                else
                {
                    anyUnknown = true;
                }
            }

            if (anyUnknown)
                container.Add(MakeLabel("There's more to it.", font));
        }

        private static Label MakeLabel(string text, FontDefinition font) => new Label(text)
        {
            style =
            {
                color = new Color(0.7f, 0.82f, 0.7f, 0.85f),
                fontSize = 10,
                unityFontDefinition = font,
                marginTop = 1,
                marginLeft = 2,
            }
        };
    }
}
