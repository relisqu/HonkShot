using System.Collections;
using Scripts.Health;
using Scripts.Player;
using Scripts.PointSystem;
using UnityEngine;
using Zenject;

namespace Scripts.Enemies.Swamp
{
    public class SwampManiacEnemy : BaseEnemy
    {
        [Inject] private PointReceiver _pointReceiver;

        private Transform _target;

        [Header("Attack Settings")]
        [SerializeField] private float _attackRange = 1.5f;
        [SerializeField] private float _attackInterval = 1f;
        [SerializeField] private float _attackWarningTime = 0.5f;
        [SerializeField] private int _damage = 1;

        [Header("Wall Jump Settings")]
        [SerializeField] private bool _preferWallJumps = true;
        [SerializeField] private float _wallDetectionRange = 3f;
        [SerializeField] private LayerMask _obstacleMask;

        [Header("Visuals")]
        [SerializeField] private ParticleSystem _attackParticles;

        [Header("Components")]
        [SerializeField] private JumpingMovement _jumpingMovement;
        [SerializeField] private EnemyHealth _enemyHealth;
        [SerializeField] private Animator _animator;
        [SerializeField] private Rigidbody2D _rb;

        private bool _isAttacking;
        private Coroutine _jumpRoutine;
        private Coroutine _attackRoutine;

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
        }

        private void OnEnable()
        {
            var pointReceiver = _pointReceiver ?? PointReceiver.Instance;
            if (pointReceiver)
                _target = pointReceiver.transform;

            _jumpRoutine = StartCoroutine(JumpChaseRoutine());
            _attackRoutine = StartCoroutine(AttackRoutine());
        }

        private void OnDisable()
        {
            StopAllCoroutines();
        }

        private IEnumerator JumpChaseRoutine()
        {
            while (_enemyHealth.IsAlive())
            {
                yield return new WaitForSeconds(_jumpingMovement.PauseBetweenJumps);

                if (_isAttacking || !_target) continue;

                float distToPlayer = Vector2.Distance(transform.position, _target.position);
                if (distToPlayer <= _attackRange) continue;

                Vector2 directionToPlayer = ((Vector2)_target.position - (Vector2)transform.position).normalized;

                if (_preferWallJumps)
                {
                    Vector2 wallDir = _jumpingMovement.FindWallDirection(directionToPlayer);
                    if (wallDir != Vector2.zero)
                    {
                        if (_animator) _animator.SetTrigger("Jump");
                        _jumpingMovement.JumpTowards(wallDir);

                        while (_jumpingMovement.IsJumping)
                            yield return null;

                        continue;
                    }
                }

                if (_jumpingMovement.CanJumpTowards(directionToPlayer))
                {
                    if (_animator) _animator.SetTrigger("Jump");
                    _jumpingMovement.JumpTowards(directionToPlayer);

                    while (_jumpingMovement.IsJumping)
                        yield return null;
                }
            }
        }

        private IEnumerator AttackRoutine()
        {
            while (_enemyHealth.IsAlive())
            {
                yield return new WaitForSeconds(_attackInterval);
                if (_isAttacking || !_target) continue;

                float distance = Vector2.Distance(transform.position, _target.position);
                if (distance > _attackRange) continue;

                _isAttacking = true;
                _rb.linearVelocity = Vector2.zero;
                _animator.SetTrigger("Attack");

                yield return new WaitForSeconds(_attackWarningTime);

                if (_target && Vector2.Distance(transform.position, _target.position) <= _attackRange)
                {
                    if (_target.TryGetComponent(out HealthController playerHealth))
                    {
                        PlayParticles();
                        playerHealth.TakeDamage(_damage);
                    }
                }

                _isAttacking = false;
            }
        }

        public void PlayParticles()
        {
            if (_attackParticles)
                _attackParticles.Play();
        }

        public void FinishAnimation()
        {
            _isAttacking = false;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _attackRange);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _wallDetectionRange);
        }
    }
}
