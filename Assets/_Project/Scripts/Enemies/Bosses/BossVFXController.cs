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
        [SerializeField] private Material _hitFlashMaterial;
        [SerializeField] private float _flashDuration = 0.1f;

        private Coroutine _flashCoroutine;
        private Vector3 _originalScale;
        private Material _originalMaterial;

        private void Start()
        {
            if (_visualTransform)
                _originalScale = _visualTransform.localScale;

            if (_spriteRenderer)
                _originalMaterial = _spriteRenderer.sharedMaterial;

            if (_trailRenderer)
                _trailRenderer.emitting = false;

            if (_healthController)
                _healthController.OnTakeDamageTriggered += HealthController_OnTakeDamageTriggered;

            if (_shootingModule)
                _shootingModule.OnShot += ShootingModule_OnShot;
        }

        private void OnDestroy()
        {
            if (_healthController)
                _healthController.OnTakeDamageTriggered -= HealthController_OnTakeDamageTriggered;

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

        private void HealthController_OnTakeDamageTriggered()
        {
            PlayHitFlash();
        }

        public void PlayHitFlash()
        {
            if (_flashCoroutine != null)
                StopCoroutine(_flashCoroutine);
            _flashCoroutine = StartCoroutine(FlashHit());
        }

        private IEnumerator FlashHit()
        {
            if (!_spriteRenderer || !_hitFlashMaterial) yield break;

            _spriteRenderer.sharedMaterial = _hitFlashMaterial;
            yield return new WaitForSeconds(_flashDuration);
            _spriteRenderer.sharedMaterial = _originalMaterial;
            _flashCoroutine = null;
        }
    }
}
