namespace Mossmark.Development
{
    // Implemented by entities that can satisfy a PropertyAvailableCondition through a
    // passively-filled reserve, in addition to carried inventory (Iteration 53 pilot,
    // Flow-Filled Reserve). Its own interface rather than a DevelopableEntity member,
    // since only NpcAttendable tracks a reserve today and the condition already takes
    // any DevelopableEntity.
    public interface IPassiveReserveTracker
    {
        float GetPassiveReserve(string propertyId);
    }
}
