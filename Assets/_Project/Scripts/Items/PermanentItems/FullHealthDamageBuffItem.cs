using System.Collections.Generic;
using Scripts.Health;
using Scripts.Items.PlayerItemManager;
using Scripts.Items.StatSystems;
using UnityEngine;
using UnityEngine.Serialization;

namespace Scripts.Items.PermanentItems
{
    public class FullHealthDamageBuffItem : Item
    {
        [FormerlySerializedAs("NumericalStatModifiers")] public List<NumericStatModifier> DamageStatModifiers;

        private PlayerAttackController _attackController;
        private HealthController _healthController;
        public GameObject GameObject => gameObject;

        void Start()
        {
            _attackController = GetComponentInParent<PlayerAttackController>();
            _healthController = GetComponentInParent<HealthController>();
            if (_healthController != null)
            {
                _healthController.OnTakeDamageTriggered += CheckBuff;
            }

            CheckBuff();
        }

        void OnDestroy()
        {
            if (_healthController != null)
            {
                _healthController.OnTakeDamageTriggered -= CheckBuff;
            }

            RemoveBuffs();
        }

        private void CheckBuff()
        {
            if (_healthController && _attackController)
            {
                if (_healthController.RemainingHealthPercentage >= 1f)
                {
                    foreach (var statModifier in DamageStatModifiers)
                    {
                        if (_attackController.DamageBuffModifierSystem.GetModifier(statModifier.Id) == null)
                        {
                            statModifier.SetOrder(_attackController.DamageBuffModifierSystem.GetLastOrder());
                            _attackController.DamageBuffModifierSystem.AddModifier(statModifier);
                        }
                    }
                }
                else
                {
                    RemoveBuffs();
                }
            }
        }

        private void AddBuffs()
        {
            foreach (var statModifier in DamageStatModifiers)
            {
                if (_attackController.DamageBuffModifierSystem.GetModifier(statModifier.Id) == null)
                {
                    statModifier.SetOrder(_attackController.DamageBuffModifierSystem.GetLastOrder());
                    _attackController.DamageBuffModifierSystem.AddModifier(statModifier);
                }
            }
        }

        private void RemoveBuffs()
        {
            foreach (var statModifier in DamageStatModifiers)
            {
                _attackController.DamageBuffModifierSystem.RemoveModifier(statModifier.Id);
            }
        }

        public override void InitItem(PlayerItemSO playerItemSO)
        {
            foreach (var numericStatModifier in DamageStatModifiers)
            {
                numericStatModifier.SetId(playerItemSO.Id);
            }
        }
    }
}