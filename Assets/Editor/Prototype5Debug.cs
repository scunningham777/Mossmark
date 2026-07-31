using System.Reflection;
using Mossmark.Attention;
using Mossmark.Day;
using Mossmark.Prototype4;
using Mossmark.Prototype5;
using UnityEditor;
using UnityEngine;

namespace Mossmark.EditorTools
{
    // Play-mode test drivers for the Prototype5 scene, same pattern as Prototype3Debug
    // and Prototype4Debug: MCP-Unity can execute menu items but can't press keys, so
    // these reproduce what holding E does via reflection into AttentionManager.
    //
    // Note that Begin Attend followed by no Release leaves the ambient hold running, which
    // is the point — it's the only way to observe a multi-tick ambient attend draining the
    // day without a keyboard.
    public static class Prototype5Debug
    {
        [MenuItem("Mossmark/Prototype5/Begin Attend")]
        private static void BeginAttend() => InvokeManagerMethod("HandleHoldStarted");

        [MenuItem("Mossmark/Prototype5/Release Attend")]
        private static void ReleaseAttend() => InvokeManagerMethod("HandleHoldReleased");

        [MenuItem("Mossmark/Prototype5/Log Ambient State")]
        private static void LogAmbientState()
        {
            var ambient = Object.FindAnyObjectByType<AmbientSurroundingsAttendable>();
            if (ambient == null)
            {
                Debug.Log("P5Debug: no AmbientSurroundingsAttendable in scene.");
                return;
            }

            var manager = AttentionManager.Instance;
            bool registered = manager != null && ReferenceEquals(manager.FallbackTarget, ambient);
            Debug.Log($"P5Debug: ambient {ambient.DebugAmbientState()}, registeredAsFallback={registered}, " +
                $"showOverlay={ambient.ShowOverlay}, {DescribeOverlayPanels()}");
        }

        // Iteration 5.2.1: the claim being verified is that *nothing draws*, which is
        // invisible to every other readout — reflecting into AttendableOverlayUI's two
        // panel roots is the only way to prove it from here rather than infer it.
        private static string DescribeOverlayPanels()
        {
            var overlay = Object.FindAnyObjectByType<AttendableOverlayUI>();
            if (overlay == null) return "overlayUI=none";

            // Iteration 5.4.1 added the name label and interaction line to this readout:
            // the whole claim is that the panel shows *part* of itself, so "the panel is
            // visible" is no longer enough to distinguish pass from fail.
            return $"namePanel={ReadPanelDisplay(overlay, "overlayRoot")}, " +
                $"name=\"{ReadLabelText(overlay, "descriptionLabel")}\", " +
                $"interactLine={ReadPanelDisplay(overlay, "interactionLabel")}" +
                $"/\"{ReadLabelText(overlay, "interactionLabel")}\", " +
                $"detailPanel={ReadPanelDisplay(overlay, "detailRoot")}";
        }

        private static UnityEngine.UIElements.VisualElement ReadElement(
            AttendableOverlayUI overlay, string fieldName)
        {
            var field = typeof(AttendableOverlayUI)
                .GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            return field?.GetValue(overlay) as UnityEngine.UIElements.VisualElement;
        }

        private static string ReadPanelDisplay(AttendableOverlayUI overlay, string fieldName)
        {
            var element = ReadElement(overlay, fieldName);
            return element == null ? "null" : element.style.display.value.ToString();
        }

        private static string ReadLabelText(AttendableOverlayUI overlay, string fieldName)
        {
            return ReadElement(overlay, fieldName) is UnityEngine.UIElements.Label label
                ? label.text
                : "?";
        }

        [MenuItem("Mossmark/Prototype5/Teleport Player To Bedroll")]
        private static void TeleportToBedroll()
        {
            var bedroll = Object.FindAnyObjectByType<BedrollAttendable>();
            if (bedroll == null)
            {
                Debug.Log("P5Debug: no BedrollAttendable in scene.");
                return;
            }

            TeleportTo(bedroll.transform.position);
        }

        // Iteration 5.3: lands the player inside the noticing radius of the nearest
        // unresolved thing, since dwell only accrues while an ambient hold is running and
        // walking there by hand isn't possible from the editor.
        [MenuItem("Mossmark/Prototype5/Teleport Player To Nearest Unresolved Thing")]
        private static void TeleportToNearestUnresolved()
        {
            TeleportToNearest(null);
        }

        // Iteration 5.4: the two outcomes have to be reachable separately to be verified
        // separately — from the editor there is no other way to pick which one a hold
        // lands on, since the whole point is that they're indistinguishable in the scene.
        [MenuItem("Mossmark/Prototype5/Teleport Player To Nearest Something")]
        private static void TeleportToNearestSomething() => TeleportToNearest(true);

        [MenuItem("Mossmark/Prototype5/Teleport Player To Nearest Nothing")]
        private static void TeleportToNearestNothing() => TeleportToNearest(false);

        private static void TeleportToNearest(bool? holdsSomething)
        {
            var thing = FindNearestThing(unresolvedOnly: true, holdsSomething);
            if (thing == null) return;

            TeleportTo(thing.transform.position + new Vector3(0.6f, 0f, 0f));
            Debug.Log($"P5Debug: nearest unresolved is '{thing.name}'.", thing);
        }

        [MenuItem("Mossmark/Prototype5/Force Resolve Nearest Thing")]
        private static void ForceResolveNearest()
        {
            var thing = FindNearestThing(unresolvedOnly: true);
            if (thing == null) return;

            thing.DebugResolve();
        }

        [MenuItem("Mossmark/Prototype5/Log Noticeable Things")]
        private static void LogNoticeableThings()
        {
            var things = Object.FindObjectsByType<NoticeableThing>();
            if (things.Length == 0)
            {
                Debug.Log("P5Debug: no NoticeableThing in scene.");
                return;
            }

            int resolvedSomething = 0;
            int resolvedNothing = 0;
            int something = 0;
            foreach (var thing in things)
            {
                if (thing.HoldsSomething) something++;
                if (thing.IsResolved)
                {
                    if (thing.HoldsSomething) resolvedSomething++;
                    else resolvedNothing++;
                }

                Debug.Log($"P5Debug: '{thing.name}' {thing.DebugNoticeState()}", thing);
            }

            Debug.Log($"P5Debug: {things.Length} things ({something} something / " +
                $"{things.Length - something} nothing); resolved {resolvedSomething} something, " +
                $"{resolvedNothing} nothing.");
        }

        private static NoticeableThing FindNearestThing(bool unresolvedOnly, bool? holdsSomething = null)
        {
            var player = GameObject.FindWithTag("Player");
            if (player == null)
            {
                Debug.Log("P5Debug: no Player in scene.");
                return null;
            }

            NoticeableThing nearest = null;
            float nearestSqrDist = float.MaxValue;
            foreach (var thing in Object.FindObjectsByType<NoticeableThing>())
            {
                if (unresolvedOnly && thing.IsResolved) continue;
                if (holdsSomething.HasValue && thing.HoldsSomething != holdsSomething.Value) continue;

                float sqrDist = (thing.transform.position - player.transform.position).sqrMagnitude;
                if (sqrDist < nearestSqrDist)
                {
                    nearestSqrDist = sqrDist;
                    nearest = thing;
                }
            }

            if (nearest == null) Debug.Log("P5Debug: no matching NoticeableThing in scene.");
            return nearest;
        }

        // Iteration 5.5: the acquaintance ladder is P4's, so these three mirror
        // Prototype4Debug's equivalents rather than inventing a P5 vocabulary for it —
        // the point of the iteration is that nothing about the entity is P5-specific.
        // Teleporting lands the player *outside* the noticing radius on purpose: the
        // Fowler is unnoticed at scene load, so dropping in beside her and holding is
        // how the found-then-known sequence gets exercised end to end.
        [MenuItem("Mossmark/Prototype5/Teleport Player To Nearest Acquaintable")]
        private static void TeleportToNearestAcquaintable()
        {
            var entity = FindNearestAcquaintable();
            if (entity == null) return;

            TeleportTo(entity.transform.position + new Vector3(0.6f, 0f, 0f));
            Debug.Log($"P5Debug: nearest acquaintable is '{entity.name}'.", entity);
        }

        [MenuItem("Mossmark/Prototype5/Advance Acquaintance On Nearest Entity")]
        private static void AdvanceAcquaintance()
        {
            var entity = FindNearestAcquaintable();
            if (entity == null) return;
            entity.DebugAdvanceAcquaintance();
        }

        // Logs the NoticeableThing gate beside the entity as well: 5.5's whole surface
        // is the seam between the two, and a stage that won't advance because the thing
        // was never resolved looks identical to a stage that won't advance for its own
        // reasons unless both are on the same line.
        [MenuItem("Mossmark/Prototype5/Log Acquaintance State")]
        private static void LogAcquaintanceState()
        {
            var entities = Object.FindObjectsByType<AcquaintableAttendable>();
            if (entities.Length == 0)
            {
                Debug.Log("P5Debug: no AcquaintableAttendable in scene.");
                return;
            }

            foreach (var entity in entities)
            {
                var gate = entity.GetComponent<NoticeableThing>();
                var spriteRenderer = entity.GetComponent<SpriteRenderer>();
                Debug.Log($"P5Debug: '{entity.name}' acquaintanceStage={entity.CurrentStageIndex}, " +
                    $"pendingAttends={entity.PendingProgress}, shortName=\"{entity.GetShortName()}\", " +
                    $"subject=({entity.ComputeSubjectFingerprint()}), " +
                    $"tint={(spriteRenderer != null ? spriteRenderer.color.ToString() : "n/a")}, " +
                    $"gate=({(gate != null ? gate.DebugNoticeState() : "none")}), " +
                    $"overlay=\"{entity.GetOverlayDescription()}\", " +
                    $"interaction=\"{entity.GetOverlayInteractionLine()}\"", entity);
            }
        }

        private static AcquaintableAttendable FindNearestAcquaintable()
        {
            var player = GameObject.FindWithTag("Player");
            if (player == null)
            {
                Debug.Log("P5Debug: no Player in scene.");
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

            if (nearest == null) Debug.Log("P5Debug: no AcquaintableAttendable in scene.");
            return nearest;
        }

        // Iteration 5.6: the tending shape is P4's too (4.12/4.14), so this mirrors the
        // 5.5 acquaintable drivers rather than inventing a separate P5 vocabulary —
        // same reasoning, different component.
        [MenuItem("Mossmark/Prototype5/Teleport Player To Nearest Tendable Spot")]
        private static void TeleportToNearestTendableSpot()
        {
            var spot = FindNearestTendableSpot();
            if (spot == null) return;

            TeleportTo(spot.transform.position + new Vector3(0.6f, 0f, 0f));
            Debug.Log($"P5Debug: nearest tendable spot is '{spot.name}'.", spot);
        }

        // Logs the NoticeableThing gate on the same line as the spot's own state, same
        // reasoning as Log Acquaintance State: a spot that never yields because it was
        // never resolved looks identical to one that never yields on a bad roll unless
        // both are visible together.
        [MenuItem("Mossmark/Prototype5/Log Tendable Spot State")]
        private static void LogTendableSpotState()
        {
            var spots = Object.FindObjectsByType<TendableSpotAttendable>();
            if (spots.Length == 0)
            {
                Debug.Log("P5Debug: no TendableSpotAttendable in scene.");
                return;
            }

            foreach (var spot in spots)
            {
                var gate = spot.GetComponent<NoticeableThing>();
                Debug.Log($"P5Debug: '{spot.name}' {spot.DebugRipenessState()}, " +
                    $"{spot.DebugCharacterState()}, " +
                    $"gate=({(gate != null ? gate.DebugNoticeState() : "none")})", spot);
            }
        }

        private static TendableSpotAttendable FindNearestTendableSpot()
        {
            var player = GameObject.FindWithTag("Player");
            if (player == null)
            {
                Debug.Log("P5Debug: no Player in scene.");
                return null;
            }

            TendableSpotAttendable nearest = null;
            float nearestSqrDist = float.MaxValue;
            foreach (var spot in Object.FindObjectsByType<TendableSpotAttendable>())
            {
                float sqrDist = (spot.transform.position - player.transform.position).sqrMagnitude;
                if (sqrDist < nearestSqrDist)
                {
                    nearestSqrDist = sqrDist;
                    nearest = spot;
                }
            }

            if (nearest == null) Debug.Log("P5Debug: no TendableSpotAttendable in scene.");
            return nearest;
        }

        [MenuItem("Mossmark/Prototype5/Force Rest (Advance Day)")]
        private static void ForceRest()
        {
            var day = DayCycleManager.Instance;
            if (day == null)
            {
                Debug.Log("P5Debug: no DayCycleManager (is Play Mode running?).");
                return;
            }

            day.Rest();
            Debug.Log("P5Debug: forced Rest().");
        }

        [MenuItem("Mossmark/Prototype5/Log Daylight")]
        private static void LogDaylight()
        {
            var day = DayCycleManager.Instance;
            if (day == null)
            {
                Debug.Log("P5Debug: no DayCycleManager.");
                return;
            }

            Debug.Log($"P5Debug: daylight {day.DaylightRemaining}/{day.MaxDaylight}, phase={day.CurrentPhase}, dayIndex={day.DayIndex}");
        }

        [MenuItem("Mossmark/Prototype5/Log Attention State")]
        private static void LogAttentionState()
        {
            var manager = AttentionManager.Instance;
            if (manager == null)
            {
                Debug.Log("P5Debug: no AttentionManager.");
                return;
            }

            var target = manager.CurrentTarget;
            var fallback = manager.FallbackTarget;
            var attending = manager.AttendingTarget;
            Debug.Log($"P5Debug: state={manager.State}, " +
                $"currentTarget={(target != null ? target.GetShortName() : "none")}, " +
                $"fallback={(fallback != null ? fallback.GetShortName() : "none")}, " +
                $"attending={(attending != null ? attending.GetShortName() : "none")}, " +
                $"holdProgress={manager.HoldProgress01:0.00}, {DescribeOverlayPanels()}");
        }

        private static void TeleportTo(Vector3 destination)
        {
            var player = GameObject.FindWithTag("Player");
            if (player == null)
            {
                Debug.Log("P5Debug: no Player in scene.");
                return;
            }

            var rb = player.GetComponent<Rigidbody2D>();
            if (rb != null) rb.position = destination;
            else player.transform.position = destination;
            Physics2D.SyncTransforms();
            Debug.Log($"P5Debug: teleported player to {destination}.");
        }

        private static void InvokeManagerMethod(string methodName)
        {
            var manager = AttentionManager.Instance;
            if (manager == null)
            {
                Debug.Log("P5Debug: no AttentionManager (is Play Mode running?).");
                return;
            }

            typeof(AttentionManager)
                .GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(manager, null);
            Debug.Log($"P5Debug: invoked {methodName}.");
        }
    }
}
