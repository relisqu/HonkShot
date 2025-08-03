using UnityEngine;

namespace Scripts.Items.PlayerItemManager
{
    public interface IItem
    {
        public void InitItem(PlayerItemSO playerItemSO);
        public GameObject GameObject { get; }
    }
}