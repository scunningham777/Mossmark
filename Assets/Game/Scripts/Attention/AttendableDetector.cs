using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mossmark.Attention
{
    [RequireComponent(typeof(Collider2D))]
    public class AttendableDetector : MonoBehaviour
    {
        public event Action<IAttendable> TargetChanged;

        private readonly List<AttendableZone> zonesInRange = new();

        public IAttendable CurrentTarget { get; private set; }

        // P5's ambient attend: the thing E resolves to when there is no zone in range at
        // all — attending the place you are standing in rather than a thing in it.
        // Deliberately kept separate from CurrentTarget rather than folded into it, so
        // CurrentTarget still means exactly what it always meant (nearest zone in range,
        // null when there is none). Anything reading CurrentTarget to decide whether to
        // show something — AttendableOverlayUI, most of all — therefore stays quiet while
        // only a fallback is available. Null in every scene that doesn't register one, in
        // which case this class behaves exactly as it did before.
        public IAttendable FallbackTarget { get; private set; }

        public void SetFallbackTarget(IAttendable attendable) => FallbackTarget = attendable;

        public bool IsInRange(IAttendable attendable)
        {
            // The fallback has no zone and no position, so it is never out of range. This
            // is also what makes P5's no-hand-off rule fall out for free: an ambient hold
            // can't be cancelled by the world changing around it, only by release.
            if (attendable != null && ReferenceEquals(attendable, FallbackTarget)) return true;

            foreach (var zone in zonesInRange)
            {
                if (ReferenceEquals(zone.Attendable, attendable)) return true;
            }
            return false;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var zone = other.GetComponent<AttendableZone>();
            if (zone == null || zone.Attendable == null) return;

            zonesInRange.Add(zone);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            var zone = other.GetComponent<AttendableZone>();
            if (zone == null) return;

            zonesInRange.Remove(zone);
        }

        private void Update()
        {
            UpdateCurrentTarget();
        }

        private void UpdateCurrentTarget()
        {
            AttendableZone nearest = null;
            float nearestSqrDist = float.MaxValue;

            foreach (var zone in zonesInRange)
            {
                float sqrDist = ((Vector2)zone.transform.position - (Vector2)transform.position).sqrMagnitude;
                if (sqrDist < nearestSqrDist)
                {
                    nearestSqrDist = sqrDist;
                    nearest = zone;
                }
            }

            var newTarget = nearest != null ? nearest.Attendable : null;
            if (!ReferenceEquals(newTarget, CurrentTarget))
            {
                CurrentTarget = newTarget;
                TargetChanged?.Invoke(CurrentTarget);
            }
        }
    }
}
