#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.Splines;
using Unity.Mathematics;

namespace Scripts.LevelLab
{
    public static class LevelLabRoomBuilder
    {
        public const string RecipesFolder = "Assets/_Project/LevelLab/Recipes";
        public const string OutputFolder = "Assets/_Project/Resources/Prefabs/Rooms/LevelLab_Floor1";
        private const string BaseRoomPath = "Assets/_Project/Resources/Prefabs/Rooms/Room.prefab";
        private const float DoorOutwardOffset = 0.58f;

        [MenuItem("Tools/LevelLab/Build All Recipes")]
        public static void BuildAllMenu()
        {
            var built = BuildAll();
            Debug.Log($"[LevelLab] Built {built.Count} rooms into {OutputFolder}");
        }

        public static List<string> BuildAll()
        {
            var result = new List<string>();
            foreach (var file in Directory.GetFiles(RecipesFolder, "*.json"))
            {
                if (Path.GetFileName(file).StartsWith("_")) continue;
                var path = BuildFile(file);
                if (path != null) result.Add(path);
            }

            AssetDatabase.SaveAssets();
            return result;
        }

        public static string BuildFile(string jsonPath)
        {
            RoomRecipe recipe;
            try
            {
                recipe = JsonUtility.FromJson<RoomRecipe>(File.ReadAllText(jsonPath));
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[LevelLab] Can't parse {jsonPath}: {e.Message}");
                return null;
            }

            return Build(recipe);
        }

        public static string Build(RoomRecipe recipe)
        {
            var outline = recipe.OutlinePoints();
            if (outline.Count < 3)
            {
                Debug.LogError($"[LevelLab] {recipe.id}: outline needs at least 3 points");
                return null;
            }

            if (SignedArea(outline) > 0) outline.Reverse();

            if (!AssetDatabase.IsValidFolder(OutputFolder))
                AssetDatabase.CreateFolder(Path.GetDirectoryName(OutputFolder).Replace('\\', '/'), Path.GetFileName(OutputFolder));

            var basePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(BaseRoomPath);
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(basePrefab);
            string savePath = $"{OutputFolder}/{recipe.FileName}.prefab";

            try
            {
                instance.name = recipe.FileName;
                instance.transform.position = Vector3.zero;
                var root = instance.transform;

                var field = root.Find("Field");
                SetSpline(field.GetComponent<SpriteShapeController>(), outline, field.localPosition);

                ClearChildren(root.Find("grassDecorations"), null);
                var levelObjects = root.Find("LevelObjects");
                ClearChildren(levelObjects, "Walls");

                root.Find("SpawnPoint").localPosition = RecipeObjectPosition(RoomRecipe.ToVector(recipe.spawn, Vector2.zero), 0);
                root.Find("ExitPoint").localPosition =
                    RecipeObjectPosition(RoomRecipe.ToVector(recipe.exit, RoomRecipe.ToVector(recipe.spawn, Vector2.zero)), 0);

                var door = root.Find("Door");
                var doorPos = recipe.door != null && recipe.door.Length >= 2
                    ? RoomRecipe.ToVector(recipe.door, Vector2.zero)
                    : DefaultDoorPosition(outline);
                door.localPosition = new Vector3(doorPos.x, doorPos.y, door.localPosition.z);

                if (recipe.objects != null)
                {
                    var walkerContext = LevelLabWalkerPaths.BuildContext(recipe, outline);
                    foreach (var obj in recipe.objects)
                        SpawnObject(obj, levelObjects, recipe.id, walkerContext);
                }

                PrefabUtility.SaveAsPrefabAsset(instance, savePath, out bool saved);
                if (!saved)
                {
                    Debug.LogError($"[LevelLab] {recipe.id}: failed to save {savePath}");
                    return null;
                }
            }
            finally
            {
                Object.DestroyImmediate(instance);
            }

            RefreshWalls(savePath);
            return savePath;
        }

        private static void SpawnObject(RecipeObject obj, Transform levelObjects, string recipeId, LevelLabWalkerPaths.Context walkerContext)
        {
            if (!LevelLabCatalog.TryGetPrefabPath(obj.key, out var prefabPath))
            {
                Debug.LogError($"[LevelLab] {recipeId}: unknown object key '{obj.key}'");
                return;
            }

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (!prefab)
            {
                Debug.LogError($"[LevelLab] {recipeId}: prefab missing at {prefabPath}");
                return;
            }

            var spawned = (GameObject)PrefabUtility.InstantiatePrefab(prefab, levelObjects);
            float z = prefab.transform.localPosition.z;
            var path = obj.PathPoints();
            var anchor = path.Count > 0 ? path[0] : obj.Position;
            spawned.transform.position = new Vector3(anchor.x, anchor.y, z);
            spawned.transform.localRotation = Quaternion.Euler(0, 0, obj.rot);
            var scale = obj.Scale;
            spawned.transform.localScale = new Vector3(prefab.transform.localScale.x * scale.x,
                prefab.transform.localScale.y * scale.y, prefab.transform.localScale.z);

            if (LevelLabWalkerPaths.IsWalkerKey(obj.key))
            {
                var route = LevelLabWalkerPaths.ChoosePath(obj, walkerContext, out bool closed);
                LevelLabWalkerPaths.Apply(spawned, route, closed, recipeId);
            }
            else if (path.Count > 0)
            {
                ApplyRoadPath(spawned, path, recipeId);
            }
        }

        private static void ApplyRoadPath(GameObject road, List<Vector2> path, string recipeId)
        {
            var movement = road.GetComponentInChildren<Scripts.LevelObjects.MoveRoads.MoveRoadMovement>(true);
            var container = movement
                ? new SerializedObject(movement).FindProperty("_splineContainer").objectReferenceValue as SplineContainer
                : null;
            if (!container)
            {
                Debug.LogError($"[LevelLab] {recipeId}: '{road.name}' has a path but no MoveRoad spline");
                return;
            }

            if (path.Count < 2)
            {
                Debug.LogError($"[LevelLab] {recipeId}: MoveRoad path needs at least 2 points");
                return;
            }

            var spline = container.Spline;
            spline.Clear();
            var locals = new List<Vector3>();
            foreach (var p in path)
            {
                var local = container.transform.InverseTransformPoint(new Vector3(p.x, p.y, container.transform.position.z));
                locals.Add(new Vector3(local.x, local.y, 0));
            }

            for (int i = 0; i < locals.Count; i++)
            {
                // Hand-made roads keep knot up = -Z so the instanced sprites lie flat on the floor.
                var forward = locals[Mathf.Min(i + 1, locals.Count - 1)] - locals[Mathf.Max(i - 1, 0)];
                var rotation = Quaternion.LookRotation(forward.normalized, Vector3.back);
                spline.Add(new BezierKnot(locals[i], float3.zero, float3.zero, rotation), TangentMode.AutoSmooth);
            }

            EditorUtility.SetDirty(container);
            var visual = road.GetComponentInChildren<Scripts.LevelSystem.LevelObjects.MoveRoads.MoveRoadSplineVisual>(true);
            if (visual) visual.UpdateSecondSpline();

            foreach (var instantiate in road.GetComponentsInChildren<SplineInstantiate>(true))
                if (BakedRoadSplines.Contains(instantiate.name)) BakeSplineInstances(instantiate);
        }

        // Same as the inspector's "Bake Instances" the hand-made rooms use: live instancers don't save their sprites.
        private static readonly HashSet<string> BakedRoadSplines = new() { "MainSpline", "GrassSpline" };

        private static void BakeSplineInstances(SplineInstantiate instantiate)
        {
            instantiate.UpdateInstances();
            var instances = typeof(SplineInstantiate)
                .GetProperty("instances", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                ?.GetValue(instantiate) as List<GameObject>;
            if (instances == null) return;

            int index = 0;
            var parent = instantiate.transform;
            foreach (var instance in instances)
            {
                if (!instance) continue;
                var source = PrefabUtility.GetCorrespondingObjectFromSource(instance);
                var copy = source
                    ? (GameObject)PrefabUtility.InstantiatePrefab(source, parent)
                    : Object.Instantiate(instance, parent);
                copy.hideFlags = HideFlags.None;
                copy.name = $"Instance-{index++}";
                copy.transform.SetPositionAndRotation(instance.transform.position, instance.transform.rotation);
                var parentScale = parent.lossyScale;
                var scale = instance.transform.lossyScale;
                copy.transform.localScale = new Vector3(scale.x / parentScale.x, scale.y / parentScale.y, scale.z / parentScale.z);
            }

            instantiate.Clear();
            Object.DestroyImmediate(instantiate);
        }

        public static bool TryGetRoadEnd(GameObject road, out Vector2 end, out Vector2 direction)
        {
            end = direction = Vector2.zero;
            var movement = road.GetComponentInChildren<Scripts.LevelObjects.MoveRoads.MoveRoadMovement>(true);
            var container = movement
                ? new SerializedObject(movement).FindProperty("_splineContainer").objectReferenceValue as SplineContainer
                : null;
            if (!container || container.Spline.Count < 2) return false;
            container.Evaluate(1f, out float3 pos, out float3 tangent, out _);
            end = new Vector2(pos.x, pos.y);
            direction = ((Vector2)new Vector2(tangent.x, tangent.y)).normalized;
            return direction.sqrMagnitude > 0.5f;
        }

        private static Vector3 RecipeObjectPosition(Vector2 roomLocal, float z) => new Vector3(roomLocal.x, roomLocal.y, z);

        private static void SetSpline(SpriteShapeController controller, List<Vector2> roomLocalOutline, Vector3 fieldLocalPos)
        {
            var spline = controller.spline;
            spline.Clear();
            for (int i = 0; i < roomLocalOutline.Count; i++)
            {
                var p = roomLocalOutline[i];
                spline.InsertPointAt(i, new Vector3(p.x - fieldLocalPos.x, p.y - fieldLocalPos.y, 0));
                spline.SetTangentMode(i, ShapeTangentMode.Linear);
            }

            spline.isOpenEnded = false;
            controller.RefreshSpriteShape();
        }

        private static Vector2 DefaultDoorPosition(List<Vector2> outline)
        {
            float bestY = float.MinValue;
            Vector2 best = Vector2.zero;
            for (int i = 0; i < outline.Count; i++)
            {
                var a = outline[i];
                var b = outline[(i + 1) % outline.Count];
                var dir = b - a;
                if (dir.magnitude < 2f || Mathf.Abs(dir.normalized.y) > 0.3f) continue;
                var mid = (a + b) * 0.5f;
                if (mid.y <= bestY) continue;
                bestY = mid.y;
                best = mid;
            }

            return best + Vector2.up * DoorOutwardOffset;
        }

        private static void ClearChildren(Transform parent, string keepName)
        {
            if (!parent) return;
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                var child = parent.GetChild(i);
                if (child.name == keepName) continue;
                Object.DestroyImmediate(child.gameObject);
            }
        }

        public static float SignedArea(List<Vector2> points)
        {
            float area = 0;
            for (int i = 0; i < points.Count; i++)
            {
                var a = points[i];
                var b = points[(i + 1) % points.Count];
                area += a.x * b.y - b.x * a.y;
            }

            return area * 0.5f;
        }

        private static void RefreshWalls(string prefabPath)
        {
            var refresher = System.Type.GetType("RoomPrefabRefresher, Assembly-CSharp-Editor");
            var method = refresher?.GetMethod("RefreshRoomPrefab", BindingFlags.NonPublic | BindingFlags.Static);
            if (method == null)
            {
                Debug.LogError("[LevelLab] RoomPrefabRefresher.RefreshRoomPrefab not found; walls not generated");
                return;
            }

            var args = new object[] { prefabPath, 0 };
            var result = method.Invoke(null, args);
            if (result.ToString() != "Refreshed")
                Debug.LogWarning($"[LevelLab] Wall refresh for {prefabPath}: {result}");
        }
    }
}
#endif
