using Scripts.Items.PlayerItemManager;
using Scripts.Player;
using Scripts.Enemies;
using Scripts.Services.Pooling;
using UnityEngine;

namespace Scripts.Items.PermanentItems.CustomItems
{
    public class BigBombItem : Item
    {
        [SerializeField] private float _baseDamage = 20f;
        [SerializeField] private float _arenaRadius = 50f;
        [SerializeField] private float _cooldown = 20f;
        [SerializeField] private int _extraExplosionsCount = 5;
        [SerializeField] private float _extraExplosionSpread = 10f;
        [SerializeField] private Explosion _explosionPrefab;
        [SerializeField] private int _poolSize = 20;

        private ComponentPool<Explosion> _explosionPool;
        private GooseFireSystem _fireSystem;
        private float _lastExplosionTime = -Mathf.Infinity;

        private void Start()
        {
            _explosionPool = new ComponentPool<Explosion>(_explosionPrefab, _poolSize);

            _fireSystem = GooseFireSystem.Instance;
            if (_fireSystem)
                _fireSystem.UltimateStarted += GooseFireSystem_UltimateStarted;
        }

        private void OnDestroy()
        {
            if (_fireSystem)
                _fireSystem.UltimateStarted -= GooseFireSystem_UltimateStarted;
        }

        private void GooseFireSystem_UltimateStarted()
        {
            if (!_explosionPrefab || !ExplosionSystem.Instance) return;
            if (Time.time - _lastExplosionTime < _cooldown) return;

            _lastExplosionTime = Time.time;

            var hits = Physics2D.OverlapCircleAll(
                transform.position, _arenaRadius, ExplosionSystem.Instance.EnemyLayerMask);

            foreach (var hit in hits)
            {
                if (hit.TryGetComponent(out EnemyHealth _))
                {
                    var explosion = _explosionPool.Get(hit.transform.position, Quaternion.identity);
                    explosion.Init(_baseDamage);
                }
            }

            for (int i = 0; i < _extraExplosionsCount; i++)
            {
                Vector2 randomOffset = Random.insideUnitCircle * _extraExplosionSpread;
                Vector2 spawnPos = (Vector2)transform.position + randomOffset;
                var vfxExplosion = _explosionPool.Get(spawnPos, Quaternion.identity);
                vfxExplosion.Init(_baseDamage);
            }
        }

        public override void InitItem(PlayerItemSO playerItemSO)
        {
            Debug.Log("[BigBombItem] InitItem");
        }
    }
}
