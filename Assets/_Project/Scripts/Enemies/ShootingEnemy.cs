using System.Collections;
using Scripts.PointSystem;
using UnityEngine;

namespace Scripts.Enemies
{
    public class ShooterEnemy : MonoBehaviour
    {
        [SerializeField] private ShootingModule _shootingModule;
        [SerializeField] private float _shootInterval = 1.5f;
        [SerializeField] private EnemyHealth _enemyHealth;

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

        private IEnumerator ShootingRoutine()
        {
            while (_enemyHealth.IsAlive())
            {
                yield return new WaitForSeconds(_shootInterval);
                Debug.Log("Trying shooting");
                _shootingModule.TryShoot();
            }
        }
    }
}