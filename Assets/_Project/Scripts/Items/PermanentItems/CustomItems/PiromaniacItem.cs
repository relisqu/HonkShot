using Scripts.Items.PlayerItemManager;
using Scripts.Player;
using UnityEngine;

namespace Scripts.Items.PermanentItems.CustomItems
{
    public class PiromaniacItem : Item
    {
        [SerializeField] private float _fireAmount = 5f;

        private void Start()
        {
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
            if (GooseFireSystem.Instance)
                GooseFireSystem.Instance.AddFire(_fireAmount);
        }

        public override void InitItem(PlayerItemSO playerItemSO)
        {
            Debug.Log("[PiromaniacItem] InitItem");
        }
    }
}
