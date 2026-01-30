using Sirenix.OdinInspector;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Scripts.Items.PlayerItemManager
{
    public abstract class Item : MonoBehaviour
    {
        public abstract void InitItem(PlayerItemSO playerItemSO);

#if UNITY_EDITOR
        [Button("Link to SO")]
        private void LinkToSO()
        {
            var allSOs = Resources.LoadAll<PlayerItemSO>("Data/Items");
            var goName = gameObject.name;

            foreach (var so in allSOs)
            {
                if ("Item " + so.Id == goName)
                {
                    so.ItemPrefab = this;
                    EditorUtility.SetDirty(so);
                    AssetDatabase.SaveAssets();
                    Debug.Log($"Linked to SO '{so.name}' (Id: {so.Id})");
                    EditorGUIUtility.PingObject(so);
                    return;
                }
            }

            Debug.LogError($"No PlayerItemSO found with Id matching '{goName}'. Expected SO with Id that makes 'Item {{Id}}' == '{goName}'.");
        }
#endif
    }
}
