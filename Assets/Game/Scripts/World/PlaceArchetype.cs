using Mossmark.Development;
using Mossmark.Inventory;
using UnityEngine;

namespace Mossmark.World
{
    // Iteration 12: a self-contained bundle of wilderness-spot data and the building/NPC
    // specializations that place implies, per IDEAS.md's "Place Archetypes" framing.
    //
    // Iteration 21: building section replaced with BuildingStageDef[] so any number of
    // stages can be authored without code changes. buildingDilapidatedName / Color
    // stay separate as the pre-revival identity.
    [CreateAssetMenu(fileName = "PlaceArchetype", menuName = "Mossmark/World/Place Archetype")]
    public class PlaceArchetype : ScriptableObject
    {
        [SerializeField] private string archetypeId;
        [SerializeField] private string displayName;

        [Header("Wilderness Spot")]
        [SerializeField] private string spotDisplayName;
        [SerializeField] private string spotVerb = "forage";
        [SerializeField] private Color spotColor = Color.white;
        [SerializeField] private ItemYield[] commonYields;
        [SerializeField] private ItemYield rareYield;
        [SerializeField, Range(0f, 1f)] private float rareDropChance = 0.08f;
        [SerializeField, Min(0.1f)] private float archetypeSpotMinTickInterval = 1.5f;
        [SerializeField, Min(0.1f)] private float archetypeSpotMaxTickInterval = 2f;

        [Header("Specialization")]
        [SerializeField] private string specializationId;
        [SerializeField] private string stageDisplayName;
        [SerializeField] private string npcTitle;
        [SerializeField] private Color npcTint = Color.white;

        [Header("Point of Interest")]
        [SerializeField] private string poiDisplayName;
        [SerializeField] private string poiLockedDescription;
        [SerializeField] private string poiVerb = "search";
        [SerializeField] private Color poiColor = Color.white;
        [SerializeField] private ItemYield[] poiCommonYields;
        [SerializeField] private ItemYield poiRareYield;
        [SerializeField, Range(0f, 1f)] private float poiRareDropChance = 0.05f;

        [Header("NPC Post-Spec Stages")]
        [SerializeField] private NpcPostSpecStageDef[] npcPostSpecStages = System.Array.Empty<NpcPostSpecStageDef>();

        [Header("Building")]
        [SerializeField] private string buildingDilapidatedName;
        [SerializeField] private Color buildingDilapidatedColor = new(0.35f, 0.3f, 0.28f, 1f);
        [SerializeField] private BuildingStageDef[] buildingStages = System.Array.Empty<BuildingStageDef>();

        public string ArchetypeId => archetypeId;
        public string DisplayName => displayName;

        public string SpotDisplayName => spotDisplayName;
        public string SpotVerb => spotVerb;
        public Color SpotColor => spotColor;
        public ItemYield[] CommonYields => commonYields;
        public ItemYield RareYield => rareYield;
        public float RareDropChance => rareDropChance;
        public float ArchetypeSpotMinTickInterval => archetypeSpotMinTickInterval;
        public float ArchetypeSpotMaxTickInterval => archetypeSpotMaxTickInterval;

        public string SpecializationId => specializationId;
        public string StageDisplayName => stageDisplayName;
        public string NpcTitle => npcTitle;
        public Color NpcTint => npcTint;

        public string PoiDisplayName => poiDisplayName;
        public string PoiLockedDescription => poiLockedDescription;
        public string PoiVerb => poiVerb;
        public Color PoiColor => poiColor;
        public ItemYield[] PoiCommonYields => poiCommonYields;
        public ItemYield PoiRareYield => poiRareYield;
        public float PoiRareDropChance => poiRareDropChance;

        public NpcPostSpecStageDef[] NpcPostSpecStages => npcPostSpecStages;

        public string BuildingDilapidatedName => buildingDilapidatedName;
        public Color BuildingDilapidatedColor => buildingDilapidatedColor;
        public BuildingStageDef[] BuildingStages => buildingStages;
    }
}
