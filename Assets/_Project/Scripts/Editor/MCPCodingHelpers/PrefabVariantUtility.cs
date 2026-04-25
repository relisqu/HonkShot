using UnityEditor;
using UnityEngine;

namespace Scripts.Editor.MCPCodingHelpers
{
    public static class PrefabVariantUtility
    {
        public static GameObject SaveSceneObjectAsVariant(string sceneObjectName, string savePath)
        {
            var go = GameObject.Find(sceneObjectName);
            if (!go)
            {
                Debug.LogError($"[PrefabVariantUtility] Scene GameObject '{sceneObjectName}' not found.");
                return null;
            }

            var saved = PrefabUtility.SaveAsPrefabAssetAndConnect(go, savePath, InteractionMode.AutomatedAction);
            if (!saved)
            {
                Debug.LogError("[PrefabVariantUtility] SaveAsPrefabAssetAndConnect returned null.");
                return null;
            }

            Debug.Log($"[PrefabVariantUtility] Saved variant at {AssetDatabase.GetAssetPath(saved)} (parent preserved if source was a prefab instance).");
            return saved;
        }
    }
}
