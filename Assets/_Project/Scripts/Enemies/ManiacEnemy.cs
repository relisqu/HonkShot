using System;
using System.Collections;
using UnityEngine;
using Pathfinding;
using Scripts.Health;
using Scripts.Player;
using Scripts.PointSystem;

public class ManiacEnemy : MonoBehaviour
{
    private Transform _target;

    [Header("Movement Settings")] [SerializeField]
    private float _movementSpeed = 200f;

    [SerializeField] private float _nextWaypointDistance = 3f;

    [Header("Attack Settings")] [SerializeField]
    private float _attackRange = 1.5f;

    [SerializeField] private float _attackInterval = 1f;
    [SerializeField] private float _attackWarningTime = 0.5f;
    [SerializeField] private int _damage = 1;
    [Header("Visuals")] [Space]
    [SerializeField] private ParticleSystem _attackParticles;
    [SerializeField] private Color _prefireSpriteColor;

    [Header("Components")] [SerializeField]
    private Seeker _seeker;

    [SerializeField] private Rigidbody2D _rb;
    [SerializeField] private Animator _animator;

    private Path _path;
    private int _currentWaypoint = 0;
    private bool _isAttacking = false;


    private Coroutine _attackRoutine = null;
    private Coroutine _pathRoutine = null;

    private void OnEnable()
    {
        _target = PointReceiver.Instance.transform;
        _pathRoutine = StartCoroutine(UpdatePath());
        _attackRoutine = StartCoroutine(AttackRoutine());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private IEnumerator UpdatePath()
    {
        while (true)
        {
            if ((_seeker.IsDone() && _target != null))
            {
                _seeker.StartPath(_rb.position, _target.position, OnPathComplete);
            }

            yield return new WaitForSeconds(0.3f);
        }
    }

    private void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            _path = p;
            _currentWaypoint = 0;
        }
    }

    private void FixedUpdate()
    {
        if (_path == null || _isAttacking) return;

        float playerDistance = Vector2.Distance(transform.position, _target.position);
        if (playerDistance <= _attackRange * 0.6f)
        {
            return;
        }

        if (_currentWaypoint >= _path.vectorPath.Count)
        {
            return;
        }

        transform.position = Vector2.MoveTowards(transform.position, _target.position,
            _movementSpeed * Time.deltaTime);

        float distance = Vector2.Distance(_rb.position, _path.vectorPath[_currentWaypoint]);
        if (distance < _nextWaypointDistance)
        {
            _currentWaypoint++;
        }

        _animator.SetBool("IsWalking", true);
    }

    private IEnumerator AttackRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(_attackInterval);
            if (_isAttacking || _target == null)
            {
                yield return null;
                continue;
            }

            float distance = Vector2.Distance(transform.position, _target.position);
            if (distance <= _attackRange)
            {
                _isAttacking = true;
                _resetPath = true;
                _animator.SetBool("IsWalking", false);
                _animator.SetTrigger("Attack");

                yield return new WaitForSeconds(_attackWarningTime);
                if (Vector2.Distance(transform.position, _target.position) <= _attackRange &&
                    _target.TryGetComponent(out HealthController playerHealth))
                {
                    PlayParticles();
                    playerHealth.TakeDamage(_damage);
                    _seeker.StartPath(_rb.position, _target.position, OnPathComplete);
                }
            }
        }
    }

    private bool _resetPath;

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
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _attackRange);
    }
}