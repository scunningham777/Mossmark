using System.Reflection;
using Mossmark.Attention;
using Mossmark.Day;
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

            return $"namePanel={ReadPanelDisplay(overlay, "overlayRoot")}, " +
                $"detailPanel={ReadPanelDisplay(overlay, "detailRoot")}";
        }

        private static string ReadPanelDisplay(AttendableOverlayUI overlay, string fieldName)
        {
            var field = typeof(AttendableOverlayUI)
                .GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (field == null) return "?";

            var element = field.GetValue(overlay) as UnityEngine.UIElements.VisualElement;
            return element == null ? "null" : element.style.display.value.ToString();
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
