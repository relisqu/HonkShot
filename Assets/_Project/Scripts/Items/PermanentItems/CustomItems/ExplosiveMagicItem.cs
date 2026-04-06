using Scripts.Health;
using Scripts.Items.PlayerItemManager;
using Scripts.Player;
using Scripts.Services.Pooling;
using UnityEngine;

namespace Scripts.Items.PermanentItems.CustomItems
{
    public class ExplosiveMagicItem : Item
    {
        [SerializeField] private float _baseDamage = 10f;
        [SerializeField] private Explosion _explosionPrefab;
        [SerializeField] private int _poolSize = 5;

        private ComponentPool<Explosion> _explosionPool;
        private PlayerAttackController _attack;

        private void Start()
        {
            _explosionPool = new ComponentPool<Explosion>(_explosionPrefab, _poolSize);

            _attack = GetComponentInParent<PlayerAttackController>();
            if (_attack)
                _attack.OnHit += PlayerAttackController_OnHit;
        }

        private void OnDestroy()
        {
            if (_attack)
                _attack.OnHit -= PlayerAttackController_OnHit;
        }

        private void PlayerAttackController_OnHit(GameObject enemy)
        {
            if (!enemy || !_explosionPrefab) return;

            var explosion = _explosionPool.Get(enemy.transform.position, Quaternion.identity);
            explosion.Init(_baseDamage);
        }

        public override void InitItem(PlayerItemSO playerItemSO)
        {
            Debug.Log("[ExplosiveMagicItem] InitItem");
        }
    }
}
