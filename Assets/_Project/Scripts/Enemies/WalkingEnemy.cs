using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Scripts.Health;
using Scripts.Player;
using Scripts.PointSystem;
using UnityEngine;

namespace Scripts.Enemies
{
    public class WalkingEnemy : MonoBehaviour
    {
        [Header("Path Settings")] [SerializeField]
        private Transform[] _pathPoints;

        [SerializeField] private float _moveSpeed = 2f;
        [SerializeField] private SpriteRenderer _prefireSpriteRenderer;
        [SerializeField] private Color _prefireSpriteColor;
        [SerializeField] private int _currentTargetIndex = 0;

        [Header("Attack Settings")] [SerializeField]
        private float _attackRange = 1.5f;

        [SerializeField] private float _attackInterval = 1f;
        [SerializeField] private float _attackWarningTime = 0.5f;
        [SerializeField] private int _damage = 1;
        [SerializeField] private ParticleSystem _attackWarningEffect;
        [SerializeField] private LayerMask _playerLayer;

        [Header("Components")] [SerializeField]
        private Animator _animator;

        [SerializeField] private EnemyHealth _enemyHealth;


        private bool _isAttacking = false;

        private void Start()
        {
            if (_pathPoints.Length == 0)
            {
                Debug.LogError("No path points assigned.");
                enabled = false;
                return;
            }

            StartCoroutine(FollowPath());
            StartCoroutine(AttackRoutine());
        }

        private IEnumerator FollowPath()
        {
            while (_enemyHealth.IsAlive())
            {
                Transform targetPoint = _pathPoints[_currentTargetIndex];
                _animator.SetBool("IsWalking", true);

                while (Vector2.Distance(transform.position, targetPoint.position) > 0.1f)
                {
                    while (_isAttacking)
                    {
                        yield return null;
                    }

                    transform.position = Vector2.MoveTowards(transform.position, targetPoint.position,
                        _moveSpeed * Time.deltaTime);
                    yield return null;
                }

                _currentTargetIndex = (_currentTargetIndex + 1) % _pathPoints.Length;
            }
        }


        private IEnumerator AttackRoutine()
        {
            while (_enemyHealth.IsAlive())
            {
                if (_isAttacking) yield return null;
                yield return new WaitForSeconds(_attackInterval);

                _animator.SetBool("IsWalking", false);
                _isAttacking = true;

                _prefireSpriteRenderer.DOColor(_prefireSpriteColor, _attackWarningTime - 0.1f).SetEase(Ease.OutCirc);
                _prefireSpriteRenderer.transform.localScale = Vector3.one * (_attackRange * 2f);
                _attackWarningEffect.Play();
                _animator.SetTrigger("Attack");
                yield return new WaitForSeconds(_attackWarningTime);
                var distance = Vector2.Distance(transform.position, PointReceiver.Instance.transform.position);
                if (distance <= _attackRange)
                {
                    PointReceiver.Instance.GetComponent<HealthController>()?.TakeDamage(_damage);
                }

                _prefireSpriteRenderer.color = Color.clear;
            }
        }

        public void FinishAnimation()
        {
            _animator.SetBool("IsWalking", true);
            _isAttacking = false;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _attackRange);
        }
    }
}