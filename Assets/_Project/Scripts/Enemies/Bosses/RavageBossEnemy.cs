using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Scripts.Health;
using Scripts.Player;
using Scripts.PointSystem;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;
using Random = UnityEngine.Random;

namespace Scripts.Enemies.Bosses
{
    public class RavageBossEnemy : BaseEnemy
    {
        [Header("References")]
        [SerializeField] private BossIntro _bossIntro;
        [SerializeField] private RavageBossShootingModule _shootingModule;
        [SerializeField] private ShieldController _shieldController;
        [SerializeField] private Rigidbody2D _rigidbody2D;
        [SerializeField] private BossVFXController _vfxController;

        [SerializeField] private HealthController _bossHealth;
        [Space]
        [Header("Minions in first phase")]
        [SerializeField] private Transform _minionsParentTransform;
        [SerializeField] private EnemyHealth _minionPrefab;
        [SerializeField] private int _minionCount;
        [FormerlySerializedAs("_minionRangeRotation")] [SerializeField]
        private float _minionRotationRadius;
        [SerializeField] private float _minionSpeed;
        public float _rotationSpeed;
        [FormerlySerializedAs("_rotationSpeedIncrease")] public float _rotationSpeedIncreaseAfterMinionDeath;
        public float _minSpaceBetweenPlayerAndBoss;
        [Space]
        [Header("Shooting and movement during first phase ")]
        public int _firstPhaseShieldCount;
        public float _followSpeed;
        public float _shootingCooldown;
        public float _bulletSpeed;
        public float _bulletDamage;
        public float _bulletPrediction;
        [Space]
        [Header("Second phase with dashing and movement")]
        [FormerlySerializedAs("_secondPhasePointsList")] [SerializeField] private List<Transform> _thirdPhasePointsList = new();
        public float _thirdPhaseDashCooldownMin;
        public float _thirdPhaseDashCooldownMax;
        public float _thirdPhaseMoveSpeed;
        public float _thirdPhaseDashSpeed;
        public int _thirdPhaseShieldsCount;




        private List<EnemyHealth> _minions = new();
        [Inject] private PointReceiver _pointReceiver;
        private Transform _playerTransform;

        private void Start()
        {
            _playerTransform = _pointReceiver.transform;
            _bossHealth.SetInvincible(0, true);
            _bossHealth.OnNonLethalDamageReceived += BossHealth_OnNonLethalDamageReceived;
            _bossHealth.OnDied += BossHealth_Died;

            if (_bossIntro)
            {
                _bossIntro.IntroFinished += StartBossAI;
                _bossIntro.Play();
            }
            else
            {
                StartBossAI();
            }
        }

        private void StartBossAI()
        {
            _firstPhaseHealth = _minionCount;
            for (int i = 0; i < _minionCount; i++)
            {
                var minionHealth = Instantiate(_minionPrefab, _minionsParentTransform);
                _minions.Add(minionHealth);
                minionHealth.HealthController.OnDied += Minion_Died;
            }

            StartCoroutine(RotateMinions());
            StartCoroutine(FollowPlayerCoroutine());
            StartCoroutine(ShootingCoroutine());
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (other.gameObject.TryGetComponent(out PlayerGhostProjectile _))
            {
                return;
            }

            if (_bossHealth.IsInvincible()) return;
            if (other.gameObject.TryGetComponent(out PlayerAttackController playerAttackController))
            {
                _bossHealth.TakeDamage(playerAttackController.GetDamage());
                playerAttackController.Damage(gameObject);
            }
        }

        private void OnDestroy()
        {
            _bossHealth.OnNonLethalDamageReceived -= BossHealth_OnNonLethalDamageReceived;
            _bossHealth.OnDied -= BossHealth_Died;
            if (_bossIntro) _bossIntro.IntroFinished -= StartBossAI;
        }

        private void BossHealth_OnNonLethalDamageReceived(float damage)
        {
        }

        private void BossHealth_Died()
        {
            Debug.Log($"[RavageBossEnemy] BossHealth_Died fired on '{gameObject.name}'. Destroying.");
            Destroy(gameObject);
        }

        private int _firstPhaseHealth;

        private void Minion_Died()
        {
            _firstPhaseHealth--;
            Debug.Log($"[RavageBossEnemy] Minion died. Remaining minions: {_firstPhaseHealth}");

            if (_firstPhaseHealth > 0)
            {
                _minionRotationSpeed += _rotationSpeedIncreaseAfterMinionDeath;
            }
            else
            {
                GoToSecondPhase();
            }
        }

        private int _currentPhase = 0;
        private float angle;
        private float _minionRotationSpeed;

        public float _thirdPhaseShootingSpeed;
        public float _thirdPhaseBulletDamage;
        public float _thirdPhaseShootingCooldown;

        [Header("Third Phase Attack Pattern")]
        [Tooltip("Pattern of attacks: Single=0, Spread=1. Example: [0,0,0,1] = 3 single, 1 spread")]
        public BossAttackType[] _thirdPhaseAttackPattern = { BossAttackType.Single, BossAttackType.Single, BossAttackType.Single, BossAttackType.Spread };
        public float _thirdPhasePatternCycleDelay = 1f;
        private int _attackPatternIndex;
        private IEnumerator FollowPlayerCoroutine()
        {
            while (_currentPhase < 2)
            {
                if (Vector2.Distance(transform.position, _playerTransform.position) > _minSpaceBetweenPlayerAndBoss)
                {
                    var follow = _playerTransform.position - transform.position;
                    _rigidbody2D.linearVelocity = follow * (_followSpeed * Time.deltaTime);
                }
                else
                {
                    _rigidbody2D.linearVelocity = Vector2.zero;
                }

                yield return null;
            }
        }

        private IEnumerator ShootingCoroutine()
        {
            _shootingModule.SetParameters(_bulletSpeed, _bulletDamage,_bulletPrediction);
            while (_currentPhase < 2)
            {
                yield return new WaitForSeconds(_shootingCooldown);
                _shootingModule.TryShoot(() => { });
            }
        }

        private IEnumerator LastPhaseShootingCoroutine()
        {
            _shootingModule.SetParameters(_thirdPhaseShootingSpeed, _thirdPhaseBulletDamage);
            _attackPatternIndex = 0;

            while (true)
            {
                yield return new WaitForSeconds(_thirdPhaseShootingCooldown);

                if (_thirdPhaseAttackPattern.Length > 0)
                {
                    _shootingModule.SetAttackType(_thirdPhaseAttackPattern[_attackPatternIndex]);
                    _attackPatternIndex++;

                    if (_attackPatternIndex >= _thirdPhaseAttackPattern.Length)
                    {
                        _attackPatternIndex = 0;
                        _shootingModule.TryShoot(() => { });
                        yield return new WaitForSeconds(_thirdPhasePatternCycleDelay);
                        continue;
                    }
                }

                _shootingModule.TryShoot(() => { });
            }
        }

        private IEnumerator RotateMinions()
        {
            _minionRotationSpeed = _rotationSpeed;
            while (_currentPhase == 0)
            {
                angle += _minionRotationSpeed * Time.deltaTime;
                for (var index = 0; index < _minions.Count; index++)
                {
                    var minion = _minions[index];
                    if (minion)
                    {
                        minion.transform.position = _minionsParentTransform.transform.position +
                                                    Quaternion.AngleAxis(
                                                        angle + index * 1f / _minionCount * 360f,
                                                        _minionsParentTransform.transform.forward
                                                    )
                                                    * new Vector3(_minionRotationRadius, _minionRotationRadius,
                                                        _minionRotationRadius);
                    }
                }

                yield return null;
            }

            yield return null;
        }


        private void GoToSecondPhase()
        {
            Debug.Log($"[RavageBossEnemy] -> Phase 2 (vulnerable, {_firstPhaseShieldCount} shields)");
            _bossHealth.SetInvincible(0, false);
            _currentPhase = 1;
            _shieldController.AddShield(_firstPhaseShieldCount);
            _shieldController.OnShieldDestroyed += ShieldController_ShieldDestroyed;
        }

        private void ShieldController_ShieldDestroyed(Shield obj)
        {
            if (_shieldController.GetCurrentShieldsCount == 0)
            {
                GoToThirdPhase();
            }
        }


        private void GoToThirdPhase()
        {
            if (_currentPhase == 2) return;

            Debug.Log($"[RavageBossEnemy] -> Phase 3 (dashing, {_thirdPhaseShieldsCount} shields)");
            _currentPhase = 2;
            _shieldController.AddShield(_thirdPhaseShieldsCount);

            if (_thirdPhaseMovementCoroutine != null)
                StopCoroutine(_thirdPhaseMovementCoroutine);
            if (_lastPhaseShootingCoroutine != null)
                StopCoroutine(_lastPhaseShootingCoroutine);

            _thirdPhaseMovementCoroutine = StartCoroutine(ThirdPhaseMovement());
            _lastPhaseShootingCoroutine = StartCoroutine(LastPhaseShootingCoroutine());
        }

        private int currentPointIndex;

        private Coroutine _thirdPhaseMovementCoroutine;
        private Coroutine _lastPhaseShootingCoroutine;

        private int _bezierStartIndex;

        private Vector2 _bezierA;
        private Vector2 _bezierB;
        private Vector2 _bezierC;
        private Vector2 _bezierD;
        private float _bezierT;
        private float _bezierLength;
        private bool _bezierActive;

        private Vector2 EvaluateCubicBezier(Vector2 a, Vector2 b, Vector2 c, Vector2 d, float t)
        {
            var u = 1f - t;
            return u * u * u * a + 3f * u * u * t * b + 3f * u * t * t * c + t * t * t * d;
        }

        private float EstimateCubicBezierLength(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
        {
            var length = 0f;
            var prev = a;
            const int steps = 12;

            for (int i = 1; i <= steps; i++)
            {
                var t = i / (float)steps;
                var p = EvaluateCubicBezier(a, b, c, d, t);
                length += Vector2.Distance(prev, p);
                prev = p;
            }

            return Mathf.Max(length, 0.0001f);
        }

        private void GetCubicBezierSegment(int startIndex, out Vector2 a, out Vector2 b, out Vector2 c, out Vector2 d)
        {
            var n = _thirdPhasePointsList.Count;
            var i0 = (startIndex - 1 + n) % n;
            var i1 = startIndex;
            var i2 = (startIndex + 1) % n;
            var i3 = (startIndex + 2) % n;

            var p0 = (Vector2)_thirdPhasePointsList[i0].position;
            var p1 = (Vector2)_thirdPhasePointsList[i1].position;
            var p2 = (Vector2)_thirdPhasePointsList[i2].position;
            var p3 = (Vector2)_thirdPhasePointsList[i3].position;

            a = p1;
            b = p1 + (p2 - p0) / 6f;
            c = p2 - (p3 - p1) / 6f;
            d = p2;
        }

        private void SetupThirdPhaseBezier()
        {
            GetCubicBezierSegment(_bezierStartIndex, out _bezierA, out _bezierB, out _bezierC, out _bezierD);
            _bezierLength = EstimateCubicBezierLength(_bezierA, _bezierB, _bezierC, _bezierD);
            _bezierT = 0f;
            _bezierActive = true;
        }

        private IEnumerator MoveToPoint(Transform target, float speed)
        {
            while (Vector2.Distance(_rigidbody2D.position, target.position) > 0.2f)
            {
                var direction = (Vector2)(target.position - transform.position);
                _rigidbody2D.linearVelocity = direction.normalized * speed;
                yield return null;
            }

            _rigidbody2D.linearVelocity = Vector2.zero;
        }

        public IEnumerator ThirdPhaseMovement()
        {
            _bossHealth.SetInvincible(0, true);

            var minPointIndex = 0;
            for (var index = 0; index < _thirdPhasePointsList.Count; index++)
            {
                var distance = _thirdPhasePointsList[index].position - transform.position;
                var minPointDistance = _thirdPhasePointsList[minPointIndex].position - transform.position;
                if (distance.magnitude < minPointDistance.magnitude)
                    minPointIndex = index;
            }

            currentPointIndex = minPointIndex;

            float dashTime = Time.time;
            float currentDashCooldown = Random.Range(_thirdPhaseDashCooldownMin, _thirdPhaseDashCooldownMax);

            while (_currentPhase == 2)
            {
                var currentTime = Time.time;

                if (currentTime - dashTime > currentDashCooldown)
                {
                    _bezierActive = false;

                    yield return MoveToPoint(_thirdPhasePointsList[currentPointIndex], _thirdPhaseMoveSpeed);
                    _rigidbody2D.position = _thirdPhasePointsList[currentPointIndex].position;
                    _rigidbody2D.linearVelocity = Vector2.zero;

                    yield return new WaitForSeconds(1f);

                    var dashPoint = Random.Range(0, _thirdPhasePointsList.Count);
                    while (Mathf.Abs(dashPoint - currentPointIndex) < 2)
                        dashPoint = Random.Range(0, _thirdPhasePointsList.Count);

                    if (_vfxController) _vfxController.SetDashTrailActive(true);
                    yield return MoveToPoint(_thirdPhasePointsList[dashPoint], _thirdPhaseDashSpeed);
                    _rigidbody2D.position = _thirdPhasePointsList[dashPoint].position;
                    _rigidbody2D.linearVelocity = Vector2.zero;

                    yield return new WaitForSeconds(1.5f);
                    if (_vfxController) _vfxController.SetDashTrailActive(false);

                    _rigidbody2D.position = _thirdPhasePointsList[dashPoint].position;
                    _bezierStartIndex = dashPoint;
                    currentPointIndex = (_bezierStartIndex + 1) % _thirdPhasePointsList.Count;
                    SetupThirdPhaseBezier();

                    dashTime = Time.time;
                    currentDashCooldown = Random.Range(_thirdPhaseDashCooldownMin, _thirdPhaseDashCooldownMax);
                }
                else
                {
                    if (!_bezierActive)
                    {
                        var point = _thirdPhasePointsList[currentPointIndex];
                        if (Vector2.Distance(_rigidbody2D.position, point.position) > 0.2f)
                        {
                            var direction = (Vector2)(point.position - transform.position);
                            _rigidbody2D.linearVelocity = direction.normalized * _thirdPhaseMoveSpeed;
                        }
                        else
                        {
                            _rigidbody2D.linearVelocity = Vector2.zero;
                            _rigidbody2D.position = point.position;
                            _bossHealth.SetInvincible(0, false);

                            _bezierStartIndex = currentPointIndex;
                            currentPointIndex = (_bezierStartIndex + 1) % _thirdPhasePointsList.Count;
                            SetupThirdPhaseBezier();
                        }
                    }
                    else
                    {
                        _bezierT += (_thirdPhaseMoveSpeed * Time.deltaTime) / _bezierLength;

                        if (_bezierT >= 1f)
                        {
                            _bezierT = 1f;
                            _rigidbody2D.linearVelocity = Vector2.zero;
                            _rigidbody2D.position = _bezierD;

                            _bezierStartIndex = currentPointIndex;
                            currentPointIndex = (_bezierStartIndex + 1) % _thirdPhasePointsList.Count;
                            SetupThirdPhaseBezier();
                        }
                        else
                        {
                            _rigidbody2D.linearVelocity = Vector2.zero;
                            _rigidbody2D.position = EvaluateCubicBezier(_bezierA, _bezierB, _bezierC, _bezierD, _bezierT);
                        }
                    }
                }

                yield return null;
            }

            yield return null;
        }
    }
}