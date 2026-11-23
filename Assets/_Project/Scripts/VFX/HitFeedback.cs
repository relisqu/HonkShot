using System.Collections;
using DG.Tweening;
using Scripts.Health;
using Scripts.PointSystem;
using UnityEngine;

namespace Scripts.VFX
{
    public class HitFeedback : MonoBehaviour
    {
        [SerializeField] private HealthController _healthController;

        [Tooltip("Visual child that wobbles and gets knocked back. Must not hold colliders, so they never scale or move.")]
        [SerializeField] private Transform _visualTarget;

        [Header("Hit Strength")]
        [Tooltip("Damage at or below this plays the lightest reaction.")]
        [SerializeField] private float _lightHitDamage = 10f;
        [Tooltip("Damage at or above this plays the strongest reaction.")]
        [SerializeField] private float _heavyHitDamage = 35f;
        [Tooltip("Hits below this damage (e.g. resting contact ticks) only react when the previous reaction has finished, with no kick or spark.")]
        [SerializeField] private float _minFullReactionDamage = 7f;

        [Header("Flash")]
        [Tooltip("Leave empty to flash every sprite under the visual target.")]
        [SerializeField] private SpriteRenderer[] _flashRenderers;
        [SerializeField] private Material _flashMaterial;
        [SerializeField] private float _flashDuration = 0.1f;

        [Header("Squash Wobble")]
        [Tooltip("Scale offset at the first wobble peak for a medium hit: +x wider, -y flatter.")]
        [SerializeField] private Vector3 _punch = new Vector3(0.45f, -0.38f, 0f);
        [Tooltip("Wobble strength multiplier for the lightest (x) and heaviest (y) hit.")]
        [SerializeField] private Vector2 _punchStrengthRange = new Vector2(0.75f, 1.5f);
        [SerializeField] private float _punchDuration = 0.35f;
        [SerializeField] private int _punchVibrato = 7;
        [SerializeField, Range(0f, 1f)] private float _punchElasticity = 0.8f;

        [Header("Knockback Kick")]
        [Tooltip("How far the sprite is kicked away from the player for the lightest (x) and heaviest (y) hit.")]
        [SerializeField] private Vector2 _kickDistanceRange = new Vector2(0.12f, 0.32f);
        [SerializeField] private float _kickDuration = 0.28f;
        [SerializeField] private int _kickVibrato = 3;
        [SerializeField, Range(0f, 1f)] private float _kickElasticity = 0.6f;

        [Header("Hit Spark")]
        [SerializeField] private GameObject _hitParticlePrefab;
        [Tooltip("Spark spawn distance from the visual's center towards the player.")]
        [SerializeField] private float _sparkOffset = 0.3f;
        [Tooltip("Spark size for the lightest (x) and heaviest (y) hit.")]
        [SerializeField] private Vector2 _sparkScaleRange = new Vector2(0.8f, 1.4f);

        [Header("Stun Tremble")]
        [SerializeField] private float _stunShakeStrength = 0.07f;
        [SerializeField] private int _stunShakeVibrato = 35;

        private Material[] _originalMaterials;
        private bool[] _flashedRenderers;
        private Coroutine _flashRoutine;

        private Tweener _punchTween;
        private Vector3 _punchOffset;
        private Vector3 _lastWrittenScale;
        private Vector3 _lastMultiplier = Vector3.one;
        private bool _hasWrittenScale;

        private Tweener _kickTween;
        private Vector3 _kickOffset;
        private Tweener _shakeTween;
        private Vector3 _shakeOffset;
        private Vector3 _lastWrittenPosition;
        private Vector3 _lastLocalOffset;
        private bool _hasWrittenPosition;

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

            KillTween(ref _punchTween);
            KillTween(ref _kickTween);
            KillTween(ref _shakeTween);
        }

        private void OnDisable()
        {
            StopFlash();
            KillTween(ref _punchTween);
            KillTween(ref _kickTween);
            KillTween(ref _shakeTween);
            _punchOffset = Vector3.zero;
            _kickOffset = Vector3.zero;
            _shakeOffset = Vector3.zero;
            RestoreBaseScale();
            RestoreBasePosition();
        }

        private void HealthController_DamageReceived(float damage)
        {
            Play(damage);
        }

        public void Play(float damage)
        {
            if (!isActiveAndEnabled) return;

            if (damage < _minFullReactionDamage)
            {
                if (IsPlaying(_punchTween)) return;
                PlayFlash();
                PlayPunch(0f);
                return;
            }

            float heaviness = _heavyHitDamage > _lightHitDamage
                ? Mathf.InverseLerp(_lightHitDamage, _heavyHitDamage, damage)
                : 0.5f;
            var awayFromPlayer = GetDirectionAwayFromPlayer();

            PlayFlash();
            PlayPunch(heaviness);
            PlayKick(awayFromPlayer, heaviness);
            SpawnSpark(awayFromPlayer, heaviness);
        }

        public void PlayStun(float duration)
        {
            if (!isActiveAndEnabled || !_visualTarget || duration <= 0f) return;

            KillTween(ref _shakeTween);
            _shakeOffset = Vector3.zero;
            _shakeTween = DOTween.Shake(() => _shakeOffset, value => _shakeOffset = value, duration,
                    _stunShakeStrength, _stunShakeVibrato, 90f, true, true)
                .SetLink(gameObject)
                .OnComplete(() =>
                {
                    _shakeTween = null;
                    _shakeOffset = Vector3.zero;
                });
        }

        private Vector2 GetDirectionAwayFromPlayer()
        {
            var origin = _visualTarget ? _visualTarget.position : transform.position;
            var player = PointReceiver.Instance;
            if (!player) return Vector2.up;

            Vector2 direction = origin - player.transform.position;
            return direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector2.up;
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

        private void PlayPunch(float heaviness)
        {
            if (!_visualTarget) return;

            KillTween(ref _punchTween);
            _punchOffset = Vector3.zero;
            var punch = _punch * Mathf.Lerp(_punchStrengthRange.x, _punchStrengthRange.y, heaviness);
            _punchTween = DOTween.Punch(() => _punchOffset, value => _punchOffset = value, punch, _punchDuration,
                    _punchVibrato, _punchElasticity)
                .SetLink(gameObject)
                .OnComplete(() =>
                {
                    _punchTween = null;
                    _punchOffset = Vector3.zero;
                });
        }

        private void PlayKick(Vector2 direction, float heaviness)
        {
            if (!_visualTarget) return;

            KillTween(ref _kickTween);
            _kickOffset = Vector3.zero;
            var kick = (Vector3)(direction * Mathf.Lerp(_kickDistanceRange.x, _kickDistanceRange.y, heaviness));
            _kickTween = DOTween.Punch(() => _kickOffset, value => _kickOffset = value, kick, _kickDuration,
                    _kickVibrato, _kickElasticity)
                .SetLink(gameObject)
                .OnComplete(() =>
                {
                    _kickTween = null;
                    _kickOffset = Vector3.zero;
                });
        }

        private void SpawnSpark(Vector2 awayFromPlayer, float heaviness)
        {
            if (!_hitParticlePrefab) return;

            var origin = _visualTarget ? _visualTarget.position : transform.position;
            var position = origin - (Vector3)(awayFromPlayer * _sparkOffset);
            var spark = Instantiate(_hitParticlePrefab, position, Quaternion.identity);
            spark.transform.localScale *= Mathf.Lerp(_sparkScaleRange.x, _sparkScaleRange.y, heaviness);
        }

        private static void KillTween(ref Tweener tween)
        {
            if (tween != null && tween.IsActive())
                tween.Kill();
            tween = null;
        }

        private static bool IsPlaying(Tweener tween)
        {
            return tween != null && tween.IsActive();
        }

        private void LateUpdate()
        {
            if (!_visualTarget) return;
            ApplyScale();
            ApplyPosition();
        }

        private void ApplyScale()
        {
            if (!IsPlaying(_punchTween))
            {
                _punchOffset = Vector3.zero;
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

        private void ApplyPosition()
        {
            bool kicking = IsPlaying(_kickTween);
            bool shaking = IsPlaying(_shakeTween);
            if (!kicking) _kickOffset = Vector3.zero;
            if (!shaking) _shakeOffset = Vector3.zero;

            if (!kicking && !shaking)
            {
                RestoreBasePosition();
                return;
            }

            var basePosition = GetBasePosition();
            var worldOffset = _kickOffset + _shakeOffset;
            var parent = _visualTarget.parent;
            var localOffset = parent ? parent.InverseTransformVector(worldOffset) : worldOffset;
            localOffset.z = 0f;

            _lastWrittenPosition = basePosition + localOffset;
            _visualTarget.localPosition = _lastWrittenPosition;
            _lastLocalOffset = localOffset;
            _hasWrittenPosition = true;
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

        private Vector3 GetBasePosition()
        {
            var current = _visualTarget.localPosition;
            if (!_hasWrittenPosition || current != _lastWrittenPosition)
                return current;

            return current - _lastLocalOffset;
        }

        private void RestoreBaseScale()
        {
            if (!_hasWrittenScale || !_visualTarget) return;
            _visualTarget.localScale = GetBaseScale();
            _hasWrittenScale = false;
            _lastMultiplier = Vector3.one;
        }

        private void RestoreBasePosition()
        {
            if (!_hasWrittenPosition || !_visualTarget) return;
            _visualTarget.localPosition = GetBasePosition();
            _hasWrittenPosition = false;
            _lastLocalOffset = Vector3.zero;
        }

        private static float SafeDivide(float value, float divisor)
        {
            return Mathf.Approximately(divisor, 0f) ? value : value / divisor;
        }
    }
}
