using System;
using System.Linq;
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
            if (DebugEnabled && Input.GetKeyDown(KeyCode.F9))
            {
                var health = PlayerInventory.Instance.GetComponent<HealthController>();
                if (health)
                    health.TakeDamage(1000000);
            }
        }
    }
}