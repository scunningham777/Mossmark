using System;
using Mossmark.Attention;
using Mossmark.Day;
using Mossmark.Development;
using Mossmark.Visuals;
using UnityEngine;

namespace Mossmark.World
{
    // Iteration 30: checks each ArrivalTrigger's condition on every day advance.
    // When a trigger's ArrivalCondition is satisfied, spawns an ArrivalAttendable
    // just outside the town boundary — "the place attracted someone."
    //
    // Once the player attends the arrival through its wariness threshold (default 3
    // ticks), promotes it to a permanent NpcAttendable and sets "settlement_grew"
    // in WorldState. Each trigger fires at most once per session.
    //
    // Multiple triggers with different conditions support a two-arrival arc:
    // the first arrival is gated on early development depth; the second requires
    // more developed flags, representing a more mature settlement.
    public class ArrivalSpawner : MonoBehaviour
    {
        [SerializeField] private ArrivalTrigger[] triggers;
        [SerializeField, Min(0f)] private float minDistFromTown = 2f;
        [SerializeField, Min(1f)] private float maxDistFromTown = 6f;

        private void Start()
        {
            if (DayCycleManager.Instance != null)
                DayCycleManager.Instance.DayAdvanced += OnDayAdvanced;
        }

        private void OnDestroy()
        {
            if (DayCycleManager.Instance != null)
                DayCycleManager.Instance.DayAdvanced -= OnDayAdvanced;
        }

        private void OnDayAdvanced()
        {
            if (triggers == null) return;

            foreach (var trigger in triggers)
            {
                // Skip if already fired or already waiting for player to approach.
                if (trigger.fired || trigger.spawnedArrival != null) continue;

                if (trigger.condition != null && trigger.condition.IsSatisfied(null))
                    SpawnArrival(trigger);
            }
        }

        private void SpawnArrival(ArrivalTrigger trigger)
        {
            var position = FindSpawnPosition();

            var go = new GameObject(trigger.arrivalName);
            go.SetActive(false);
            go.transform.position = position;

            go.AddComponent<SpriteRenderer>();
            go.AddComponent<TriangleSpriteGenerator>().Initialize(trigger.arrivalColor);

            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.5f;

            var arrival = go.AddComponent<ArrivalAttendable>();
            arrival.Initialize(trigger.arrivalName, trigger.warnessThreshold,
                arrivalGo => OnArrivalPromoted(trigger, arrivalGo));

            go.AddComponent<AttendableZone>();
            go.SetActive(true);

            trigger.spawnedArrival = arrival;
            Debug.Log($"Settlement growth: {trigger.arrivalName} arrives near the settlement at {(Vector2)go.transform.position}.", go);
        }

        private void OnArrivalPromoted(ArrivalTrigger trigger, GameObject arrivalGo)
        {
            var position = arrivalGo.transform.position;

            // Spawn the permanent NpcAttendable at the arrival's position, following
            // the inactive-GO pattern so Awake fires with correct field values.
            var npcGo = new GameObject(trigger.arrivalName);
            npcGo.SetActive(false);
            npcGo.transform.position = position;

            npcGo.AddComponent<SpriteRenderer>();
            npcGo.AddComponent<TriangleSpriteGenerator>().Initialize(trigger.arrivalColor);

            var col = npcGo.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.5f;

            var npc = npcGo.AddComponent<NpcAttendable>();
            npc.Initialize(trigger.arrivalName, trigger.npcProgressCost,
                trigger.npcMinTickInterval, trigger.npcMaxTickInterval);

            npcGo.AddComponent<AttendableZone>();
            npcGo.AddComponent<EntityFeedback>();
            npcGo.SetActive(true);

            WorldState.SetFlag("settlement_grew", true);

            Debug.Log($"Settlement growth: {trigger.arrivalName} has settled. settlement_grew = true.", npcGo);
            NotificationManager.Post("The settlement grows.");

            trigger.fired = true;
            trigger.spawnedArrival = null;

            Destroy(arrivalGo);
        }

        // Position just outside the town edge, within wilderness bounds.
        private Vector2 FindSpawnPosition(int maxAttempts = 50)
        {
            var town = WorldLayoutGenerator.TownBounds;
            var wilderness = WorldLayoutGenerator.WildernessBounds;

            for (int i = 0; i < maxAttempts; i++)
            {
                // Pick a random side of the town boundary and place just outside it.
                int side = UnityEngine.Random.Range(0, 4);
                float dist = UnityEngine.Random.Range(minDistFromTown, maxDistFromTown);
                Vector2 candidate;

                switch (side)
                {
                    case 0: // south
                        candidate = new Vector2(UnityEngine.Random.Range(town.xMin, town.xMax), town.yMin - dist);
                        break;
                    case 1: // north
                        candidate = new Vector2(UnityEngine.Random.Range(town.xMin, town.xMax), town.yMax + dist);
                        break;
                    case 2: // west
                        candidate = new Vector2(town.xMin - dist, UnityEngine.Random.Range(town.yMin, town.yMax));
                        break;
                    default: // east
                        candidate = new Vector2(town.xMax + dist, UnityEngine.Random.Range(town.yMin, town.yMax));
                        break;
                }

                if (wilderness.Contains(candidate) && !town.Contains(candidate))
                    return candidate;
            }

            // Fallback: due south of town centre.
            return new Vector2(0f, town.yMin - minDistFromTown);
        }
    }

    // Per-trigger data: what condition gates this arrival, what the NPC looks like,
    // and NPC development parameters once promoted. Runtime-only fields (fired,
    // spawnedArrival) are NonSerialized.
    [Serializable]
    public class ArrivalTrigger
    {
        public string arrivalName = "A Stranger";
        public Color arrivalColor = new Color(0.6f, 0.56f, 0.5f, 1f);
        public ArrivalCondition condition;
        [Min(1)] public int warnessThreshold = 3;
        [Min(1)] public int npcProgressCost = 8;
        [Min(0.1f)] public float npcMinTickInterval = 1.5f;
        [Min(0.1f)] public float npcMaxTickInterval = 2f;

        [NonSerialized] public bool fired;
        [NonSerialized] public ArrivalAttendable spawnedArrival;
    }
}
