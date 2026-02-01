using System.Collections;
using DG.Tweening;
using Scripts.Player;
using Scripts.Services.Pooling;
using UnityEngine;

namespace Scripts.Items.PermanentItems
{
    public class StickyMine : MonoBehaviour
    {
        private float _delay;
        private float _damage;
        private ComponentPool<Explosion> _explosionPool;

        public void Init(GameObject enemy, float delay, float damage, ComponentPool<Explosion> explosionPool)
        {
            _delay = delay;
            _damage = damage;
            _explosionPool = explosionPool;
            StartCoroutine(ExplodeAfterDelay());
        }

        private IEnumerator ExplodeAfterDelay()
        {
            float scaleTime = 0.2f;
            float waitTime = Mathf.Max(0, _delay - scaleTime);
            yield return new WaitForSeconds(waitTime);
            transform.DOScale(2f, scaleTime).SetEase(Ease.InBack);
            yield return new WaitForSeconds(scaleTime);

            if (_explosionPool != null)
            {
                var explosion = _explosionPool.Get(transform.position, Quaternion.identity);
                explosion.Init(_damage);
            }

            Destroy(gameObject);
        }
    }
}
