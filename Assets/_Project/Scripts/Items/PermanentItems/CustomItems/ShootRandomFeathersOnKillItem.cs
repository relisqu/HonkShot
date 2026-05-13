using System.Collections.Generic;
using Scripts.Enemies;
using Scripts.Health;
using Scripts.Items.PlayerItemManager;
using Scripts.LevelSystem.LevelGeneration;
using Scripts.Player.Dash;
using Scripts.Player.Shooting;
using UnityEngine;

namespace Scripts.Items.PermanentItems.CustomItems
{
    public class ShootRandomFeathersOnKillItem : Item
    {
        private int _featherQuantity = 5;
        private List<HealthController> _subscribedEnemies = new List<HealthController>();
        private PlayerFeatherShooter _playerFeatherShooter;

        void Start()
        {
            _playerFeatherShooter = GetComponent<PlayerFeatherShooter>();
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
            ShootRandomFeathers();
        }

        void ShootRandomFeathers()
        {
            if (!_playerFeatherShooter) return;

            for (int i = 0; i < _featherQuantity; i++)
            {
                float randomAngle = Random.Range(0f, 360f);
                
                Vector2 randomDirection = new Vector2(
                    Mathf.Cos(randomAngle * Mathf.Deg2Rad),
                    Mathf.Sin(randomAngle * Mathf.Deg2Rad)
                );
                
                _playerFeatherShooter.Shoot(randomDirection);
            }
        }
        public override void InitItem(PlayerItemSO playerItemSO)
        {
        }
    }
}
