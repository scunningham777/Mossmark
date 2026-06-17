using System.Collections;
using Mossmark.Attention;
using Mossmark.Visuals;
using UnityEngine;

namespace Mossmark.World
{
    // Iteration 14: keeps exactly one WanderingThingAttendable alive at a time -
    // "static-with-lifespan ... roaming/pathing not required" per PROTOTYPE2.md.
    // Spawns it at a random position from spawnPositions, waits for it to be resolved
    // or to time out (the spawned instance reports this via the onGone callback), then
    // waits a random cooldown before spawning the next one.
    public class WanderingThingSpawner : MonoBehaviour
    {
        [SerializeField] private string displayName = "Wary Traveler";
        [SerializeField] private string approachDescription = "A traveler lingers at the path's edge, watching you with wary eyes.";
        [SerializeField] private string attendVerb = "approach";
        [SerializeField] private Color color = new(0.5f, 0.45f, 0.55f, 1f);
        [SerializeField] private float colliderRadius = 0.5f;

        [SerializeField] private ItemYield[] goodYields;
        [SerializeField] private string goodFlavor = "decides you mean no harm, and presses something into your hand before moving on.";
        [SerializeField] private string badFlavor = "bolts, snatching everything from your pack as they go.";
        [SerializeField, Min(0)] private int badDaylightCost = 1;
        [SerializeField, Range(0f, 1f)] private float baseGoodChance = 0.5f;

        [SerializeField, Min(1f)] private float minLifespan = 20f;
        [SerializeField, Min(1f)] private float maxLifespan = 35f;
        [SerializeField, Min(0f)] private float minSpawnDelay = 5f;
        [SerializeField, Min(0f)] private float maxSpawnDelay = 15f;

        // A row along the wilderness's southern edge, well clear of WorldLayoutGenerator's
        // town bounds.
        [SerializeField] private Vector2[] spawnPositions =
        {
            new(-20, -25), new(-10, -25), new(0, -25), new(10, -25), new(20, -25)
        };

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
            var position = spawnPositions[Random.Range(0, spawnPositions.Length)];
            float lifespan = Random.Range(minLifespan, maxLifespan);

            var go = new GameObject(displayName);
            go.SetActive(false);
            go.transform.position = position;

            go.AddComponent<SpriteRenderer>();
            go.AddComponent<TriangleSpriteGenerator>().Initialize(color);
            go.AddComponent<CircleCollider2D>().radius = colliderRadius;

            var attendable = go.AddComponent<WanderingThingAttendable>();
            attendable.Initialize(displayName, approachDescription, attendVerb,
                goodYields, goodFlavor, badFlavor, badDaylightCost,
                baseGoodChance, favorableSpecializationId,
                lifespan, () => current = null);

            go.AddComponent<AttendableZone>();

            go.SetActive(true);

            current = attendable;
            Debug.Log($"{displayName}: appears, watching the path.", go);
        }
    }
}
