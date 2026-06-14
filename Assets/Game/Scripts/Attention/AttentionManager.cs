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
            if (State != AttentionState.InRange) return;

            var target = CurrentTarget;
            if (target == null || !target.CanAttend()) return;

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
            if (State != AttentionState.Attending) return;

            CancelAttention();
        }

        private void CompleteAttention()
        {
            attendingTarget.OnAttentionComplete();
            FinishAttending();
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
