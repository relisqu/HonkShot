using System;
using UnityEngine;

namespace Scripts.Health
{
    public class HealthController : MonoBehaviour
    {
        [SerializeField] private float _maxHealth;
        [SerializeField] private float _defaultHealth;

        private bool _isInvincible;
        private float _currentHealth;


        public Action OnDied;
        public Action OnDamaged;
        public bool IsInvincible => _isInvincible;
        public bool IsAlive => _currentHealth > 0;

        public float RemainingHealthPercentage => _currentHealth / _defaultHealth;

        private void Awake()
        {
            _currentHealth = _defaultHealth;
        }

        public void TakeDamage(float damageAmount)
        {
            if (_currentHealth == 0)
            {
                return;
            }

            if (IsInvincible)
            {
                return;
            }

            _currentHealth -= damageAmount;

            if (_currentHealth < 0)
            {
                _currentHealth = 0;
            }

            if (_currentHealth == 0)
            {
                OnDied?.Invoke();
            }
            else
            {
                OnDamaged?.Invoke();
            }
        }


        public void AddHealth(float amountToAdd)
        {
            if (_currentHealth >= _maxHealth)
            {
                return;
            }

            _currentHealth += amountToAdd;

            if (_currentHealth > _maxHealth)
            {
                _currentHealth = _maxHealth;
            }
        }

        public void SetInvincible(bool isInvincible)
        {
            _isInvincible = isInvincible;
        }
    }
}