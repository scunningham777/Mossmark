using System.Collections;
using Mossmark.Development;
using Mossmark.World;
using UnityEngine;

namespace Mossmark.Visuals
{
    // Iteration 32: per-entity visual feedback for three signal tiers.
    //
    //   1. Progress tick  — brief scale pulse on any productive OnAttentionComplete().
    //      Covers DevelopableEntity (buildings, NPCs, landmarks), WildernessYieldAttendable
    //      (generic spots, POIs), and TendedSpotAttendable (mark and harvest).
    //
    //   2. Stage crossed  — permanent shape swap (triangle ↔ circle) with a larger pop
    //      scale. Fires on DevelopableEntity.OnDeveloped. The sprite color read at swap
    //      time is always the post-stage tint (HandleDeveloped runs first via Awake
    //      subscription order). When a stage crosses the pop replaces the progress pulse
    //      for that tick via suppressNextPulse.
    //
    //   3. Passive drift  — white halo child object appears when NpcAttendable reports
    //      passive drift has accrued since the last player visit (Iteration 31 pilot).
    //      Halo is hidden when the player makes productive progress on the same entity.
    public class EntityFeedback : MonoBehaviour
    {
        private const int ShapeTextureSize = 32;
        private const float PulseScale = 1.3f;
        private const float PulseDuration = 0.18f;
        private const float StagePopScale = 1.65f;
        private const float StagePopDuration = 0.35f;

        private SpriteRenderer spriteRenderer;
        private Vector3 baseScale;
        private bool shapeIsCircle;

        // Separate coroutine handles so pulse and pop don't trample each other's bookkeeping.
        private Coroutine activePulse;
        private Coroutine activePop;

        // Set by HandleStageCrossed before OnProgressMade fires for the same tick —
        // prevents the smaller pulse from overriding the larger pop on stage-cross frames.
        private bool suppressNextPulse;

        private GameObject haloGo;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            baseScale = transform.localScale;
        }

        private void Start()
        {
            var dev = GetComponent<DevelopableEntity>();
            if (dev != null)
            {
                dev.OnProgressMade += HandleProgressMade;
                dev.OnDeveloped += HandleStageCrossed;
            }

            var spot = GetComponent<WildernessYieldAttendable>();
            if (spot != null)
                spot.OnProgressMade += HandleProgressMade;

            var tended = GetComponent<TendedSpotAttendable>();
            if (tended != null)
                tended.OnProgressMade += HandleProgressMade;

            var npc = GetComponent<NpcAttendable>();
            if (npc != null)
                npc.OnPassiveDriftAccrued += ShowHalo;
        }

        private void OnDestroy()
        {
            var dev = GetComponent<DevelopableEntity>();
            if (dev != null)
            {
                dev.OnProgressMade -= HandleProgressMade;
                dev.OnDeveloped -= HandleStageCrossed;
            }

            var spot = GetComponent<WildernessYieldAttendable>();
            if (spot != null)
                spot.OnProgressMade -= HandleProgressMade;

            var tended = GetComponent<TendedSpotAttendable>();
            if (tended != null)
                tended.OnProgressMade -= HandleProgressMade;

            var npc = GetComponent<NpcAttendable>();
            if (npc != null)
                npc.OnPassiveDriftAccrued -= ShowHalo;
        }

        private void HandleProgressMade()
        {
            HideHalo();

            if (suppressNextPulse)
            {
                suppressNextPulse = false;
                return;
            }

            if (activePulse != null) StopCoroutine(activePulse);
            activePulse = StartCoroutine(ScalePulse(PulseScale, PulseDuration));
        }

        private void HandleStageCrossed(DevelopmentStage _)
        {
            // Shape is one-way: triangle → circle on the first stage cross, stays circle
            // for all subsequent crosses. The pop is the per-cross "moment" signal;
            // the circle shape is the persistent "this entity has developed" indicator.
            if (spriteRenderer != null && !shapeIsCircle)
            {
                spriteRenderer.sprite = CircleSpriteGenerator.CreateSprite(
                    spriteRenderer.color, ShapeTextureSize);
                shapeIsCircle = true;
            }

            // Pop replaces the progress pulse for this tick.
            suppressNextPulse = true;
            if (activePulse != null) StopCoroutine(activePulse);
            if (activePop != null) StopCoroutine(activePop);
            activePop = StartCoroutine(ScalePulse(StagePopScale, StagePopDuration));
        }

        private void ShowHalo()
        {
            if (haloGo == null)
            {
                haloGo = new GameObject("PassiveDriftHalo");
                haloGo.transform.SetParent(transform, false);
                haloGo.transform.localScale = Vector3.one * 1.9f;

                var sr = haloGo.AddComponent<SpriteRenderer>();
                sr.sprite = CircleSpriteGenerator.CreateSprite(new Color(1f, 1f, 1f, 0.35f), ShapeTextureSize);
                sr.sortingOrder = spriteRenderer != null ? spriteRenderer.sortingOrder - 1 : -1;
            }

            haloGo.SetActive(true);
        }

        private void HideHalo()
        {
            if (haloGo != null) haloGo.SetActive(false);
        }

        private IEnumerator ScalePulse(float peak, float duration)
        {
            float half = duration * 0.5f;
            float t = 0f;

            while (t < half)
            {
                t += Time.deltaTime;
                transform.localScale = baseScale * Mathf.Lerp(1f, peak, Mathf.Clamp01(t / half));
                yield return null;
            }

            t = 0f;
            while (t < half)
            {
                t += Time.deltaTime;
                transform.localScale = baseScale * Mathf.Lerp(peak, 1f, Mathf.Clamp01(t / half));
                yield return null;
            }

            transform.localScale = baseScale;
        }
    }
}
