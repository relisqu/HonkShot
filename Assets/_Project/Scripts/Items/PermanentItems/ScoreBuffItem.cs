using System.Collections.Generic;
using Scripts.Items.PlayerItemManager;
using Scripts.Items.StatSystems;
using Scripts.ScoreSystem;
using UnityEngine;

namespace Scripts.Items.PermanentItems
{
    public class ScoreBuffItem : Item
    {
        [Tooltip("Score modifiers to apply. Use Add type for flat bonus, Mult type for multiplier.")]
        public List<NumericStatModifier> ScoreStatModifiers;

        public GameObject GameObject => gameObject;

        private void Start()
        {
            if (!ScoreManager.Instance) return;

            foreach (var modifier in ScoreStatModifiers)
            {
                ScoreManager.Instance.ScoreBonusModifierSystem.AddModifier(modifier);
            }
        }

        public override void InitItem(PlayerItemSO playerItemSO)
        {
            foreach (var modifier in ScoreStatModifiers)
            {
                modifier.SetId(playerItemSO.Id);
            }
        }
    }
}
