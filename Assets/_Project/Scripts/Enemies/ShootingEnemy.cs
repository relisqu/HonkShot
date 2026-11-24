using System.Collections;
using UnityEngine;

namespace Scripts.Enemies
{
    public class ShooterEnemy : BaseEnemy, IAttackInterruptible
    {
        private static readonly int StartShootingTrigger = Animator.StringToHash("startShooting");
        private static readonly int FinishShootingTrigger = Animator.StringToHash("finishShooting");

        [SerializeField] private ShootingModule _shootingModule;
        [Tooltip("Time between wind-up attempts. A new wind-up waits until the previous shot has finished.")]
        [SerializeField] private float _shootInterval = 1.5f;
        [SerializeField] private EnemyHealth _enemyHealth;
        [SerializeField] private Animator _shooterAnimator;

        [Tooltip("Animator state played when a hit interrupts the wind-up.")]
        [SerializeField] private string _idleStateName = "defaultShoot_idle";
        [Tooltip("Safety net: if a shot never reports finishing, the shooter starts a new cycle after this many seconds.")]
        [SerializeField] private float _shotTimeout = 3f;

        private Coroutine _shootingRoutine;
        private bool _isWindingUp;
        private bool _isShooting;
        private float _shotStartTime;

        public bool IsWindingUp => _isWindingUp;

        private void OnEnable()
        {
            if (!_shootingModule)
                _shootingModule = GetComponentInChildren<ShootingModule>();

            StartShootingRoutine();
        }

        private void OnDisable()
        {
            if (_shootingRoutine != null)
                StopCoroutine(_shootingRoutine);
            _shootingRoutine = null;
            _isWindingUp = false;
            _isShooting = false;
        }

        public void PlayShootStartAnimation()
        {
            _isWindingUp = true;
            _isShooting = true;
            _shotStartTime = Time.time;

            if (_shooterAnimator)
                _shooterAnimator.SetTrigger(StartShootingTrigger);
            if (_shootingModule)
                _shootingModule.NotifyWindUpStarted();
        }

        public void FinishShootingAnimation()
        {
            _isShooting = false;
            if (_shooterAnimator)
                _shooterAnimator.SetTrigger(FinishShootingTrigger);
        }

        // Animation event at the end of the prepare clip.
        public void TryShoot()
        {
            if (!_isWindingUp) return;

            _isWindingUp = false;
            _shootingModule.TryShoot(FinishShootingAnimation);
        }

        public void InterruptAttack()
        {
            if (_shootingRoutine != null)
                StopCoroutine(_shootingRoutine);
            _shootingRoutine = null;
            _isWindingUp = false;
            _isShooting = false;

            EnemyAttackVisuals.ResetToIdle(_shooterAnimator, _idleStateName, StartShootingTrigger, FinishShootingTrigger);
            if (_shootingModule)
                _shootingModule.NotifyWindUpCancelled();
        }

        public void ResumeAfterStun()
        {
            StartShootingRoutine();
        }

        private void StartShootingRoutine()
        {
            _shootingRoutine = StartCoroutine(ShootingRoutine());
        }

        private IEnumerator ShootingRoutine()
        {
            while (_enemyHealth.IsAlive())
            {
                yield return new WaitForSeconds(_shootInterval);

                if (_isShooting && Time.time - _shotStartTime < _shotTimeout)
                    continue;

                PlayShootStartAnimation();
            }
        }
    }
}
