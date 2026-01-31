using Scripts.Health;
using Scripts.Items.PlayerItemManager;
using Scripts.Player;
using UnityEngine;

namespace Scripts.Items.PermanentItems.CustomItems
{
    public class DamageToFireConvertorItem : Item
    {
        [SerializeField] private float _conversionCoefficient = 0.5f;

        private HealthController _healthController;

        private void Start()
        {
            _healthController = GetComponentInParent<HealthController>();
            if (_healthController)
                _healthController.OnDamageReceived += HealthController_OnDamageReceived;
        }

        private void OnDestroy()
        {
            if (_healthController)
                _healthController.OnDamageReceived -= HealthController_OnDamageReceived;
        }

        private void HealthController_OnDamageReceived(float damageAmount)
        {
            if (GooseFireSystem.Instance)
                GooseFireSystem.Instance.AddFire(damageAmount * _conversionCoefficient);
        }

        public override void InitItem(PlayerItemSO playerItemSO)
        {
            Debug.Log("[DamageToFireConvertorItem] InitItem");
        }
    }
}
