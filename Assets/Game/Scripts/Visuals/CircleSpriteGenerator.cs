using UnityEngine;

namespace Mossmark.Visuals
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class CircleSpriteGenerator : MonoBehaviour
    {
        [SerializeField] private Color _color = new Color(0.55f, 0.55f, 0.55f);
        [SerializeField] private int _textureSize = 32;

        public void Initialize(Color color) => _color = color;

        private void Awake()
        {
            GetComponent<SpriteRenderer>().sprite = CreateSprite(_color, _textureSize);
        }

        public static Sprite CreateSprite(Color color, int textureSize = 32)
        {
            var tex = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point
            };

            var pixels = new Color[textureSize * textureSize];
            float center = (textureSize - 1) * 0.5f;
            float radiusSq = center * center;

            for (int y = 0; y < textureSize; y++)
            {
                for (int x = 0; x < textureSize; x++)
                {
                    float dx = x - center;
                    float dy = y - center;
                    pixels[y * textureSize + x] = dx * dx + dy * dy <= radiusSq ? color : Color.clear;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();

            return Sprite.Create(tex, new Rect(0, 0, textureSize, textureSize),
                new Vector2(0.5f, 0.5f), textureSize);
        }
    }
}
