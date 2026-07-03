using UnityEngine;

namespace Mossmark.Development
{
    // Ordered list of BuildingStageDef references — the building-side analogue of
    // NpcStagePool (P1's UpgradePool shape). Stage order is track order: index 0 is
    // always the revival stage. The asset name is the pool's id (CSV `pool` column).
    [CreateAssetMenu(menuName = "Mossmark/Development/Building Stage Pool", fileName = "NewBuildingStagePool")]
    public class BuildingStagePool : ScriptableObject
    {
        [SerializeField] private BuildingStageDef[] stages = System.Array.Empty<BuildingStageDef>();

        public BuildingStageDef[] Stages => stages;
    }
}
