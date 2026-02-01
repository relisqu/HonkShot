using Scripts.Health;
using Scripts.Items.PlayerItemManager;
using Scripts.Player;
using Scripts.Services.Pooling;
using UnityEngine;
using UnityEngine.Serialization;

namespace Scripts.Items.PermanentItems
{
    public class StickyMineItem : Item
    {
        [SerializeField] private StickyMine _minePrefab;
        [SerializeField] private float _explosionDelay = 1f;
        [SerializeField] private float _explosionDamage = 20f;
        [SerializeField] private int _minesPoolSize = 5;

        private PlayerAttackController _attack;
        private ComponentPool<StickyMine> _stickyMinesPool;

        private void Start()
        {
            _stickyMinesPool = new ComponentPool<StickyMine>(_minePrefab, _minesPoolSize);

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
            var mine = _stickyMinesPool.Get(Vector3.zero, Quaternion.identity);
            mine.transform.localPosition = Vector3.zero;
            mine.transform.parent = enemy.transform;
        }

        public override void InitItem(PlayerItemSO playerItemSO)
        {
            Debug.Log("[StickyMineItem] InitItem");
        }
    }
}