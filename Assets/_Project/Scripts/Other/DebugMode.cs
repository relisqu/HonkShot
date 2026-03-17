using System;
using System.Linq;
using Scripts.Enemies;
using Scripts.Health;
using Scripts.Items.PlayerItemManager;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Scripts.Other
{
    public class DebugMode : MonoBehaviour
    {
        [FormerlySerializedAs("NeedGenerateLevels")]
        public bool GeneratingLevels;

        public static DebugMode Instance;

        public bool DebugEnabled;
        public string itemId;

        [Button]
        public void SpawnItemWithIndex()
        {
            var so = ItemManager.instance.AllItems.FirstOrDefault(x => x.Id == itemId);
            if (!so)
            {
                Debug.LogError($"Item with Id '{itemId}' not found in allItems");
                return;
            }

            PlayerInventory.Instance.AddItem(so);
            Debug.Log($"Spawned item: {so.name} (Id: {so.Id})");
        }

        [Button]
        public void SpawnRandomItem()
        {
            PlayerInventory.Instance.AddItem(
                ItemManager.instance.AllItems[UnityEngine.Random.Range(0, ItemManager.instance.AllItems.Count)]);
        }

        private void Awake()
        {
            Instance = this;
        }

        private void Update()
        {
            if (!DebugEnabled) return;

            if (Input.GetKeyDown(KeyCode.F9))
            {
                var health = PlayerInventory.Instance.GetComponent<HealthController>();
                if (health)
                    health.TakeDamage(1000000);
            }

            if (Input.GetKeyDown(KeyCode.F))
            {
                KillAllEnemies();
            }
        }

        private void KillAllEnemies()
        {
            var enemies = FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);
            foreach (var enemy in enemies)
            {
                if (enemy && enemy.IsAlive())
                    enemy.HealthController.TakeDamage(1000000);
            }

            var baseEnemies = FindObjectsByType<BaseEnemy>(FindObjectsSortMode.None);
            foreach (var baseEnemy in baseEnemies)
            {
                if (!baseEnemy) continue;
                var health = baseEnemy.GetComponentInChildren<HealthController>();
                if (health && health.IsAlive)
                {
                    health.SetInvincible(0, false);
                    health.TakeDamage(1000000);
                }
            }
        }
    }
}