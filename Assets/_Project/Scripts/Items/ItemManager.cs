using System.Collections.Generic;
using UnityEngine;

namespace Scripts.Items
{
    public class ItemManager : MonoBehaviour
    {
        public List<PlayerItemSO> allItems; // Assign all item SOs in inspector

        public List<PlayerItemSO> GetRandomItems(int count, PlayerInventory inventory)
        {
            var available = new List<PlayerItemSO>(allItems);
            available.RemoveAll(item => inventory.HasItem(item));
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