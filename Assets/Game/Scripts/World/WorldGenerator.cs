using System;
using System.Collections.Generic;
using Mossmark.Attention;
using Mossmark.Development;
using Mossmark.Visuals;
using UnityEngine;

namespace Mossmark.World
{
    // Selects this session's PlaceArchetypes and spawns all wilderness entities
    // (archetype spots, buildings, POIs, and random generic/tended spots) using
    // rejection-sampling placement so no two objects overlap. Runs before every
    // other Awake (NpcAttendable in particular) so SelectedArchetypes is populated
    // before tracks are built. WorldLayoutGenerator (-2000) runs first, so
    // TownBounds/WildernessBounds are ready when Awake fires here.
    [DefaultExecutionOrder(-1000)]
    public class WorldGenerator : MonoBehaviour
    {
        [SerializeField] private RegionData regionData;
        [SerializeField] private int seed;
        [SerializeField] private float colliderRadius = 0.5f;

        // One building position per selected archetype — fixed in town.
        [SerializeField] private Vector2[] buildingPositions =
        {
            new(-5, 7), new(0, 7), new(5, 7)
        };

        // Pool of generic/tended spot types to draw from for random placement.
        [SerializeField] private WildernessSpotDefinition[] spotPool;
        [SerializeField, Min(1)] private int minSpotCount = 10;
        [SerializeField, Min(1)] private int maxSpotCount = 12;

        // Minimum world-space distance between any two placed wilderness objects.
        [SerializeField, Min(0f)] private float minSeparation = 2f;

        public static IReadOnlyList<PlaceArchetype> SelectedArchetypes { get; private set; } = Array.Empty<PlaceArchetype>();

        // Tracks every position placed so far; used by FindValidPosition to enforce
        // the minimum separation between all wilderness objects.
        private readonly List<Vector2> placedPositions = new();

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
            SpawnGenericWildernessSpots();
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

        // --- Archetype-driven spawns (positions randomized) ---

        private void SpawnWildernessSpots()
        {
            foreach (var archetype in SelectedArchetypes)
                SpawnWildernessSpot(archetype, FindValidPosition());
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
                archetype.CommonYields, archetype.RareYield, archetype.RareDropChance,
                archetype.ArchetypeSpotMinTickInterval, archetype.ArchetypeSpotMaxTickInterval,
                archetype.SpotKnowledgeYields);

            go.AddComponent<AttendableZone>();

            go.SetActive(true);
        }

        // One building per selected archetype — still fixed in town so the settlement
        // layout stays legible; only wilderness objects use random placement.
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

            go.AddComponent<BuildingAttendable>().Initialize(
                archetype.BuildingDilapidatedName,
                archetype.BuildingStages,
                archetype.SpecializationId,
                2f, 3f,
                archetype.BuildingRestoredFlavors);

            go.AddComponent<AttendableZone>();

            go.SetActive(true);
        }

        private void SpawnPois()
        {
            foreach (var archetype in SelectedArchetypes)
                SpawnPoi(archetype, FindValidPosition());
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
                archetype.ArchetypeSpotMinTickInterval, archetype.ArchetypeSpotMaxTickInterval, gate);

            go.AddComponent<AttendableZone>();

            go.SetActive(true);
        }

        // Randomly places 10-12 spots drawn from spotPool, enforcing minSeparation
        // from every previously placed object.
        private void SpawnGenericWildernessSpots()
        {
            if (spotPool == null || spotPool.Length == 0)
            {
                Debug.LogWarning("WorldGenerator: spotPool is empty — no generic wilderness spots spawned.", this);
                return;
            }

            int count = UnityEngine.Random.Range(minSpotCount, maxSpotCount + 1);
            for (int i = 0; i < count; i++)
            {
                var def = spotPool[UnityEngine.Random.Range(0, spotPool.Length)];
                if (def == null) continue;

                var pos = FindValidPosition();
                SpawnSpotFromDefinition(def, pos);
            }
        }

        private void SpawnSpotFromDefinition(WildernessSpotDefinition def, Vector2 position)
        {
            var go = new GameObject(def.displayName);
            go.SetActive(false);
            go.transform.position = position;

            go.AddComponent<SpriteRenderer>();
            go.AddComponent<TriangleSpriteGenerator>().Initialize(def.color);
            go.AddComponent<CircleCollider2D>().radius = colliderRadius;

            if (def.kind == WildernessSpotDefinition.SpotKind.Generic)
            {
                go.AddComponent<GenericWildernessSpotAttendable>().Initialize(
                    def.displayName, def.interactionVerb,
                    def.commonYields, def.rareYield, def.rareDropChance,
                    def.minTickInterval, def.maxTickInterval,
                    def.knowledgeYields);
            }
            else
            {
                go.AddComponent<TendedSpotAttendable>().Initialize(
                    def.displayName, def.tendVerb,
                    def.harvestYields, def.restsToHarvest, def.maxConcurrentMarked);
            }

            go.AddComponent<AttendableZone>();

            go.SetActive(true);
        }

        // --- Position utilities ---

        // Rejection-sample a random point in the wilderness (outside town bounds)
        // that is at least minSeparation units from every already-placed position.
        // Falls back to the best candidate found after maxAttempts if none satisfies
        // the full constraint — avoids infinite loops on very crowded maps.
        private Vector2 FindValidPosition(int maxAttempts = 200)
        {
            var wb = WorldLayoutGenerator.WildernessBounds;
            var tb = WorldLayoutGenerator.TownBounds;

            Vector2 best = Vector2.zero;
            float bestMinDist = -1f;

            for (int i = 0; i < maxAttempts; i++)
            {
                var candidate = new Vector2(
                    UnityEngine.Random.Range(wb.xMin, wb.xMax),
                    UnityEngine.Random.Range(wb.yMin, wb.yMax));

                // Must be in the wilderness, not the town.
                if (tb.Contains(candidate)) continue;

                float minDist = MinDistToPlaced(candidate);

                if (minDist >= minSeparation)
                {
                    placedPositions.Add(candidate);
                    return candidate;
                }

                if (minDist > bestMinDist)
                {
                    bestMinDist = minDist;
                    best = candidate;
                }
            }

            Debug.LogWarning($"WorldGenerator: couldn't find a fully separated position after {maxAttempts} attempts — using closest valid candidate.", this);
            placedPositions.Add(best);
            return best;
        }

        private float MinDistToPlaced(Vector2 pos)
        {
            if (placedPositions.Count == 0) return float.MaxValue;
            float min = float.MaxValue;
            foreach (var p in placedPositions)
            {
                float d = Vector2.Distance(pos, p);
                if (d < min) min = d;
            }
            return min;
        }
    }
}
