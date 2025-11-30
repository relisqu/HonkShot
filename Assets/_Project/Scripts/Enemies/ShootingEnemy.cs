using System.Collections;
using Scripts.Config;
using Scripts.Health;
using Scripts.PointSystem;
using UnityEngine;
using Zenject;

namespace Scripts.Enemies
{
    public class ShooterEnemy : BaseEnemy
    {
        [Inject] private EnemyConfig _enemyConfig;
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
            
            // Apply config settings if available
            ApplyConfigSettings();

            _shootingRoutine = StartCoroutine(ShootingRoutine());
        }
        
        private void ApplyConfigSettings()
        {
            if (_enemyConfig == null) return;
            
            var settings = _enemyConfig.GetSettingsForEnemy(EnemyId);
            _shootInterval = settings.shootInterval;
            
            // Apply health settings
            if (_enemyHealth != null && _enemyHealth.HealthController != null)
            {
                var healthController = _enemyHealth.HealthController;
                var healthType = typeof(HealthController);
                var maxHealthField = healthType.GetField("_maxHealth", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var defaultHealthField = healthType.GetField("_defaultHealth", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (maxHealthField != null)
                    maxHealthField.SetValue(healthController, settings.maxHealth);
                if (defaultHealthField != null)
                    defaultHealthField.SetValue(healthController, settings.defaultHealth);
            }
        }

        private void OnDisable()
        {
            if (_shootingRoutine != null)
                StopCoroutine(_shootingRoutine);
        }

        public void PlayShootStartAnimation()
        {
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