using System;
using System.Collections.Generic;
using Mossmark.Attention;
using Mossmark.Development;
using Mossmark.Visuals;
using UnityEngine;

namespace Mossmark.World
{
    // Iteration 12: selects this session's PlaceArchetypes and spawns the wilderness
    // spots/building/POIs that derive from that selection. Runs before every other Awake
    // (NpcAttendable in particular) so SelectedArchetypes is populated before tracks
    // are built.
    //
    // Content pass: spawns one building per selected archetype (replacing the single
    // generic Workshop). buildingMaterial removed — each archetype now carries its own
    // building material. buildingPositions is an array sized to match SelectedArchetypes.
    [DefaultExecutionOrder(-1000)]
    public class WorldGenerator : MonoBehaviour
    {
        [SerializeField] private RegionData regionData;
        [SerializeField] private int seed;
        [SerializeField] private float colliderRadius = 0.5f;

        [SerializeField] private Vector2[] wildernessSpotPositions =
        {
            new(-20, 10), new(-20, 0), new(-20, -10)
        };

        // One building per selected archetype — sized to ArchetypeSelectionCount (default 3).
        [SerializeField] private Vector2[] buildingPositions =
        {
            new(-5, 7), new(0, 7), new(5, 7)
        };

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
            SpawnBuildings();
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
                SpawnWildernessSpot(SelectedArchetypes[i], wildernessSpotPositions[i]);
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

        // One building per selected archetype. Each archetype carries its own building
        // data (dilapidatedName/revivedName/material/etc.) and stage 2 data. Stage 2
        // material is always the archetype's PoiCommonYields[0] — the item that becomes
        // gatherable once the NPC specializes and the POI opens.
        private void SpawnBuildings()
        {
            for (int i = 0; i < SelectedArchetypes.Count && i < buildingPositions.Length; i++)
                SpawnBuilding(SelectedArchetypes[i], buildingPositions[i]);
        }

        private void SpawnBuilding(PlaceArchetype archetype, Vector2 position)
        {
            if (string.IsNullOrEmpty(archetype.BuildingDilapidatedName)) return;

            var go = new GameObject(archetype.BuildingDilapidatedName);
            go.SetActive(false);
            go.transform.position = position;

            go.AddComponent<SpriteRenderer>();
            go.AddComponent<TriangleSpriteGenerator>().Initialize(archetype.BuildingDilapidatedColor);
            go.AddComponent<CircleCollider2D>().radius = colliderRadius;

            var building = go.AddComponent<BuildingAttendable>();
            building.Initialize(
                archetype.BuildingDilapidatedName, archetype.BuildingRevivedName,
                archetype.BuildingRepairVerb, archetype.BuildingMaterial,
                archetype.BuildingMaterialCostPerTick, archetype.BuildingProgressCost,
                2f, 3f, archetype.BuildingRevivedTint, archetype.SpecializationId);

            // Stage 2: available once the NPC specializes (gated by SpecializationRealized)
            // and costs the POI's common yield — naturally acquired once the POI opens.
            var stage2Mat = archetype.PoiCommonYields?.Length > 0 ? archetype.PoiCommonYields[0].Item : null;
            if (!string.IsNullOrEmpty(archetype.BuildingStage2DisplayName) && stage2Mat != null)
            {
                building.InitializeStage2(
                    archetype.BuildingStage2DisplayName, archetype.BuildingStage2Verb,
                    stage2Mat, archetype.BuildingStage2MaterialCostPerTick,
                    archetype.BuildingStage2ProgressCost,
                    archetype.SpecializationId,
                    archetype.BuildingStage2Tint);
            }

            go.AddComponent<AttendableZone>();

            go.SetActive(true);
        }

        private void SpawnPois()
        {
            for (int i = 0; i < SelectedArchetypes.Count && i < poiPositions.Length; i++)
                SpawnPoi(SelectedArchetypes[i], poiPositions[i]);
        }

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
