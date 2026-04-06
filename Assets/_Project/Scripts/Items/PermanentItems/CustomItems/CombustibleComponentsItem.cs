using System.Collections;
using Scripts.Items.PlayerItemManager;
using Scripts.Player;
using Scripts.Services.Pooling;
using UnityEngine;

namespace Scripts.Items.PermanentItems.CustomItems
{
    public class CombustibleComponentsItem : Item
    {
        [SerializeField] private float _spawnDelay = 0.5f;
        [SerializeField] private float _damagePerSecond = 1f;
        [SerializeField] private float _duration = 3f;
        [SerializeField] private float _radius = 2f;
        [SerializeField] private float _tickInterval = 0.5f;
        [SerializeField] private FireZone _fireZonePrefab;
        [SerializeField] private int _poolSize = 10;

        private ComponentPool<FireZone> _fireZonePool;

        private void Start()
        {
            _fireZonePool = new ComponentPool<FireZone>(_fireZonePrefab, _poolSize);

            if (ExplosionSystem.Instance)
                ExplosionSystem.Instance.OnExplosionCreated += ExplosionSystem_OnExplosionCreated;
        }

        private void OnDestroy()
        {
            if (ExplosionSystem.Instance)
                ExplosionSystem.Instance.OnExplosionCreated -= ExplosionSystem_OnExplosionCreated;
        }

        private void ExplosionSystem_OnExplosionCreated(Vector2 position, float damage)
        {
            if (!_fireZonePrefab) return;

            StartCoroutine(SpawnFireZoneDelayed(position));
        }

        private IEnumerator SpawnFireZoneDelayed(Vector2 position)
        {
            yield return new WaitForSeconds(_spawnDelay);

            var fireZone = _fireZonePool.Get(position, Quaternion.identity);
            fireZone.Init(_damagePerSecond, _duration, _radius, _tickInterval);
        }

        public override void InitItem(PlayerItemSO playerItemSO)
        {
            Debug.Log("[CombustibleComponentsItem] InitItem");
        }
    }
}
