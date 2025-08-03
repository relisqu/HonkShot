using Scripts.Health;
using Scripts.LevelSystem.LevelGeneration;
using UnityEngine;

namespace Scripts.Items.PermanentItems
{
    public class VampiricItem : MonoBehaviour
    {
        public float healPercent = 0.1f; // 10% of max health

        private HealthController _health;
        private bool _tookDamageThisLevel = false;

        void Start()
        {
            _health = GetComponentInParent<HealthController>();
            if (_health)
            {
                _health.OnDamaged += OnDamaged;
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
                _health.OnDamaged -= OnDamaged;
            }
            if (LevelManager.Instance)
            {
                LevelManager.Instance.CompletedRoom -= OnLevelComplete;
            }
        }

        private void OnDamaged()
        {
            _tookDamageThisLevel = true;
        }

        private void OnLevelComplete()
        {
            if (_health && !_tookDamageThisLevel)
            {
                float healAmount = _health.GetMaxHealth() * healPercent;
                _health.AddHealth(healAmount);
            }
            _tookDamageThisLevel = false; // Reset for next level
        }
    }
} 