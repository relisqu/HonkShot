using System;
using DG.Tweening;
using Scripts.Audio;
using Scripts.Camera;
using Scripts.Health;
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

        private float _lastDamageReceived;

        public HealthController HealthController => _healthController;

        private void Awake()
        {
            if (!_baseEnemy)
            {
                _baseEnemy = GetComponent<BaseEnemy>();
            }
        }

        private void HealthController_Died()
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

        private Tweener _punchTween;

        private void HealthController_OnNonLethalDamageReceived(float damage)
        {
            if (ScoreManager.Instance && _baseEnemy)
            {
                ScoreManager.Instance.OnDamageDealt(_baseEnemy, _lastDamageReceived);
            }

            AudioManager.Instance.PlayOneShot(SoundChanelType.Enemy, "damageEnemy");
            if (_punchTween != null) return;

            _punchTween = transform.DOPunchScale(0.4f * Vector3.one, 0.2f).OnComplete(() => { _punchTween = null; });
        }

        public bool IsAlive()
        {
            return _healthController.IsAlive;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.TryGetComponent(out PlayerGhostProjectile _))
            {
                return;
            }

            if (_healthController.IsInvincible()) return;
            if (other.gameObject.TryGetComponent(out PlayerAttackController playerAttackController))
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
            if (other.gameObject.TryGetComponent(out PlayerGhostProjectile _))
            {
                return;
            }

            if (_healthController.IsInvincible()) return;
            if (other.gameObject.TryGetComponent(out PlayerAttackController playerAttackController))
            {
                ReceiveDamage(playerAttackController);
            }
        }

        private void OnCollisionStay2D(Collision2D other)
        {
            if (other.gameObject.TryGetComponent(out PlayerGhostProjectile _))
            {
                return;
            }

            if (_healthController.IsInvincible()) return;
            if (other.gameObject.TryGetComponent(out PlayerAttackController playerAttackController))
            {
                ReceiveDamage(playerAttackController);
            }
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (other.gameObject.TryGetComponent(out PlayerGhostProjectile _))
            {
                return;
            }

            if (_healthController.IsInvincible()) return;
            if (other.gameObject.TryGetComponent(out PlayerAttackController playerAttackController))
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