using UnityEngine;

namespace Mossmark.World
{
    // Ordered list of SpotStageDef references — mirrors NpcStagePool/BuildingStagePool.
    // A wilderness spot's Standing track is composed from this pool rather than embedding
    // stage rows, so future stages beyond Fen Bog's single authored Familiar stage can be
    // added without touching DevelopingWildernessSpotAttendable.
    [CreateAssetMenu(menuName = "Mossmark/World/Spot Stage Pool", fileName = "NewSpotStagePool")]
    public class SpotStagePool : ScriptableObject
    {
        [SerializeField] private SpotStageDef[] stages = System.Array.Empty<SpotStageDef>();

        public SpotStageDef[] Stages => stages;
    }
}
