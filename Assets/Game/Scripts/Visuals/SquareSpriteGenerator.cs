using UnityEngine;

namespace Mossmark.Visuals
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class SquareSpriteGenerator : MonoBehaviour
    {
        [SerializeField] private Color _color = Color.white;

        public void Initialize(Color color) => _color = color;

        private void Awake()
        {
            var renderer = GetComponent<SpriteRenderer>();
            renderer.sprite = CreateSquareSprite();
        }

        private Sprite CreateSquareSprite()
        {
            var tex = new Texture2D(1, 1, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point
            };

            tex.SetPixel(0, 0, _color);
            tex.Apply();

            return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        }
    }
}
