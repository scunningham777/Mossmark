using System.Collections;
using Mossmark.Attention;
using Mossmark.Visuals;
using UnityEngine;

namespace Mossmark.World
{
    // Iteration 14: keeps exactly one WanderingThingAttendable alive at a time.
    // Spawns it at a random wilderness position (minDistFromTown clear of town bounds),
    // waits for it to be resolved or to time out (via the onGone callback), then
    // waits a random cooldown before spawning the next one.
    // Iteration 23 (G4): replaced per-creature serialized fields with a
    // WanderingThingDefinition[] pool; Spawn() picks one at random each cycle.
    public class WanderingThingSpawner : MonoBehaviour
    {
        [SerializeField] private WanderingThingDefinition[] pool;

        [SerializeField, Min(1f)] private float minLifespan = 20f;
        [SerializeField, Min(1f)] private float maxLifespan = 35f;
        [SerializeField, Min(0f)] private float minSpawnDelay = 5f;
        [SerializeField, Min(0f)] private float maxSpawnDelay = 15f;
        [SerializeField, Min(0f)] private float minDistFromTown = 5f;

        private string favorableSpecializationId;
        private WanderingThingAttendable current;

        private void Start()
        {
            favorableSpecializationId = PickFavorableSpecialization();
            StartCoroutine(SpawnLoop());
        }

        // "Odds can be shifted by town development" per PROTOTYPE2.md - one of this
        // session's selected archetypes (WorldGenerator.Awake runs before this Start,
        // per its DefaultExecutionOrder) is picked at random as the specialization that,
        // once realized by some NPC, makes this encounter's good outcome more likely.
        private static string PickFavorableSpecialization()
        {
            var archetypes = WorldGenerator.SelectedArchetypes;
            if (archetypes == null || archetypes.Count == 0) return "";

            var archetype = archetypes[Random.Range(0, archetypes.Count)];
            Debug.Log($"Wandering things: travelers will be more trusting once this town has a {archetype.NpcTitle}.");
            return archetype.SpecializationId;
        }

        private IEnumerator SpawnLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(Random.Range(minSpawnDelay, maxSpawnDelay));
                Spawn();
                while (current != null) yield return null;
            }
        }

        private void Spawn()
        {
            if (pool == null || pool.Length == 0) return;

            var def = pool[Random.Range(0, pool.Length)];
            var position = FindSpawnPosition();
            float lifespan = Random.Range(minLifespan, maxLifespan);

            var go = new GameObject(def.displayName);
            go.SetActive(false);
            go.transform.position = position;

            go.AddComponent<SpriteRenderer>();
            go.AddComponent<TriangleSpriteGenerator>().Initialize(def.color);
            go.AddComponent<CircleCollider2D>().radius = def.colliderRadius;

            var attendable = go.AddComponent<WanderingThingAttendable>();
            attendable.Initialize(def, favorableSpecializationId, lifespan, () => current = null);

            go.AddComponent<AttendableZone>();

            go.SetActive(true);

            current = attendable;
            Debug.Log($"{def.displayName}: appears, watching the path.", go);
        }

        private Vector2 FindSpawnPosition(int maxAttempts = 50)
        {
            var wilderness = WorldLayoutGenerator.WildernessBounds;
            var town = WorldLayoutGenerator.TownBounds;
            // Expand town bounds outward to enforce a clear buffer zone.
            var exclusion = new Rect(
                town.xMin - minDistFromTown, town.yMin - minDistFromTown,
                town.width + minDistFromTown * 2f, town.height + minDistFromTown * 2f);

            for (int i = 0; i < maxAttempts; i++)
            {
                var candidate = new Vector2(
                    Random.Range(wilderness.xMin, wilderness.xMax),
                    Random.Range(wilderness.yMin, wilderness.yMax));
                if (!exclusion.Contains(candidate))
                    return candidate;
            }
            // Fallback: wilderness corner, always outside the town exclusion zone.
            return new Vector2(wilderness.xMin + minDistFromTown, wilderness.yMin + minDistFromTown);
        }
    }
}
