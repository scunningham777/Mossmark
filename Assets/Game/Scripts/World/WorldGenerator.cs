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

        // Kept clear of the Iteration 1-11 hand-placed entities, which occupy roughly
        // x in [-3, 6], y in [-3, 3].
        [SerializeField] private Vector2[] wildernessSpotPositions =
        {
            new(-6, 3), new(-6, 0), new(-6, -3)
        };
        [SerializeField] private Vector2 buildingPosition = new(0, 6);

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
    }
}
