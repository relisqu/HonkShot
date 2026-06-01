using DG.Tweening;
using UnityEngine;

namespace Scripts.LevelSystem.LevelObjects
{
    public class PunchObjectAnimation : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private float _punchScale = 1.2f;
        [SerializeField] private int _punchVibrato = 4;
        [SerializeField] private Material _blinkColorMaterial;

        private Tweener _animationTween;

        public void ShowPunchAnimation()
        {
            if (_animationTween != null && _animationTween.IsPlaying()) return;

            Material previousMaterial = null;
            if (_spriteRenderer)
            {
                previousMaterial = _spriteRenderer.material;
                _spriteRenderer.material = _blinkColorMaterial;
            }

            var jumpTransform  = _spriteRenderer ? _spriteRenderer.transform : transform.GetChild(0);
            _animationTween = jumpTransform.DOPunchScale(_punchScale * Vector3.one, 0.2f, _punchVibrato, 0f)
                .OnComplete(() =>
                {
                    if (_spriteRenderer)
                        _spriteRenderer.material = previousMaterial;
                });
        }
    }
}