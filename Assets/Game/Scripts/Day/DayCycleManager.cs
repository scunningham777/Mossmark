using System;
using System.Collections;
using UnityEngine;

namespace Mossmark.Day
{
    public enum DayPhase
    {
        Dawn,
        Morning,
        Midday,
        Afternoon,
        Evening,
        Dusk
    }

    public class DayCycleManager : MonoBehaviour
    {
        public static DayCycleManager Instance { get; private set; }

        [SerializeField] private int maxDaylight = 24;
        [SerializeField] private DayCycleAmbientTextData ambientTextData;
        [SerializeField] private float transitionFadeDuration = 0.6f;

        public int MaxDaylight => maxDaylight;
        public int DaylightRemaining { get; private set; }
        // Increments once per Rest(), before DayAdvanced fires. Iteration 47: gives
        // WorldSite.RegisterGoodDay() a stable value all member spots agree on that
        // day, regardless of which spot's DayAdvanced subscriber happens to run first.
        public int DayIndex { get; private set; }
        public DayPhase CurrentPhase { get; private set; }
        public string CurrentAmbientText { get; private set; }
        public bool HasDaylight => DaylightRemaining > 0;

        // True for the full fade-out/reset/fade-in span of Rest(). PlayerController and
        // AttentionManager both check this to lock movement/attention for its duration.
        public bool IsTransitioning { get; private set; }

        // 0 = scene fully visible, 1 = fully black. DayTransitionFadeUI reads this each
        // frame, same "manager owns progress, UI just displays it" pattern as HoldProgress01.
        public float FadeAmount01 { get; private set; }

        public event Action<int, int> DaylightChanged;
        public event Action<DayPhase> PhaseChanged;
        public event Action<string> AmbientTextChanged;

        // Fired once per Rest(), after daylight/phase reset but before the fade back in -
        // the "reseed wilderness spots" hook deferred from Iteration 6. Tended-style spots
        // subscribe to advance their mark -> wait -> harvest countdown.
        public event Action DayAdvanced;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DaylightRemaining = maxDaylight;
            CurrentPhase = GetPhase(DaylightRemaining);
            CurrentAmbientText = ambientTextData != null ? ambientTextData.GetTextForPhase(CurrentPhase) : null;
        }

        public void SpendDaylight(int amount = 1)
        {
            if (DaylightRemaining <= 0) return;

            DaylightRemaining = Mathf.Max(0, DaylightRemaining - amount);
            DaylightChanged?.Invoke(DaylightRemaining, maxDaylight);

            var newPhase = GetPhase(DaylightRemaining);
            if (newPhase != CurrentPhase)
            {
                SetPhase(newPhase);
            }
        }

        // Bedroll's OnAttentionComplete entry point. Direct successor to P1's
        // DayCycleManager.Rest(): fade to black, restore daylight and reset to Dawn,
        // fade back in. IsTransitioning/FadeAmount01 drive the lock and the fade visual.
        public void Rest()
        {
            if (IsTransitioning) return;

            StartCoroutine(RestRoutine());
        }

        private IEnumerator RestRoutine()
        {
            IsTransitioning = true;

            yield return Fade(0f, 1f);

            DaylightRemaining = maxDaylight;
            DaylightChanged?.Invoke(DaylightRemaining, maxDaylight);
            SetPhase(DayPhase.Dawn);
            DayIndex++;
            DayAdvanced?.Invoke();

            yield return Fade(1f, 0f);

            IsTransitioning = false;
        }

        private IEnumerator Fade(float from, float to)
        {
            if (transitionFadeDuration <= 0f)
            {
                FadeAmount01 = to;
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < transitionFadeDuration)
            {
                elapsed += Time.deltaTime;
                FadeAmount01 = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / transitionFadeDuration));
                yield return null;
            }

            FadeAmount01 = to;
        }

        private void SetPhase(DayPhase newPhase)
        {
            CurrentPhase = newPhase;
            PhaseChanged?.Invoke(CurrentPhase);

            CurrentAmbientText = ambientTextData != null ? ambientTextData.GetTextForPhase(CurrentPhase) : null;
            if (!string.IsNullOrEmpty(CurrentAmbientText))
            {
                Debug.Log($"[{CurrentPhase}] {CurrentAmbientText}");
                AmbientTextChanged?.Invoke(CurrentAmbientText);
            }
        }

        // Six even phases across the daylight pool (Dawn = full, Dusk = empty),
        // mirroring P1's DayPhase progression but driven by the daylight value directly.
        private DayPhase GetPhase(int daylight)
        {
            float ratio = (float)daylight / maxDaylight;

            if (ratio > 5f / 6f) return DayPhase.Dawn;
            if (ratio > 4f / 6f) return DayPhase.Morning;
            if (ratio > 3f / 6f) return DayPhase.Midday;
            if (ratio > 2f / 6f) return DayPhase.Afternoon;
            if (ratio > 1f / 6f) return DayPhase.Evening;
            return DayPhase.Dusk;
        }
    }
}
