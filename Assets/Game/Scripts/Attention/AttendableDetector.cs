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

        public bool IsInRange(IAttendable attendable)
        {
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
