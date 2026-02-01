using Scripts.Items.PlayerItemManager;
using Scripts.Player;
using Scripts.Services.Pooling;
using UnityEngine;

namespace Scripts.Items.PermanentItems.CustomItems
{
    public class BigBombItem : Item
    {
        [SerializeField] private float _baseDamage = 50f;
        [SerializeField] private float _baseRadius = 5f;
        [SerializeField] private float _baseKnockback = 10f;
        [SerializeField] private float _cooldown = 30f;
        [SerializeField] private Explosion _explosionPrefab;
        [SerializeField] private int _poolSize = 2;

        private ComponentPool<Explosion> _explosionPool;
        private float _lastExplosionTime = -Mathf.Infinity;

        private void Start()
        {
            _explosionPool = new ComponentPool<Explosion>(_explosionPrefab, _poolSize);

            if (GooseFireSystem.Instance)
                GooseFireSystem.Instance.UltimateStarted += GooseFireSystem_UltimateStarted;
        }

        private void OnDestroy()
        {
            if (GooseFireSystem.Instance)
                GooseFireSystem.Instance.UltimateStarted -= GooseFireSystem_UltimateStarted;
        }

        private void GooseFireSystem_UltimateStarted()
        {
            if (!_explosionPrefab) return;
            if (Time.time - _lastExplosionTime < _cooldown) return;

            _lastExplosionTime = Time.time;
            var explosion = _explosionPool.Get(transform.position, Quaternion.identity);
            explosion.Init(_baseDamage, _baseRadius, _baseKnockback);
        }

        public override void InitItem(PlayerItemSO playerItemSO)
        {
            Debug.Log("[BigBombItem] InitItem");
        }
    }
}
