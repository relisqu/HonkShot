using System.Collections.Generic;
using Scripts.Health;
using Scripts.Items.PlayerItemManager;
using Scripts.Items.StatSystems;
using UnityEngine;
using UnityEngine.Serialization;

namespace Scripts.Items.PermanentItems
{
    public class HealthBuffItem : Item
    {
        public List<NumericStatModifier> HealthModifiers;
        private HealthController _healthController;

        void Start()
        {
            _healthController = GetComponentInParent<HealthController>();

            SetBuffs();
        }

        void OnDestroy()
        {
            // RemoveBuffs();
        }

        private void SetBuffs()
        {
            foreach (var statModifier in HealthModifiers)
            {
                _healthController.MaxHpModifierSystem.AddModifier(statModifier);
            }
        }


        public override void InitItem(PlayerItemSO playerItemSO)
        {
            foreach (var numericStatModifier in HealthModifiers)
            {
                numericStatModifier.SetId(playerItemSO.NumericId);
            }
        }
    }
}