 using System;
using UnityEngine;
using Scripts.Items.StatSystems;

namespace Scripts.Health
{
    public class HealthController : MonoBehaviour
    {
        [SerializeField] private float _maxHealth;
        [SerializeField] private float _defaultHealth;
        [SerializeField] private ShieldController _shieldController;
        [SerializeField] private ReviveSystem _reviveSystem;

        private float _defaultCurrentHealth = 10;

        private BoolStatModifierSystem _invisibilitySystem = new BoolStatModifierSystem();
        private NumericStatModifierSystem _damageTakenModifierSystem = new NumericStatModifierSystem();
        private NumericStatModifierSystem _maxHpModifierSystem = new NumericStatModifierSystem();

        public Action OnTakeDamageTriggered;
        public Action<float> OnDamageBlocked;
        public Action<float> OnDamageReceived;
        public Action<HealthController> OnDied;
        public Action<float> OnNonLethalDamageReceived;
        public Action OnRevived;


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
            if (!_shieldController)
                _shieldController = GetComponent<ShieldController>();
            if (!_reviveSystem)
                _reviveSystem = GetComponent<ReviveSystem>();
        }

        public float GetCurrentHealth()
        {
            return _defaultCurrentHealth;
        }

        public float TakeDamage(float damageAmount)
        {
            
            if (_defaultCurrentHealth == 0)
            {
                return 0;
            }

            if (IsInvincible())
            {
                OnDamageBlocked?.Invoke(damageAmount);
                return 0;
            }

            OnTakeDamageTriggered?.Invoke();

            float finalDamage = CalculateDamageTaken(damageAmount);

            if (_shieldController)
            {
                finalDamage = _shieldController.ProcessDamageThroughShields(finalDamage);
            }

            if (finalDamage <= 0 && damageAmount > 0)
            {
                OnDamageBlocked?.Invoke(damageAmount);
            }

            if (finalDamage > 0)
            {
                _defaultCurrentHealth -= finalDamage;

                if (_defaultCurrentHealth < 0)
                {
                    _defaultCurrentHealth = 0;
                }

                OnDamageReceived?.Invoke(finalDamage);

                if (_defaultCurrentHealth == 0)
                {
                    if (_reviveSystem&&  _reviveSystem.TryRevive(this))
                    {
                        OnRevived?.Invoke();
                    }
                    else
                    {
                        OnDied?.Invoke(this);
                    }
                }
                else
                {
                    OnNonLethalDamageReceived?.Invoke(finalDamage);
                }
            }
            return finalDamage;
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

        public void ResetHealthToDefault()
        {
            AddHealth(_maxHealth);
        }
    }
}