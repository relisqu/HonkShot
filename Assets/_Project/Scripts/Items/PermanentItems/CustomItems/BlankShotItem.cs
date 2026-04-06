using Scripts.Items.PlayerItemManager;
using Scripts.Player;
using Scripts.Player.Dash;
using Scripts.Player.InputHandling;
using Scripts.Services.Pooling;
using UnityEngine;

namespace Scripts.Items.PermanentItems.CustomItems
{
    public class BlankShotItem : Item
    {
        [SerializeField] private float _baseDamage = 10f;
        [SerializeField] private Explosion _explosionPrefab;
        [SerializeField] private int _poolSize = 3;

        private ComponentPool<Explosion> _explosionPool;
        private InputHandler _inputHandler;
        private PlayerDashController _dashController;

        private void Start()
        {
            _explosionPool = new ComponentPool<Explosion>(_explosionPrefab, _poolSize);

            _inputHandler = GetComponentInParent<InputHandler>();
            _dashController = GetComponentInParent<PlayerDashController>();

            if (_inputHandler)
                _inputHandler.OnDragFinished += InputHandler_OnDragFinished;
        }

        private void OnDestroy()
        {
            if (_inputHandler)
                _inputHandler.OnDragFinished -= InputHandler_OnDragFinished;
        }

        private void InputHandler_OnDragFinished(Vector2 force)
        {
            if (!_dashController || !_explosionPrefab) return;
            if (_dashController.CanDash) return;

            var explosion = _explosionPool.Get(transform.position, Quaternion.identity);
            explosion.Init(_baseDamage);
        }

        public override void InitItem(PlayerItemSO playerItemSO)
        {
            Debug.Log("[BlankShotItem] InitItem");
        }
    }
}
