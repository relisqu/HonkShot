using System.Collections;
using DG.Tweening;
using Scripts.Player;
using Scripts.Services.Pooling;
using UnityEngine;

namespace Scripts.Items.PermanentItems
{
    public class StickyMine : MonoBehaviour
    {
        [SerializeField] private Explosion _explosion;
        private float _delay;
        private float _damage;

        public void Init(float delay, float damage, float radius, float knockback)
        {
            _delay = delay;
            _damage = damage;
            _explosion.Init(_damage, radius, knockback);
            StartCoroutine(ExplodeAfterDelay());
        }

        private IEnumerator ExplodeAfterDelay()
        {
            float scaleTime = 0.2f;
            float waitTime = Mathf.Max(0, _delay - scaleTime);
            yield return new WaitForSeconds(waitTime);
            transform.DOScale(2f, scaleTime).SetEase(Ease.InBack);
            yield return new WaitForSeconds(scaleTime);
            _explosion.gameObject.SetActive(true);
            _explosion.Explode();

            gameObject.SetActive(false);
        }
    }
}