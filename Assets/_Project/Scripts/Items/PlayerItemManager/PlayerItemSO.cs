using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Scripts.Items.PlayerItemManager
{
    [CreateAssetMenu(fileName = "Item", menuName = "ScriptableObjects/Items", order = 1)]
    public class PlayerItemSO : ScriptableObject
    {
        public int Id;
        [FormerlySerializedAs("ItemKey")] public string ItemNotes;
        public Sprite icon;
        [FormerlySerializedAs("itemPrefab")] [FormerlySerializedAs("itemPrefabs")] public Item ItemPrefab;
        public bool Enabled;

        public virtual Item OnPickup(GameObject player)
        {
            if (ItemPrefab)
            {
                var item = Instantiate(ItemPrefab, player.transform);
                item.InitItem(this);
                return item;
            }

            return null;
        }
    }
}