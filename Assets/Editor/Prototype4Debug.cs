using System.Reflection;
using Mossmark.Attention;
using Mossmark.Day;
using Mossmark.Prototype4;
using UnityEditor;
using UnityEngine;

namespace Mossmark.EditorTools
{
    // Play-mode test drivers for the Prototype4 scene, same pattern as Prototype3Debug:
    // MCP-Unity can execute menu items but can't press keys, so these reproduce what
    // holding E does via reflection into AttentionManager.
    public static class Prototype4Debug
    {
        [MenuItem("Mossmark/Prototype4/Begin Attend")]
        private static void BeginAttend() => InvokeManagerMethod("HandleHoldStarted");

        [MenuItem("Mossmark/Prototype4/Release Attend")]
        private static void ReleaseAttend() => InvokeManagerMethod("HandleHoldReleased");

        [MenuItem("Mossmark/Prototype4/Teleport Player To Netmender")]
        private static void TeleportToNetmender() => TeleportToNamed("The Netmender");

        [MenuItem("Mossmark/Prototype4/Teleport Player To Smokehouse")]
        private static void TeleportToSmokehouse() => TeleportToNamed("The Smokehouse");

        [MenuItem("Mossmark/Prototype4/Teleport Player To Osier Bed")]
        private static void TeleportToOsierBed() => TeleportToNamed("The Osier Bed");

        [MenuItem("Mossmark/Prototype4/Teleport Player To The Landing")]
        private static void TeleportToLanding() => TeleportToNamed("The Landing");

        [MenuItem("Mossmark/Prototype4/Teleport Player To Collier")]
        private static void TeleportToCollier() => TeleportToNamed("The Collier");

        [MenuItem("Mossmark/Prototype4/Teleport Player To Bothy")]
        private static void TeleportToBothy() => TeleportToNamed("The Colliers Bothy");

        [MenuItem("Mossmark/Prototype4/Teleport Player To Hearth Ring")]
        private static void TeleportToHearthRing() => TeleportToNamed("The Hearth Ring");

        // Iteration 4.14: A Char Knot / A Fused Clinker were condensed into The Ash
        // Bed (mirrors 4.12's river-landing condensation) — their standalone
        // PropertyPickupAttendable GameObjects no longer exist.
        [MenuItem("Mossmark/Prototype4/Teleport Player To Ash Bed")]
        private static void TeleportToAshBed() => TeleportToNamed("The Ash Bed");

        [MenuItem("Mossmark/Prototype4/Teleport Player To Smoking Racks")]
        private static void TeleportToSmokingRacks() => TeleportToNamed("The Smoking Racks");

        [MenuItem("Mossmark/Prototype4/Teleport Player To Bedroll")]
        private static void TeleportToBedroll()
        {
            TeleportTo(Object.FindAnyObjectByType<BedrollAttendable>());
        }

        // Iteration 4.2: force the nearest entity's next acquaintance stage across
        // (replaces 4.1's bool flip, which the real track superseded).
        [MenuItem("Mossmark/Prototype4/Advance Acquaintance On Nearest Entity")]
        private static void AdvanceAcquaintance()
        {
            var entity = FindNearestAcquaintable();
            if (entity == null) return;
            entity.DebugAdvanceAcquaintance();
        }

        // Iteration 4.10: force the nearest entity's teach tick without requiring the
        // player to have actually worked the paired property out at the racks first —
        // no-ops (logged) if there's no pending want or the entity already knows it.
        [MenuItem("Mossmark/Prototype4/Force Teach On Nearest Entity")]
        private static void ForceTeach()
        {
            var entity = FindNearestAcquaintable();
            if (entity == null) return;
            entity.DebugForceTeach();
        }

        [MenuItem("Mossmark/Prototype4/Log Entity State")]
        private static void LogEntityState()
        {
            foreach (var entity in Object.FindObjectsByType<AcquaintableAttendable>())
            {
                var spriteRenderer = entity.GetComponent<SpriteRenderer>();
                Debug.Log($"P4Debug: '{entity.name}' acquaintanceStage={entity.CurrentStageIndex}, " +
                    $"pendingAttends={entity.PendingProgress}, shortName=\"{entity.GetShortName()}\", " +
                    $"subject=({entity.ComputeSubjectFingerprint()}), tint={spriteRenderer.color}, " +
                    $"overlay=\"{entity.GetOverlayDescription()}\", interaction=\"{entity.GetOverlayInteractionLine()}\"");
            }
        }

        // Iteration 4.13: surfaces TendableSpotAttendable's internal ripeness state
        // (days away, attends spent today, the next roll's effective chance) so the
        // organic ramp can be verified without inferring it from miss/hit frequency
        // alone across a long MCP session.
        [MenuItem("Mossmark/Prototype4/Log Tending Spot State")]
        private static void LogTendingSpotState()
        {
            foreach (var spot in Object.FindObjectsByType<TendableSpotAttendable>())
            {
                Debug.Log($"P4Debug: '{spot.name}' {spot.DebugRipenessState()}, {spot.DebugCharacterState()}");
            }
        }

        // Iteration 4.13: calls DayCycleManager.Rest() directly (public API, the same
        // method BedrollAttendable's OnAttentionComplete already calls) so multi-day
        // ripeness testing doesn't require walking to the bedroll and holding through
        // a real Begin/Release Attend cycle for every day advanced.
        [MenuItem("Mossmark/Prototype4/Force Rest (Advance Day)")]
        private static void ForceRest()
        {
            var day = DayCycleManager.Instance;
            if (day == null)
            {
                Debug.Log("P4Debug: no DayCycleManager (is Play Mode running?).");
                return;
            }

            day.Rest();
            Debug.Log("P4Debug: forced Rest().");
        }

        [MenuItem("Mossmark/Prototype4/Log Daylight")]
        private static void LogDaylight()
        {
            var day = DayCycleManager.Instance;
            if (day == null)
            {
                Debug.Log("P4Debug: no DayCycleManager.");
                return;
            }

            Debug.Log($"P4Debug: daylight {day.DaylightRemaining}/{day.MaxDaylight}, phase={day.CurrentPhase}, dayIndex={day.DayIndex}");
        }

        [MenuItem("Mossmark/Prototype4/Log Attention State")]
        private static void LogAttentionState()
        {
            var manager = AttentionManager.Instance;
            if (manager == null)
            {
                Debug.Log("P4Debug: no AttentionManager.");
                return;
            }

            var target = manager.CurrentTarget;
            Debug.Log($"P4Debug: state={manager.State}, target={(target != null ? target.GetShortName() : "none")}, holdProgress={manager.HoldProgress01:0.00}");
        }

        private static AcquaintableAttendable FindNearestAcquaintable()
        {
            var player = GameObject.FindWithTag("Player");
            if (player == null)
            {
                Debug.Log("P4Debug: no Player in scene.");
                return null;
            }

            AcquaintableAttendable nearest = null;
            float nearestSqrDist = float.MaxValue;
            foreach (var entity in Object.FindObjectsByType<AcquaintableAttendable>())
            {
                float sqrDist = (entity.transform.position - player.transform.position).sqrMagnitude;
                if (sqrDist < nearestSqrDist)
                {
                    nearestSqrDist = sqrDist;
                    nearest = entity;
                }
            }

            if (nearest == null) Debug.Log("P4Debug: no AcquaintableAttendable in scene.");
            return nearest;
        }

        private static void TeleportToNamed(string gameObjectName)
        {
            var target = GameObject.Find(gameObjectName);
            if (target == null)
            {
                Debug.Log($"P4Debug: no GameObject named '{gameObjectName}'.");
                return;
            }

            TeleportTo(target.transform);
        }

        private static void TeleportTo(Component target)
        {
            var player = GameObject.FindWithTag("Player");
            if (player == null || target == null)
            {
                Debug.Log("P4Debug: missing Player or teleport target.");
                return;
            }

            var destination = (Vector2)target.transform.position + new Vector2(0.6f, 0f);
            var rb = player.GetComponent<Rigidbody2D>();
            if (rb != null) rb.position = destination;
            else player.transform.position = destination;
            Physics2D.SyncTransforms();
            Debug.Log($"P4Debug: teleported player next to '{target.name}'.");
        }

        private static void InvokeManagerMethod(string methodName)
        {
            var manager = AttentionManager.Instance;
            if (manager == null)
            {
                Debug.Log("P4Debug: no AttentionManager (is Play Mode running?).");
                return;
            }

            typeof(AttentionManager)
                .GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(manager, null);
            Debug.Log($"P4Debug: invoked {methodName}.");
        }
    }
}
