using Scripts.Enemies;
using Scripts.Enemies.Bullets;
using UnityEngine;

namespace Scripts.Player.Shooting
{
    public class PlayerFeather : BaseBullet
    {
        [SerializeField] protected float _size = 1f;
        public void SetParameters(float bulletSpeed, float bulletDamage, float size)
        {
            // overload not override
            SetParameters(bulletSpeed, bulletDamage);
            _size = size;
            _baseTransform = this.transform;
        }

        public override void DamagePlayer(PlayerHealth health)
        {
            // Doing nothing here
        }

        public virtual void DamageEnemy(EnemyHealth enemyHealth)
        {
            enemyHealth.HealthController.TakeDamage(_damage);
            Destroy(gameObject);
        }
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.TryGetComponent(out EnemyHealth health))
            {
                DamageEnemy(health);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
