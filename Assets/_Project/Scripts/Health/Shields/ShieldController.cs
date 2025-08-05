using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Scripts.Items.StatSystems;
using Sirenix.OdinInspector;

namespace Scripts.Health
{
    [RequireComponent(typeof(HealthController))]
    public class ShieldController : MonoBehaviour
    {
        private List<Shield> _shields = new List<Shield>();

        [Header("Visual Settings")] 
        [SerializeField] private Shield _shieldPrefab;
        [SerializeField] private int _maxShields = 3;
        [SerializeField] private Transform _shieldVisualParent;

        private HealthController _healthController;
        private NumericStatModifierSystem _maxShieldsModifierSystem = new();

        public event Action<Shield> OnShieldDestroyed;
        public event Action<Shield> OnShieldDamaged;
        public event Action<Shield> OnShieldAdded;

        public List<Shield> Shields => _shields;
        public NumericStatModifierSystem MaxShieldsModifierSystem => _maxShieldsModifierSystem;

        private void Awake()
        {
            _healthController = GetComponent<HealthController>();
            _healthController.OnTakeDamageTriggered += OnPlayerTakeDamage;
        }

        public int GetMaxShields()
        {
            return (int)_maxShieldsModifierSystem.Calculate(_maxShields);
        }

        private void OnDestroy()
        {
            if (_healthController != null)
                _healthController.OnTakeDamageTriggered -= OnPlayerTakeDamage;
        }

        private void OnPlayerTakeDamage()
        {
            // This will be called before damage is applied to health
            // We'll handle shield logic in TakeDamage method
        }


        [Button]
        public void AddShield()
        {
            if (_shields.Count >= GetMaxShields())
            {
                Debug.LogWarning("Maximum number of shields reached!");
                return;
            }

            var newShield = Instantiate(_shieldPrefab, _shieldVisualParent);
            
            _shields.Add(newShield);
            OnShieldAdded?.Invoke(newShield);
        }

        public void RemoveShield(Shield shield)
        {
            if (_shields.Remove(shield))
            {
                shield.DestroyShield();
                OnShieldDestroyed?.Invoke(shield);
            }
        }

        public float ProcessDamageThroughShields(float incomingDamage)
        {
            float remainingDamage = incomingDamage;

            for (int i = _shields.Count - 1; i >= 0; i--)
            {
                var shield = _shields[i];

                float absorbedDamage = shield.AbsorbDamage(remainingDamage);
                remainingDamage -= absorbedDamage;

                if (absorbedDamage > 0)
                {
                    OnShieldDamaged?.Invoke(shield);
                    RemoveShield(shield);
                }

                if (remainingDamage <= 0)
                    break;
            }

            return remainingDamage;
        }

        public void ClearAllShields()
        {
            foreach (var shield in _shields.ToArray())
            {
                RemoveShield(shield);
            }
        }
    }
}