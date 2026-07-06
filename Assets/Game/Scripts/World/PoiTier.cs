namespace Mossmark.World
{
    // Iteration 45: three-tier POI reveal/interactability model, replacing Iteration 42's
    // plain dormant/spawned bool. Hidden is a pre-spawn state that only WorldGenerator's
    // dormantPois list tracks — no GameObject exists yet, so PoiAttendable itself never
    // holds this value. Once spawned (VisibleInert or promoted from Hidden), a POI's own
    // runtime state only ever spans VisibleInert -> Interactable, and never reverts.
    public enum PoiTier
    {
        Hidden,
        VisibleInert,
        Interactable
    }
}
