using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Scripts.Items.PlayerItemManager
{
    [CreateAssetMenu(fileName = "ItemPool", menuName = "ScriptableObjects/ItemPool")]
    public class ItemPoolSO : ScriptableObject
    {
        public List<PlayerItemSO> Items;

#if UNITY_EDITOR
        [Button("Load All Items")]
        private void LoadAllItems()
        {
            Items = Resources.LoadAll<PlayerItemSO>("Data/Items").ToList();
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}
