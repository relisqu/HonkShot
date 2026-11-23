using Scripts.Audio;
using Scripts.Camera;
using Scripts.Health;
using Scripts.Other;
using Scripts.Player;
using Scripts.ScoreSystem;
using Scripts.UI;
using UnityEngine;

namespace Scripts.Enemies
{
    [RequireComponent(typeof(HealthController))]
    public class EnemyHealth : MonoBehaviour
    {
        [SerializeField] private HealthController _healthController;
        [SerializeField] private BaseEnemy _baseEnemy;

        [Header("Hit Sound")]
        [SerializeField] private string _hitSound = "damageEnemy";

        [Tooltip("Damage at or below this sounds like a normal hit.")]
        [SerializeField] private float _normalHitDamage = 20f;

        [Tooltip("Damage that reaches the loudest hit.")]
        [SerializeField] private float _heavyHitDamage = 40f;

        [Tooltip("Share of starting health at or below which a hit sounds normal.")]
        [SerializeField, Range(0f, 1f)] private float _normalHitHealthShare = 0.1f;

        [Tooltip("Share of starting health that reaches the loudest hit.")]
        [SerializeField, Range(0f, 1f)] private float _heavyHitHealthShare = 0.35f;

        [Tooltip("Loudness of a normal hit (x) and the heaviest hit (y). Above 1 amplifies past the clip's volume.")]
        [SerializeField] private Vector2 _hitLoudnessRange = new Vector2(1f, 1.35f);

        [Tooltip("Shapes how hit heaviness maps onto that loudness range.")]
        [SerializeField] private AnimationCurve _hitLoudnessResponse = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        private float _lastDamageReceived;

        public HealthController HealthController => _healthController;

        private void Awake()
        {
            if (!_healthController)
                _healthController = GetComponent<HealthController>();
            if (!_baseEnemy)
                _baseEnemy = GetComponent<BaseEnemy>();
        }

        private void HealthController_Died(HealthController controller)
        {
            if (ScoreManager.Instance && _baseEnemy)
            {
                ScoreManager.Instance.OnEnemyKilled(_baseEnemy);
            }

            AudioManager.Instance.PlayOneShot(SoundChanelType.Enemy, "killEnemy");
            CameraShakeHandler.Instance.ShakeCamera(0.2f, 2f);
            new UIFactory().CreateExplosionParticle(transform.position);
            Destroy(gameObject);
        }

        private void HealthController_OnNonLethalDamageReceived(float damage)
        {
            if (ScoreManager.Instance && _baseEnemy)
            {
                ScoreManager.Instance.OnDamageDealt(_baseEnemy, _lastDamageReceived);
            }

            if (AudioManager.Instance)
                AudioManager.Instance.PlayOneShotWithGain(SoundChanelType.Enemy, _hitSound, GetHitLoudness(damage));
        }

        private float GetHitLoudness(float damage)
        {
            float byDamage = _heavyHitDamage > _normalHitDamage
                ? Mathf.InverseLerp(_normalHitDamage, _heavyHitDamage, damage)
                : 0f;

            float startingHealth = _healthController.DefaultHealth;
            float byHealthShare = startingHealth > 0f && _heavyHitHealthShare > _normalHitHealthShare
                ? Mathf.InverseLerp(_normalHitHealthShare, _heavyHitHealthShare, damage / startingHealth)
                : 0f;

            float heaviness = Mathf.Max(byDamage, byHealthShare);
            return Mathf.Lerp(_hitLoudnessRange.x, _hitLoudnessRange.y, _hitLoudnessResponse.Evaluate(heaviness));
        }

        public bool IsAlive()
        {
            return _healthController.IsAlive;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponentInBody(out PlayerGhostProjectile _))
            {
                return;
            }

            if (_healthController.IsInvincible()) return;
            if (other.TryGetComponentInBody(out PlayerAttackController playerAttackController))
            {
                ReceiveDamage(playerAttackController);
            }
        }

        private void ReceiveDamage(PlayerAttackController playerAttackController)
        {
            _lastDamageReceived = playerAttackController.GetDamage();
            var resultDamage = _healthController.TakeDamage(_lastDamageReceived);
            playerAttackController.Damage(gameObject);
            playerAttackController.OnDamaged?.Invoke(resultDamage);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (other.TryGetComponentInBody(out PlayerGhostProjectile _))
            {
                return;
            }

            if (_healthController.IsInvincible()) return;
            if (other.TryGetComponentInBody(out PlayerAttackController playerAttackController))
            {
                ReceiveDamage(playerAttackController);
            }
        }

        private void OnCollisionStay2D(Collision2D other)
        {
            if (other.collider.TryGetComponentInBody(out PlayerGhostProjectile _))
            {
                return;
            }

            if (_healthController.IsInvincible()) return;
            if (other.collider.TryGetComponentInBody(out PlayerAttackController playerAttackController))
            {
                ReceiveDamage(playerAttackController);
            }
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (other.collider.TryGetComponentInBody(out PlayerGhostProjectile _))
            {
                return;
            }

            if (_healthController.IsInvincible()) return;
            if (other.collider.TryGetComponentInBody(out PlayerAttackController playerAttackController))
            {
                ReceiveDamage(playerAttackController);
            }
        }

        private void Start()
        {
            _healthController.OnDied += HealthController_Died;
            _healthController.OnNonLethalDamageReceived += HealthController_OnNonLethalDamageReceived;
        }

        private void OnDestroy()
        {
            _healthController.OnDied -= HealthController_Died;
            _healthController.OnNonLethalDamageReceived -= HealthController_OnNonLethalDamageReceived;
        }
    }
}