using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Scripts.Enemies.Swamp
{
    public class SwampFlyEnemy : BaseEnemy
    {
        [Header("Path Settings")]
        [SerializeField] private List<Transform> _pathPoints = new();
        [SerializeField] private WalkingType _walkingType = WalkingType.Closed;

        [Header("Flight Settings")]
        [SerializeField] private float _dashSpeed = 6f;
        [SerializeField] private float _arrivalThreshold = 0.15f;
        [SerializeField] private float _accelerationSpeed = 50f;
        [SerializeField] private Vector2 _pauseRange = new Vector2(0.2f, 0.6f);

        [Header("Components")]
        [SerializeField] private Rigidbody2D _rb;
        [SerializeField] private EnemyHealth _enemyHealth;

        private int _currentTargetIndex;
        private int _currentDirection = 1;

        private void Awake()
        {
            if (!_rb)
                _rb = GetComponent<Rigidbody2D>();
            if (!_enemyHealth)
                _enemyHealth = GetComponent<EnemyHealth>();
        }

        private void OnEnable()
        {
            if (_pathPoints.Count == 0)
            {
                Debug.LogError("No path points assigned to SwampFlyEnemy.");
                enabled = false;
                return;
            }

            _pathPoints.RemoveAll(point => point == null);

            StartCoroutine(DashRoutine());
        }

        private IEnumerator DashRoutine()
        {
            while (_enemyHealth.IsAlive())
            {
                Vector2 target = _pathPoints[_currentTargetIndex].position;

                while (_enemyHealth.IsAlive() &&
                       Vector2.Distance(transform.position, target) > _arrivalThreshold)
                {
                    Vector2 desiredVelocity = (target - (Vector2)transform.position).normalized * _dashSpeed;
                    _rb.linearVelocity = Vector2.MoveTowards(
                        _rb.linearVelocity,
                        desiredVelocity,
                        _accelerationSpeed * Time.deltaTime
                    );
                    yield return null;
                }

                _rb.linearVelocity = Vector2.zero;
                AdvanceToNextPoint();

                float pause = Random.Range(_pauseRange.x, _pauseRange.y);
                yield return new WaitForSeconds(pause);
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

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
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
        }
    }
}
