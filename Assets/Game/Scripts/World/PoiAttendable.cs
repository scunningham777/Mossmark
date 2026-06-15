using Mossmark.Attention;
using Mossmark.Development;
using UnityEngine;

namespace Mossmark.World
{
    // Iteration 13: a session's selected-archetype POI. "Inaccessible until its gating
    // dependency is satisfied, then attendable with its distinctive yield" per
    // PROTOTYPE2.md Section 6 - mechanically an ongoing wilderness spot (same
    // tick/yield/interrupt shape as GenericWildernessSpotAttendable via
    // ItemYieldRoller), gated by an IDependencyCondition against the parent
    // archetype's specializationId: the corresponding NPC must have realized that
    // specialization (e.g. a Bog Keeper) before the hollow opens up.
    public class PoiAttendable : MonoBehaviour, IAttendable
    {
        [SerializeField] private string displayName = "Hidden Hollow";
        [SerializeField] private string lockedDescription = "Something here feels just out of reach.";
        [SerializeField] private string interactionVerb = "search";
        [SerializeField] private ItemYield[] commonYields;
        [SerializeField] private ItemYield rareYield;
        [SerializeField, Range(0f, 1f)] private float rareDropChance = 0.08f;

        // Same randomized-per-tick interval approach as GenericWildernessSpotAttendable.
        [SerializeField, Min(0.1f)] private float minTickInterval = 1.5f;
        [SerializeField, Min(0.1f)] private float maxTickInterval = 2f;

        private bool continueAttending;
        private float currentTickInterval;

        // The "optional IDependencyCondition gate" from Section 6. IsSatisfied/
        // GetNeedsDescription ignore their DevelopableEntity parameter for this
        // condition type (same as WorldStateCondition/SpecializationNeededCondition/
        // TimeCondition), so PoiAttendable can reuse the generic resolver's condition
        // types without itself being a DevelopableEntity - there's nothing here to
        // develop, only to unlock.
        private IDependencyCondition gate;

        // Procedural-spawn entry point (WorldGenerator) - same shape as
        // GenericWildernessSpotAttendable.Initialize.
        public void Initialize(string displayName, string lockedDescription, string interactionVerb,
            ItemYield[] commonYields, ItemYield rareYield, float rareDropChance,
            float minTickInterval, float maxTickInterval, IDependencyCondition gate)
        {
            this.displayName = displayName;
            this.lockedDescription = lockedDescription;
            this.interactionVerb = interactionVerb;
            this.commonYields = commonYields;
            this.rareYield = rareYield;
            this.rareDropChance = rareDropChance;
            this.minTickInterval = minTickInterval;
            this.maxTickInterval = maxTickInterval;
            this.gate = gate;
        }

        private void Awake()
        {
            RollTickInterval();
        }

        public float AttentionDuration => currentTickInterval;

        public bool RequiresDaylight => true;

        // False on the tick a rare drop is rolled - same interrupt shape as
        // GenericWildernessSpotAttendable.
        public bool ContinueAttending => continueAttending;

        public bool CanAttend() => gate == null || gate.IsSatisfied(null);

        public string GetOverlayDescription() => CanAttend() ? displayName : lockedDescription;

        public string GetOverlayInteractionLine() => CanAttend()
            ? $"Hold E to {interactionVerb}"
            : gate.GetNeedsDescription(null);

        public void OnAttentionComplete()
        {
            continueAttending = !ItemYieldRoller.Roll(displayName, "found", commonYields, rareYield, rareDropChance);
            RollTickInterval();
        }

        public void OnAttentionCancelled()
        {
        }

        private void RollTickInterval()
        {
            currentTickInterval = Random.Range(minTickInterval, maxTickInterval);
        }
    }
}
