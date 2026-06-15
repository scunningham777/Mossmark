using UnityEngine;

namespace Mossmark.World
{
    // Iteration 12: a self-contained bundle of wilderness-spot data and the building/NPC
    // specializations that place implies, per IDEAS.md's "Place Archetypes" framing.
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

        // Iteration 13: the archetype's POI - "inaccessible until its gating dependency
        // is satisfied, then attendable with its distinctive yield" per PROTOTYPE2.md
        // Section 6. The gate is built by WorldGenerator from SpecializationId/NpcTitle
        // above (a SpecializationRealizedCondition), so this POI only opens up once this
        // archetype's own NPC specialization exists in town.
        [Header("Point of Interest")]
        [SerializeField] private string poiDisplayName;
        [SerializeField] private string poiLockedDescription;
        [SerializeField] private string poiVerb = "search";
        [SerializeField] private Color poiColor = Color.white;
        [SerializeField] private ItemYield[] poiCommonYields;
        [SerializeField] private ItemYield poiRareYield;
        [SerializeField, Range(0f, 1f)] private float poiRareDropChance = 0.05f;

        public string ArchetypeId => archetypeId;
        public string DisplayName => displayName;

        public string SpotDisplayName => spotDisplayName;
        public string SpotVerb => spotVerb;
        public Color SpotColor => spotColor;
        public ItemYield[] CommonYields => commonYields;
        public ItemYield RareYield => rareYield;
        public float RareDropChance => rareDropChance;

        // Shared between a building's declared specialization (when this archetype is the
        // bias for a spawned building) and the corresponding NPC track stage's Id - matches
        // DeclaredSpecializationNeeds/SpecializationNeededCondition's existing id scheme.
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
    }
}
