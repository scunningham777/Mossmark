using System;
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

        [SerializeField] private int maxStamina = 24;
        [SerializeField] private DayCycleAmbientTextData ambientTextData;

        public int MaxStamina => maxStamina;
        public int StaminaRemaining { get; private set; }
        public DayPhase CurrentPhase { get; private set; }
        public bool HasStamina => StaminaRemaining > 0;

        public event Action<int, int> StaminaChanged;
        public event Action<DayPhase> PhaseChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            StaminaRemaining = maxStamina;
            CurrentPhase = GetPhase(StaminaRemaining);
        }

        public void SpendStamina(int amount = 1)
        {
            if (StaminaRemaining <= 0) return;

            StaminaRemaining = Mathf.Max(0, StaminaRemaining - amount);
            StaminaChanged?.Invoke(StaminaRemaining, maxStamina);

            var newPhase = GetPhase(StaminaRemaining);
            if (newPhase == CurrentPhase) return;

            CurrentPhase = newPhase;
            PhaseChanged?.Invoke(CurrentPhase);

            string ambientText = ambientTextData != null ? ambientTextData.GetTextForPhase(CurrentPhase) : null;
            if (!string.IsNullOrEmpty(ambientText))
            {
                Debug.Log($"[{CurrentPhase}] {ambientText}");
            }
        }

        // Six even phases across the stamina pool (Dawn = full, Dusk = empty),
        // mirroring P1's DayPhase progression but driven by the stamina value directly.
        private DayPhase GetPhase(int stamina)
        {
            float ratio = (float)stamina / maxStamina;

            if (ratio > 5f / 6f) return DayPhase.Dawn;
            if (ratio > 4f / 6f) return DayPhase.Morning;
            if (ratio > 3f / 6f) return DayPhase.Midday;
            if (ratio > 2f / 6f) return DayPhase.Afternoon;
            if (ratio > 1f / 6f) return DayPhase.Evening;
            return DayPhase.Dusk;
        }
    }
}
