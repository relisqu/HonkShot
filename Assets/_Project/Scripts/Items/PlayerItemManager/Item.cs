using UnityEngine;

namespace Scripts.Items.PlayerItemManager
{
    public abstract class Item : MonoBehaviour
    {
        public abstract void InitItem(PlayerItemSO playerItemSO);
    }
}