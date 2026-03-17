using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Scripts.Health;
using Scripts.Player;
using Scripts.PointSystem;
using UnityEngine;
using Zenject;

namespace Scripts.Enemies.Swamp
{
    public class SwampWalkingEnemy : BaseEnemy
    {
        [Inject] private PointReceiver _pointReceiver;

        [Header("Path Settings")]
        [SerializeField] private List<Transform> _pathPoints = new();
        [SerializeField] private WalkingType _walkingType = WalkingType.Closed;

        [Header("Attack Settings")]
        [SerializeField] private float _attackRange = 1.5f;
        [SerializeField] private float _attackInterval = 1f;
        [SerializeField] private float _attackWarningTime = 0.5f;
        [SerializeField] private int _damage = 1;

        [Header("Visuals")]
        [SerializeField] private ParticleSystem _attackWarningEffect;
        [SerializeField] private SpriteRenderer _prefireSpriteRenderer;
        [SerializeField] private Color _prefireSpriteColor;

        [Header("Components")]
        [SerializeField] private JumpingMovement _jumpingMovement;
        [SerializeField] private EnemyHealth _enemyHealth;
        [SerializeField] private Animator _animator;
        [SerializeField] private Rigidbody2D _rb;

        private int _currentTargetIndex;
        private int _currentDirection = 1;
        private bool _isAttacking;

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

            StartCoroutine(JumpAlongPath());
            StartCoroutine(AttackRoutine());
        }

        private IEnumerator JumpAlongPath()
        {
            while (_enemyHealth.IsAlive())
            {
                while (_isAttacking)
                    yield return null;

                Transform targetPoint = _pathPoints[_currentTargetIndex];
                Vector2 direction = ((Vector2)targetPoint.position - (Vector2)transform.position).normalized;

                if (_jumpingMovement.CanJumpTowards(direction))
                {
                    _animator.SetTrigger("Jump");
                    _jumpingMovement.JumpToPosition(targetPoint.position);

                    while (_jumpingMovement.IsJumping)
                        yield return null;
                }

                if (Vector2.Distance(transform.position, targetPoint.position) < 0.5f)
                    AdvanceToNextPoint();

                yield return new WaitForSeconds(_jumpingMovement.PauseBetweenJumps);
            }
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

        private IEnumerator AttackRoutine()
        {
            while (_enemyHealth.IsAlive())
            {
                if (_isAttacking) yield return null;
                yield return new WaitForSeconds(_attackInterval);

                var pointReceiver = _pointReceiver ?? PointReceiver.Instance;
                if (pointReceiver == null) continue;

                float distance = Vector2.Distance(transform.position, pointReceiver.transform.position);
                if (distance > _attackRange) continue;

                _isAttacking = true;
                _rb.linearVelocity = Vector2.zero;

                _prefireSpriteRenderer.DOColor(_prefireSpriteColor, _attackWarningTime - 0.1f).SetEase(Ease.OutCirc);
                _prefireSpriteRenderer.transform.localScale = Vector3.one * (_attackRange * 2f);
                if (_attackWarningEffect)
                    _attackWarningEffect.Play();
                _animator.SetTrigger("Attack");

                yield return new WaitForSeconds(_attackWarningTime);

                if (pointReceiver)
                {
                    var dist = Vector2.Distance(transform.position, pointReceiver.transform.position);
                    if (dist <= _attackRange)
                        pointReceiver.GetComponent<HealthController>()?.TakeDamage(_damage);
                }

                _prefireSpriteRenderer.color = Color.clear;
                _isAttacking = false;
            }
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
