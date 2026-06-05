using System.Collections;
using UnityEngine;
using Scripts.Health;
using Scripts.Items;
using Scripts.Items.PlayerItemManager;
using Scripts.Items.StatSystems;
using UnityEngine.Serialization;

namespace Scripts.Items.PermanentItems.CustomItems
{
    public class DamageShieldRegenItem : Item
    {
        private ShieldController _shieldController;
        private HealthController _healthController;
        [SerializeField] private float _cooldownSeconds = 1f;
        [SerializeField] private int _shieldsToRestore = 1;

        private bool _isOnCooldown;
        private Coroutine _cooldownCoroutine;
        
        
        
        private void Start()
        {
            if (_shieldController == null)
            {
                _shieldController = GetComponentInParent<ShieldController>();
            }
            if (_healthController == null)
            {
                _healthController = GetComponentInParent<HealthController>();
            }
        }
        private void OnEnable()
        {
            if (_healthController != null)
                _healthController.OnDamageReceived += HandleDamageReceived;
        }

        private void OnDisable()
        {
            if (_healthController != null)
                _healthController.OnDamageReceived -= HandleDamageReceived;

            if (_cooldownCoroutine != null)
                StopCoroutine(_cooldownCoroutine);

            _cooldownCoroutine = null;
            _isOnCooldown = false;
        }
        private void HandleDamageReceived(float damageAmount)
        {
            if (_isOnCooldown)
                return;

            if (_shieldController == null)
                return;

            _shieldController.AddShield(_shieldsToRestore);
            _cooldownCoroutine = StartCoroutine(CooldownRoutine());
        }
        private IEnumerator CooldownRoutine()
        {
            _isOnCooldown = true;
            yield return new WaitForSeconds(_cooldownSeconds);
            _isOnCooldown = false;
            _cooldownCoroutine = null;
        }
        public override void InitItem(PlayerItemSO playerItemSO)
        {
            Debug.Log("[DamageShieldRegen] InitItem");
        }
    }
    
}

