using System.Collections;
using DG.Tweening;
using Scripts.Health;
using UnityEngine;

namespace Scripts.VFX
{
    public class HitFeedback : MonoBehaviour
    {
        [SerializeField] private HealthController _healthController;

        [Tooltip("Visual child that wobbles. Must not hold colliders, so they never scale.")]
        [SerializeField] private Transform _visualTarget;

        [Header("Flash")]
        [Tooltip("Leave empty to flash every sprite under the visual target.")]
        [SerializeField] private SpriteRenderer[] _flashRenderers;
        [SerializeField] private Material _flashMaterial;
        [SerializeField] private float _flashDuration = 0.08f;

        [Header("Squash Wobble")]
        [Tooltip("Scale offset at the first wobble peak: +x wider, -y flatter.")]
        [SerializeField] private Vector3 _punch = new Vector3(0.35f, -0.3f, 0f);
        [SerializeField] private float _punchDuration = 0.3f;
        [SerializeField] private int _punchVibrato = 6;
        [SerializeField, Range(0f, 1f)] private float _punchElasticity = 0.8f;

        private Material[] _originalMaterials;
        private bool[] _flashedRenderers;
        private Coroutine _flashRoutine;

        private Tweener _punchTween;
        private Vector3 _punchOffset;
        private Vector3 _lastWrittenScale;
        private Vector3 _lastMultiplier = Vector3.one;
        private bool _hasWrittenScale;

        private void Awake()
        {
            if (!_healthController)
                _healthController = GetComponent<HealthController>();
            if (!_healthController)
                _healthController = GetComponentInParent<HealthController>();

            if (_flashRenderers == null || _flashRenderers.Length == 0)
            {
                var root = _visualTarget ? _visualTarget : transform;
                _flashRenderers = root.GetComponentsInChildren<SpriteRenderer>(true);
            }

            _originalMaterials = new Material[_flashRenderers.Length];
            _flashedRenderers = new bool[_flashRenderers.Length];
        }

        private void Start()
        {
            if (_healthController)
                _healthController.OnDamageReceived += HealthController_DamageReceived;
        }

        private void OnDestroy()
        {
            if (_healthController)
                _healthController.OnDamageReceived -= HealthController_DamageReceived;

            KillPunch();
        }

        private void OnDisable()
        {
            StopFlash();
            KillPunch();
            RestoreBaseScale();
        }

        private void HealthController_DamageReceived(float damage)
        {
            Play();
        }

        public void Play()
        {
            if (!isActiveAndEnabled) return;
            PlayFlash();
            PlayPunch();
        }

        private void PlayFlash()
        {
            if (!_flashMaterial) return;

            if (_flashRoutine != null)
                StopCoroutine(_flashRoutine);
            else
                ApplyFlashMaterial();

            _flashRoutine = StartCoroutine(FlashRoutine());
        }

        private void ApplyFlashMaterial()
        {
            for (int i = 0; i < _flashRenderers.Length; i++)
            {
                var spriteRenderer = _flashRenderers[i];
                _flashedRenderers[i] = false;
                if (!spriteRenderer || !spriteRenderer.enabled || !spriteRenderer.gameObject.activeInHierarchy) continue;
                if (spriteRenderer.color.a <= 0f) continue;

                _originalMaterials[i] = spriteRenderer.sharedMaterial;
                spriteRenderer.sharedMaterial = _flashMaterial;
                _flashedRenderers[i] = true;
            }
        }

        private IEnumerator FlashRoutine()
        {
            yield return new WaitForSeconds(_flashDuration);
            RestoreMaterials();
            _flashRoutine = null;
        }

        private void StopFlash()
        {
            if (_flashRoutine == null) return;
            StopCoroutine(_flashRoutine);
            _flashRoutine = null;
            RestoreMaterials();
        }

        private void RestoreMaterials()
        {
            for (int i = 0; i < _flashRenderers.Length; i++)
            {
                if (!_flashedRenderers[i]) continue;
                _flashedRenderers[i] = false;
                if (_flashRenderers[i])
                    _flashRenderers[i].sharedMaterial = _originalMaterials[i];
            }
        }

        private void PlayPunch()
        {
            if (!_visualTarget) return;

            KillPunch();
            _punchOffset = Vector3.zero;
            _punchTween = DOTween.Punch(() => _punchOffset, value => _punchOffset = value, _punch, _punchDuration,
                    _punchVibrato, _punchElasticity)
                .SetLink(gameObject)
                .OnComplete(() =>
                {
                    _punchTween = null;
                    _punchOffset = Vector3.zero;
                });
        }

        private void KillPunch()
        {
            if (_punchTween != null && _punchTween.IsActive())
                _punchTween.Kill();
            _punchTween = null;
            _punchOffset = Vector3.zero;
        }

        private void LateUpdate()
        {
            if (!_visualTarget) return;

            if (_punchTween == null)
            {
                RestoreBaseScale();
                return;
            }

            var baseScale = GetBaseScale();
            var multiplier = Vector3.one + _punchOffset;
            _lastWrittenScale = Vector3.Scale(baseScale, multiplier);
            _visualTarget.localScale = _lastWrittenScale;
            _lastMultiplier = multiplier;
            _hasWrittenScale = true;
        }

        private Vector3 GetBaseScale()
        {
            var current = _visualTarget.localScale;
            if (!_hasWrittenScale || current != _lastWrittenScale)
                return current;

            return new Vector3(
                SafeDivide(current.x, _lastMultiplier.x),
                SafeDivide(current.y, _lastMultiplier.y),
                SafeDivide(current.z, _lastMultiplier.z));
        }

        private void RestoreBaseScale()
        {
            if (!_hasWrittenScale || !_visualTarget) return;
            _visualTarget.localScale = GetBaseScale();
            _hasWrittenScale = false;
            _lastMultiplier = Vector3.one;
        }

        private static float SafeDivide(float value, float divisor)
        {
            return Mathf.Approximately(divisor, 0f) ? value : value / divisor;
        }
    }
}
