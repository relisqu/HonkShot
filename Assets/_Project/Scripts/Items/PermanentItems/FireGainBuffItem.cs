using System.Collections.Generic;
using Scripts.Items.PlayerItemManager;
using Scripts.Items.StatSystems;
using Scripts.Player;
using UnityEngine;

namespace Scripts.Items.PermanentItems
{
    public class FireGainBuffItem : Item
    {
        public List<NumericStatModifier> FireGainModifiers;

        private void Start()
        {
            var fireSystem = GetComponentInParent<GooseFireSystem>();
            if (!fireSystem) return;

            foreach (var modifier in FireGainModifiers)
            {
                fireSystem.FireGainModifierSystem.AddModifier(modifier);
            }
        }

        public override void InitItem(PlayerItemSO playerItemSO)
        {
            foreach (var modifier in FireGainModifiers)
            {
                modifier.SetId(playerItemSO.NumericId);
            }
        }
    }
}
