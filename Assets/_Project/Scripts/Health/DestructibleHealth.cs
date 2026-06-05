using Scripts.Player;
using UnityEngine;

namespace Scripts.Health
{
    public class DestructibleHealth : MonoBehaviour
    {
        [SerializeField] private HealthController _healthController;

        public HealthController HealthController => _healthController;
        public bool IsDamagedByTrigger;
        public bool IsDamagedByCollision;
[Space]
        [SerializeField] private ParticleSystem _damageParticles;
        [SerializeField] private Vector2 _damageParticlesRange;
        [SerializeField] private float _damageCoefficient;
        [SerializeField] private Transform _visualTransform;
        [SerializeField] private Collider2D _solidCollider;
        private void Awake()
        {
            if (!_healthController)
                _healthController = GetComponent<HealthController>();
        }

        private void Start()
        {
            _healthController.OnDied += HealthController_Died;
        }

        private void OnDestroy()
        {
            _healthController.OnDied -= HealthController_Died;
        }

        private void HealthController_Died(HealthController controller = null)
        {
            _visualTransform.gameObject.SetActive(false);
            Destroy(gameObject,5);
            _solidCollider.enabled = false;
            _damageParticles.Emit((int)_damageParticlesRange.y);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if(!IsDamagedByTrigger) return;
            if (other.gameObject.TryGetComponent(out PlayerGhostProjectile _))
                return;

            if (_healthController.IsInvincible()) return;
            if (other.gameObject.TryGetComponent(out PlayerAttackController playerAttackController))
            {
                ReceiveDamage(playerAttackController);
            }
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            
            if(!IsDamagedByTrigger) return;
            if (other.gameObject.TryGetComponent(out PlayerGhostProjectile _))
                return;

            if (_healthController.IsInvincible()) return;
            if (other.gameObject.TryGetComponent(out PlayerAttackController playerAttackController))
            {
                ReceiveDamage(playerAttackController);
            }
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if(!IsDamagedByCollision) return;
            if (other.gameObject.TryGetComponent(out PlayerGhostProjectile _))
                return;

            if (_healthController.IsInvincible()) return;
            if (other.gameObject.TryGetComponent(out PlayerAttackController playerAttackController))
            {
                ReceiveDamage(playerAttackController);
            }
        }
        public void ReceiveDamage(PlayerAttackController playerAttackController)
        {
            var damage = playerAttackController.GetDamage();
            var finalDamage = _healthController.TakeDamage(damage);
            playerAttackController.Damage(gameObject);
            playerAttackController.OnDamaged?.Invoke(finalDamage);
            Debug.Log("Final damage: " + finalDamage);
            
            
            var particlesAmount = (int)Mathf.Clamp(finalDamage * _damageCoefficient, _damageParticlesRange.x,
                _damageParticlesRange.y);
            Debug.Log("emited" +particlesAmount);
            _damageParticles.Emit(particlesAmount);
        }
    }
}
