using System.Collections;
using UnityEngine;

namespace Scripts.Enemies.Swamp
{
    public class SwampFlyEnemy : BaseEnemy
    {
        [Header("Random Flight Settings")]
        [SerializeField] private float _maxDistanceFromStart = 4f;
        [SerializeField] private float _dashDistance = 2f;
        [SerializeField] private float _dashSpeed = 6f;
        [SerializeField] private float _arrivalThreshold = 0.15f;
        [SerializeField] private float _accelerationSpeed = 50f;
        [SerializeField] private Vector2 _pauseRange = new Vector2(0.2f, 0.6f);

        [Header("Components")]
        [SerializeField] private Rigidbody2D _rb;
        [SerializeField] private EnemyHealth _enemyHealth;

        private Vector2 _startPosition;

        private void Awake()
        {
            if (!_rb)
                _rb = GetComponent<Rigidbody2D>();
            if (!_enemyHealth)
                _enemyHealth = GetComponent<EnemyHealth>();

            _startPosition = transform.position;
        }

        private void OnEnable()
        {
            StartCoroutine(RandomDashRoutine());
        }

        private IEnumerator RandomDashRoutine()
        {
            while (_enemyHealth.IsAlive())
            {
                Vector2 target = PickRandomTarget();

                while (_enemyHealth.IsAlive() &&
                       Vector2.Distance(transform.position, target) > _arrivalThreshold)
                {
                    Vector2 desiredVelocity = (target - (Vector2)transform.position).normalized * _dashSpeed;
                    _rb.linearVelocity = Vector2.MoveTowards(
                        _rb.linearVelocity,
                        desiredVelocity,
                        _accelerationSpeed* Time.deltaTime
                    );
                    yield return null;
                }

                _rb.linearVelocity = Vector2.zero;

                float pause = Random.Range(_pauseRange.x, _pauseRange.y);
                yield return new WaitForSeconds(pause);
            }
        }

        private Vector2 PickRandomTarget()
        {
            Vector2 currentPos = transform.position;

            for (int i = 0; i < 15; i++)
            {
                Vector2 dir = Random.insideUnitCircle.normalized;
                Vector2 candidate = currentPos + dir * _dashDistance;

                if (Vector2.Distance(candidate, _startPosition) <= _maxDistanceFromStart)
                    return candidate;
            }

            // Fallback: aim back toward the spawn point so we never drift outside the bound.
            Vector2 toStart = (_startPosition - currentPos).sqrMagnitude > 1e-4f
                ? (_startPosition - currentPos).normalized
                : Vector2.right;
            return currentPos + toStart * _dashDistance;
        }

        private void OnDrawGizmosSelected()
        {
            Vector3 center = Application.isPlaying ? (Vector3)_startPosition : transform.position;

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(center, _maxDistanceFromStart);

            Gizmos.color = new Color(0f, 1f, 1f, 0.35f);
            Gizmos.DrawWireSphere(transform.position, _dashDistance);
        }
    }
}
