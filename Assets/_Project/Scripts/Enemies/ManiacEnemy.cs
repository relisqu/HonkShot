using System.Collections;
using UnityEngine;
using Scripts.Health;
using Scripts.Player;
using Scripts.PointSystem;
using UnityEngine.Serialization;

public class ManiacEnemy : MonoBehaviour
{
    private Transform _target;

    [Header("Movement Settings")] [SerializeField]
    private float _movementSpeed = 5f;

    [SerializeField] private float _avoidanceForce = 2f;
    [SerializeField] private float _raycastDistance = 2f;
    [SerializeField] private float _avoidanceCheckInterval = 0.2f;
    [SerializeField] private LayerMask _obstacleMask;

    [Header("Attack Settings")] [SerializeField]
    private float _attackRange = 1.5f;

    [SerializeField] private float _attackInterval = 1f;
    [SerializeField] private float _attackWarningTime = 0.5f;
    [SerializeField] private int _damage = 1;

    [Header("Visuals")] [Space] [SerializeField]
    private ParticleSystem _attackParticles;

    [SerializeField] private Color _prefireSpriteColor;

    [Header("Components")] [SerializeField]
    private Rigidbody2D _rb;

    [SerializeField] private Animator _animator;

    private bool _isAttacking = false;
    private Coroutine _attackRoutine = null;
    private Coroutine _avoidanceRoutine = null;
    private Vector2 _currentDirection;

    private void OnEnable()
    {
        _target = PointReceiver.Instance.transform;
        _avoidanceRoutine = StartCoroutine(AvoidanceCheck());
        _attackRoutine = StartCoroutine(AttackRoutine());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private IEnumerator AvoidanceCheck()
    {
        while (true)
        {
            if (!_isAttacking && _target != null)
            {
                Vector2 desiredDirection = (_target.position - transform.position).normalized;
                Vector2 avoidance = GetAvoidanceVector(desiredDirection);
                _currentDirection = (desiredDirection + avoidance).normalized;
            }

            yield return new WaitForSeconds(_avoidanceCheckInterval);
        }
    }

    private Vector2 GetAvoidanceVector(Vector2 desiredDirection)
    {
        Vector2 avoidance = Vector2.zero;

        // Main forward check
        if (Physics2D.Raycast(transform.position, desiredDirection, _raycastDistance, _obstacleMask))
        {
            avoidance += GetBestAlternateDirection(desiredDirection) * _avoidanceForce;
        }

        // Perpendicular checks
        CheckPerpendicularDirections(ref avoidance, desiredDirection);

        return avoidance;
    }

    private Vector2 GetBestAlternateDirection(Vector2 originalDirection)
    {
        // Test left and right directions
        float[] angles = { -45f, 45f };
        Vector2 bestDirection = originalDirection;
        float maxDistance = 0;

        foreach (float angle in angles)
        {
            Vector2 dir = Quaternion.Euler(0, 0, angle) * originalDirection;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, _raycastDistance, _obstacleMask);

            if (!hit || hit.distance > maxDistance)
            {
                maxDistance = hit.distance;
                bestDirection = dir;
            }
        }

        return bestDirection.normalized;
    }

    private void CheckPerpendicularDirections(ref Vector2 avoidance, Vector2 desiredDirection)
    {
        Vector2[] perpendicularDirs =
        {
            Vector2.Perpendicular(desiredDirection),
            -Vector2.Perpendicular(desiredDirection)
        };

        foreach (Vector2 dir in perpendicularDirs)
        {
            if (Physics2D.Raycast(transform.position, dir, _raycastDistance * 0.5f, _obstacleMask))
            {
                avoidance += -dir * _avoidanceForce;
            }
        }
    }

    private void FixedUpdate()
    {
        if (_isAttacking || _target == null) return;

        float distanceToTarget = Vector2.Distance(transform.position, _target.position);
        if (distanceToTarget > _attackRange)
        {
            _rb.velocity = _currentDirection * _movementSpeed;
            _animator.SetBool("IsWalking", true);
        }
        else
        {
            _rb.velocity = Vector2.zero;
            _animator.SetBool("IsWalking", false);
        }
    }

    private IEnumerator AttackRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(_attackInterval);
            if (_isAttacking || _target == null) continue;

            float distance = Vector2.Distance(transform.position, _target.position);
            if (distance <= _attackRange)
            {
                _isAttacking = true;
                _rb.velocity = Vector2.zero;
                _animator.SetBool("IsWalking", false);
                _animator.SetTrigger("Attack");

                yield return new WaitForSeconds(_attackWarningTime);

                if (Vector2.Distance(transform.position, _target.position) <= _attackRange &&
                    _target.TryGetComponent(out HealthController playerHealth))
                {
                    PlayParticles();
                    playerHealth.TakeDamage(_damage);
                }

                _isAttacking = false;
            }
        }
    }

    public void PlayParticles()
    {
        _attackParticles.Play();
    }

    public void FinishAnimation()
    {
        _animator.SetBool("IsWalking", true);
        _isAttacking = false;
    }

    private void OnDrawGizmosSelected()
    {
        // Attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _attackRange);

        if (Application.isPlaying && _target != null)
        {
            Vector2 desiredDirection = (_target.position - transform.position).normalized;

            // Draw main desired direction
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, desiredDirection * _raycastDistance);

            // Draw alternate directions
            float angle = 45f;
            DrawDirectionRay(desiredDirection, -angle, Color.cyan); // Left alternate
            DrawDirectionRay(desiredDirection, angle, Color.cyan); // Right alternate

            // Draw perpendicular directions
            DrawPerpendicularRays(desiredDirection, Color.green);

            // Draw final adjusted direction
            Gizmos.color = Color.magenta;
            Gizmos.DrawRay(transform.position, _currentDirection * _raycastDistance);

            // Draw obstacle detection sphere
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(transform.position, 0.2f);
        }
    }

    private void DrawDirectionRay(Vector2 baseDirection, float angle, Color color)
    {
        Vector2 direction = Quaternion.Euler(0, 0, angle) * baseDirection;
        Gizmos.color = color;
        Gizmos.DrawRay(transform.position, direction * _raycastDistance);

        // Add small end marker
        Gizmos.DrawWireSphere(transform.position + (Vector3)(direction * _raycastDistance), 0.1f);
    }

    private void DrawPerpendicularRays(Vector2 baseDirection, Color color)
    {
        Vector2[] perpDirs =
        {
            Vector2.Perpendicular(baseDirection),
            -Vector2.Perpendicular(baseDirection)
        };

        Gizmos.color = color;
        foreach (Vector2 dir in perpDirs)
        {
            Vector3 endPoint = transform.position + (Vector3)(dir * _raycastDistance * 0.5f);
            Gizmos.DrawLine(transform.position, endPoint);

            // Draw perpendicular indicator
            Gizmos.DrawWireCube(endPoint, Vector3.one * 0.15f);
        }
    }
}