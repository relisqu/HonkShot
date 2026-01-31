using Scripts.Health;
using Scripts.Items.PlayerItemManager;
using Scripts.Player;
using UnityEngine;

namespace Scripts.Items.PermanentItems.CustomItems
{
    public class HonkHpConverterItem : Item
    {
        [SerializeField] private float _conversionCoefficient = 0.5f;
        [SerializeField] private float _cooldown = 5f;

        private HealthController _healthController;
        private float _lastConvertTime = -Mathf.Infinity;

        private void Start()
        {
            _healthController = GetComponentInParent<HealthController>();
            if (_healthController)
                _healthController.OnDamageBlocked += HealthController_OnDamageBlocked;
        }

        private void OnDestroy()
        {
            if (_healthController)
                _healthController.OnDamageBlocked -= HealthController_OnDamageBlocked;
        }

        private void HealthController_OnDamageBlocked(float damageAmount)
        {
            if (!GooseFireSystem.Instance || !GooseFireSystem.Instance.IsHonk)
                return;

            if (Time.time - _lastConvertTime < _cooldown)
                return;

            _lastConvertTime = Time.time;
            _healthController.AddHealth(damageAmount * _conversionCoefficient);
        }

        public override void InitItem(PlayerItemSO playerItemSO)
        {
        }
    }
}
