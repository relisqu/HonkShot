using System.Collections;
using Scripts.Enemies;
using Scripts.Enemies.Bullets;
using Scripts.Services.Pooling;
using UnityEngine;

namespace Scripts.Player.Shooting
{
    public class PlayerFeather : BaseBullet, IPoolable<PlayerFeather>
    {
        [SerializeField] protected float _size = 1f;
        private ComponentPool<PlayerFeather> _pool; //блятьблятьблять
        private Coroutine _lifetimeCoroutine;
        public void SetPool(ComponentPool<PlayerFeather> pool)
        {
            _pool = pool;
        }
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
            _pool?.Return(this);
        }
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.TryGetComponent(out EnemyHealth health))
            {
                DamageEnemy(health);
            }
            else
            {
                _pool?.Return(this);
            }
        }

        protected override void Start()
        {
            // nononono
        }
        private void OnEnable()
        {
            
            if (_lifetimeCoroutine != null)
            {
                StopCoroutine(_lifetimeCoroutine);
            }
            
            _lifetimeCoroutine = StartCoroutine(LifetimeRoutine());
        }
        private void OnDisable()
        {
            if (_lifetimeCoroutine != null)
            {
                StopCoroutine(_lifetimeCoroutine);
                _lifetimeCoroutine = null;
            }
        }
        private IEnumerator LifetimeRoutine()
        {
            yield return new WaitForSeconds(_maxLifetime);
            _pool?.Return(this);
        }
        
    }
}
