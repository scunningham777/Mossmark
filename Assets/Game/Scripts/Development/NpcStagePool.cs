using UnityEngine;

namespace Mossmark.Development
{
    // Ordered list of NpcStageDef references — P2's analogue of P1's UpgradePool.
    // PlaceArchetype references a pool rather than embedding stage rows, so pools can be
    // composed from independently-authored stages and shared across archetypes. The asset
    // name is the pool's id (CSV `pool` column).
    [CreateAssetMenu(menuName = "Mossmark/Development/NPC Stage Pool", fileName = "NewNpcStagePool")]
    public class NpcStagePool : ScriptableObject
    {
        [SerializeField] private NpcStageDef[] stages = System.Array.Empty<NpcStageDef>();

        public NpcStageDef[] Stages => stages;
    }
}
