using Scripts.Health;
using Scripts.LevelSystem.LevelObjects;
using UnityEngine;

namespace Scripts.Audio
{
    /// <summary>
    /// Bounce audio for an entity that deals damage on impact. Where the thing being hit can actually
    /// take damage - an enemy, but equally a destructible obstacle - loudness follows the damage that
    /// landed instead of raw speed, so a hit a shield swallowed stays quiet. Everything else falls
    /// back to the speed-based reading in the base emitter.
    /// </summary>
    public class PlayerBounceSoundEmitter : BounceSoundEmitter
    {
        [Header("Damage")]
        [SerializeField] private PlayerAttackController _attackController;

        [Tooltip("Damage that maps to the loudest bounce.")]
        [SerializeField] private float _maxDamage = 40f;

        [Tooltip("Damage is applied by the target's trigger collider, which fires just before the " +
                 "physical contact. A damage report younger than this counts as belonging to this bounce.")]
        [SerializeField] private float _damageMatchWindow = 0.15f;

        private float _lastDamage;
        private float _lastDamageTime = float.NegativeInfinity;

        protected override void Awake()
        {
            base.Awake();

            if (!_attackController)
                _attackController = GetComponentInChildren<PlayerAttackController>(true);
        }

        private void OnEnable()
        {
            if (_attackController)
                _attackController.OnDamaged += AttackController_Damaged;
        }

        private void OnDisable()
        {
            if (_attackController)
                _attackController.OnDamaged -= AttackController_Damaged;
        }

        private void AttackController_Damaged(float damage)
        {
            _lastDamage = damage;
            _lastDamageTime = Time.time;
        }

        protected override float ResolveIntensity(Collision2D collision, BounceSurface surface, float impactSpeed)
        {
            if (!IsDamageable(collision))
                return base.ResolveIntensity(collision, surface, impactSpeed);

            if (Time.time - _lastDamageTime > _damageMatchWindow)
            {
                // Damageable, but nothing landed: a shield ate the hit, the target is invincible, or
                // it is already dying. Stay quiet rather than fake a heavy impact.
                return 0f;
            }

            return _maxDamage > 0f ? _lastDamage / _maxDamage : 1f;
        }

        private static bool IsDamageable(Collision2D collision)
        {
            var healthController = collision.gameObject.GetComponentInParent<HealthController>();
            return healthController && healthController.IsAlive;
        }
    }
}
