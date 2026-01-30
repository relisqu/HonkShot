using System;
using System.Linq;
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

        public string itemId;

        [Button]
        public void SpawnItemWithIndex()
        {
            var so = ItemManager.instance.allItems.FirstOrDefault(x => x.Id == itemId);
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
                ItemManager.instance.allItems[UnityEngine.Random.Range(0, ItemManager.instance.allItems.Count)]);
        }

        private void Awake()
        {
            Instance = this;
        }
    }
}