using System.Collections.Generic;
using Scripts.Items.PlayerItemManager;
using Scripts.Items.StatSystems;
using Scripts.Player;
using Scripts.Player.Dash;

namespace Scripts.Items
{
    public class GooseThrowCooldownBuffItem : Item
    {
        public List<NumericStatModifier> ThrowModifiers;
        private PlayerDashController _playerDashController;

        void Start()
        {
            _playerDashController = GetComponentInParent<PlayerDashController>();

            SetBuffs();
        }

        void OnDestroy()
        {
            // RemoveBuffs();
        }

        private void SetBuffs()
        {
            foreach (var statModifier in ThrowModifiers)
            {
                _playerDashController.MaxDashModifierSystem.AddModifier(statModifier);
            }
        }


        public override void InitItem(PlayerItemSO playerItemSO)
        {
            foreach (var numericStatModifier in ThrowModifiers)
            {
                numericStatModifier.SetId(playerItemSO.Id);
            }
        }
    }
}