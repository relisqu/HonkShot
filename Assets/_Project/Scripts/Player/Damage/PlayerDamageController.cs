using System;
using Scripts.Enemies;
using UnityEngine;

namespace Scripts.Player.Damage
{
    public class PlayerDamageController : MonoBehaviour
    {
        [SerializeField] private float _defaultDamage = 10;

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (other.gameObject.TryGetComponent(out EnemyHealth enemyHealth))
            {
                enemyHealth.HealthController.TakeDamage(_defaultDamage);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.TryGetComponent(out EnemyHealth enemyHealth))
            {
            }
        }
    }
}