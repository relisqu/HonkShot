using System.Collections.Generic;
using UnityEngine;

namespace Scripts.Items.PlayerItemManager
{
    public class PlayerInventory : MonoBehaviour
    {
        public List<PlayerItemSO> ownedItems = new List<PlayerItemSO>();

        public void AddItem(PlayerItemSO item)
        {
            if (!ownedItems.Contains(item))
            {
                ownedItems.Add(item);
                item.OnPickup(gameObject);
            }
        }

        public bool HasItem(PlayerItemSO item) => ownedItems.Contains(item);
    }
} 