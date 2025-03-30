using System.Collections;
using DG.Tweening;
using Scripts.PointSystem;
using UnityEngine;

namespace Scripts.Health
{
    public class EnemyAttackController : AttackController
    {
        [Header("Attack Settings")] [SerializeField]
        private float _attackRange = 1.5f;

        [SerializeField] private float _attackInterval = 1f;
        [SerializeField] private float _attackWarningTime = 0.5f;
        [SerializeField] private int _damage = 1;
        [SerializeField] private ParticleSystem _attackWarningEffect;
        [SerializeField] private SpriteRenderer _prefireSpriteRenderer;
        [SerializeField] private Color _prefireSpriteColor;

        [Header("Attack Target")] [SerializeField]
        private Transform _attackTarget;

        private bool _isAttacking;
        private Coroutine _attackRoutine;

        private void Start()
        {
            if (_attackTarget == null && PointReceiver.Instance != null)
            {
                _attackTarget = PointReceiver.Instance.transform;
            }

            _attackRoutine = StartCoroutine(AttackLoop());
        }

        private IEnumerator AttackLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(_attackInterval);

                if (_isAttacking || _attackTarget == null)
                    continue;

                float distance = Vector2.Distance(transform.position, _attackTarget.position);
                if (distance <= _attackRange)
                {
                    StartCoroutine(PerformAttack());
                }
            }
        }

        private IEnumerator PerformAttack()
        {
            _isAttacking = true;

            _prefireSpriteRenderer.DOColor(_prefireSpriteColor, _attackWarningTime - 0.1f).SetEase(Ease.OutCirc);
            _prefireSpriteRenderer.transform.localScale = Vector3.one * (_attackRange * 2f);
            _attackWarningEffect.Play();

            yield return new WaitForSeconds(_attackWarningTime);

            if (Vector2.Distance(transform.position, _attackTarget.position) <= _attackRange &&
                _attackTarget.TryGetComponent(out HealthController targetHealth))
            {
                targetHealth.TakeDamage(_damage);
            }

            _prefireSpriteRenderer.color = Color.clear;
            _isAttacking = false;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _attackRange);
        }

        public void SetTarget(Transform target)
        {
            _attackTarget = target;
        }
    }
}