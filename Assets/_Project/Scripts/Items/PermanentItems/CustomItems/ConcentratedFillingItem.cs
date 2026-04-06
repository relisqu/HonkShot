using Scripts.Items.PlayerItemManager;
using Scripts.Items.StatSystems;
using Scripts.Player;
using UnityEngine;

namespace Scripts.Items.PermanentItems.CustomItems
{
    public class ConcentratedFillingItem : Item
    {
        [SerializeField] private float _radiusMultiplier = 0.5f;
        [SerializeField] private float _damagePercentBonus = 30f;

        private int _modifierId;

        public override void InitItem(PlayerItemSO playerItemSO)
        {
            _modifierId = playerItemSO.NumericId;
        }

        private void Start()
        {
            if (!ExplosionSystem.Instance) return;

            float damageMult = 1f + _damagePercentBonus / 100f;

            ExplosionSystem.Instance.RadiusModifierSystem.AddModifier(
                new NumericStatModifier(_modifierId, NumericModType.Mult, _radiusMultiplier,
                    ExplosionSystem.Instance.RadiusModifierSystem.GetLastOrder() + 1));

            ExplosionSystem.Instance.DamageModifierSystem.AddModifier(
                new NumericStatModifier(_modifierId, NumericModType.Mult, damageMult,
                    ExplosionSystem.Instance.DamageModifierSystem.GetLastOrder() + 1));
        }

        private void OnDestroy()
        {
            if (!ExplosionSystem.Instance) return;

            ExplosionSystem.Instance.RadiusModifierSystem.RemoveModifier(_modifierId);
            ExplosionSystem.Instance.DamageModifierSystem.RemoveModifier(_modifierId);
        }
    }
}
