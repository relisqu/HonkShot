using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Scripts.Health;
using UnityEngine;

namespace Scripts.Enemies.Swamp
{
    public class SwampWalkingEnemy : BaseEnemy
    {
        [Header("Path Settings")] [SerializeField]
        private List<Transform> _pathPoints = new();

        [SerializeField] private WalkingType _walkingType = WalkingType.Closed;

        [Header("Attack Settings")] [SerializeField]
        private float _attackRange = 1.5f;

        [SerializeField] private LayerMask _playerLayer;
        [SerializeField] private float _attackInterval = 1f;
        [SerializeField] private float _attackWarningTime = 0.5f;
        [SerializeField] private int _damage = 1;

        [Header("Visuals")] [SerializeField] private ParticleSystem _attackWarningEffect;
        [SerializeField] private SpriteRenderer _prefireSpriteRenderer;
        [SerializeField] private Color _prefireSpriteColor;

        [Header("Components")] [SerializeField]
        private JumpingMovement _jumpingMovement;

        [SerializeField] private EnemyHealth _enemyHealth;
        [SerializeField] private Animator _animator;
        [SerializeField] private Rigidbody2D _rb;

        private int _currentTargetIndex;
        private int _currentDirection = 1;
        private bool _isAttacking;
        private float _nextAttackTime;

        private void Awake()
        {
            if (!_jumpingMovement)
                _jumpingMovement = GetComponent<JumpingMovement>();
            if (!_enemyHealth)
                _enemyHealth = GetComponent<EnemyHealth>();
            if (!_animator)
                _animator = GetComponent<Animator>();
            if (!_rb)
                _rb = GetComponent<Rigidbody2D>();
            if (!_attackWarningEffect)
                _attackWarningEffect = GetComponentInChildren<ParticleSystem>();
            if (!_prefireSpriteRenderer)
            {
                var attackRadius = transform.Find("AttackRadius");
                if (attackRadius)
                    _prefireSpriteRenderer = attackRadius.GetComponent<SpriteRenderer>();
            }
        }

        private void OnEnable()
        {
            if (_pathPoints.Count == 0)
            {
                Debug.LogError("No path points assigned to SwampWalkingEnemy.");
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
                // pauses the path-walking cycle for a strike, then resumes jumping next iteration.
                if (Time.time >= _nextAttackTime && TryFindPlayerHealth(out _))
                {
                    yield return AttackOnce();
                    _nextAttackTime = Time.time + _attackInterval;
                    continue;
                }

                yield return JumpAlongPathOnce();
                yield return new WaitForSeconds(_jumpingMovement.PauseBetweenJumps);
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

        private IEnumerator JumpAlongPathOnce()
        {
            Transform targetPoint = _pathPoints[_currentTargetIndex];
            Vector2 direction = ((Vector2)targetPoint.position - (Vector2)transform.position).normalized;

            if (!_jumpingMovement.CanJumpTowards(direction))
            {
                // Path blocked — skip this waypoint instead of looping forever on it.
                AdvanceToNextPoint();
                yield break;
            }

            if (_animator) _animator.SetTrigger("Jump");
            _jumpingMovement.JumpToPosition(targetPoint.position);

            while (_jumpingMovement.IsJumping)
                yield return null;

            if (Vector2.Distance(transform.position, targetPoint.position) < 0.5f)
                AdvanceToNextPoint();
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

        private IEnumerator AttackOnce()
        {
            _isAttacking = true;
            _rb.linearVelocity = Vector2.zero;

            if (_prefireSpriteRenderer)
            {
                _prefireSpriteRenderer.DOColor(_prefireSpriteColor, _attackWarningTime - 0.1f).SetEase(Ease.OutCirc);
                _prefireSpriteRenderer.transform.localScale = Vector3.one * (_attackRange * 2f);
            }

            if (_attackWarningEffect)
                _attackWarningEffect.Play();

            if (_animator) _animator.SetTrigger("Attack");

            yield return new WaitForSeconds(_attackWarningTime);

            if (TryFindPlayerHealth(out var playerHealth))
                playerHealth.TakeDamage(_damage);

            if (_prefireSpriteRenderer)
                _prefireSpriteRenderer.color = Color.clear;

            _isAttacking = false;
        }

        public void FinishAnimation()
        {
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
                if (_pathPoints[0] != null && _pathPoints[^1] != null)
                    Gizmos.DrawLine(_pathPoints[0].position, _pathPoints[^1].position);
            }

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _attackRange);
        }
    }
}