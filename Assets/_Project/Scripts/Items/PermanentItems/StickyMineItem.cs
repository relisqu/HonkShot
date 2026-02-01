using Scripts.Health;
using Scripts.Items.PlayerItemManager;
using Scripts.Player;
using Scripts.Services.Pooling;
using UnityEngine;

namespace Scripts.Items.PermanentItems
{
    public class StickyMineItem : Item
    {
        [SerializeField] private StickyMine _minePrefab;
        [SerializeField] private Explosion _explosionPrefab;
        [SerializeField] private float _explosionDelay = 1f;
        [SerializeField] private float _explosionDamage = 20f;
        [SerializeField] private int _explosionPoolSize = 5;

        private PlayerAttackController _attack;
        private ComponentPool<Explosion> _explosionPool;

        private void Start()
        {
            if (_explosionPrefab)
                _explosionPool = new ComponentPool<Explosion>(_explosionPrefab, _explosionPoolSize);

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
            if (!enemy || !_minePrefab) return;

            var mine = Instantiate(_minePrefab, enemy.transform);
            mine.transform.localPosition = Vector3.zero;
            mine.Init(enemy, _explosionDelay, _explosionDamage, _explosionPool);
        }

        public override void InitItem(PlayerItemSO playerItemSO)
        {
            Debug.Log("[StickyMineItem] InitItem");
        }
    }
}
