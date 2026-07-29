using System.Collections.Generic;
using Mossmark.Attention;
using UnityEngine;

namespace Mossmark.Prototype5
{
    // Iteration 5.2 (The Ambient Attend, With Nothing To Find): attending the place you
    // are standing in rather than a thing in it. Lives on the Player and registers itself
    // as the AttendableDetector's fallback, so E resolves here only when nothing specific
    // is in range — this is what's left when there's nothing else, not a mode the player
    // switches into.
    //
    // Everything that makes it read as a real attend rather than an idle animation comes
    // from running through the ordinary AttentionManager tick loop rather than around it:
    // PlayerController's rock keys off AttentionManager.State, daylight is spent by the
    // manager's own SpendDaylight() call, and the day-phase messaging follows from that.
    // Nothing here reimplements any of it.
    //
    // The tick is longer than a focused attend's (3s vs P4's 2s) and costs the same one
    // daylight, which is the whole of "slower rate" — no change to the daylight model, no
    // fractional costs. Undirected attention buys less per second than focused attention
    // because it isn't pointed at anything; that it costs anything at all is the point.
    //
    // 5.2 deliberately has nothing to find. The dwell/notice pass hangs off this
    // component at 5.3; if holding attention on a place isn't worth doing before there is
    // anything to notice, noticing is just a loot roll with extra steps.
    public class AmbientSurroundingsAttendable : MonoBehaviour, IAttendable
    {
        [SerializeField] private string displayName = "Here";
        [SerializeField] private string interactionLine = "Hold E to attend to your surroundings";

        // Iteration 5.2.1: off, so ambient attending draws no UI at all — the player rock
        // and the day clock advancing are the whole feedback surface. This is the reversal
        // switch for that decision: tick it in the Inspector (live, no recompile) and the
        // name panel, progress bar, and detail panel all come back, along with the
        // attendingLines below, which are kept for exactly that reason rather than deleted.
        [SerializeField] private bool showOverlay;

        // One daylight per tick, so this is the rate knob. Longer than a focused attend.
        [SerializeField, Min(0.1f)] private float tickDuration = 3f;

        // Rotated one per completed tick rather than held static, so a long hold reads as
        // time passing in a place instead of as a state you are parked in — the daylight
        // going down should feel like the day going by. Nothing here reports progress,
        // hints, or points anywhere; there is nothing to point at.
        //
        // Deliberately phase-agnostic: these are drawn at random against a day clock that
        // is running underneath them, so any line naming a time of day ("the afternoon
        // is…") will eventually fire at dawn and contradict the HUD. Caught in the 5.2
        // verification run doing exactly that.
        [SerializeField, TextArea] private string[] attendingLines =
        {
            "You stop, and let the place be what it is.",
            "Wind in something. The light shifting very slightly.",
            "Nothing announces itself.",
            "The ground, the air over it, the sound of neither.",
            "Time going by at the pace it goes by at.",
            "You are not looking for anything in particular.",
            "Somewhere behind you, something small moves and stops.",
            "The day is doing what days do.",
            "Nothing here needs you for anything.",
            "You keep still. The place carries on around it.",
        };

        private int lineIndex = -1;
        private string currentLine = string.Empty;

        public float AttentionDuration => tickDuration;

        public bool RequiresDaylight => true;

        // Runs until the player lets go (or the day does). It has no completion state to
        // reach — that's what distinguishes attending a place from attending a thing.
        public bool ContinueAttending => true;

        public bool CanAttend() => true;

        public bool ShowOverlay => showOverlay;

        public string GetShortName() => displayName;

        public string GetOverlayDescription() => currentLine;

        public string GetOverlayInteractionLine() => interactionLine;

        public IReadOnlyList<string> GetAppliedUpgrades() => System.Array.Empty<string>();

        private void Start()
        {
            var detector = GetComponent<AttendableDetector>() ?? FindAnyObjectByType<AttendableDetector>();
            if (detector == null)
            {
                Debug.LogError("AmbientSurroundingsAttendable found no AttendableDetector to register with.", this);
                return;
            }

            detector.SetFallbackTarget(this);
            AdvanceLine();
        }

        public void OnAttentionComplete()
        {
            AdvanceLine();
            Debug.Log($"P5: ambient tick — \"{currentLine}\"", this);
        }

        public void OnAttentionCancelled() { }

        private void AdvanceLine()
        {
            if (attendingLines == null || attendingLines.Length == 0) return;
            if (attendingLines.Length == 1)
            {
                currentLine = attendingLines[0];
                return;
            }

            int index;
            do
            {
                index = Random.Range(0, attendingLines.Length);
            }
            while (index == lineIndex);

            lineIndex = index;
            currentLine = attendingLines[index];
        }

        public string DebugAmbientState() =>
            $"tickDuration={tickDuration}, currentLine=\"{currentLine}\"";
    }
}
