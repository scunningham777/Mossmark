using Mossmark.Day;
using Mossmark.Development;
using Mossmark.Inventory;
using UnityEngine;

namespace Mossmark.Attention
{
    public enum AttentionState
    {
        Idle,
        InRange,
        Attending
    }

    public class AttentionManager : MonoBehaviour
    {
        public static AttentionManager Instance { get; private set; }

        private AttendableDetector detector;
        private AttentionInput input;
        private IAttendable attendingTarget;
        private float holdElapsed;
        private bool isHeld;

        public AttentionState State { get; private set; } = AttentionState.Idle;
        public IAttendable CurrentTarget => detector != null ? detector.CurrentTarget : null;
        public IAttendable AttendingTarget => attendingTarget;
        public float HoldProgress01 { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            detector = FindAnyObjectByType<AttendableDetector>();
            input = FindAnyObjectByType<AttentionInput>();

            if (input != null)
            {
                input.HoldStarted += HandleHoldStarted;
                input.HoldReleased += HandleHoldReleased;
            }
        }

        private void OnDestroy()
        {
            if (input == null) return;

            input.HoldStarted -= HandleHoldStarted;
            input.HoldReleased -= HandleHoldReleased;
        }

        private void Update()
        {
            if (State == AttentionState.Attending)
            {
                UpdateAttending();
            }
            else
            {
                State = CurrentTarget != null ? AttentionState.InRange : AttentionState.Idle;
            }
        }

        private void HandleHoldStarted()
        {
            isHeld = true;

            if (State != AttentionState.InRange) return;

            // Bedroll's Rest() locks attention (and movement, via PlayerController) for
            // the duration of the day-transition fade.
            if (DayCycleManager.Instance != null && DayCycleManager.Instance.IsTransitioning) return;

            // The chest menu (ChestUI) owns input while open; don't start a new hold
            // underneath it.
            if (ChestUI.Instance != null && ChestUI.Instance.IsOpen) return;

            // The workshop menu (WorkshopUI) owns input while open.
            if (WorkshopUI.Instance != null && WorkshopUI.Instance.IsOpen) return;

            // The Horizon panel (HorizonUI) covers the screen while open; same reasoning.
            if (HorizonUI.Instance != null && HorizonUI.Instance.IsOpen) return;

            var target = CurrentTarget;
            if (target == null || !target.CanAttend()) return;

            // At zero daylight, a daylight-costing attention can't even start - the overlay
            // already shows the "too late to start that now" line for this case.
            if (target.RequiresDaylight && DayCycleManager.Instance != null && !DayCycleManager.Instance.HasDaylight)
            {
                return;
            }

            attendingTarget = target;
            holdElapsed = 0f;
            HoldProgress01 = 0f;
            State = AttentionState.Attending;

            if (target.AttentionDuration <= 0f)
            {
                CompleteAttention();
            }
        }

        private void UpdateAttending()
        {
            if (detector == null || !detector.IsInRange(attendingTarget))
            {
                CancelAttention();
                return;
            }

            holdElapsed += Time.deltaTime;
            HoldProgress01 = attendingTarget.AttentionDuration > 0f
                ? Mathf.Clamp01(holdElapsed / attendingTarget.AttentionDuration)
                : 1f;

            if (holdElapsed >= attendingTarget.AttentionDuration)
            {
                CompleteAttention();
            }
        }

        private void HandleHoldReleased()
        {
            isHeld = false;

            if (State != AttentionState.Attending) return;

            CancelAttention();
        }

        // Each tick runs the hold-timer/progress-bar over AttentionDuration (above),
        // then resolves the attendable's response here. If the attend key is still
        // held, the target is still in range, ContinueAttending is true, and (when
        // this tick spent daylight) daylight remains, the loop resets HoldProgress01
        // and starts another tick; otherwise the hold ends as before.
        private void CompleteAttention()
        {
            var target = attendingTarget;
            target.OnAttentionComplete();

            bool spentDaylight = target.RequiresDaylight;
            if (spentDaylight)
            {
                DayCycleManager.Instance?.SpendDaylight();
            }

            if (ShouldContinue(target, spentDaylight))
            {
                holdElapsed = 0f;
                HoldProgress01 = 0f;
                return;
            }

            FinishAttending();
        }

        private bool ShouldContinue(IAttendable target, bool spentDaylightThisTick)
        {
            if (!isHeld) return false;
            if (!target.ContinueAttending) return false;
            if (detector == null || !detector.IsInRange(target)) return false;

            if (spentDaylightThisTick && DayCycleManager.Instance != null && !DayCycleManager.Instance.HasDaylight)
            {
                return false;
            }

            return true;
        }

        private void CancelAttention()
        {
            attendingTarget.OnAttentionCancelled();
            FinishAttending();
        }

        private void FinishAttending()
        {
            attendingTarget = null;
            HoldProgress01 = 0f;
            State = CurrentTarget != null ? AttentionState.InRange : AttentionState.Idle;
        }
    }
}
