using Scripts.Health;
using Scripts.Items.PlayerItemManager;
using Scripts.Player.Dash;
using UnityEngine;

namespace Scripts.Items.PermanentItems.CustomItems
{
    public class SecondChanceItem : Item, IReviveProvider
    {
        [SerializeField] private int _priority = 0;
        [SerializeField] private float _healthRestorePercent = 0.5f;

        private PlayerDashController _dashController;
        private bool _hasBeenUsed;

        public int Priority => _priority;
        public bool CanRevive => !_hasBeenUsed;

        private void Start()
        {
            _dashController = GetComponentInParent<PlayerDashController>();

            if (ReviveSystem.Instance)
            {
                ReviveSystem.Instance.Register(this);
            }
        }

        private void OnDestroy()
        {
            if (ReviveSystem.Instance)
            {
                ReviveSystem.Instance.Unregister(this);
            }
        }

        public bool TryRevive(HealthController healthController)
        {
            if (_hasBeenUsed) return false;

            _hasBeenUsed = true;

            float healAmount = healthController.GetMaxHealth() * _healthRestorePercent;
            healthController.AddHealth(healAmount);

            if (_dashController)
            {
                _dashController.ResetDashes();
            }

            Debug.Log($"[SecondChanceItem] Revived with {_healthRestorePercent * 100}% health and full dashes!");
            return true;
        }

        public override void InitItem(PlayerItemSO playerItemSO)
        {
        }
    }
}
