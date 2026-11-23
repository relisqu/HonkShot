using DG.Tweening;
using Scripts.Enemies;
using Scripts.Health;
using UnityEngine;

namespace Scripts.VFX
{
    // Visual reaction to taking damage: white flash, squash wobble, knockback kick away from the
    // damage source, hit spark, and a tremble while stunned. Only the visual child moves/scales,
    // never the root, so colliders are untouched. Tuning lives in the shared HitFeedbackConfig.
    public class HitFeedback : MonoBehaviour
    {
        [SerializeField] private HitFeedbackConfig _config;
        [SerializeField] private HealthController _healthController;

        [Tooltip("Visual child that wobbles and gets knocked back. Must not hold colliders. Leave empty for flash only.")]
        [SerializeField] private Transform _visualTarget;

        [Tooltip("Leave empty to flash every sprite under the visual target.")]
        [SerializeField] private SpriteRenderer[] _flashRenderers;

        private EnemyHitStun _hitStun;
        private SpriteFlasher _flasher;
        private VisualTransformLayer _transformLayer;

        private Tweener _punchTween;
        private Vector3 _punchOffset;
        private Tweener _kickTween;
        private Vector3 _kickOffset;
        private Tweener _shakeTween;
        private Vector3 _shakeOffset;

        private void Awake()
        {
            if (!_healthController)
                _healthController = GetComponent<HealthController>();
            if (!_healthController)
                _healthController = GetComponentInParent<HealthController>();
            _hitStun = GetComponent<EnemyHitStun>();
            if (!_config)
                Debug.LogWarning($"{name}: HitFeedback has no HitFeedbackConfig assigned, hit reactions are off.", this);

            if (_flashRenderers == null || _flashRenderers.Length == 0)
                _flashRenderers = (_visualTarget ? _visualTarget : transform).GetComponentsInChildren<SpriteRenderer>(true);

            _flasher = new SpriteFlasher(_flashRenderers);
            if (_visualTarget)
                _transformLayer = new VisualTransformLayer(_visualTarget);
        }

        private void Start()
        {
            if (_healthController)
                _healthController.OnDamageReceived += HealthController_DamageReceived;
            if (_hitStun)
                _hitStun.OnStunned += HitStun_Stunned;
        }

        private void OnDestroy()
        {
            if (_healthController)
                _healthController.OnDamageReceived -= HealthController_DamageReceived;
            if (_hitStun)
                _hitStun.OnStunned -= HitStun_Stunned;
        }

        private void OnDisable()
        {
            KillTweens();
            _flasher?.Restore();
            _transformLayer?.Restore();
        }

        private void HealthController_DamageReceived(float damage, DamageSource source)
        {
            Play(damage, source);
        }

        private void HitStun_Stunned(float duration)
        {
            PlayStun(duration);
        }

        public void Play(float damage, DamageSource source)
        {
            if (!_config || !isActiveAndEnabled) return;

            if (!_config.IsFullReaction(damage))
            {
                if (IsPlaying(_punchTween)) return;
                Flash();
                Punch(0f);
                return;
            }

            float strength = _config.GetHitStrength(damage);
            Flash();
            Punch(strength);

            var origin = GetVisualOrigin();
            if (!source.HasPosition)
            {
                SpawnSpark(origin, strength);
                return;
            }

            var awayFromSource = (Vector2)origin - source.Position;
            awayFromSource = awayFromSource.sqrMagnitude > 0.0001f ? awayFromSource.normalized : Vector2.up;
            Kick(awayFromSource * (_config.GetKickDistance(strength) * source.KickScale));
            SpawnSpark(origin - (Vector3)(awayFromSource * _config.SparkOffset), strength);
        }

        private void PlayStun(float duration)
        {
            if (!_config || _transformLayer == null || !isActiveAndEnabled || duration <= 0f) return;

            KillTween(ref _shakeTween);
            _shakeOffset = Vector3.zero;
            _shakeTween = DOTween.Shake(() => _shakeOffset, value => _shakeOffset = value, duration,
                    _config.StunShakeStrength, _config.StunShakeVibrato, 90f, true, true)
                .SetLink(gameObject);
        }

        private void LateUpdate()
        {
            if (_transformLayer == null) return;

            var scaleMultiplier = Vector3.one + (IsPlaying(_punchTween) ? _punchOffset : Vector3.zero);
            var offset = (IsPlaying(_kickTween) ? _kickOffset : Vector3.zero)
                         + (IsPlaying(_shakeTween) ? _shakeOffset : Vector3.zero);
            _transformLayer.Apply(scaleMultiplier, offset);
        }

        private void Flash()
        {
            _flasher.Flash(_config.FlashMaterial, _config.FlashDuration, gameObject);
        }

        private void Punch(float strength)
        {
            if (_transformLayer == null) return;

            KillTween(ref _punchTween);
            _punchOffset = Vector3.zero;
            _punchTween = DOTween.Punch(() => _punchOffset, value => _punchOffset = value, _config.GetPunch(strength),
                    _config.PunchDuration, _config.PunchVibrato, _config.PunchElasticity)
                .SetLink(gameObject);
        }

        private void Kick(Vector2 worldOffset)
        {
            if (_transformLayer == null || worldOffset == Vector2.zero) return;

            KillTween(ref _kickTween);
            _kickOffset = Vector3.zero;
            _kickTween = DOTween.Punch(() => _kickOffset, value => _kickOffset = value, worldOffset,
                    _config.KickDuration, _config.KickVibrato, _config.KickElasticity)
                .SetLink(gameObject);
        }

        private void SpawnSpark(Vector3 position, float strength)
        {
            if (!_config.SparkPrefab) return;

            var spark = Instantiate(_config.SparkPrefab, position, Quaternion.identity);
            spark.transform.localScale *= _config.GetSparkScale(strength);
        }

        private Vector3 GetVisualOrigin()
        {
            return _visualTarget ? _visualTarget.position : transform.position;
        }

        private void KillTweens()
        {
            KillTween(ref _punchTween);
            KillTween(ref _kickTween);
            KillTween(ref _shakeTween);
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
    }
}
