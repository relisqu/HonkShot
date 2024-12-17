using UnityEngine;

namespace Scripts.Player
{
    public abstract class BallLayer : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _renderer;
        [SerializeField] private float _spriteSizeCoeff = 1f;
        private Vector2 _offset;

        public SpriteRenderer Renderer => _renderer;

        public Vector2 Offset
        {
            get => _offset;
            set => _offset = value;
        }

        public void UpdateOffset(Vector2 movementDelta, Transform objectTransform)
        {
            if (_renderer == null) return;

            Offset += movementDelta;
            Offset = WrapOffset(Offset, GetSpriteSize(_renderer.sprite));
            ApplyOffset();
        }

        protected virtual void ApplyOffset()
        {
        }

        protected static Vector2 WrapOffset(Vector2 offset, Vector2 spriteSize)
        {
            Vector2 halfSize = spriteSize / 2;
            return Vector2Mod(offset + halfSize, spriteSize) - halfSize;
        }

        protected Vector2 GetSpriteSize(Sprite sprite)
        {
            if (sprite == null) return Vector2.zero;
            Rect rect = sprite.rect;
            return new Vector2(rect.width, rect.height) / 32f * _spriteSizeCoeff;
        }

        protected static float FloatMod(float value, float mod)
        {
            return (value % mod + mod) % mod;
        }

        protected static Vector2 Vector2Mod(Vector2 vector, Vector2 mod)
        {
            return new Vector2(FloatMod(vector.x, mod.x), FloatMod(vector.y, mod.y));
        }
    }
}