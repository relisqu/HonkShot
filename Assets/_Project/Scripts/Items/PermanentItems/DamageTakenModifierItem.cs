using System.Collections.Generic;
using Scripts.Health;
using Scripts.Items.PlayerItemManager;
using Scripts.Items.StatSystems;
using UnityEngine;
using UnityEngine.Serialization;

namespace Scripts.Items.PermanentItems
{
    public class DamageTakenModifierItem : Item
    {
        [FormerlySerializedAs("DamageStatModifiers")] public List<NumericStatModifier> DamageTakenStatModifiers;
        void Start()
        {

            var health = GetComponentInParent<HealthController>();
            if (health)
            {
                foreach (var healthStatModifier in DamageTakenStatModifiers)
                {
                    healthStatModifier.SetOrder(health.DamageTakenModifierSystem.GetLastOrder() + 1);
                    health.DamageTakenModifierSystem.AddModifier(healthStatModifier);
                }
            }
        }

        public override void InitItem(PlayerItemSO playerItemSO)
        {
            foreach (var numericStatModifier in DamageTakenStatModifiers)
            {
                numericStatModifier.SetId(playerItemSO.NumericId);
            }

        }
    }
}