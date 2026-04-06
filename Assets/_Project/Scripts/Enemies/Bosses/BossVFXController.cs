using System.Collections;
using DG.Tweening;
using Scripts.Health;
using UnityEngine;

namespace Scripts.Enemies.Bosses
{
    public class BossVFXController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RavageBossEnemy _boss;
        [SerializeField] private HealthController _healthController;
        [SerializeField] private RavageBossShootingModule _shootingModule;
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Transform _visualTransform;

        [Header("Shoot Squash-Stretch")]
        [SerializeField] private float _squashScaleX = 1.3f;
        [SerializeField] private float _squashScaleY = 0.7f;
        [SerializeField] private float _squashDuration = 0.08f;
        [SerializeField] private float _stretchScaleX = 0.85f;
        [SerializeField] private float _stretchScaleY = 1.2f;
        [SerializeField] private float _stretchDuration = 0.1f;
        [SerializeField] private float _recoverDuration = 0.12f;

        [Header("Dash Trail")]
        [SerializeField] private TrailRenderer _trailRenderer;

        [Header("Damage Flash")]
        [SerializeField] private float _flashDuration = 0.1f;

        private Color _originalColor;
        private Coroutine _flashCoroutine;
        private Vector3 _originalScale;

        private void Start()
        {
            if (_spriteRenderer)
                _originalColor = _spriteRenderer.color;

            if (_visualTransform)
                _originalScale = _visualTransform.localScale;

            if (_trailRenderer)
                _trailRenderer.emitting = false;

            if (_healthController)
                _healthController.OnNonLethalDamageReceived += HealthController_OnDamaged;

            if (_shootingModule)
                _shootingModule.OnShot += ShootingModule_OnShot;
        }

        private void OnDestroy()
        {
            if (_healthController)
                _healthController.OnNonLethalDamageReceived -= HealthController_OnDamaged;

            if (_shootingModule)
                _shootingModule.OnShot -= ShootingModule_OnShot;

            if (_visualTransform)
                _visualTransform.DOKill();
        }

        private void ShootingModule_OnShot()
        {
            if (!_visualTransform) return;

            _visualTransform.DOKill();
            _visualTransform.localScale = _originalScale;

            var squash = new Vector3(_originalScale.x * _squashScaleX, _originalScale.y * _squashScaleY, _originalScale.z);
            var stretch = new Vector3(_originalScale.x * _stretchScaleX, _originalScale.y * _stretchScaleY, _originalScale.z);
            var halfSquash = Vector3.Lerp(_originalScale, squash, 0.4f);

            var seq = DOTween.Sequence();
            seq.Append(_visualTransform.DOScale(squash, _squashDuration).SetEase(Ease.OutQuad));
            seq.Append(_visualTransform.DOScale(stretch, _stretchDuration).SetEase(Ease.OutQuad));
            seq.Append(_visualTransform.DOScale(halfSquash, _recoverDuration * 0.5f).SetEase(Ease.InOutQuad));
            seq.Append(_visualTransform.DOScale(_originalScale, _recoverDuration * 0.5f).SetEase(Ease.OutBack));
        }

        public void SetDashTrailActive(bool active)
        {
            if (_trailRenderer)
                _trailRenderer.emitting = active;
        }

        private void HealthController_OnDamaged(float damage)
        {
            if (_flashCoroutine != null)
                StopCoroutine(_flashCoroutine);
            _flashCoroutine = StartCoroutine(FlashBlack());
        }

        private IEnumerator FlashBlack()
        {
            if (!_spriteRenderer) yield break;

            _spriteRenderer.color = Color.black;
            yield return new WaitForSeconds(_flashDuration);
            _spriteRenderer.color = _originalColor;
            _flashCoroutine = null;
        }
    }
}
