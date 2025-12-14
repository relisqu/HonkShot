using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Scripts.Health;
using Scripts.PointSystem;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace Scripts.Enemies.Bosses
{
    public class RavageBossEnemy : BaseEnemy
    {
        [SerializeField] private EnemyHealth _minionPrefab;
        [SerializeField] private ShootingModule _shootingModule;
        [SerializeField] private int _minionCount;
        [SerializeField] private float _minionSpeed;

        [FormerlySerializedAs("_minionRangeRotation")] [SerializeField]
        private float _minionRotationRadius;

        [SerializeField] private Transform _minionsParentTransform;
        [SerializeField] private List<Transform> _secondPhasePointsList = new();

        [SerializeField] private HealthController _bossHealth;

        private List<EnemyHealth> _minions = new();
        [Inject] private PointReceiver _pointReceiver;

        private Transform _playerTransform;

        private void Start()
        {
            _playerTransform = _pointReceiver.transform;
            _bossHealth.SetInvincible(0, true);
            _firstPhaseHealth = _minionCount;
            for (int i = 0; i < _minionCount; i++)
            {
                var minionHealth = Instantiate(_minionPrefab, _minionsParentTransform);
                _minions.Add(minionHealth);
                minionHealth.HealthController.OnDied += Minion_Died;
            }

            _bossHealth.OnDamaged += BossHealth_Damaged;
            _bossHealth.OnDied += BossHealth_Died;
            StartCoroutine(RotateMinions());
        }

        private void OnDestroy()
        {
            _bossHealth.OnDamaged -= BossHealth_Damaged;
            _bossHealth.OnDied -= BossHealth_Died;
        }

        private void BossHealth_Damaged()
        {
        }

        private void BossHealth_Died()
        {
            Destroy(gameObject);
        }

        private int _firstPhaseHealth;

        private void Minion_Died()
        {
            _firstPhaseHealth--;

            if (_firstPhaseHealth > 0)
            {
            }
            else
            {
                GoToSecondPhase();
            }
        }

        private int _currentPhase = 0;
        private float angle;
        public float _rotationSpeed;

        private IEnumerator RotateMinions()
        {
            while (_currentPhase == 0)
            {
                angle += _rotationSpeed * Time.deltaTime;
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
                                                    * new Vector3(_minionRotationRadius, _minionRotationRadius, _minionRotationRadius);
                    }
                }

                yield return null;
            }

            yield return null;
        }

        private IEnumerator FirstPhaseMovement()
        {
            while (_currentPhase == 0)
            {
            }

            yield return null;
        }

        private IEnumerator SecondPhaseMovement()
        {
            while (_currentPhase == 1)
            {
            }

            yield return null;
        }


        private void GoToSecondPhase()
        {
            _currentPhase = 1;
            _bossHealth.SetInvincible(0, false);
        }

        private void Finish()
        {
            _currentPhase = 2;
        }
    }
}