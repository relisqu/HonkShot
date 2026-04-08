using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Scripts.Health;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Scripts.Enemies
{
    public class WalkingEnemy : BaseEnemy
    {
        [Header("Path Settings")] [SerializeField]
        private List<Transform> _pathPoints = new();

        [InfoBox("We have 3 points with index 1,2,3." +
                 "\n YoYo type will walk 1 2 3 2 1 2 3." +
                 "\n Closed type will walk 1 2 3 1 2 3."
        )]
        [SerializeField]
        private WalkingType _walkingType = WalkingType.Closed;

        [SerializeField] private float _moveSpeed = 2f;
        [SerializeField] private SpriteRenderer _prefireSpriteRenderer;
        [SerializeField] private Color _prefireSpriteColor;
        [SerializeField] private int _currentTargetIndex = 0;

        [Header("Attack Settings")] [SerializeField]
        private float _attackRange = 1.5f;

        [SerializeField] private float _attackInterval = 1f;
        [SerializeField] private float _attackWarningTime = 0.5f;
        [SerializeField] private int _damage = 1;

        [Header("Visuals")] [Space] [SerializeField]
        private ParticleSystem _attackWarningEffect;

        [SerializeField] private LayerMask _playerLayer;

        [Header("Components")] [SerializeField]
        private Animator _animator;

        [SerializeField] private EnemyHealth _enemyHealth;
        [SerializeField] private Rigidbody2D _rigidbody2D;


        private bool _isAttacking = false;
        private float _nextAttackTime;
        private int _currentDirection = 1;

        private void OnEnable()
        {
            if (_pathPoints.Count == 0)
            {
                Debug.LogError("No path points assigned.");
                enabled = false;
                return;
            }

            _pathPoints.RemoveAll(point => point == null);

            _nextAttackTime = Time.time + _attackInterval;
            StartCoroutine(BehaviourRoutine());
        }

        private IEnumerator BehaviourRoutine()
        {
            while (_enemyHealth.IsAlive())
            {
                // Attack always wins when ready and a player is inside the attack circle —
                // pauses the walk for a strike, then resumes pathing on the next iteration.
                if (Time.time >= _nextAttackTime && TryFindPlayerHealth(out _))
                {
                    yield return AttackOnce();
                    _nextAttackTime = Time.time + _attackInterval;
                    continue;
                }

                yield return WalkStep();
            }
        }

        private IEnumerator WalkStep()
        {
            Transform targetPoint = _pathPoints[_currentTargetIndex];
            if (_animator) _animator.SetBool("IsWalking", true);

            Vector2 targetVelocity = ((Vector2)targetPoint.position - (Vector2)transform.position).normalized * _moveSpeed;
            _rigidbody2D.linearVelocity = Vector2.MoveTowards(_rigidbody2D.linearVelocity, targetVelocity, _moveSpeed * Time.deltaTime);

            if (Vector2.Distance(transform.position, targetPoint.position) <= 0.1f)
                AdvanceToNextPoint();

            yield return null;
        }

        private void AdvanceToNextPoint()
        {
            switch (_walkingType)
            {
                case WalkingType.YoYo:
                    if (_currentTargetIndex == 0)
                        _currentDirection = 1;
                    else if (_currentTargetIndex >= _pathPoints.Count - 1)
                        _currentDirection = -1;

                    _currentTargetIndex = (_currentTargetIndex + _currentDirection) % _pathPoints.Count;
                    break;

                case WalkingType.Closed:
                    _currentTargetIndex = (_currentTargetIndex + 1) % _pathPoints.Count;
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private bool TryFindPlayerHealth(out HealthController playerHealth)
        {
            var hits = Physics2D.OverlapCircleAll(transform.position, _attackRange, _playerLayer);
            for (int i = 0; i < hits.Length; i++)
            {
                var hc = hits[i].GetComponentInParent<HealthController>();
                if (hc)
                {
                    playerHealth = hc;
                    return true;
                }
            }

            playerHealth = null;
            return false;
        }

        private IEnumerator AttackOnce()
        {
            _isAttacking = true;
            _rigidbody2D.linearVelocity = Vector2.zero;
            if (_animator)
            {
                _animator.SetBool("IsWalking", false);
                _animator.SetTrigger("Attack");
            }

            if (_prefireSpriteRenderer)
            {
                _prefireSpriteRenderer.DOColor(_prefireSpriteColor, _attackWarningTime - 0.1f).SetEase(Ease.OutCirc);
                _prefireSpriteRenderer.transform.localScale = Vector3.one * (_attackRange * 2f);
            }

            if (_attackWarningEffect)
                _attackWarningEffect.Play();

            yield return new WaitForSeconds(_attackWarningTime);

            if (TryFindPlayerHealth(out var playerHealth))
                playerHealth.TakeDamage(_damage);

            if (_prefireSpriteRenderer)
                _prefireSpriteRenderer.color = Color.clear;

            _isAttacking = false;
        }

        public void FinishAnimation()
        {
            _animator.SetBool("IsWalking", true);
            _isAttacking = false;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            for (int i = 0; i < _pathPoints.Count; i++)
            {
                if (_pathPoints[i] != null)
                {
                    Gizmos.DrawSphere(_pathPoints[i].position, 0.2f);
                    if (i < _pathPoints.Count - 1 && _pathPoints[i + 1] != null)
                        Gizmos.DrawLine(_pathPoints[i].position, _pathPoints[i + 1].position);
                }
            }

            if (_pathPoints.Count >= 2 && _walkingType == WalkingType.Closed)
            {
                Gizmos.DrawLine(_pathPoints[0].position, _pathPoints[^1].position);
            }

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _attackRange);
        }
    }

    public enum WalkingType
    {
        YoYo,
        Closed,
    }
}