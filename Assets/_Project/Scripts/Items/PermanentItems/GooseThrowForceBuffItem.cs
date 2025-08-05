using System.Collections.Generic;
using Scripts.Health;
using Scripts.Items.PlayerItemManager;
using Scripts.Items.StatSystems;
using Scripts.Player;
using UnityEditor;

namespace Scripts.Items.PermanentItems
{
    public class GooseThrowForceBuffItem : Item
    {
        public List<NumericStatModifier> ThrowModifiers;
        private PlayerBallMovement _ballMovementSystem;

        void Start()
        {
            _ballMovementSystem = GetComponentInParent<PlayerBallMovement>();

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
                _ballMovementSystem.ThrowForceModifierSystem.AddModifier(statModifier);
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