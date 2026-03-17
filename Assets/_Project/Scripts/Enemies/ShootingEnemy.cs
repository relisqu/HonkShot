using System.Collections;
using Scripts.Health;
using UnityEngine;

namespace Scripts.Enemies
{
    public class ShooterEnemy : BaseEnemy
    {
        [SerializeField] private ShootingModule _shootingModule;
        [SerializeField] private float _shootInterval = 1.5f;
        [SerializeField] private EnemyHealth _enemyHealth;

        [SerializeField] private Animator _shooterAnimator;
        private Coroutine _shootingRoutine;

        private void OnEnable()
        {
            if (_shootingModule == null)
            {
                _shootingModule = GetComponentInChildren<ShootingModule>();
            }

            _shootingRoutine = StartCoroutine(ShootingRoutine());
        }

        private void OnDisable()
        {
            if (_shootingRoutine != null)
                StopCoroutine(_shootingRoutine);
        }

        public void PlayShootStartAnimation()
        {
            if (_shooterAnimator)
                _shooterAnimator.SetTrigger("startShooting");
        }

        public void FinishShootingAnimation()
        {
            if (_shooterAnimator)
                _shooterAnimator.SetTrigger("finishShooting");
        }

        public void TryShoot()
        {
            _shootingModule.TryShoot(FinishShootingAnimation);
        }

        private IEnumerator ShootingRoutine()
        {
            while (_enemyHealth.IsAlive())
            {
                yield return new WaitForSeconds(_shootInterval);
                Debug.Log("Trying shooting");
                PlayShootStartAnimation();
            }
        }
    }
}