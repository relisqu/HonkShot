using Scripts.Items.PlayerItemManager;
using Scripts.Player;
using Scripts.Player.Dash;
using Scripts.Services.Pooling;
using UnityEngine;

namespace Scripts.Items.PermanentItems.CustomItems
{
    public class DashExplosionItem : Item
    {
        [SerializeField] private float _baseDamage = 15f;
        [SerializeField] private Explosion _explosionPrefab;
        [SerializeField] private int _poolSize = 3;

        private ComponentPool<Explosion> _explosionPool;
        private PlayerDashController _dashController;

        private void Start()
        {
            _explosionPool = new ComponentPool<Explosion>(_explosionPrefab, _poolSize);

            _dashController = GetComponentInParent<PlayerDashController>();
            if (_dashController)
                _dashController.OnDashUsed += PlayerDashController_OnDashUsed;
        }

        private void OnDestroy()
        {
            if (_dashController)
                _dashController.OnDashUsed -= PlayerDashController_OnDashUsed;
        }

        private void PlayerDashController_OnDashUsed()
        {
            if (!_explosionPrefab) return;

            var explosion = _explosionPool.Get(transform.position, Quaternion.identity);
            explosion.Init(_baseDamage);
        }

        public override void InitItem(PlayerItemSO playerItemSO)
        {
            Debug.Log("[DashExplosionItem] InitItem");
        }
    }
}
