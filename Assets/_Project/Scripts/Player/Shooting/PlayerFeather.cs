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
    }
}
