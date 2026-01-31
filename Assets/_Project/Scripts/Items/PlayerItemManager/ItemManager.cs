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
        public ItemPoolSO itemPool;
        public PlayerInventory playerInventory;

        private List<PlayerItemSO> _allItems;
        public List<PlayerItemSO> AllItems => _allItems;
        public static ItemManager instance;

        private void Awake()
        {
            instance = this;
            _allItems = itemPool.Items.Where(item => item.Enabled).ToList();
        }

        public List<PlayerItemSO> GetRandomItems(int count)
        {
            var available = new List<PlayerItemSO>(_allItems);
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