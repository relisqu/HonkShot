using System.Collections;
using Scripts.Items.PlayerItemManager;
using Scripts.Player;
using Scripts.Enemies;
using Scripts.Services.Pooling;
using Scripts.Player.Dash;
using UnityEngine;


namespace Scripts.Items.PermanentItems.CustomItems
{
    public class FireAbilityRegenItem : Item
    {
        
        [SerializeField] private float _cooldownSeconds = 1f;
        
        private GooseFireSystem _fireSystem;
        private PlayerDashController _playerDashController;
        private bool _isOnCooldown;
        private Coroutine _cooldownCoroutine;
        private void OnEnable()
        {
            if (_fireSystem)
                _fireSystem.UltimateStarted += GooseFireSystem_UltimateStarted;
        }

        private void OnDisable()
        {
            if (_fireSystem)
                _fireSystem.UltimateStarted -= GooseFireSystem_UltimateStarted;

            if (_cooldownCoroutine != null)
                StopCoroutine(_cooldownCoroutine);

            _cooldownCoroutine = null;
            _isOnCooldown = false;
        }
        public override void InitItem(PlayerItemSO playerItemSO)
        {
            Debug.Log("[FireAbilityRegen] InitItem");
        }

        public void GooseFireSystem_UltimateStarted()
        {
            //recharge all abilities
            _playerDashController.ResetDashes();
            
        }
        private IEnumerator CooldownRoutine()
        {
            _isOnCooldown = true;
            yield return new WaitForSeconds(_cooldownSeconds);
            _isOnCooldown = false;
            _cooldownCoroutine = null;
        }
    }
}
