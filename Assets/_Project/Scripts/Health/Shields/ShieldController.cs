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

        /// <summary>Raised when a hit was heavy enough to break a shield. The hit is still absorbed.</summary>
        public event Action<Shield> OnShieldDamaged;

        /// <summary>Raised when a shield stopped a hit and stayed up.</summary>
        public event Action<Shield> OnShieldBlocked;

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

        /// <summary>
        /// Runs an incoming hit through the shield stack, newest shield first, and returns whatever
        /// damage is left for health to take. A single hit is handled by a single shield: it is
        /// absorbed either way, and the shield survives only if the hit was within its threshold.
        /// </summary>
        public float ProcessDamageThroughShields(float incomingDamage)
        {
            if (incomingDamage <= 0)
                return incomingDamage;

            for (int i = _shields.Count - 1; i >= 0; i--)
            {
                var shield = _shields[i];
                if (!shield.IsActive) continue;

                if (shield.CanBlockDamage(incomingDamage))
                {
                    OnShieldBlocked?.Invoke(shield);
                    return 0f;
                }

                // Heavier than this shield can hold. It still eats the hit, but breaks doing so.
                shield.Break();
                OnShieldDamaged?.Invoke(shield);
                RemoveShield(shield);
                return 0f;
            }

            return incomingDamage;
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