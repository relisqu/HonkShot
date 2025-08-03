using System.Collections;
using DG.Tweening;
using Scripts.Health;
using Scripts.UI;
using UnityEngine;

namespace Scripts.Items.PermanentItems
{
    public class StickyMine : MonoBehaviour
    {
        private GameObject _enemy;
        private float _delay;
        private float _damage;

        public void Init(GameObject enemy, float delay, float damage)
        {
            _enemy = enemy;
            _delay = delay;
            _damage = damage;
            StartCoroutine(ExplodeAfterDelay());
        }

        private IEnumerator ExplodeAfterDelay()
        {
            // Scale up a bit before explosion
            float scaleTime = 0.2f;
            float waitTime = Mathf.Max(0, _delay - scaleTime);
            yield return new WaitForSeconds(waitTime);
            transform.DOScale(1f, scaleTime).SetEase(Ease.InBack);
            yield return new WaitForSeconds(scaleTime);
            if (_enemy)
            {
                var health = _enemy.GetComponent<HealthController>();
                if (health)
                {
                    health.TakeDamage(_damage);
                }
            }
            // Play explosion VFX using UIFactory
            new UIFactory().CreateExplosionParticle(transform.position);
            Destroy(gameObject);
        }
    }
} 