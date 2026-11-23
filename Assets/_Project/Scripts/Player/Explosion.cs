using System.Collections;
using Scripts.Enemies;
using Scripts.Health;
using UnityEngine;

namespace Scripts.Player
{
    public class Explosion : MonoBehaviour
    {
        [SerializeField] private float _returnDelay = 0.5f;
        [Tooltip("How hard the blast kicks enemy sprites away from its center (1 = normal hit). " +
                 "Cosmetic only; the physical push is the knockback below.")]
        [SerializeField] private float _spriteKickScale = 1.5f;

        private float _damage;
        private float _radius;
        private float _knockback;

        public float Radius => _radius;

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
        }

        public void Explode()
        {
            var hits = Physics2D.OverlapCircleAll(transform.position, _radius, ExplosionSystem.Instance.EnemyLayerMask);
            foreach (var hit in hits)
            {
                if (hit.TryGetComponent(out EnemyHealth enemyHealth))
                {
                    Debug.Log("Explode");
                    enemyHealth.HealthController.TakeDamage(_damage,
                        DamageSource.At(transform.position, _spriteKickScale));
                }

                if (hit.TryGetComponent(out Rigidbody2D rb))
                {
                    Vector2 dir = ((Vector2)hit.transform.position - (Vector2)transform.position).normalized;
                    rb.AddForce(dir * _knockback, ForceMode2D.Impulse);
                }
            }

            ExplosionSystem.Instance.NotifyExplosionCreated(transform.position, _damage);

            StartCoroutine(ReturnAfterDelay());
        }

        private IEnumerator ReturnAfterDelay()
        {
            yield return new WaitForSeconds(_returnDelay);
            gameObject.SetActive(false);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, _radius);
        }
    }
}
