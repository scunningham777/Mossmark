using System;
using System.Collections.Generic;
using Mossmark.Attention;
using Mossmark.Day;
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

        // Iteration 42 (Site Clustering pilot): each archetype's spots + POI jitter around
        // one shared anchor point instead of scattering independently across the wilderness.
        [SerializeField, Min(0f)] private float siteJitterRadius = 3f;

        public static IReadOnlyList<PlaceArchetype> SelectedArchetypes { get; private set; } = Array.Empty<PlaceArchetype>();

        // Iteration 31 pilot, re-keyed in the relational-data migration: spawned Generic
        // spots with a non-empty WildernessSpotDefinition.spotId register here, so other
        // systems (NpcStageDef.passiveDriftSourceSpotId) can read a specific spot's
        // tendedness without positional coupling to "the archetype's spot". Iteration 43:
        // typed to the ITendednessSource interface (rather than GenericWildernessSpotAttendable
        // directly) so a spot opted into DevelopingWildernessSpotAttendable's exhaustion/
        // Standing model still registers and reports a faithful synthetic tendedness reading —
        // the Bog Keeper's passive-drift seam (Iteration 34) keeps working unchanged.
        private static readonly Dictionary<string, ITendednessSource> spotsById = new();

        public static ITendednessSource GetSpot(string spotId) =>
            spotsById.TryGetValue(spotId, out var spot) ? spot : null;

        // Tracks every position placed so far; used by FindValidPosition to enforce
        // the minimum separation between all wilderness objects.
        private readonly List<Vector2> placedPositions = new();

        // Iteration 42: archetypes whose POI was skipped at world gen (PoiDormantByDefault)
        // wait here, anchor remembered, until PoiRevealWorldStateFlag reads true.
        private readonly List<(PlaceArchetype archetype, Vector2 anchor)> dormantPois = new();

        private void Awake()
        {
            spotsById.Clear();

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
            SpawnArchetypeSites();
            SpawnBuildings();
            SpawnGenericWildernessSpots();

            if (DayCycleManager.Instance != null)
                DayCycleManager.Instance.DayAdvanced += CheckDormantSiteReveals;
        }

        private void OnDestroy()
        {
            if (DayCycleManager.Instance != null)
                DayCycleManager.Instance.DayAdvanced -= CheckDormantSiteReveals;
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

        // Iteration 42 (Site Clustering pilot): each archetype gets one anchor point, and
        // its spot definitions (relational-data migration: an archetype can bring any number)
        // plus its POI are placed within siteJitterRadius of that anchor instead of scattering
        // independently — the pieces of one archetype now read as one place. A POI marked
        // PoiDormantByDefault is held back (its anchor remembered) rather than spawned now.
        private void SpawnArchetypeSites()
        {
            foreach (var archetype in SelectedArchetypes)
            {
                var anchor = FindValidPosition();

                foreach (var def in archetype.Spots)
                    if (def != null) SpawnSpotFromDefinition(def, FindValidPositionNear(anchor));

                if (archetype.PoiDormantByDefault)
                {
                    dormantPois.Add((archetype, anchor));
                    continue;
                }

                SpawnPoi(archetype, FindValidPositionNear(anchor));
            }
        }

        // Checked on every rest: spawns a dormant archetype's POI, clustered near its
        // remembered anchor, the first time its reveal flag reads true. No spawn-moment
        // fanfare — the player finds it there on return, same as passive drift (Iteration 31).
        private void CheckDormantSiteReveals()
        {
            for (int i = dormantPois.Count - 1; i >= 0; i--)
            {
                var (archetype, anchor) = dormantPois[i];
                if (string.IsNullOrEmpty(archetype.PoiRevealWorldStateFlag)
                    || !WorldState.GetFlag(archetype.PoiRevealWorldStateFlag))
                    continue;

                dormantPois.RemoveAt(i);
                SpawnPoi(archetype, FindValidPositionNear(anchor));
                Debug.Log($"{archetype.PoiDisplayName}: reveals itself near the {archetype.DisplayName}.", this);
            }
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
                archetype.BuildingStagePool != null
                    ? archetype.BuildingStagePool.Stages
                    : System.Array.Empty<BuildingStageDef>(),
                archetype.SpecializationId,
                2f, 3f,
                archetype.BuildingRestoredFlavors,
                archetype.BuildingColdFlavor,
                archetype.BuildingMaintenanceCost);

            go.AddComponent<AttendableZone>();
            go.AddComponent<EntityFeedback>();

            go.SetActive(true);
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
                archetype.PoiCommonYields, archetype.PoiRareYields, archetype.PoiRareDropChance,
                archetype.PoiMinTickInterval, archetype.PoiMaxTickInterval, gate);

            go.AddComponent<AttendableZone>();
            go.AddComponent<EntityFeedback>();

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
                // Iteration 44: every Generic spot now gets the exhaustion/Standing model
                // (generalized from Iteration 43's Fen Bog pilot) — GenericWildernessSpotAttendable
                // (continuous tendedness) is retired for this kind. Tended spots (below) and
                // POIs (PoiAttendable, which still extends WildernessYieldAttendable) are
                // explicitly out of scope and keep their existing behavior.
                var stagedSpot = go.AddComponent<DevelopingWildernessSpotAttendable>();
                stagedSpot.Initialize(
                    def.displayName, def.interactionVerb,
                    def.commonYields, def.EffectiveRareYields, def.rareDropChance,
                    def.minTickInterval, def.maxTickInterval, def.spotStagePool,
                    def.knowledgeYields, def.hintFlavors);
                if (!string.IsNullOrEmpty(def.spotId))
                    spotsById[def.spotId] = stagedSpot;
            }
            else
            {
                go.AddComponent<TendedSpotAttendable>().Initialize(
                    def.displayName, def.tendVerb,
                    def.harvestYields, def.restsToHarvest, def.maxConcurrentMarked);
            }

            go.AddComponent<AttendableZone>();
            go.AddComponent<EntityFeedback>();

            go.SetActive(true);
        }

        // --- Position utilities ---

        // Rejection-sample a random point anywhere in the wilderness (outside town bounds)
        // that is at least minSeparation units from every already-placed position. Used for
        // generic pool spots and to pick each archetype's site anchor.
        private Vector2 FindValidPosition(int maxAttempts = 200)
        {
            var wb = WorldLayoutGenerator.WildernessBounds;
            return FindValidPositionInternal(
                () => new Vector2(UnityEngine.Random.Range(wb.xMin, wb.xMax), UnityEngine.Random.Range(wb.yMin, wb.yMax)),
                maxAttempts);
        }

        // Iteration 42: same rejection-sampling constraints as FindValidPosition, but
        // candidates jitter around a given anchor instead of scattering across the whole
        // wilderness — this is what makes one archetype's pieces read as one place.
        private Vector2 FindValidPositionNear(Vector2 anchor, int maxAttempts = 200)
        {
            return FindValidPositionInternal(
                () => anchor + UnityEngine.Random.insideUnitCircle * siteJitterRadius,
                maxAttempts);
        }

        // Shared rejection-sampling core: draws candidates from candidateGenerator, requires
        // they land in the wilderness (not the town), and prefers minSeparation from every
        // already-placed position. Falls back to the best candidate found after maxAttempts
        // if none satisfies the full constraint — avoids infinite loops on crowded maps.
        private Vector2 FindValidPositionInternal(Func<Vector2> candidateGenerator, int maxAttempts)
        {
            var wb = WorldLayoutGenerator.WildernessBounds;
            var tb = WorldLayoutGenerator.TownBounds;

            Vector2 best = Vector2.zero;
            float bestMinDist = -1f;
            bool haveCandidate = false;

            for (int i = 0; i < maxAttempts; i++)
            {
                var candidate = candidateGenerator();

                // Must be in the wilderness, not the town.
                if (!wb.Contains(candidate) || tb.Contains(candidate)) continue;

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
                    haveCandidate = true;
                }
            }

            if (!haveCandidate) best = candidateGenerator();
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
