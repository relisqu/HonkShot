using System.Collections.Generic;
using Scripts.Health;
using Scripts.Items.PlayerItemManager;
using Scripts.Items.StatSystems;
using Scripts.Player;
using UnityEngine;
using UnityEngine.Serialization;

namespace Scripts.Items.PermanentItems.CustomItems
{
    public class BlindRageItem : Item
    {
        public List<NumericStatModifier> DamageStatModifiers;

        [FormerlySerializedAs("ForceStatModifiers")]
        public List<NumericStatModifier> DashForceStatModifiers;

        public GameObject GameObject => gameObject;

        void Start()
        {
            var attackController = GetComponentInParent<PlayerAttackController>();
            if (attackController)
            {
                foreach (var damageStatModifier in DamageStatModifiers)
                {
                    damageStatModifier.SetOrder(attackController.DamageBuffModifierSystem.GetLastOrder() + 1);
                    attackController.DamageBuffModifierSystem.AddModifier(damageStatModifier);
                }
            }

            var playerMovement = GetComponentInParent<PlayerBallMovement>();
            if (playerMovement)
            {
                foreach (var dashForceMovement in DashForceStatModifiers)
                {
                    dashForceMovement.SetOrder(playerMovement.ThrowForceModifierSystem.GetLastOrder() + 1);
                    playerMovement.ThrowForceModifierSystem.AddModifier(dashForceMovement);
                }
            }

            Projection.Instance.EnableLine(false);
        }

        public override void InitItem(PlayerItemSO playerItemSO)
        {
            foreach (var numericStatModifier in DamageStatModifiers)
            {
                numericStatModifier.SetId(playerItemSO.Id);
            }

            foreach (var numericStatModifier in DashForceStatModifiers)
            {
                numericStatModifier.SetId(playerItemSO.Id);
            }
        }
    }
}