using System.Collections.Generic;
using Mossmark.Attention;
using Mossmark.Visuals;
using UnityEngine;

namespace Mossmark.Prototype5
{
    // Iteration 5.3: what a noticed thing is, before it is anything in particular. The
    // "unknown register" — you have registered that it is there and that it is worth a
    // look, and that is all you have. It sits beside a NoticeableThing, which decides
    // whether it can be attended at all.
    //
    // Deliberately thin, because "what the noticed objects actually do when attended" is
    // 5.5's question, not this one. Attending costs a daylight and says one line about not
    // being able to make more of it yet. At 5.5 this component is simply replaced on one
    // of the things by P4's AcquaintableAttendable, with no change to the gate beside it —
    // which is the whole reason the two are separate components.
    public class UnknownThingAttendable : MonoBehaviour, IAttendable
    {
        [SerializeField] private string displayName = "Something";
        [SerializeField, TextArea] private string description =
            "Something that isn't the rest of the wood. You've registered it, and no more than that.";
        [SerializeField] private string interactionLine = "Hold E to look closer";

        [SerializeField, TextArea] private string[] lookLines =
        {
            "You look at it a while. It stays what it was.",
            "Whatever it is, it isn't telling you today.",
            "You take it in. Nothing about it comes clear.",
        };

        [SerializeField, Min(0.1f)] private float attendDuration = 2f;

        private int lastLineIndex = -1;

        public float AttentionDuration => attendDuration;

        public bool RequiresDaylight => true;

        public bool ContinueAttending => false;

        public bool CanAttend() => true;

        public string GetShortName() => displayName;

        public string GetOverlayDescription() => description;

        public string GetOverlayInteractionLine() => interactionLine;

        public IReadOnlyList<string> GetAppliedUpgrades() => System.Array.Empty<string>();

        public void OnAttentionComplete()
        {
            if (lookLines == null || lookLines.Length == 0) return;

            int index = lookLines.Length == 1 ? 0 : NextLineIndex();
            NotificationManager.Post(lookLines[index]);
        }

        public void OnAttentionCancelled() { }

        private int NextLineIndex()
        {
            int index;
            do
            {
                index = Random.Range(0, lookLines.Length);
            }
            while (index == lastLineIndex);

            lastLineIndex = index;
            return index;
        }
    }
}
