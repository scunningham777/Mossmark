using Mossmark.Visuals;
using UnityEngine;

namespace Mossmark.World
{
    // Draws the town/wilderness ground backgrounds and exposes their bounds so the
    // world feels spatially organized even as a grey-box - "town" holds the
    // buildings/NPCs/bedroll/chest, "wilderness" holds everything else. Runs before
    // WorldGenerator (-1000) so TownBounds/WildernessBounds are available to it if
    // later procedural placement needs to validate positions against these bounds.
    [DefaultExecutionOrder(-2000)]
    public class WorldLayoutGenerator : MonoBehaviour
    {
        [SerializeField] private Vector2 townSize = new(20, 20);
        [SerializeField] private Vector2 wildernessSize = new(70, 70);
        [SerializeField] private Color townGroundColor = new(0.16f, 0.33f, 0.18f, 1f);
        [SerializeField] private Color wildernessGroundColor = new(0.55f, 0.72f, 0.42f, 1f);

        public static Rect TownBounds { get; private set; }
        public static Rect WildernessBounds { get; private set; }

        // Lets callers validate a position against the current layout without knowing
        // its sizes - e.g. HorizonUI gates panel visibility on the player's position.
        public static bool IsInTown(Vector2 position) => TownBounds.Contains(position);
        public static bool IsInWilderness(Vector2 position) => WildernessBounds.Contains(position) && !IsInTown(position);

        private void Awake()
        {
            TownBounds = new Rect(-townSize / 2f, townSize);
            WildernessBounds = new Rect(-wildernessSize / 2f, wildernessSize);

            SpawnGround("Wilderness Ground", wildernessSize, wildernessGroundColor, -2);
            SpawnGround("Town Ground", townSize, townGroundColor, -1);
        }

        private static void SpawnGround(string name, Vector2 size, Color color, int sortingOrder)
        {
            var go = new GameObject(name);
            go.SetActive(false);
            go.transform.position = Vector3.zero;
            go.transform.localScale = new Vector3(size.x, size.y, 1f);

            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sortingOrder = sortingOrder;
            go.AddComponent<SquareSpriteGenerator>().Initialize(color);

            go.SetActive(true);
        }
    }
}
