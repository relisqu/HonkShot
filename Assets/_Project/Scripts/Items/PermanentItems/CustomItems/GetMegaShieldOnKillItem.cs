using System.Collections.Generic;
using Scripts.Items.PlayerItemManager;
using Scripts.LevelSystem.LevelGeneration;
using Scripts.Enemies;
using Scripts.Health;
using Scripts.Player.Dash;
using UnityEngine;

namespace Scripts.Items.PermanentItems.CustomItems
{
    public class GetMegaShieldOnKillItem : Item
    {
        private ShieldController _shieldController;
        private List<HealthController> _subscribedEnemies = new List<HealthController>();
        void Start()
        {
            _shieldController = GetComponentInParent<ShieldController>();
            if (LevelManager.Instance)
            {
                LevelManager.Instance.EnteredRoom += OnRoomEntered;
                OnRoomEntered();
            }
        }
        void OnDisable()
        {
            if (LevelManager.Instance)
            {
                LevelManager.Instance.EnteredRoom -= OnRoomEntered;
            }

            UnsubscribeAll();
        }
        private void OnRoomEntered()
        {
            UnsubscribeAll();
            var room = LevelManager.Instance.CurrentRoom;
            if (!room) return;
            var enemyHealths = room.GetComponentsInChildren<EnemyHealth>();
            foreach (var enemyHealth in enemyHealths)
            {
                enemyHealth.HealthController.OnDied += OnEnemyKilled;
                _subscribedEnemies.Add(enemyHealth.HealthController);
            }
        }
        private void UnsubscribeAll()
        {
            foreach (var enemyHealth in _subscribedEnemies)
            {
                enemyHealth.OnDied -= OnEnemyKilled;
            }
            _subscribedEnemies.Clear();
        }

        private void OnEnemyKilled(HealthController controller)
        {
            _shieldController.AddIndestructibleShield();
            controller.OnDied -= OnEnemyKilled;
            _subscribedEnemies.Remove(controller);
        }
        
        public override void InitItem(PlayerItemSO playerItemSO)
        {
        }
    }
}
