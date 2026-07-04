namespace Mossmark.Development
{
    // What a completed attend would actually do right now, for entities whose
    // attend outcome branches by circumstance (Iteration 41). Shared as a type so
    // future entities beyond BuildingAttendable can adopt the same resolution
    // pipeline without a new enum; the resolution logic itself stays per-entity.
    public enum AttentionOutcomeKind
    {
        Develop,
        Open,
        Maintain,
        Visit
    }
}
