using UnityEngine;

namespace Scripts.Items
{
    [CreateAssetMenu(fileName = "Item", menuName = "ScriptableObjects/Items", order = 1)]
    public class PlayerItemSO : ScriptableObject
    {
        public string itemName;
        [TextArea] public string description;
        public Sprite icon;
        public Sprite inGameSprite;
        public GameObject itemPrefab;

        public virtual void OnPickup(GameObject player)
        {
            if (itemPrefab)
            {
                GameObject instance = Instantiate(itemPrefab, player.transform);
            }
        }
    }
} 