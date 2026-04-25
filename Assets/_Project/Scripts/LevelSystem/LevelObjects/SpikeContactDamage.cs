using System.Collections.Generic;
using Scripts.Enemies;
using Scripts.Health;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Scripts.LevelSystem.LevelObjects
{
    public class SpikeContactDamage : MonoBehaviour
    {
        [SerializeField] private float _defaultDamage = 15f;
        [SerializeField] private bool _hasTheSameContactDamageForAllEntities = true;

        [HideIf(nameof(_hasTheSameContactDamageForAllEntities))]
        [SerializeField] private float _enemyContactDamage = 15f;

        [SerializeField] private float _damageIFrame = 0.2f;

        private readonly Dictionary<HealthController, float> _lastDamageTimes = new();

        private void OnCollisionEnter2D(Collision2D other) => TryDamage(other.collider);
        private void OnCollisionStay2D(Collision2D other) => TryDamage(other.collider);
        private void OnTriggerEnter2D(Collider2D other) => TryDamage(other);
        private void OnTriggerStay2D(Collider2D other) => TryDamage(other);

        private void TryDamage(Collider2D col)
        {
            if (col.gameObject.TryGetComponent(out PlayerHealth playerHealth))
            {
                ApplyDamage(playerHealth.HealthController, _defaultDamage);
                return;
            }

            if (col.gameObject.TryGetComponent(out EnemyHealth enemyHealth))
            {
                float damage = _hasTheSameContactDamageForAllEntities ? _defaultDamage : _enemyContactDamage;
                ApplyDamage(enemyHealth.HealthController, damage);
            }
        }

        private void ApplyDamage(HealthController health, float damage)
        {
            if (!health || !health.IsAlive) return;
            if (health.IsInvincible()) return;

            float now = Time.time;
            if (_lastDamageTimes.TryGetValue(health, out float last) && now - last < _damageIFrame)
            {
                return;
            }

            health.TakeDamage(damage);
            _lastDamageTimes[health] = now;
        }
    }
}
