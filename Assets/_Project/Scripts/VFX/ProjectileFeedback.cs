using DG.Tweening;
using Scripts.Enemies.Bullets;
using UnityEngine;

namespace Scripts.VFX
{
    // Bullet juice: pops in on spawn, leaves a puff when it hits something, shrinks away when it times out.
    // Animates only the visual child, so the bullet's collider keeps its size.
    public class ProjectileFeedback : MonoBehaviour
    {
        [SerializeField] private BaseBullet _bullet;
        [Tooltip("Visual child (sprite) that pops and shrinks. Must not hold the collider.")]
        [SerializeField] private Transform _visual;

        [Header("Spawn Pop")]
        [SerializeField] private float _popDuration = 0.15f;
        [Tooltip("How far the pop overshoots before settling (DOTween OutBack overshoot).")]
        [SerializeField] private float _popOvershoot = 3f;

        [Header("Impact Puff")]
        [SerializeField] private GameObject _impactPrefab;
        [SerializeField] private float _impactScale = 0.6f;

        [Header("Timeout")]
        [Tooltip("Shrink duration at the end of the bullet's lifetime.")]
        [SerializeField] private float _timeoutShrinkDuration = 0.12f;

        private Vector3 _visualScale;

        private void Awake()
        {
            if (!_bullet)
                _bullet = GetComponent<BaseBullet>();
            if (_visual)
                _visualScale = _visual.localScale;
        }

        private void Start()
        {
            if (_bullet)
                _bullet.OnImpact += Bullet_Impact;

            if (!_visual) return;

            _visual.localScale = Vector3.zero;
            _visual.DOScale(_visualScale, _popDuration).SetEase(Ease.OutBack, _popOvershoot).SetLink(gameObject);

            if (_bullet && _bullet.MaxLifetime > _timeoutShrinkDuration)
            {
                DOVirtual.DelayedCall(_bullet.MaxLifetime - _timeoutShrinkDuration,
                        () => _visual.DOScale(Vector3.zero, _timeoutShrinkDuration).SetEase(Ease.InBack).SetLink(gameObject), false)
                    .SetLink(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (_bullet)
                _bullet.OnImpact -= Bullet_Impact;
        }

        private void Bullet_Impact(Vector2 point)
        {
            if (!_impactPrefab) return;

            var puff = Instantiate(_impactPrefab, point, Quaternion.identity);
            puff.transform.localScale *= _impactScale;
        }
    }
}
