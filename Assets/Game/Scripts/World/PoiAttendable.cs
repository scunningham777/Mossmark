using Mossmark.Development;
using UnityEngine;

namespace Mossmark.World
{
    // Archetype POI (Point of Interest) — thin subclass of WildernessYieldAttendable that
    // adds a two-tier gate (Iteration 45's VisibleInert/Interactable split) and a locked
    // overlay description. Existence itself (Hidden -> spawned) is decided by WorldGenerator,
    // which only ever creates this component once a POI has left the Hidden tier — so this
    // class's own runtime state only ever spans VisibleInert -> Interactable, never Hidden.
    //
    // VisibleInert disables the sibling Collider2D that AttendableZone/AttendableDetector
    // rely on to register a target at all, so an inert POI is visually present (sprite still
    // renders) but never enters attention range, never shows a hover tooltip, and CanAttend()
    // is never even reached in practice. This is deliberate, per the design's own tier
    // definition: "present, not interactable" — the player notices it by sight, not by a hint.
    //
    // POIs intentionally do NOT apply TwilightChanceModifier — they are distinctive rare-access
    // locations, not ambient foraging spots, so their base rareDropChance is used unchanged.
    public class PoiAttendable : WildernessYieldAttendable
    {
        private string lockedDescription = "Something here feels just out of reach.";
        private IDependencyCondition unlockCondition;
        private Collider2D zoneCollider;
        private PoiTier tier;

        public void Initialize(string displayName, string lockedDescription, string interactionVerb,
            ItemYield[] commonYields, ItemYield[] rareYields, float rareDropChance,
            float minTickInterval, float maxTickInterval, IDependencyCondition unlockCondition)
        {
            InitializeBase(displayName, interactionVerb, commonYields, rareYields, rareDropChance,
                minTickInterval, maxTickInterval);
            this.lockedDescription = lockedDescription;
            this.unlockCondition = unlockCondition;
            foundVerb = "found";

            // Called before SetActive(true) (inactive-GO spawning pattern), so no Awake has
            // run yet on any sibling component — safe to read/write the collider now.
            zoneCollider = GetComponent<Collider2D>();
            tier = IsUnlockConditionSatisfied() ? PoiTier.Interactable : PoiTier.VisibleInert;
            if (zoneCollider != null) zoneCollider.enabled = tier == PoiTier.Interactable;
        }

        private void Update()
        {
            // Latches permanently once Interactable — matches every other Standing-style
            // gate in the game (a crossed threshold never un-crosses).
            if (tier == PoiTier.Interactable) return;
            if (!IsUnlockConditionSatisfied()) return;

            tier = PoiTier.Interactable;
            if (zoneCollider != null) zoneCollider.enabled = true;
        }

        private bool IsUnlockConditionSatisfied() => unlockCondition == null || unlockCondition.IsSatisfied(null);

        public override bool CanAttend() => tier == PoiTier.Interactable;

        public override string GetOverlayDescription() => CanAttend() ? WithTendednessSuffix(displayName) : lockedDescription;

        public override string GetOverlayInteractionLine() => CanAttend()
            ? $"Hold E to {interactionVerb}"
            : unlockCondition.GetNeedsDescription(null);
    }
}
