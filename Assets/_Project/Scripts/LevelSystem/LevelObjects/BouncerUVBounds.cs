using UnityEngine;

namespace Scripts.LevelSystem.LevelObjects
{
    [RequireComponent(typeof(SpriteRenderer))]
    [ExecuteAlways]
    public class BouncerUVBounds : MonoBehaviour
    {
        private static readonly int SpriteUVBoundsID = Shader.PropertyToID("_SpriteUVBounds");

        private SpriteRenderer _sr;
        private MaterialPropertyBlock _mpb;
        private Sprite _lastSprite;

        private void OnEnable()
        {
            _sr = GetComponent<SpriteRenderer>();
            _mpb = new MaterialPropertyBlock();
            UpdateBounds();
        }

        private void LateUpdate()
        {
            if (_sr.sprite != _lastSprite)
                UpdateBounds();
        }

        private void UpdateBounds()
        {
            var sprite = _sr.sprite;
            _lastSprite = sprite;
            if (!sprite) return;

            var tex = sprite.texture;
            var rect = sprite.textureRect;
            var bounds = new Vector4(
                rect.xMin / tex.width,
                rect.yMin / tex.height,
                rect.xMax / tex.width,
                rect.yMax / tex.height
            );

            _sr.GetPropertyBlock(_mpb);
            _mpb.SetVector(SpriteUVBoundsID, bounds);
            _sr.SetPropertyBlock(_mpb);
        }
    }
}
