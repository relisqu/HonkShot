using System.Collections.Generic;
using Scripts.Enemies;
using Scripts.Health;
using Scripts.Items.PlayerItemManager;
using Scripts.LevelSystem.LevelGeneration;
using UnityEngine;

namespace Scripts.Items.PermanentItems
{
    public class VampiricOnKillItem : MonoBehaviour, IItem
    {
        public float healPercent = 0.05f; // 5% of max health per kill

        private HealthController _health;
        private List<HealthController> _subscribedEnemies = new List<HealthController>();
        public GameObject GameObject => gameObject;

        void Start()
        {
            _health = GetComponentInParent<HealthController>();
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
            if (_health)
            {
                float healAmount = _health.GetMaxHealth() * healPercent;
                _health.AddHealth(healAmount);
            }
        }

        public void InitItem(PlayerItemSO playerItemSO)
        {
            throw new System.NotImplementedException();
        }
    }
}