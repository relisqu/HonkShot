using Scripts.Items.PlayerItemManager;
using Scripts.Player;
using UnityEngine;

namespace Scripts.Items.PermanentItems.CustomItems
{
    public class PassiveFireRegenItem : Item
    {
        [SerializeField] private float _fireAmount = 2f;
        [SerializeField] private float _interval = 1f;
        [SerializeField] private float _fireThreshold = 30f;

        private float _lastRegenTime = -Mathf.Infinity;

        private void Update()
        {
            if (!GooseFireSystem.Instance) return;
            if (GooseFireSystem.Instance.Fire >= _fireThreshold || Time.time - _lastRegenTime < _interval) return;

            _lastRegenTime = Time.time;
            GooseFireSystem.Instance.AddFire(_fireAmount);
        }

        public override void InitItem(PlayerItemSO playerItemSO)
        {
            Debug.Log("[PassiveFireRegenItem] InitItem");
        }
    }
}