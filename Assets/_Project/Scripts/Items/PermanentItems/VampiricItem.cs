using Scripts.Health;
using Scripts.Items.PlayerItemManager;
using Scripts.LevelSystem.LevelGeneration;
using UnityEngine;

namespace Scripts.Items.PermanentItems
{
    public class VampiricItem : Item
    {
        [Tooltip("The percentage of the health of healed before adding healing amount")]
        public float healPercentAmount = 0.1f; 
        private HealthController _health;
        private bool _tookDamageThisLevel = false;

        void Start()
        {
            _health = GetComponentInParent<HealthController>();
            if (_health)
            {
                _health.OnNonLethalDamageReceived += HealthController_OnNonLethalDamageReceived;
            }
            if (LevelManager.Instance)
            {
                LevelManager.Instance.CompletedRoom += OnLevelComplete;
            }
        }

        void OnDestroy()
        {
            if (_health)
            {
                _health.OnNonLethalDamageReceived -= HealthController_OnNonLethalDamageReceived;
            }
            if (LevelManager.Instance)
            {
                LevelManager.Instance.CompletedRoom -= OnLevelComplete;
            }
        }

        private void HealthController_OnNonLethalDamageReceived(float damage)
        {
            _tookDamageThisLevel = true;
        }

        private void OnLevelComplete()
        {
            if (_health && !_tookDamageThisLevel)
            {
                float healAmount = _health.GetMaxHealth() * healPercentAmount;
                _health.AddHealth(healAmount);
            }
            _tookDamageThisLevel = false; // Reset for next level
        }

        public override void InitItem(PlayerItemSO playerItemSO)
        {
        }
    }
} 