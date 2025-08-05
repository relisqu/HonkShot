using System;
using System.Collections.Generic;
using UnityEngine;

namespace Scripts.Items.PlayerItemManager
{
    public class PlayerInventory : MonoBehaviour
    {
        public List<PlayerItemSO> ownedItems = new List<PlayerItemSO>();

        public static PlayerInventory Instance;

        private void Start()
        {
            Instance = this;
        }

        public void AddItem(PlayerItemSO item)
        {
            if (!ownedItems.Contains(item))
            {
                ownedItems.Add(item);
                var itemGameObj = item.OnPickup(gameObject);
            }
        }

        public bool HasItem(PlayerItemSO item) => ownedItems.Contains(item);
    }
}