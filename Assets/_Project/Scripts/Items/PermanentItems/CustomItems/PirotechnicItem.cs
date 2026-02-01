using Scripts.Items.PlayerItemManager;
using Scripts.Items.StatSystems;
using Scripts.Player;
using UnityEngine;

namespace Scripts.Items.PermanentItems.CustomItems
{
    public class PirotechnicItem : Item
    {

        [SerializeField]  private NumericStatModifier _radiusModifier;

        private void Start()
        {
            if (ExplosionSystem.Instance)
                ExplosionSystem.Instance.RadiusModifierSystem.AddModifier(_radiusModifier);
        }

        private void OnDestroy()
        {
            if (ExplosionSystem.Instance && _radiusModifier != null)
                ExplosionSystem.Instance.RadiusModifierSystem.RemoveModifier(_radiusModifier.Id);
        }

        public override void InitItem(PlayerItemSO playerItemSO)
        {
            Debug.Log("[PirotechnicItem] InitItem");
        }
    }
}
