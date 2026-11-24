using System.Collections;
using Scripts.Enemies;
using UnityEngine;

namespace Scripts.Enemies.Swamp
{
    public class SwampHidingShooter : BaseEnemy
    {
        [Header("Shooting")]
        [SerializeField] private ShootingModule _shootingModule;
        [SerializeField] private float _shootInterval = 1.5f;
        [SerializeField] private EnemyHealth _enemyHealth;
        [SerializeField] private Animator _shooterAnimator;

        [Header("State")]
        [SerializeField] private HidingShootingEnemyStateMachine _stateMachine;

        private Coroutine _shootingRoutine;

        private void Awake()
        {
            if (!_enemyHealth)
                _enemyHealth = GetComponent<EnemyHealth>();
            if (!_shootingModule)
                _shootingModule = GetComponentInChildren<ShootingModule>();
            if (!_stateMachine)
                _stateMachine = GetComponent<HidingShootingEnemyStateMachine>();
            if (!_shooterAnimator)
                _shooterAnimator = GetComponent<Animator>();
        }

        private void OnEnable()
        {
            _shootingRoutine = StartCoroutine(ShootingRoutine());
        }

        private void OnDisable()
        {
            if (_shootingRoutine != null)
                StopCoroutine(_shootingRoutine);
        }

        private IEnumerator ShootingRoutine()
        {
            while (_enemyHealth.IsAlive())
            {
                yield return new WaitForSeconds(_shootInterval);

                if (!_stateMachine.IsVisible)
                    continue;

                PlayShootStartAnimation();
                Shoot();
            }
        }

        public void PlayShootStartAnimation()
        {
            if (_shooterAnimator)
                _shooterAnimator.SetTrigger("startShooting");
            if (_shootingModule)
                _shootingModule.NotifyWindUpStarted();
        }

        public void FinishShootingAnimation()
        {
            if (_shooterAnimator)
                _shooterAnimator.SetTrigger("finishShooting");
        }

        // Receiver for the TryShoot animation event in the shared shooter clips. Swamp shooters fire from their
        // own routine instead (their animator is switched off during hide/show), so the event must not shoot again.
        public void TryShoot()
        {
        }

        private void Shoot()
        {
            if (!_stateMachine.IsVisible)
                return;

            _shootingModule.TryShoot(FinishShootingAnimation);
        }
    }
}
