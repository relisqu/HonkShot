using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Scripts.Items.PlayerItemManager
{
    [CreateAssetMenu(fileName = "Item", menuName = "ScriptableObjects/Items", order = 1)]
    public class PlayerItemSO : ScriptableObject
    {
        public int Id;
        public string ItemKey;
        public Sprite icon;
        public Sprite inGameSprite;
        [FormerlySerializedAs("itemPrefabs")] public Item itemPrefab;

        public virtual Item OnPickup(GameObject player)
        {
            if (itemPrefab)
            {
                var item = Instantiate(itemPrefab, player.transform);
                item.InitItem(this);
                return item;
            }

            return null;
        }
    }
}