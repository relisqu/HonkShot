using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Scripts.Items.PlayerItemManager
{
    public class ItemManager : MonoBehaviour
    {
        public List<PlayerItemSO> allItems;
        public PlayerInventory playerInventory;

        public static ItemManager instance;

        private void Awake()
        {
            instance = this;
        }

        [Button]
        public void LoadItems()
        {
            var items = Resources.LoadAll<PlayerItemSO>("Data/Items").ToList();
            foreach (var item in items.ToList())
            {
                if (!item.Enabled)
                {
                    items.Remove(item);
                }
            }
            allItems = items;
        }

        public List<PlayerItemSO> GetRandomItems(int count)
        {
            var available = new List<PlayerItemSO>(allItems);
            available.RemoveAll(item => playerInventory.HasItem(item));
            var result = new List<PlayerItemSO>();
            for (int i = 0; i < count && available.Count > 0; i++)
            {
                int idx = Random.Range(0, available.Count);
                result.Add(available[idx]);
                available.RemoveAt(idx);
            }

            return result;
        }
    }
}