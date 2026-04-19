using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;
using System.Linq;

public class BouncerRestructureTool
{
    [MenuItem("Tools/Restructure Bouncers")]
    public static void RestructureBouncers()
    {
        string bouncerFolder = "Assets/_Project/Resources/Prefabs/LevelObjects/Bouncers";
        var prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { bouncerFolder });
        var allPaths = prefabGuids.Select(AssetDatabase.GUIDToAssetPath).ToList();

        // Sort: bases first (prefabs that are not variants of another bouncer)
        // Then variants, by nesting depth
        allPaths.Sort((a, b) =>
        {
            var prefabA = AssetDatabase.LoadAssetAtPath<GameObject>(a);
            var prefabB = AssetDatabase.LoadAssetAtPath<GameObject>(b);
            bool aIsVariant = PrefabUtility.IsPartOfVariantPrefab(prefabA);
            bool bIsVariant = PrefabUtility.IsPartOfVariantPrefab(prefabB);
            if (aIsVariant != bIsVariant) return aIsVariant ? 1 : -1;
            return string.Compare(a, b);
        });

        int count = 0;
        foreach (var assetPath in allPaths)
        {
            if (ProcessPrefab(assetPath))
                count++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Done! Processed {count} bouncer prefabs.");
    }

    private static bool ProcessPrefab(string assetPath)
    {
        var root = PrefabUtility.LoadPrefabContents(assetPath);
        try
        {
            // Find active visual child that has BounceObject
            Transform visualChild = null;
            for (int i = 0; i < root.transform.childCount; i++)
            {
                var child = root.transform.GetChild(i);
                if (child.gameObject.activeSelf
                    && child.GetComponent<Scripts.LevelSystem.LevelObjects.BounceObject>()
                    && child.name != "Collider")
                {
                    visualChild = child;
                    break;
                }
            }

            if (!visualChild)
            {
                PrefabUtility.UnloadPrefabContents(root);
                Debug.Log($"Skipping {assetPath} - no visual child with BounceObject to move");
                return false;
            }

            // Delete any existing Collider children (from prior runs or inheritance)
            var toDelete = new List<GameObject>();
            for (int i = 0; i < root.transform.childCount; i++)
            {
                var child = root.transform.GetChild(i);
                if (child.name == "Collider")
                    toDelete.Add(child.gameObject);
            }
            foreach (var go in toDelete)
                Object.DestroyImmediate(go);

            // Create Collider child
            var colliderObj = new GameObject("Collider");
            colliderObj.transform.SetParent(root.transform);
            colliderObj.transform.localPosition = Vector3.zero;
            colliderObj.transform.localRotation = Quaternion.identity;
            colliderObj.transform.localScale = Vector3.one;

            // Copy components
            CopyIfExists<Collider2D>(visualChild, colliderObj);
            CopyIfExists<Scripts.LevelSystem.LevelObjects.BounceObject>(visualChild, colliderObj);
            CopyIfExists<Scripts.LevelSystem.LevelObjects.BouncerLevelObject>(visualChild, colliderObj);
            CopyIfExists<Scripts.PointSystem.AttackPointsObject>(visualChild, colliderObj);
            CopyIfExists<Scripts.LevelSystem.LevelObjects.BouncerColliderAligner>(visualChild, colliderObj);

            // Remove from visual (reverse order)
            DestroyIfExists<Scripts.LevelSystem.LevelObjects.BouncerColliderAligner>(visualChild);
            DestroyIfExists<Scripts.PointSystem.AttackPointsObject>(visualChild);
            DestroyIfExists<Scripts.LevelSystem.LevelObjects.BouncerLevelObject>(visualChild);
            DestroyIfExists<Scripts.LevelSystem.LevelObjects.BounceObject>(visualChild);
            DestroyIfExists<Collider2D>(visualChild);

            // Wire up references
            var newCollider = colliderObj.GetComponent<Collider2D>();
            var newBounce = colliderObj.GetComponent<Scripts.LevelSystem.LevelObjects.BounceObject>();
            var newBouncerLevel = colliderObj.GetComponent<Scripts.LevelSystem.LevelObjects.BouncerLevelObject>();
            var newAligner = colliderObj.GetComponent<Scripts.LevelSystem.LevelObjects.BouncerColliderAligner>();

            if (newBouncerLevel && newBounce)
                SetSerializedRef(newBouncerLevel, "_bounceObject", newBounce);

            if (newAligner && newCollider)
                SetSerializedRef(newAligner, "_collider", newCollider);

            if (newAligner && visualChild)
                SetSerializedRef(newAligner, "_visual", visualChild);

            PrefabUtility.SaveAsPrefabAsset(root, assetPath);
            Debug.Log($"Restructured {assetPath}");
            return true;
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    private static void CopyIfExists<T>(Transform source, GameObject target) where T : Component
    {
        var comp = source.GetComponent<T>();
        if (comp)
        {
            ComponentUtility.CopyComponent(comp);
            ComponentUtility.PasteComponentAsNew(target);
        }
    }

    private static void DestroyIfExists<T>(Transform source) where T : Component
    {
        var comp = source.GetComponent<T>();
        if (comp) Object.DestroyImmediate(comp);
    }

    private static void SetSerializedRef(Component target, string fieldName, Object value)
    {
        var so = new SerializedObject(target);
        var prop = so.FindProperty(fieldName);
        if (prop != null)
        {
            prop.objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
