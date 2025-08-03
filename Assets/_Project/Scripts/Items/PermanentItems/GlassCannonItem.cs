using System.Collections.Generic;
using Scripts.Health;
using Scripts.Items.PlayerItemManager;
using Scripts.Items.StatSystems;
using UnityEngine;

namespace Scripts.Items.PermanentItems
{
    public class GlassCannonItem : MonoBehaviour, IItem
    {
        public List<NumericStatModifier> DamageStatModifiers;
        public List<NumericStatModifier> HealthStatModifiers;

        public GameObject GameObject => gameObject;

        void Start()
        {
            var attackController = GetComponentInParent<PlayerAttackController>();
            if (attackController)
            {
                foreach (var damageStatModifier in DamageStatModifiers)
                {
                    damageStatModifier.SetOrder(attackController.NumericStatModifierSystem.GetLastOrder() + 1);
                    attackController.NumericStatModifierSystem.AddModifier(damageStatModifier);
                }
            }

            var health = GetComponentInParent<HealthController>();
            if (health)
            {
                foreach (var healthStatModifier in HealthStatModifiers)
                {
                    healthStatModifier.SetOrder(attackController.NumericStatModifierSystem.GetLastOrder() + 1);
                    health.DamageTakenModifierSystem.AddModifier(healthStatModifier);
                }
            }
        }

        public void InitItem(PlayerItemSO playerItemSO)
        {
            foreach (var numericStatModifier in DamageStatModifiers)
            {
                numericStatModifier.SetId(playerItemSO.Id);
            }

            foreach (var numericStatModifier in HealthStatModifiers)
            {
                numericStatModifier.SetId(playerItemSO.Id);
            }
        }
    }
}