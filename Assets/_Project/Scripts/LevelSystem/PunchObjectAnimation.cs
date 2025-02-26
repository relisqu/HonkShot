using DG.Tweening;
using UnityEngine;

namespace Scripts.LevelSystem
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

            var previousMaterial = _spriteRenderer.material;
            _spriteRenderer.material = _blinkColorMaterial;
            _animationTween = _spriteRenderer.transform.DOPunchScale(_punchScale * Vector3.one, 0.2f, _punchVibrato, 0f).OnComplete(() =>
            {
                _spriteRenderer.material = previousMaterial;
            });
        }
    }
}