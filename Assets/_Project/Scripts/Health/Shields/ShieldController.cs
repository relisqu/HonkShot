using System;
using System.Collections.Generic;
using UnityEngine;
using Scripts.Items.StatSystems;
using Sirenix.OdinInspector;

namespace Scripts.Health
{
    [RequireComponent(typeof(HealthController))]
    public class ShieldController : MonoBehaviour
    {
        [SerializeField] private int _defaultShields = 0;
        [SerializeField] private int _maxShields = 3;
        [SerializeField] private float _shieldDamageThreshold = 5f;

        private List<Shield> _shields = new List<Shield>();

        private HealthController _healthController;
        private NumericStatModifierSystem _maxShieldsModifierSystem = new();

        public event Action<Shield> OnShieldDestroyed;
        public event Action<Shield> OnShieldDamaged;
        public event Action<Shield> OnShieldAdded;

        public List<Shield> Shields => _shields;
        public NumericStatModifierSystem MaxShieldsModifierSystem => _maxShieldsModifierSystem;
        public int GetCurrentShieldsCount => _shields.Count;

        private void Awake()
        {
            _healthController = GetComponent<HealthController>();
        }

        private void Start()
        {
            if (_defaultShields > 0 && _shields.Count == 0)
                AddShield(_defaultShields);
        }

        public int GetMaxShields()
        {
            return (int)_maxShieldsModifierSystem.Calculate(_maxShields);
        }

        [Button]
        public void AddShield(int count = 1)
        {
            if (_shields.Count >= GetMaxShields())
            {
                Debug.LogWarning("Maximum number of shields reached!");
                return;
            }

            for (int i = 0; i < count; i++)
            {
                var newShield = new Shield(_shieldDamageThreshold);
                _shields.Add(newShield);
                OnShieldAdded?.Invoke(newShield);
            }
        }

        public void AddIndestructibleShield(int count = 1)
        {
            if (_shields.Count >= GetMaxShields())
            {
                Debug.Log($"Nowhere to put shields!");
                return;
            }

            for (int i = 0; i < count; ++i)
            {
                var newShield = new Shield(Mathf.Infinity);
                _shields.Add(newShield);
                OnShieldAdded?.Invoke(newShield); // should we have a visual thing
                                                  // to tell player they have a
                                                  // mega shield?
            }
        }

        public void RemoveShield(Shield shield)
        {
            if (_shields.Remove(shield))
            {
                Debug.Log("[ShieldController] Removed Shield for " +  transform.name);
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
                    return 0;
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