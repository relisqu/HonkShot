using System;
using DG.Tweening;
using Scripts.Audio;
using Scripts.Camera;
using Scripts.Health;
using Scripts.Player;
using Scripts.UI;
using UnityEngine;

namespace Scripts.Enemies
{
    [RequireComponent(typeof(HealthController))]
    public class EnemyHealth : MonoBehaviour
    {
        [SerializeField] private HealthController _healthController;
        public HealthController HealthController => _healthController;

        private void HealthController_Died()
        {
            AudioManager.Instance.PlayOneShot(SoundChanelType.Enemy, "killEnemy");
            CameraShakeHandler.Instance.ShakeCamera(0.2f, 2f);
            new UIFactory().CreateExplosionParticle(transform.position);
            Destroy(gameObject);
        }

        private Tweener _punchTween;

        private void HealthController_Damaged()
        {
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
                _healthController.TakeDamage(playerAttackController.GetDamage());
                playerAttackController.Damage(gameObject);
            }
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
                _healthController.TakeDamage(playerAttackController.GetDamage());
                playerAttackController.Damage(gameObject);
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
                _healthController.TakeDamage(playerAttackController.GetDamage());
                playerAttackController.Damage(gameObject);
            }
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (other.gameObject.TryGetComponent(out PlayerGhostProjectile _))
            {
                Debug.Log("AAAAAAA");
                return;
            }
            if (_healthController.IsInvincible()) return;
            if (other.gameObject.TryGetComponent(out PlayerAttackController playerAttackController))
            {
                _healthController.TakeDamage(playerAttackController.GetDamage());
                playerAttackController.Damage(gameObject);
            }
        }

        private void Start()
        {
            _healthController.OnDied += HealthController_Died;
            _healthController.OnDamaged += HealthController_Damaged;
        }

        private void OnDestroy()
        {
            _healthController.OnDied -= HealthController_Died;
            _healthController.OnDamaged -= HealthController_Damaged;
        }
    }
}