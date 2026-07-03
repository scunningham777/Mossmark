using System;
using UnityEngine;

namespace Mossmark.Inventory
{
    [CreateAssetMenu(fileName = "ConversionDef", menuName = "Mossmark/Inventory/Conversion Definition")]
    public class ConversionDef : ScriptableObject
    {
        [Serializable]
        public class Input
        {
            public enum Kind { Item, Property }

            public Kind kind;
            public ItemDefinition item;
            [Min(1)] public int quantity = 1;
            public string propertyId;
        }

        [SerializeField] public Input[] inputs = Array.Empty<Input>();
        [SerializeField] public ItemDefinition outputItem;
        [SerializeField, Min(1)] public int outputQuantity = 1;
        [SerializeField] public string flavorText;

        // Returns true if all inputs can be satisfied by the given placed items.
        // For Item inputs: enough of the specific item must be present.
        // For Property inputs: exactly one item (not already consumed by a prior input)
        // carrying the required property must be present.
        public bool TryMatch(ItemDefinition[] placed, out int[] matchedSlots)
        {
            var remaining = new ItemDefinition[placed.Length];
            System.Array.Copy(placed, remaining, placed.Length);

            matchedSlots = new int[inputs.Length];

            for (int i = 0; i < inputs.Length; i++)
            {
                var input = inputs[i];

                if (input.kind == Input.Kind.Item)
                {
                    int needed = input.quantity;
                    for (int j = 0; j < remaining.Length && needed > 0; j++)
                    {
                        if (remaining[j] == input.item)
                        {
                            remaining[j] = null;
                            needed--;
                            matchedSlots[i] = j;
                        }
                    }

                    if (needed > 0) { matchedSlots = null; return false; }
                }
                else
                {
                    bool found = false;
                    for (int j = 0; j < remaining.Length; j++)
                    {
                        if (remaining[j] == null) continue;
                        var ids = remaining[j].PropertyIds;
                        if (ids == null) continue;
                        foreach (var pid in ids)
                        {
                            if (pid == input.propertyId)
                            {
                                matchedSlots[i] = j;
                                remaining[j] = null;
                                found = true;
                                break;
                            }
                        }

                        if (found) break;
                    }

                    if (!found) { matchedSlots = null; return false; }
                }
            }

            return true;
        }
    }
}
