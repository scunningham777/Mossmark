using UnityEngine;

namespace Mossmark.Inventory
{
    [CreateAssetMenu(fileName = "ItemDefinition", menuName = "Mossmark/Inventory/Item Definition")]
    public class ItemDefinition : ScriptableObject
    {
        [SerializeField] private string itemId;
        [SerializeField] private string displayName;
        [SerializeField] private Color color = Color.white;
        [SerializeField] private int stackCap = 8;
        [SerializeField] private string[] propertyIds = {};

        public string ItemId => itemId;
        public string DisplayName => displayName;
        public Color Color => color;
        public int StackCap => stackCap;
        public string[] PropertyIds => propertyIds;
    }
}
