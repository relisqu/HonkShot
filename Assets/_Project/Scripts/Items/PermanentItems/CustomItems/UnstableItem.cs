using Scripts.Items.PlayerItemManager;
using Scripts.Player;
using Scripts.Services.Pooling;
using UnityEngine;

namespace Scripts.Items.PermanentItems.CustomItems
{
    public class UnstableItem : Item
    {
        [SerializeField] private float _baseDamage = 8f;
        [SerializeField] private float _explosionChance = 0.3f;
        [SerializeField] private Explosion _explosionPrefab;
        [SerializeField] private int _poolSize = 5;

        private ComponentPool<Explosion> _explosionPool;
        private PlayerBallMovement _ballMovement;

        private void Start()
        {
            _explosionPool = new ComponentPool<Explosion>(_explosionPrefab, _poolSize);

            _ballMovement = GetComponentInParent<PlayerBallMovement>();
            if (_ballMovement)
                _ballMovement.OnBounced += PlayerBallMovement_OnBounced;
        }

        private void OnDestroy()
        {
            if (_ballMovement)
                _ballMovement.OnBounced -= PlayerBallMovement_OnBounced;
        }

        private void PlayerBallMovement_OnBounced(Collision2D collision)
        {
            if (Random.value > _explosionChance) return;
            if (!_explosionPrefab) return;

            Vector2 contactPoint = collision.GetContact(0).point;
            var explosion = _explosionPool.Get(contactPoint, Quaternion.identity);
            explosion.Init(_baseDamage);
        }

        public override void InitItem(PlayerItemSO playerItemSO)
        {
            Debug.Log("[UnstableItem] InitItem");
        }
    }
}
