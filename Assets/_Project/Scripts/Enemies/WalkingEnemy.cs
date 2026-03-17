using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Scripts.Health;
using Scripts.Player;
using Scripts.PointSystem;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace Scripts.Enemies
{
    public class WalkingEnemy : BaseEnemy
    {
        [Inject] private PointReceiver _pointReceiver;

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

        private void OnEnable()
        {
            if (_pathPoints.Count == 0)
            {
                Debug.LogError("No path points assigned.");
                enabled = false;
                return;
            }

            _pathPoints.RemoveAll(point => point == null);

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

                    Vector2 targetVelocity = ((Vector2)targetPoint.position - (Vector2)transform.position).normalized * _moveSpeed;
                    _rigidbody2D.linearVelocity = Vector2.MoveTowards(_rigidbody2D.linearVelocity, targetVelocity, _moveSpeed * Time.deltaTime);
                    yield return null;
                }

                switch (_walkingType)
                {
                    case WalkingType.YoYo:

                        if (_currentTargetIndex == 0)
                        {
                            _currentDirection = 1;
                        }
                        else if (_currentTargetIndex >= _pathPoints.Count - 1)
                        {
                            _currentDirection = -1;
                        }

                        _currentTargetIndex = (_currentTargetIndex + 1 * _currentDirection) % _pathPoints.Count;
                        break;
                    case WalkingType.Closed:
                        _currentTargetIndex = (_currentTargetIndex + 1) % _pathPoints.Count;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private int _currentDirection = 1;

        private IEnumerator AttackRoutine()
        {
            while (_enemyHealth.IsAlive())
            {
                if (_isAttacking) yield return null;
                yield return new WaitForSeconds(_attackInterval);

                _rigidbody2D.linearVelocity = Vector2.zero;
                _animator.SetBool("IsWalking", false);
                _isAttacking = true;

                _prefireSpriteRenderer.DOColor(_prefireSpriteColor, _attackWarningTime - 0.1f).SetEase(Ease.OutCirc);
                _prefireSpriteRenderer.transform.localScale = Vector3.one * (_attackRange * 2f);
                _attackWarningEffect.Play();
                _animator.SetTrigger("Attack");
                yield return new WaitForSeconds(_attackWarningTime);

                var pointReceiver =
                    _pointReceiver ?? PointReceiver.Instance; // Fallback to Instance if injection failed
                if (pointReceiver != null)
                {
                    var distance = Vector2.Distance(transform.position, pointReceiver.transform.position);
                    if (distance <= _attackRange)
                    {
                        pointReceiver.GetComponent<HealthController>()?.TakeDamage(_damage);
                    }
                }

                _prefireSpriteRenderer.color = Color.clear;
            }
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