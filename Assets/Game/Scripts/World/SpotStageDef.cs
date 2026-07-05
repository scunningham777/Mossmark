using Mossmark.Development;
using UnityEngine;

namespace Mossmark.World
{
    // Iteration 43 (Fen Bog Pilot): gives wilderness spots the same standalone,
    // ID-addressable stage-def treatment NPCs (NpcStageDef) and Buildings (BuildingStageDef)
    // already have. A spot's "Standing" track — latched, multi-day changes, distinct from
    // the session-scoped exhaustion mechanic — is authored as a pool of these.
    //
    // Not wired into the CSV pipeline (ExportGameData/ImportGameData) — hand-authored only,
    // same discipline as Iteration 42's PoiDormantByDefault fields. This is a single-spot
    // pilot; building CSV support for a schema with exactly one authored row isn't warranted
    // yet (see NpcStageDef/BuildingStageDef, which earned CSV support as part of the much
    // larger Iteration 38 relational migration, not as a single pilot).
    [CreateAssetMenu(menuName = "Mossmark/World/Spot Stage", fileName = "NewSpotStage")]
    public class SpotStageDef : ScriptableObject
    {
        [SerializeField] private string stageId;
        [SerializeField] private string displayName;
        [SerializeField, Min(1)] private int progressCost = 1;
        [SerializeReference] private IDependencyCondition[] conditions = System.Array.Empty<IDependencyCondition>();
        // Logged/posted as "{spotDisplayName}: {flavorText}" on apply.
        [SerializeField] private string flavorText;
        // Multiplies the spot's rare-drop chance once this stage has applied. 1 = no change.
        // Deliberately its own field rather than reusing the tendedness qty/chance-band
        // trick GenericWildernessSpotAttendable uses — Standing is meant to read as a
        // bigger, permanent step change, not the same band ordinary attention could already
        // reach under the old tendedness model.
        [SerializeField, Min(0f)] private float rareChanceMultiplier = 1f;
        // Sprite color applied when this stage applies — mirrors BuildingStageDef.tint.
        // Needed because TriangleSpriteGenerator/CircleSpriteGenerator bake color into the
        // sprite texture itself rather than reading SpriteRenderer.color, so without this,
        // EntityFeedback's stage-cross shape swap (which bakes its circle sprite from
        // SpriteRenderer.color) would read Unity's untouched default white.
        [SerializeField] private Color tint = new(0.5f, 0.5f, 0.45f, 1f);

        public string StageId => stageId;
        public string DisplayName => displayName;
        public int ProgressCost => progressCost;
        public IDependencyCondition[] Conditions => conditions;
        public string FlavorText => flavorText;
        public float RareChanceMultiplier => rareChanceMultiplier;
        public Color Tint => tint;
    }
}
