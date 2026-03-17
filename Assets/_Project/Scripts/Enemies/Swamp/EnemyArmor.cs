using System;
using Scripts.Health;
using UnityEngine;

namespace Scripts.Enemies.Swamp
{
    public class EnemyArmor : MonoBehaviour
    {
        [Header("Armor Settings")]
        [SerializeField] private int _armorHits = 3;
        [SerializeField] private float _damageThreshold = 5f;

        [Header("Components")]
        [SerializeField] private HealthController _healthController;
        [SerializeField] private SpriteRenderer _armorVisual;

        private int _remainingHits;
        private bool _armorDestroyed;

        public bool IsArmorActive => !_armorDestroyed && _remainingHits > 0;
        public int RemainingHits => _remainingHits;
        public event Action OnArmorHit;
        public event Action OnArmorDestroyed;

        private void Awake()
        {
            if (!_healthController)
                _healthController = GetComponent<HealthController>();

            _remainingHits = _armorHits;
        }

        private void Start()
        {
            _healthController.OnTakeDamageTriggered += HealthController_OnTakeDamageTriggered;
            _healthController.OnDamageReceived += HealthController_OnDamageReceived;
        }

        private void OnDestroy()
        {
            if (_healthController)
            {
                _healthController.OnTakeDamageTriggered -= HealthController_OnTakeDamageTriggered;
                _healthController.OnDamageReceived -= HealthController_OnDamageReceived;
            }
        }

        private void HealthController_OnTakeDamageTriggered()
        {
            // Armor intercepts before damage is applied
        }

        private void HealthController_OnDamageReceived(float damage)
        {
            if (_armorDestroyed) return;
            if (damage < _damageThreshold) return;

            _remainingHits--;
            OnArmorHit?.Invoke();

            if (_remainingHits <= 0)
            {
                DestroyArmor();
            }

            UpdateVisual();
        }

        private void DestroyArmor()
        {
            _armorDestroyed = true;

            if (_armorVisual)
                _armorVisual.enabled = false;

            OnArmorDestroyed?.Invoke();
        }

        private void UpdateVisual()
        {
            if (!_armorVisual || _armorDestroyed) return;

            float ratio = (float)_remainingHits / _armorHits;
            Color color = _armorVisual.color;
            color.a = Mathf.Lerp(0.3f, 1f, ratio);
            _armorVisual.color = color;
        }
    }
}
