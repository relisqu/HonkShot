using Scripts.Enemies;
using UnityEngine;

namespace Scripts.Player
{
    public class Explosion : MonoBehaviour
    {
        [SerializeField] private float _returnDelay = 0.5f;

        private float _damage;
        private float _radius;
        private float _knockback;

        public void Init(float baseDamage)
        {
            if (!ExplosionSystem.Instance) return;

            _damage = ExplosionSystem.Instance.GetDamage(baseDamage);
            _radius = ExplosionSystem.Instance.GetRadius(ExplosionSystem.Instance.DefaultRadius);
            _knockback = ExplosionSystem.Instance.GetKnockback(ExplosionSystem.Instance.DefaultKnockback);
            Explode();
        }

        public void Init(float baseDamage, float baseRadius, float baseKnockback)
        {
            if (!ExplosionSystem.Instance) return;

            _damage = ExplosionSystem.Instance.GetDamage(baseDamage);
            _radius = ExplosionSystem.Instance.GetRadius(baseRadius);
            _knockback = ExplosionSystem.Instance.GetKnockback(baseKnockback);
            Explode();
        }

        private void Explode()
        {
            var hits = Physics2D.OverlapCircleAll(transform.position, _radius, ExplosionSystem.Instance.EnemyLayerMask);
            foreach (var hit in hits)
            {
                if (hit.TryGetComponent(out EnemyHealth enemyHealth))
                    enemyHealth.HealthController.TakeDamage(_damage);

                if (hit.TryGetComponent(out Rigidbody2D rb))
                {
                    Vector2 dir = ((Vector2)hit.transform.position - (Vector2)transform.position).normalized;
                    rb.AddForce(dir * _knockback, ForceMode2D.Impulse);
                }
            }

            ExplosionSystem.Instance.NotifyExplosionCreated(transform.position, _damage);
            Invoke(nameof(ReturnToPool), _returnDelay);
        }

        private void ReturnToPool()
        {
            gameObject.SetActive(false);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, _radius);
        }
    }
}
