using System;
using System.Collections.Generic;
using Scripts.Items.PlayerItemManager;

namespace Scripts.Items.PermanentItems
{
    public class MultipleItem : Item
    {
        public List<Item> Items = new();

        public override void InitItem(PlayerItemSO playerItemSO)
        {
            foreach (var item in Items)
            {
                item.InitItem(playerItemSO);
            }
        }
    }
}