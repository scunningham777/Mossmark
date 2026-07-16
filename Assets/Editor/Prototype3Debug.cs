using System.Reflection;
using Mossmark.Attention;
using Mossmark.Day;
using Mossmark.Development;
using Mossmark.Inventory;
using Mossmark.Prototype3;
using UnityEditor;
using UnityEngine;

namespace Mossmark.EditorTools
{
    // Play-mode test drivers for the Prototype3 scene: MCP-Unity can execute menu items
    // but can't press keys, so these reproduce what holding E does. BeginAttend calls
    // AttentionManager.HandleHoldStarted via reflection; the manager's own Update loop
    // then runs the hold to completion exactly as a real key-hold would (release never
    // fires, but one-shot attendables finish and repeating ones keep ticking until
    // ReleaseAttend is invoked).
    public static class Prototype3Debug
    {
        [MenuItem("Mossmark/Prototype3/Teleport Player To Nearest Attendable")]
        private static void TeleportToNearestAttendable()
        {
            var player = GameObject.FindWithTag("Player");
            if (player == null)
            {
                Debug.Log("P3Debug: no Player in scene.");
                return;
            }

            AttendableZone nearest = null;
            float nearestSqrDist = float.MaxValue;
            foreach (var zone in Object.FindObjectsByType<AttendableZone>())
            {
                float sqrDist = (zone.transform.position - player.transform.position).sqrMagnitude;
                if (sqrDist < nearestSqrDist)
                {
                    nearestSqrDist = sqrDist;
                    nearest = zone;
                }
            }

            if (nearest == null)
            {
                Debug.Log("P3Debug: no AttendableZone in scene.");
                return;
            }

            var destination = (Vector2)nearest.transform.position + new Vector2(0.6f, 0f);
            var rb = player.GetComponent<Rigidbody2D>();
            if (rb != null) rb.position = destination;
            else player.transform.position = destination;
            Physics2D.SyncTransforms();
            Debug.Log($"P3Debug: teleported player next to '{nearest.name}'.");
        }

        [MenuItem("Mossmark/Prototype3/Begin Attend")]
        private static void BeginAttend()
        {
            InvokeManagerMethod("HandleHoldStarted");
        }

        [MenuItem("Mossmark/Prototype3/Release Attend")]
        private static void ReleaseAttend()
        {
            InvokeManagerMethod("HandleHoldReleased");
        }

        [MenuItem("Mossmark/Prototype3/Teleport Player To Clay")]
        private static void TeleportToClay() => TeleportToNamed("Lump of Clay");

        [MenuItem("Mossmark/Prototype3/Teleport Player To Bark")]
        private static void TeleportToBark() => TeleportToNamed("Bark Strips");

        [MenuItem("Mossmark/Prototype3/Teleport Player To Reeds")]
        private static void TeleportToReeds() => TeleportToNamed("Reeds");

        [MenuItem("Mossmark/Prototype3/Teleport Player To Bench")]
        private static void TeleportToBench() => TeleportToNamed("Scouring Bench");

        private static void TeleportToNamed(string gameObjectName)
        {
            var target = GameObject.Find(gameObjectName);
            if (target == null)
            {
                Debug.Log($"P3Debug: no GameObject named '{gameObjectName}'.");
                return;
            }

            TeleportTo(target.transform);
        }

        [MenuItem("Mossmark/Prototype3/Teleport Player To Weir")]
        private static void TeleportToWeir()
        {
            TeleportTo(Object.FindAnyObjectByType<LandmarkAttendable>());
        }

        [MenuItem("Mossmark/Prototype3/Teleport Player To Bedroll")]
        private static void TeleportToBedroll()
        {
            TeleportTo(Object.FindAnyObjectByType<BedrollAttendable>());
        }

        [MenuItem("Mossmark/Prototype3/Teleport Player To Entity")]
        private static void TeleportToEntity()
        {
            TeleportTo(Object.FindAnyObjectByType<KnowingEntityAttendable>());
        }

        [MenuItem("Mossmark/Prototype3/Log Daylight")]
        private static void LogDaylight()
        {
            var day = DayCycleManager.Instance;
            if (day == null)
            {
                Debug.Log("P3Debug: no DayCycleManager.");
                return;
            }

            Debug.Log($"P3Debug: daylight {day.DaylightRemaining}/{day.MaxDaylight}, phase={day.CurrentPhase}, dayIndex={day.DayIndex}");
        }

        private static void TeleportTo(Component target)
        {
            var player = GameObject.FindWithTag("Player");
            if (player == null || target == null)
            {
                Debug.Log("P3Debug: missing Player or teleport target.");
                return;
            }

            var destination = (Vector2)target.transform.position + new Vector2(0.6f, 0f);
            var rb = player.GetComponent<Rigidbody2D>();
            if (rb != null) rb.position = destination;
            else player.transform.position = destination;
            Physics2D.SyncTransforms();
            Debug.Log($"P3Debug: teleported player next to '{target.name}'.");
        }

        [MenuItem("Mossmark/Prototype3/Log Taken Ledger")]
        private static void LogTakenLedger()
        {
            if (TakenLedger.All.Count == 0)
            {
                Debug.Log("P3Debug: taken ledger is empty.");
                return;
            }

            foreach (var entry in TakenLedger.All)
            {
                var parts = new System.Collections.Generic.List<string>();
                foreach (var propertyId in entry.PropertyIds)
                {
                    parts.Add($"{propertyId}={(WorldContext.IsPropertyKnown(entry.ItemId, propertyId) ? "known" : "unknown")}");
                }

                Debug.Log($"P3Debug: {entry.DisplayName} ({entry.ItemId}) - {string.Join(", ", parts)}");
            }
        }

        [MenuItem("Mossmark/Prototype3/Log Entity Knowledge")]
        private static void LogEntityKnowledge()
        {
            var entity = Object.FindAnyObjectByType<KnowingEntityAttendable>();
            if (entity == null)
            {
                Debug.Log("P3Debug: no KnowingEntityAttendable in scene.");
                return;
            }

            var knownIds = new System.Collections.Generic.List<string>();
            foreach (var property in PropertyRegistry.All)
            {
                if (entity.Knows(property.Id)) knownIds.Add(property.Id);
            }

            var spriteRenderer = entity.GetComponent<SpriteRenderer>();
            Debug.Log($"P3Debug: {entity.DisplayName} knows [{string.Join(", ", knownIds)}], teachPending={entity.TeachPending}, attentionDuration={entity.AttentionDuration}, tint={spriteRenderer.color}, overlay=\"{entity.GetOverlayDescription()}\", interaction=\"{entity.GetOverlayInteractionLine()}\"");
        }

        [MenuItem("Mossmark/Prototype3/Log Attention State")]
        private static void LogAttentionState()
        {
            var manager = AttentionManager.Instance;
            if (manager == null)
            {
                Debug.Log("P3Debug: no AttentionManager.");
                return;
            }

            var target = manager.CurrentTarget;
            Debug.Log($"P3Debug: state={manager.State}, target={(target != null ? target.GetShortName() : "none")}, holdProgress={manager.HoldProgress01:0.00}");
        }

        private static void InvokeManagerMethod(string methodName)
        {
            var manager = AttentionManager.Instance;
            if (manager == null)
            {
                Debug.Log("P3Debug: no AttentionManager (is Play Mode running?).");
                return;
            }

            typeof(AttentionManager)
                .GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(manager, null);
            Debug.Log($"P3Debug: invoked {methodName}.");
        }
    }
}
