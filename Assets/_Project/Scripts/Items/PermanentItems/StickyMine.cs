using System.Collections;
using DG.Tweening;
using Scripts.Player;
using UnityEngine;

namespace Scripts.Items.PermanentItems
{
    public class StickyMine : MonoBehaviour
    {
        [SerializeField] private Explosion _explosion;
        [SerializeField] private float _dropSpeed = 2f;
        [SerializeField] private float _dropDistance = 1f;

        private float _delay;
        private float _damage;
        private Transform _target;
        private bool _isDropping;
        private bool _isExploding;
        private float _droppedDistance;

        public void Init(Transform target, float delay, float damage, float radius, float knockback)
        {
            _target = target;
            _delay = delay;
            _damage = damage;
            _isExploding = false;
            _isDropping = false;
            _droppedDistance = 0f;
            transform.localScale = Vector3.one;

            if (_target)
                transform.position = _target.position;

            _explosion.Init(_damage, radius, knockback);
            StartCoroutine(ExplodeAfterDelay());
        }

        private void Update()
        {
            if(_isExploding) return;
            if (_target)
            {
                transform.position = _target.position;
                return;
            }

            if (!_isDropping)
            {
                _isDropping = true;
                _droppedDistance = 0f;
            }

            if (_droppedDistance < _dropDistance)
            {
                float drop = _dropSpeed * Time.deltaTime;
                transform.position += Vector3.down * drop;
                _droppedDistance += drop;
            }
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
            _isExploding = true;
            gameObject.SetActive(false);
        }

        private void OnDisable()
        {
            _target = null;
            _isDropping = false;
            transform.DOKill();
            StopAllCoroutines();
        }
    }
}
