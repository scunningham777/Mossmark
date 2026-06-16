using System;
using System.Collections.Generic;
using Mossmark.Attention;
using Mossmark.Development;
using Mossmark.Inventory;
using Mossmark.Visuals;
using UnityEngine;

namespace Mossmark.World
{
    // Iteration 12: selects this session's PlaceArchetypes and spawns the wilderness
    // spots/building that derive from that selection. Runs before every other Awake
    // (NpcAttendable in particular) so SelectedArchetypes is populated before tracks
    // are built.
    [DefaultExecutionOrder(-1000)]
    public class WorldGenerator : MonoBehaviour
    {
        [SerializeField] private RegionData regionData;
        [SerializeField] private int seed;
        [SerializeField] private float colliderRadius = 0.5f;
        [SerializeField] private ItemDefinition buildingMaterial;

        // A column out in the wilderness, well clear of WorldLayoutGenerator's town
        // bounds (the hand-placed buildings/NPCs/bedroll/chest all live within those).
        [SerializeField] private Vector2[] wildernessSpotPositions =
        {
            new(-20, 10), new(-20, 0), new(-20, -10)
        };
        [SerializeField] private Vector2 buildingPosition = new(0, 6);

        // One column further west than the wilderness spots, near the wilderness edge.
        [SerializeField] private Vector2[] poiPositions =
        {
            new(-28, 10), new(-28, 0), new(-28, -10)
        };

        public static IReadOnlyList<PlaceArchetype> SelectedArchetypes { get; private set; } = Array.Empty<PlaceArchetype>();

        private void Awake()
        {
            if (regionData == null || regionData.ArchetypePool == null || regionData.ArchetypePool.Length == 0)
            {
                Debug.LogWarning("WorldGenerator has no region data / archetype pool assigned.", this);
                return;
            }

            int actualSeed = seed != 0 ? seed : Environment.TickCount;
            UnityEngine.Random.InitState(actualSeed);

            SelectedArchetypes = SelectArchetypes(regionData);

            var names = new List<string>();
            foreach (var archetype in SelectedArchetypes) names.Add(archetype.DisplayName);
            Debug.Log($"World generation (seed {actualSeed}): {regionData.RegionName} selected {string.Join(", ", names)}.", this);
        }

        private void Start()
        {
            SpawnWildernessSpots();
            SpawnBuilding();
            SpawnPois();
        }

        private static List<PlaceArchetype> SelectArchetypes(RegionData data)
        {
            var pool = new List<PlaceArchetype>(data.ArchetypePool);
            int count = Mathf.Clamp(data.ArchetypeSelectionCount, 1, pool.Count);
            var selected = new List<PlaceArchetype>(count);

            for (int i = 0; i < count; i++)
            {
                int index = UnityEngine.Random.Range(0, pool.Count);
                selected.Add(pool[index]);
                pool.RemoveAt(index);
            }

            return selected;
        }

        private void SpawnWildernessSpots()
        {
            for (int i = 0; i < SelectedArchetypes.Count && i < wildernessSpotPositions.Length; i++)
            {
                SpawnWildernessSpot(SelectedArchetypes[i], wildernessSpotPositions[i]);
            }
        }

        private void SpawnWildernessSpot(PlaceArchetype archetype, Vector2 position)
        {
            var go = new GameObject(archetype.SpotDisplayName);
            go.SetActive(false);
            go.transform.position = position;

            go.AddComponent<SpriteRenderer>();
            go.AddComponent<TriangleSpriteGenerator>().Initialize(archetype.SpotColor);
            go.AddComponent<CircleCollider2D>().radius = colliderRadius;

            go.AddComponent<GenericWildernessSpotAttendable>().Initialize(
                archetype.SpotDisplayName, archetype.SpotVerb,
                archetype.CommonYields, archetype.RareYield, archetype.RareDropChance, 1.5f, 2f);

            go.AddComponent<AttendableZone>();

            go.SetActive(true);
        }

        // Building specialization bias: the new building's declared specialization comes
        // from one of this session's selected archetypes, per PROTOTYPE2.md Section 7's
        // "Building -> NPC demand" loop - reviving it declares that archetype's
        // specialization as needed, which NpcAttendable's archetype-derived stages
        // (added below) can then draw.
        private void SpawnBuilding()
        {
            if (SelectedArchetypes.Count == 0) return;

            var archetype = SelectedArchetypes[UnityEngine.Random.Range(0, SelectedArchetypes.Count)];

            var go = new GameObject("Crumbling Shed");
            go.SetActive(false);
            go.transform.position = buildingPosition;

            go.AddComponent<SpriteRenderer>();
            go.AddComponent<TriangleSpriteGenerator>().Initialize(new Color(0.4f, 0.35f, 0.3f, 1f));
            go.AddComponent<CircleCollider2D>().radius = colliderRadius;

            go.AddComponent<BuildingAttendable>().Initialize(
                "Crumbling Shed", "Workshop", "repair", buildingMaterial,
                materialCostPerTick: 1, progressCost: 2, minTickInterval: 2f, maxTickInterval: 3f,
                revivedTint: new Color(0.8f, 0.7f, 0.5f, 1f),
                declaredSpecialization: archetype.SpecializationId);

            go.AddComponent<AttendableZone>();

            go.SetActive(true);
        }

        private void SpawnPois()
        {
            for (int i = 0; i < SelectedArchetypes.Count && i < poiPositions.Length; i++)
            {
                SpawnPoi(SelectedArchetypes[i], poiPositions[i]);
            }
        }

        // Iteration 13: one POI per selected archetype, "inaccessible until its gating
        // dependency is satisfied, then attendable with its distinctive yield" per
        // PROTOTYPE2.md Section 6. The gate is a SpecializationRealizedCondition against
        // this archetype's own SpecializationId - the POI opens once an NPC has realized
        // that specialization (e.g. a Bog Keeper), mirroring Iteration 11's
        // SpecializationNeededCondition in the opposite direction.
        private void SpawnPoi(PlaceArchetype archetype, Vector2 position)
        {
            var go = new GameObject(archetype.PoiDisplayName);
            go.SetActive(false);
            go.transform.position = position;

            go.AddComponent<SpriteRenderer>();
            go.AddComponent<TriangleSpriteGenerator>().Initialize(archetype.PoiColor);
            go.AddComponent<CircleCollider2D>().radius = colliderRadius;

            var gate = new SpecializationRealizedCondition(archetype.SpecializationId,
                $"needs a {archetype.NpcTitle} in town");

            go.AddComponent<PoiAttendable>().Initialize(
                archetype.PoiDisplayName, archetype.PoiLockedDescription, archetype.PoiVerb,
                archetype.PoiCommonYields, archetype.PoiRareYield, archetype.PoiRareDropChance,
                1.5f, 2f, gate);

            go.AddComponent<AttendableZone>();

            go.SetActive(true);
        }
    }
}
