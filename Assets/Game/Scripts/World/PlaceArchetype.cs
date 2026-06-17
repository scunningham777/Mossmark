using Mossmark.Inventory;
using UnityEngine;

namespace Mossmark.World
{
    // Iteration 12: a self-contained bundle of wilderness-spot data and the building/NPC
    // specializations that place implies, per IDEAS.md's "Place Archetypes" framing.
    //
    // Content pass: added Building section with stage 1 (revival) and stage 2 (development)
    // data. Stage 2 material reuses PoiCommonYields[0] — the former-rare wilderness item
    // that becomes the POI's common yield — so the building's second upgrade is naturally
    // gated behind POI access.
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

        // Building stage 1: revival via the archetype's wilderness-spot material.
        // Building stage 2: development via PoiCommonYields[0] — becomes available once
        // the NPC specializes (which also opens the POI), so the stage 2 material is
        // naturally something the player can now gather. WorldGenerator reads PoiCommonYields
        // directly for stage 2 material rather than duplicating the reference here.
        [Header("Building")]
        [SerializeField] private string buildingDilapidatedName;
        [SerializeField] private string buildingRevivedName;
        [SerializeField] private string buildingRepairVerb = "repair";
        [SerializeField] private Color buildingDilapidatedColor = new(0.35f, 0.3f, 0.28f, 1f);
        [SerializeField] private ItemDefinition buildingMaterial;
        [SerializeField, Min(1)] private int buildingMaterialCostPerTick = 2;
        [SerializeField, Min(1)] private int buildingProgressCost = 6;
        [SerializeField] private Color buildingRevivedTint = new(0.5f, 0.5f, 0.45f, 1f);
        [SerializeField] private string buildingStage2DisplayName;
        [SerializeField] private string buildingStage2Verb = "develop";
        [SerializeField, Min(1)] private int buildingStage2MaterialCostPerTick = 2;
        [SerializeField, Min(1)] private int buildingStage2ProgressCost = 4;
        [SerializeField] private Color buildingStage2Tint = Color.clear;

        public string ArchetypeId => archetypeId;
        public string DisplayName => displayName;

        public string SpotDisplayName => spotDisplayName;
        public string SpotVerb => spotVerb;
        public Color SpotColor => spotColor;
        public ItemYield[] CommonYields => commonYields;
        public ItemYield RareYield => rareYield;
        public float RareDropChance => rareDropChance;

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

        public string BuildingDilapidatedName => buildingDilapidatedName;
        public string BuildingRevivedName => buildingRevivedName;
        public string BuildingRepairVerb => buildingRepairVerb;
        public Color BuildingDilapidatedColor => buildingDilapidatedColor;
        public ItemDefinition BuildingMaterial => buildingMaterial;
        public int BuildingMaterialCostPerTick => buildingMaterialCostPerTick;
        public int BuildingProgressCost => buildingProgressCost;
        public Color BuildingRevivedTint => buildingRevivedTint;
        public string BuildingStage2DisplayName => buildingStage2DisplayName;
        public string BuildingStage2Verb => buildingStage2Verb;
        public int BuildingStage2MaterialCostPerTick => buildingStage2MaterialCostPerTick;
        public int BuildingStage2ProgressCost => buildingStage2ProgressCost;
        public Color BuildingStage2Tint => buildingStage2Tint;
    }
}
