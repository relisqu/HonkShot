using System;
using System.Collections.Generic;
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