using UnityEngine;
using UnityEngine.Serialization;

namespace Scripts.Items
{
    [CreateAssetMenu(fileName = "Item", menuName = "ScriptableObjects/Items", order = 1)]
    public class PlayerItemSO : ScriptableObject
    {
        public int Id;
        public string ItemKey;
        public Sprite icon;
        public Sprite inGameSprite;
        public GameObject itemPrefab;

        public virtual GameObject OnPickup(GameObject player)
        {
            if (itemPrefab)
            {
                return Instantiate(itemPrefab, player.transform);
            }
            return null;
        }
    }
} 