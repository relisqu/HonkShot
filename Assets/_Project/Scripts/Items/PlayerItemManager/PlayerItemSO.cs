using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Scripts.Items.PlayerItemManager
{
    [CreateAssetMenu(fileName = "Item", menuName = "ScriptableObjects/Items", order = 1)]
    public class PlayerItemSO : ScriptableObject
    {
        private const string ItemPrefix = "Item ";

        public string Id;
        [FormerlySerializedAs("ItemKey")] public string ItemNotes;
        public Sprite icon;
        [FormerlySerializedAs("itemPrefab")] [FormerlySerializedAs("itemPrefabs")] public Item ItemPrefab;
        public bool Enabled;

        private int _numericId;
        private bool _numericIdCached;

        public int NumericId
        {
            get
            {
                if (!_numericIdCached)
                {
                    _numericId = ComputeNumericId(Id);
                    _numericIdCached = true;
                }
                return _numericId;
            }
        }

        private static int ComputeNumericId(string id)
        {
            if (string.IsNullOrEmpty(id))
                return 0;

            var parts = id.Split('-');
            if (parts.Length >= 2
                && int.TryParse(parts[0], out var first)
                && int.TryParse(parts[1], out var second))
            {
                return first * 10000 + second * 10;
            }

            if (int.TryParse(id, out var singleNum))
                return singleNum * 10000;

            return 0;
        }

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

#if UNITY_EDITOR
        private string GetOrParseId()
        {
            if (!string.IsNullOrEmpty(Id))
                return Id;

            if (name.StartsWith(ItemPrefix))
            {
                Id = name.Substring(ItemPrefix.Length);
                EditorUtility.SetDirty(this);
                return Id;
            }

            return null;
        }

        [Button("Create Prefab")]
        private void CreatePrefab()
        {
            var id = GetOrParseId();
            if (string.IsNullOrEmpty(id))
            {
                Debug.LogError("Cannot create prefab: Id is empty and could not be parsed from asset name.");
                return;
            }

            var prefabName = ItemPrefix + id;
            var path = Constants.ItemPrefabsPath + "/" + prefabName + ".prefab";

            if (AssetDatabase.LoadAssetAtPath<GameObject>(path))
            {
                Debug.LogWarning($"Prefab already exists at {path}");
                EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<GameObject>(path));
                return;
            }

            var go = new GameObject(prefabName);
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            DestroyImmediate(go);

            EditorGUIUtility.PingObject(prefab);
            Debug.Log($"Prefab created at {path}. Add your Item component, then click Link to SO on it.");
        }

        [Button("Rename Assets")]
        private void RenameAssets()
        {
            if (string.IsNullOrEmpty(Id))
            {
                Debug.LogError("Cannot rename: Id is empty.");
                return;
            }

            var newName = ItemPrefix + Id;

            var soPath = AssetDatabase.GetAssetPath(this);
            if (!string.IsNullOrEmpty(soPath) && name != newName)
            {
                var result = AssetDatabase.RenameAsset(soPath, newName);
                if (string.IsNullOrEmpty(result))
                    Debug.Log($"SO renamed to '{newName}'");
                else
                    Debug.LogError($"Failed to rename SO: {result}");
            }

            if (ItemPrefab)
            {
                var prefabPath = AssetDatabase.GetAssetPath(ItemPrefab.gameObject);
                if (!string.IsNullOrEmpty(prefabPath))
                {
                    var prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
                    prefabRoot.name = newName;
                    PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
                    PrefabUtility.UnloadPrefabContents(prefabRoot);

                    var renameResult = AssetDatabase.RenameAsset(prefabPath, newName);
                    if (string.IsNullOrEmpty(renameResult))
                        Debug.Log($"Prefab renamed to '{newName}'");
                    else
                        Debug.LogError($"Failed to rename prefab: {renameResult}");
                }
            }

            AssetDatabase.SaveAssets();
        }
#endif
    }
}
