using Mossmark.Attention;
using Mossmark.Visuals;
using UnityEngine;

namespace Mossmark.Prototype5
{
    // Iteration 5.3 (Unnoticed Things, Noticed By Dwell): the gate that turns a grey mute
    // shape into something the attention system can see. Visible from the start — the
    // player can tell there is an object there — but with no name, no description, no
    // "hold E to…", and no way to attend it, because its zone collider is off and so it
    // never enters AttendableDetector's list at all.
    //
    // Deliberately NOT an IAttendable. It is a gate that sits *beside* whatever the thing
    // actually is (an UnknownThingAttendable for now, an AcquaintableAttendable from 5.5),
    // for two reasons. Two IAttendables on one GameObject is component-order-fragile —
    // AttendableZone.Awake takes the first match, a trap P4's build notes already flag —
    // and keeping the gate separate means 5.5 swaps the neighbour without touching any of
    // this. Nothing here knows what it is gating.
    //
    // Why the collider has to be off rather than the attendable simply refusing: an
    // unnoticed thing that stayed in zonesInRange would become CurrentTarget, and since
    // ResolveTarget() is `CurrentTarget ?? FallbackTarget`, standing near one would
    // *suppress the ambient attend entirely* — you could not attend the place you were in
    // precisely when there was something in it to find. Exactly backwards.
    public class NoticeableThing : MonoBehaviour
    {
        // Iteration 5.4: the authored truth, and the only thing that differs between a
        // thing and a decoy. Nothing about it is observable until the dwell crosses —
        // same grey, same radius, same threshold, same warm-up. A decoy still resolves;
        // what it resolves into is nothing.
        //
        // Note there is no useful C# default here: a scene entry written without the key
        // deserializes to false regardless, so every thing in the scene states it
        // explicitly rather than relying on absence to mean "something."
        [SerializeField] private bool holdsSomething = true;

        [SerializeField, Min(0f)] private float noticingRadius = 2.5f;
        [SerializeField, Min(0.1f)] private float dwellToNotice = 1.5f;

        // Decay is faster than accrual, so stepping away briefly is forgiving but
        // wandering off genuinely resets. Never negative, never past the threshold.
        [SerializeField, Min(0f)] private float dwellDecayMultiplier = 2f;

        // Grey matches the tint P4 gives an unfamiliar entity — the same visual grammar
        // carrying the analogous meaning one layer earlier: not "I don't know them yet"
        // but "I have not registered that this is anything."
        [SerializeField] private Color unnoticedColor = new(0.45f, 0.45f, 0.45f);
        [SerializeField] private Color noticedColor = new(0.72f, 0.66f, 0.45f);

        // Iteration 5.4: where a decoy ends up. Darker and cooler than unnoticed grey so
        // a resolved stump is distinguishable at a glance and the player doesn't re-dwell
        // on it forever — legible only *after* the fact, which costs nothing beforehand.
        [SerializeField] private Color settledColor = new(0.3f, 0.32f, 0.28f);

        // Faded down from the warm colour it had reached rather than snapped, so the felt
        // beat is "it came into focus, and what came into focus was ordinary."
        [SerializeField, Min(0f)] private float settleFadeDuration = 0.6f;

        private float dwell;
        private float settleElapsed = -1f;
        private Transform player;
        private SpriteRenderer spriteRenderer;
        private Collider2D zoneCollider;

        // Dwell crossed, permanently. Says nothing about whether anything was there —
        // read HoldsSomething for that.
        public bool IsResolved { get; private set; }

        public bool HoldsSomething => holdsSomething;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            zoneCollider = GetComponent<Collider2D>();

            // Done here rather than authored into the scene so an unnoticed thing can
            // never ship accidentally attendable.
            if (zoneCollider != null) zoneCollider.enabled = false;
            RefreshTint();
        }

        private void Start()
        {
            var playerObject = GameObject.FindWithTag("Player");
            if (playerObject != null) player = playerObject.transform;
        }

        private void Update()
        {
            if (IsResolved)
            {
                if (settleElapsed >= 0f) AdvanceSettleFade();
                return;
            }

            if (player == null) return;

            bool inRange = AmbientAttendRunning()
                && Vector2.Distance(player.position, transform.position) <= noticingRadius;

            dwell = Mathf.Clamp(
                dwell + (inRange ? Time.deltaTime : -Time.deltaTime * dwellDecayMultiplier),
                0f,
                dwellToNotice);

            RefreshTint();

            if (dwell >= dwellToNotice) Resolve();
        }

        // The dwell tell lives here, on the thing coming into focus, rather than as a bar
        // on the player — the constraint 5.2.1 set when the ambient progress bar came out.
        // Continuous, so a thing warms out of the grey while the player stands still and
        // cools again if they drift off, which is the whole of what ambient attending
        // gives back before anything has actually crossed.
        private void RefreshTint()
        {
            if (spriteRenderer == null) return;

            float t = dwellToNotice > 0f ? Mathf.Clamp01(dwell / dwellToNotice) : 0f;
            spriteRenderer.color = Color.Lerp(unnoticedColor, noticedColor, t);
        }

        // The dwell has crossed. Both outcomes are permanent, and both arrive at the warm
        // colour first — the difference is only what happens from there.
        private void Resolve()
        {
            IsResolved = true;
            dwell = dwellToNotice;
            RefreshTint();

            if (!holdsSomething)
            {
                // Nothing there. No collider, no pop, no notification: it stays scenery,
                // and the whole tell is the fade back down out of the warmth it briefly had.
                settleElapsed = 0f;
                Debug.Log($"P5: resolved '{name}' — nothing there.", this);
                return;
            }

            // Enabling the collider inside the player's detector fires OnTriggerEnter2D on
            // the next physics step, so the thing joins zonesInRange without anything
            // having to poke the detector. Permanent from here.
            if (zoneCollider != null) zoneCollider.enabled = true;

            GetComponent<EntityFeedback>()?.TriggerPop();

            // No notification line, on purpose: the moment is meant to be seen, not read.
            // The text arrives in the overlay when the player looks at it.
            Debug.Log($"P5: noticed '{name}'.", this);
        }

        private void AdvanceSettleFade()
        {
            settleElapsed += Time.deltaTime;

            float t = settleFadeDuration > 0f ? Mathf.Clamp01(settleElapsed / settleFadeDuration) : 1f;
            if (spriteRenderer != null) spriteRenderer.color = Color.Lerp(noticedColor, settledColor, t);

            if (t >= 1f) settleElapsed = -1f;
        }

        // "The hold currently running is the ambient one" — expressed purely in the
        // framework's own vocabulary, so nothing here needs a reference to
        // AmbientSurroundingsAttendable or a static of its own.
        private static bool AmbientAttendRunning()
        {
            var manager = AttentionManager.Instance;
            if (manager == null || manager.State != AttentionState.Attending) return false;

            var fallback = manager.FallbackTarget;
            return fallback != null && ReferenceEquals(manager.AttendingTarget, fallback);
        }

        public void DebugResolve() => Resolve();

        public string DebugNoticeState() =>
            $"holdsSomething={holdsSomething}, resolved={IsResolved}, " +
            $"dwell={dwell:0.00}/{dwellToNotice:0.00}, radius={noticingRadius}, " +
            $"settling={settleElapsed >= 0f}, " +
            $"colliderEnabled={(zoneCollider != null && zoneCollider.enabled)}, " +
            $"tint={(spriteRenderer != null ? spriteRenderer.color.ToString() : "n/a")}";
    }
}
