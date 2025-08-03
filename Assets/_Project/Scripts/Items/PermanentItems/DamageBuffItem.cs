using System.Collections.Generic;
using Scripts.Health;
using Scripts.Items.PlayerItemManager;
using Scripts.Items.StatSystems;
using UnityEngine;
using UnityEngine.Serialization;

namespace Scripts.Items.PermanentItems
{
    public class DamageBuffItem : MonoBehaviour, IItem
    {
        [FormerlySerializedAs("NumericStatModifiers")] public List<NumericStatModifier> DamageStatModifiers;

        public GameObject GameObject => gameObject;
        void Start()
        {
            var attack = GetComponentInParent<PlayerAttackController>();
            if (attack == null) return;

            foreach (var numericStatModifier in DamageStatModifiers)
            {
                attack.NumericStatModifierSystem.AddModifier(numericStatModifier);
            }
        }

        public void InitItem(PlayerItemSO playerItemSO)
        {
            foreach (var numericStatModifier in DamageStatModifiers)
            {
                numericStatModifier.SetId(playerItemSO.Id);
            }
        }
    }
}