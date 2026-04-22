using Scripts.Audio;
using Scripts.Health;
using Scripts.Player;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Scripts.LevelSystem.LevelObjects
{
    [RequireComponent(typeof(HealthController))]
    public class DamageableBounceObject : BounceObject
    {
        [SerializeField] private HealthController _healthController;
        [SerializeField] private Collider2D _solidCollider;

        private bool _solidDisabledForLethal;

        public HealthController HealthController => _healthController;

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

        private void HealthController_Died()
        {
            Destroy(gameObject);
        }

        

        // Trigger (bigger) - early detection of approaching player
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!_healthController.IsAlive) return;
            if (other.gameObject.TryGetComponent(out PlayerGhostProjectile _)) return;

            if (other.gameObject.TryGetComponent(out PlayerAttackController attackController))
            {
                var rb = other.attachedRigidbody;
                if (!rb) return;


                var velocity = rb.linearVelocity;
                if (velocity.sqrMagnitude < 0.01f) return;

              //  var rayDistance = velocity.magnitude * Time.fixedDeltaTime * 5f;
                //var hit = Physics2D.Raycast(rb.position, velocity.normalized, rayDistance);
             //   if (!hit.collider || hit.collider != _solidCollider) return;
//
                var damage = attackController.GetDamage();
                var modifiedDamage = _healthController.CalculateDamageTaken(damage);

                if (modifiedDamage >= _healthController.GetCurrentHealth())
                {
                    _solidCollider.enabled = false;
                    ApplyDamage(attackController);
                }
            }
        }


        private void ApplyDamage(PlayerAttackController playerAttackController)
        {
            if (!_healthController.IsAlive) return;
            if (_healthController.IsInvincible()) return;

            var damage = playerAttackController.GetDamage();
            var resultDamage = _healthController.TakeDamage(damage);
            playerAttackController.Damage(gameObject);
            playerAttackController.OnDamaged?.Invoke(resultDamage);
        }
    }
}