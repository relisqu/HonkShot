using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Scripts.Items;
using Scripts.Items.StatSystems;

namespace Scripts.Health
{
    public class HealthController : MonoBehaviour
    {
        [SerializeField] private float _maxHealth;
        [SerializeField] private float _defaultHealth;

        private float _defaultCurrentHealth = 10;

        private BoolStatModifierSystem _invisibilitySystem = new BoolStatModifierSystem();
        private NumericStatModifierSystem _damageTakenModifierSystem = new NumericStatModifierSystem();
        private NumericStatModifierSystem _maxHpModifierSystem = new NumericStatModifierSystem();

        public Action OnTakeDamageTriggered;
        public Action OnDied;
        public Action OnDamaged;


        public bool IsAlive => GetCurrentHealth() > 0;

        public float RemainingHealthPercentage => GetCurrentHealth() / _defaultHealth;
        public NumericStatModifierSystem DamageTakenModifierSystem => _damageTakenModifierSystem;
        public NumericStatModifierSystem MaxHpModifierSystem => _maxHpModifierSystem;
        public BoolStatModifierSystem InvisibilitySystem => _invisibilitySystem;

        public bool IsInvincible()
        {
            return _invisibilitySystem.Calculate(false);
        }

        private void Awake()
        {
            _defaultCurrentHealth = _defaultHealth;
        }

        public float GetCurrentHealth()
        {
            return _defaultCurrentHealth;
        }

        public void TakeDamage(float damageAmount)
        {
            OnTakeDamageTriggered?.Invoke();
            if (_defaultCurrentHealth == 0)
            {
                return;
            }

            if (IsInvincible())
            {
                return;
            }


            float finalDamage = CalculateDamageTaken(damageAmount);
            _defaultCurrentHealth -= finalDamage;

            if (_defaultCurrentHealth < 0)
            {
                _defaultCurrentHealth = 0;
            }

            if (_defaultCurrentHealth == 0)
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
            if (_defaultCurrentHealth >= _maxHealth)
            {
                return;
            }

            _defaultCurrentHealth += amountToAdd;

            if (_defaultCurrentHealth > _maxHealth)
            {
                _defaultCurrentHealth = _maxHealth;
            }
        }

        public void SetInvincible(int invTag, bool isInvincible)
        {
            if (_invisibilitySystem.GetModifier(invTag) != null)
            {
                _invisibilitySystem.UpdateModifier(invTag, isInvincible);
            }
            else
            {
                _invisibilitySystem.AddModifier(new BoolStatModifier(invTag, BoolModType.OverrideIfTrue, isInvincible,
                    _invisibilitySystem.GetLastOrder() + 1));
            }
        }

        public float CalculateDamageTaken(float baseDamage)
        {
            return _damageTakenModifierSystem.Calculate(baseDamage);
        }

        public float GetMaxHealth()
        {
            return _maxHealth;
        }
    }
}