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

        public int itemId;

        [Button]
        public void SpawnItemWithIndex()
        {
            PlayerInventory.Instance.AddItem(ItemManager.instance.allItems.First(so => so.Id == itemId));
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