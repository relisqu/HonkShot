using System.Collections.Generic;
using Scripts.Enemies;
using Scripts.Health;
using Scripts.Items.PlayerItemManager;
using Scripts.LevelSystem.LevelGeneration;
using Scripts.Player.Dash;
using UnityEngine;

namespace Scripts.Items.PermanentItems.CustomItems
{
    public class RegenDashesOnKillItem : Item
    {
        public int dashCount;

        //private HealthController _health;
        private List<HealthController> _subscribedEnemies = new List<HealthController>();

        private PlayerDashController _playerDashController;

        void Start()
        {
            _playerDashController = GetComponentInParent<PlayerDashController>();
            if (LevelManager.Instance)
            {
                LevelManager.Instance.EnteredRoom += OnRoomEntered;
                OnRoomEntered();
            }
        }

        void OnDestroy()
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
                enemyHealth.HealthController.OnDied += () => OnEnemyKilled(enemyHealth.HealthController);
                _subscribedEnemies.Add(enemyHealth.HealthController);
            }
        }

        private void UnsubscribeAll()
        {
            _subscribedEnemies.Clear();
        }

        private void OnEnemyKilled(HealthController enemy)
        {
                for (int i = 0; i < dashCount; i++)
                {
                    _playerDashController.AddDash();
                }
        }

        public override void InitItem(PlayerItemSO playerItemSO)
        {
        }
    }
}