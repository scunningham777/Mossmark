using System.Collections.Generic;
using Mossmark.Day;
using Mossmark.Inventory;
using UnityEngine;

namespace Mossmark.Development
{
    // Processes settlement maintenance drift on each day advance.
    // Runs with a high execution order so Start() fires late, ensuring all
    // DevelopableEntity Start() subscriptions (WildernessYieldAttendable, etc.) complete
    // before this subscribes to DayAdvanced.
    //
    // Order of processing per rest:
    //   1. Increment driftProgress for every developed entity (CurrentStageIndex >= 0).
    //   2. For each consumer with driftProgress > 0 and a valid material, check the
    //      settlement chest; if it holds enough, consume silently and reset drift.
    //
    // This guarantees all entities increment before any chest consumption resolves,
    // so a single chest stock can cover multiple entities in one rest if sufficient.
    [DefaultExecutionOrder(500)]
    public class MaintenanceManager : MonoBehaviour
    {
        private void Start()
        {
            if (DayCycleManager.Instance != null)
                DayCycleManager.Instance.DayAdvanced += OnDayAdvanced;
        }

        private void OnDestroy()
        {
            if (DayCycleManager.Instance != null)
                DayCycleManager.Instance.DayAdvanced -= OnDayAdvanced;
        }

        private void OnDayAdvanced()
        {
            var entities = FindObjectsByType<DevelopableEntity>(FindObjectsInactive.Exclude);
            var consumers = new List<(DevelopableEntity entity, IMaintenanceConsumer consumer)>();

            // Step 1: increment drift for all developed entities.
            foreach (var entity in entities)
            {
                if (entity.CurrentStageIndex < 0) continue;
                entity.IncrementDrift();
                if (entity is IMaintenanceConsumer mc) consumers.Add((entity, mc));
            }

            // Step 2: silent chest consumption for any consumer with drift > 0.
            if (ChestAttendable.Instance == null) return;

            foreach (var (entity, consumer) in consumers)
            {
                if (entity.DriftProgress == 0) continue;
                if (consumer.MaintenanceMaterial == null) continue;

                int available = ChestAttendable.Instance.GetQuantity(consumer.MaintenanceMaterial);
                if (available < consumer.MaintenanceCostPerReset) continue;

                ChestAttendable.Instance.Withdraw(consumer.MaintenanceMaterial, consumer.MaintenanceCostPerReset);
                entity.ResetDrift();
                // Silent — no notification. The player notices the warning doesn't appear.
                Debug.Log($"{entity.DisplayName}: maintained from chest.", entity);
            }
        }
    }
}
