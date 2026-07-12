using System.Collections.Generic;
using Mossmark.Visuals;
using UnityEngine;

namespace Mossmark.World
{
    // Iteration 47 (generalized to every selected archetype): a Site's ground truth as a
    // place, not just a spawn-clustering anchor (Iteration 42). Standing
    // (SustainedGoodAttentionCondition) was spot-scoped, which rewarded camping one exact
    // spot across days — the opposite of what a Site should mean. WorldSite gives its
    // member spots one shared goodAttentionDays counter (no new condition type: this
    // simply implements IGoodAttentionTracker itself, and member spots read it instead of
    // their own field via DevelopingWildernessSpotAttendable.site) AND owns the moment
    // Standing actually crosses — see TryApplyStandingToMembers() below — so a site with
    // several members crosses together, with one announcement, instead of each member
    // separately crossing (and separately announcing) whenever it's next attended.
    //
    // Also renders the site's ground plane — a low-alpha circle at the same radius
    // WorldGenerator already uses for spawn jitter, so the visible boundary and the
    // mechanical clustering radius are identical by construction. This is the only
    // feedback this iteration ships: approaching the cluster should read as entering one
    // place before anything is attended.
    public class WorldSite : MonoBehaviour, IGoodAttentionTracker
    {
        [SerializeField] private string archetypeId;
        [SerializeField] private string displayName;
        [SerializeField, Min(0.1f)] private float radius = 3f;
        [SerializeField] private Color color = Color.white;

        private readonly List<DevelopingWildernessSpotAttendable> members = new();

        private int goodAttentionDays;
        private int lastGoodDayIndex = -1;
        private bool hasReachedStanding;

        public string ArchetypeId => archetypeId;

        // IGoodAttentionTracker — read by SustainedGoodAttentionCondition via a member
        // spot's own GoodAttentionDays getter delegating here when spot.site != null.
        public int GoodAttentionDays => goodAttentionDays;

        // Procedural-spawn entry point (WorldGenerator), inactive-GO pattern — call
        // before SetActive(true) so Awake bakes the ground sprite with correct data.
        public void Initialize(string archetypeId, string displayName, float radius, Color color)
        {
            this.archetypeId = archetypeId;
            this.displayName = displayName;
            this.radius = radius;
            this.color = color;
        }

        public void RegisterMember(DevelopingWildernessSpotAttendable spot) => members.Add(spot);

        // Called by a member spot's OnDayAdvanced() in place of incrementing its own
        // counter, on a day it was attended and not overworked. Gated on dayIndex rather
        // than event firing order — DayCycleManager.DayIndex is incremented once before
        // DayAdvanced fires, so every member spot's handler sees the same value that day
        // regardless of which one's subscriber happens to run first.
        public void RegisterGoodDay(int dayIndex)
        {
            if (dayIndex == lastGoodDayIndex) return;
            lastGoodDayIndex = dayIndex;
            goodAttentionDays++;

            TryApplyStandingToMembers();
        }

        // The moment the shared counter reaches the threshold every member's own
        // SustainedGoodAttentionCondition reads (they all see the same GoodAttentionDays
        // value via the delegation above), force every member across its own stage in the
        // same pass instead of waiting for each to separately be attended — that staggered
        // wait was the bug: three near-simultaneous but separate "X: feels familiar" beats
        // instead of one shared moment. TryForceStanding() is idempotent (a member not yet
        // eligible, or already applied, just no-ops), so calling it on every member every
        // rest is safe and needs no per-member bookkeeping here. Once any member actually
        // crosses, every member necessarily does too (shared dependency), so a single
        // consolidated announcement — using whichever member's own authored flavor text —
        // replaces the N separate per-spot messages that used to fire.
        private void TryApplyStandingToMembers()
        {
            if (hasReachedStanding) return;

            DevelopingWildernessSpotAttendable crossedMember = null;
            foreach (var member in members)
                if (member.TryForceStanding() && crossedMember == null)
                    crossedMember = member;

            if (crossedMember == null) return;

            hasReachedStanding = true;
            string name = !string.IsNullOrEmpty(displayName) ? displayName : archetypeId;
            string flavor = !string.IsNullOrEmpty(crossedMember.LastStandingFlavorText)
                ? crossedMember.LastStandingFlavorText
                : "feels like familiar ground now.";
            NotificationManager.Post($"{name}: {flavor}");
            Debug.Log($"{name} (site): reached Standing.", this);
        }

        private void Awake()
        {
            var sr = GetComponent<SpriteRenderer>();
            // Bake pure white into the circle texture and apply the identity color as a
            // low-alpha renderer tint (rather than baking the color itself), so the alpha
            // multiplies cleanly instead of compounding with the baked color.
            sr.sprite = CircleSpriteGenerator.CreateSprite(Color.white);
            sr.color = new Color(color.r, color.g, color.b, 0.18f);
            // Between the wilderness ground (-2) and default entities (0) — reads as a
            // ground detail, never occludes anything standing on it.
            sr.sortingOrder = -1;
            transform.localScale = Vector3.one * (radius * 2f);
        }
    }
}
