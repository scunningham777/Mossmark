using UnityEngine;

namespace Mossmark.Visuals
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class TriangleSpriteGenerator : MonoBehaviour
    {
        [SerializeField] private Color _color = new Color(0.55f, 0.55f, 0.55f);
        [SerializeField] private int _textureSize = 32;

        public void Initialize(Color color) => _color = color;

        private void Awake()
        {
            var renderer = GetComponent<SpriteRenderer>();
            renderer.sprite = CreateTriangleSprite();
        }

        private Sprite CreateTriangleSprite()
        {
            var tex = new Texture2D(_textureSize, _textureSize, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point
            };

            var pixels = new Color[_textureSize * _textureSize];
            int half = _textureSize / 2;

            for (int y = 0; y < _textureSize; y++)
            {
                float t = (float)y / (_textureSize - 1);
                float leftEdge = t * half;
                float rightEdge = (_textureSize - 1) - t * half;

                for (int x = 0; x < _textureSize; x++)
                    pixels[y * _textureSize + x] = (x >= leftEdge && x <= rightEdge) ? _color : Color.clear;
            }

            tex.SetPixels(pixels);
            tex.Apply();

            return Sprite.Create(tex, new Rect(0, 0, _textureSize, _textureSize), new Vector2(0.5f, 0.5f), _textureSize);
        }
    }
}
