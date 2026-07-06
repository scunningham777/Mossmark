using Mossmark.Development;
using Mossmark.Inventory;
using UnityEngine;

namespace Mossmark.World
{
    // Iteration 12: a self-contained bundle of wilderness-spot data and the building/NPC
    // specializations that place implies, per IDEAS.md's "Place Archetypes" framing.
    //
    // Relational-data migration: this is a thin composition root now — it bundles
    // independently-real pieces (spot definition assets, NPC/building stage pools) by
    // reference rather than embedding their data. Only what is genuinely archetype-level
    // (identity, specialization, POI, exchange flavor, maintenance) lives inline.
    [CreateAssetMenu(fileName = "PlaceArchetype", menuName = "Mossmark/World/Place Archetype")]
    public class PlaceArchetype : ScriptableObject
    {
        [SerializeField] private string archetypeId;
        [SerializeField] private string displayName;

        // Relational-data migration: the old inline spot block (display name, verb,
        // color, yields, knowledge yields, tick intervals) is a real list of independently
        // authorable spot assets now — an archetype can bring any number of spots, and a
        // spot type can exist without an archetype.
        [Header("Wilderness Spots")]
        [SerializeField] private WildernessSpotDefinition[] spots = System.Array.Empty<WildernessSpotDefinition>();

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
        [SerializeField] private ItemYield[] poiRareYields;
        [SerializeField, Range(0f, 1f)] private float poiRareDropChance = 0.05f;
        // POIs previously borrowed the archetype spot's tick intervals; with spots
        // extracted they own theirs (defaults match the iteration-26 tuned values).
        [SerializeField, Min(0.1f)] private float poiMinTickInterval = 2f;
        [SerializeField, Min(0.1f)] private float poiMaxTickInterval = 2.5f;

        [Header("NPC Post-Spec Stages")]
        [SerializeField] private NpcStagePool npcStagePool;

        [Header("NPC Exchange (Post-Full-Development)")]
        [SerializeField, Range(0f, 1f)] private float npcExchangeChance = 0.2f;
        [SerializeField] private ItemYield[] npcExchangeGifts = System.Array.Empty<ItemYield>();
        [SerializeField] private string[] npcVisitFlavors = System.Array.Empty<string>();
        [SerializeField] private string[] npcExchangeFlavors = System.Array.Empty<string>();

        [Header("Maintenance")]
        [SerializeField] private string buildingColdFlavor;
        [SerializeField, Min(1)] private int buildingMaintenanceCost = 2;
        [SerializeField] private string npcColdFlavor;
        [SerializeField, Min(1)] private int npcMaintenanceCost = 1;
        // Explicit reference — previously resolved positionally as CommonYields[0].Item
        // of the archetype's (single) spot, a hidden slot-0 coupling with no home once
        // spots became a list.
        [SerializeField] private ItemDefinition npcMaintenanceMaterial;

        [Header("Building")]
        [SerializeField] private string buildingDilapidatedName;
        [SerializeField] private Color buildingDilapidatedColor = new(0.35f, 0.3f, 0.28f, 1f);
        [SerializeField] private BuildingStagePool buildingStagePool;
        [SerializeField] private string[] buildingRestoredFlavors = System.Array.Empty<string>();

        // Iteration 45 (POI Three-Tier Reveal): generalizes Iteration 42's dormant/spawned
        // bool + single WorldState-flag reveal into a Hidden/VisibleInert starting tier plus
        // two authored IDependencyCondition gates. poiStartingTier defaults to VisibleInert
        // (unchanged spawn timing for every archetype that doesn't opt into Hidden).
        // poiRevealCondition only matters when starting Hidden (Hidden -> VisibleInert).
        // poiUnlockCondition governs VisibleInert -> Interactable for every POI, not just
        // this iteration's two pilots — null falls back to the original archetype-wide
        // SpecializationRealizedCondition gate WorldGenerator has always used, so any
        // archetype that doesn't author its own harder/different gate keeps identical unlock
        // behavior to before this iteration. Not wired into the CSV pipeline — hand-authored,
        // same discipline as Iteration 42/43's single-pilot fields.
        [Header("Point of Interest Tiering (Iteration 45)")]
        [SerializeField] private PoiTier poiStartingTier = PoiTier.VisibleInert;
        [SerializeReference] private IDependencyCondition poiRevealCondition;
        [SerializeReference] private IDependencyCondition poiUnlockCondition;

        public PoiTier PoiStartingTier => poiStartingTier;
        public IDependencyCondition PoiRevealCondition => poiRevealCondition;
        public IDependencyCondition PoiUnlockCondition => poiUnlockCondition;

        public string ArchetypeId => archetypeId;
        public string DisplayName => displayName;

        public WildernessSpotDefinition[] Spots => spots;

        public string SpecializationId => specializationId;
        public string StageDisplayName => stageDisplayName;
        public string NpcTitle => npcTitle;
        public Color NpcTint => npcTint;

        public string PoiDisplayName => poiDisplayName;
        public string PoiLockedDescription => poiLockedDescription;
        public string PoiVerb => poiVerb;
        public Color PoiColor => poiColor;
        public ItemYield[] PoiCommonYields => poiCommonYields;
        public ItemYield[] PoiRareYields => poiRareYields;
        public float PoiRareDropChance => poiRareDropChance;
        public float PoiMinTickInterval => poiMinTickInterval;
        public float PoiMaxTickInterval => poiMaxTickInterval;

        public NpcStagePool NpcStagePool => npcStagePool;

        public float NpcExchangeChance => npcExchangeChance;
        public ItemYield[] NpcExchangeGifts => npcExchangeGifts;
        public string[] NpcVisitFlavors => npcVisitFlavors;
        public string[] NpcExchangeFlavors => npcExchangeFlavors;

        public string BuildingColdFlavor => buildingColdFlavor;
        public int BuildingMaintenanceCost => buildingMaintenanceCost;
        public string NpcColdFlavor => npcColdFlavor;
        public int NpcMaintenanceCost => npcMaintenanceCost;
        public ItemDefinition NpcMaintenanceMaterial => npcMaintenanceMaterial;

        public string BuildingDilapidatedName => buildingDilapidatedName;
        public Color BuildingDilapidatedColor => buildingDilapidatedColor;
        public BuildingStagePool BuildingStagePool => buildingStagePool;
        public string[] BuildingRestoredFlavors => buildingRestoredFlavors;
    }
}
