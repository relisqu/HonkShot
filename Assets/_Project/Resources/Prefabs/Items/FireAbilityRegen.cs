using Scripts.Items.PlayerItemManager;
using Scripts.Player;
using Scripts.Enemies;
using Scripts.Services.Pooling;
using Scripts.Player.Dash;
using UnityEngine;


namespace Scripts.Items.PermanentItems.CustomItems
{
    public class FireAbilityRegen : Item
    {
        
        private GooseFireSystem _fireSystem;
        private PlayerDashController _playerDashController;
        void Start()
        {
            _fireSystem = GooseFireSystem.Instance;
            if (_fireSystem)
                _fireSystem.UltimateStarted += GooseFireSystem_UltimateStarted;
        }
        private void OnDestroy()
        {
            if (_fireSystem)
                _fireSystem.UltimateStarted -= GooseFireSystem_UltimateStarted;
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
    }
}
