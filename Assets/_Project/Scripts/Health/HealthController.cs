using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Scripts.Items;

namespace Scripts.Health
{
    public class HealthController : MonoBehaviour
    {
        [SerializeField] private float _maxHealth;
        [SerializeField] private float _defaultHealth;

        private bool _isInvincible;
        private float _currentHealth = 10;

        // Stat modifier system for damage taken
        private List<StatModifier> damageTakenModifiers = new List<StatModifier>();

        public Action OnTakeDamageTriggered;
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
            
            OnTakeDamageTriggered?.Invoke();
            
            float finalDamage = CalculateDamageTaken(damageAmount);
            _currentHealth -= finalDamage;

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

        public void AddDamageTakenModifier(StatModifier mod)
        {
            damageTakenModifiers.Add(mod);
            damageTakenModifiers = damageTakenModifiers.OrderBy(m => m.order).ToList();
        }

        public void RemoveDamageTakenModifier(StatModifier mod)
        {
            damageTakenModifiers.Remove(mod);
        }

        public float CalculateDamageTaken(float baseDamage)
        {
            float result = baseDamage;
            foreach (var mod in damageTakenModifiers)
            {
                if (mod.type == StatModType.Add)
                    result += mod.value;
                else if (mod.type == StatModType.Mult)
                    result *= mod.value;
            }
            return result;
        }

        public float GetMaxHealth()
        {
            return _maxHealth;
        }
    }
}