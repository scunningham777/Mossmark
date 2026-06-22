using Mossmark.Inventory;

namespace Mossmark.Development
{
    // Implemented by DevelopableEntity subclasses that participate in the settlement
    // maintenance system (BuildingAttendable and NpcAttendable). Exposes per-entity
    // maintenance parameters so MaintenanceManager can process all consumers generically.
    public interface IMaintenanceConsumer
    {
        // Rests before the entity enters cold state. BuildingAttendable: 5, NpcAttendable: 7.
        int DriftThreshold { get; }

        // Item consumed when tending the entity directly or drawn from the chest at rest.
        // Null for universal-spec NPCs (forager/caretaker/tinkerer) — they maintain via visits.
        ItemDefinition MaintenanceMaterial { get; }

        // Units of MaintenanceMaterial consumed to reset driftProgress to 0.
        int MaintenanceCostPerReset { get; }
    }
}
