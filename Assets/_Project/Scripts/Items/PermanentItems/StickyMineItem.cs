using Scripts.Health;
using Scripts.Items.PlayerItemManager;
using UnityEngine;

namespace Scripts.Items.PermanentItems
{
    public class StickyMineItem : Item
    {
        public StickyMine minePrefab;
        public float explosionDelay = 1f;
        public float explosionDamage = 20f;

        private PlayerAttackController _attack;

        public GameObject GameObject => gameObject;

        void Start()
        {
            _attack = GetComponentInParent<PlayerAttackController>();
            if (_attack != null)
            {
                _attack.OnHit += AttachMineToEnemy;
            }
        }

        void OnDestroy()
        {
            if (_attack != null)
            {
                _attack.OnHit -= AttachMineToEnemy;
            }
        }

        private void AttachMineToEnemy(GameObject enemy)
        {
            if (enemy == null || minePrefab == null) return;
            var mine = Instantiate(minePrefab, enemy.transform);
            mine.transform.localPosition = Vector3.zero;
            if (mine != null)
            {
                mine.Init(enemy, explosionDelay, explosionDamage);
            }
        }

        public override void InitItem(PlayerItemSO playerItemSO)
        {
        }
    }
}