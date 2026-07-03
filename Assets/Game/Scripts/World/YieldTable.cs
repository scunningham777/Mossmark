using UnityEngine;

namespace Mossmark.World
{
    // Relational-data migration: an ID-addressable weighted yield pool that any number
    // of owners can reference — P2's analogue of P1's LootTable. The asset name is the
    // table's id (CSV files reference tables by asset name, the same key every other
    // P2 asset uses); there is deliberately no separate id field to drift out of sync.
    [CreateAssetMenu(menuName = "Mossmark/World/Yield Table", fileName = "NewYieldTable")]
    public class YieldTable : ScriptableObject
    {
        [SerializeField] private ItemYield[] entries = System.Array.Empty<ItemYield>();

        public ItemYield[] Entries => entries;
    }
}
