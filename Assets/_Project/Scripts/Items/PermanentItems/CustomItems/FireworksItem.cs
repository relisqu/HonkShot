using Scripts.Items.PlayerItemManager;
using Scripts.Player;
using Scripts.Services.Pooling;
using UnityEngine;

namespace Scripts.Items.PermanentItems.CustomItems
{
    public class FireworksItem : Item
    {
        [SerializeField] private float _baseDamage = 12f;
        [SerializeField] private float _cooldown = 5f;
        [SerializeField] private Explosion _explosionPrefab;
        [SerializeField] private int _poolSize = 3;

        private ComponentPool<Explosion> _explosionPool;
        private float _lastExplosionTime = -Mathf.Infinity;

        private void Start()
        {
            _explosionPool = new ComponentPool<Explosion>(_explosionPrefab, _poolSize);
        }

        private void Update()
        {
            if (!_explosionPrefab) return;
            if (Time.time - _lastExplosionTime < _cooldown) return;

            _lastExplosionTime = Time.time;
            var explosion = _explosionPool.Get(transform.position, Quaternion.identity);
            explosion.Init(_baseDamage);
        }

        public override void InitItem(PlayerItemSO playerItemSO)
        {
            Debug.Log("[FireworksItem] InitItem");
        }
    }
}
