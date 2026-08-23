using System.Collections.Generic;
using Scripts.LevelSystem.LevelGeneration;
using Scripts.LevelSystem.LevelObjects;
using Scripts.LevelSystem.TechnicalScripts;
using UnityEditor;
using UnityEngine;

// Rebuilds generated wall / corner objects INSIDE room prefab assets.
// Generation must never run straight on a prefab asset: Unity refuses to parent new objects into
// one, so the generators would dump their output into the open scene instead. Every room is opened
// with PrefabUtility.LoadPrefabContents, regenerated there and saved back.
[InitializeOnLoad]
public static class RoomPrefabRefresher
{
    private const string SelectionMenu = "Assets/Rooms/Refresh Selected Room Prefabs";

    static RoomPrefabRefresher()
    {
        FloorConfigSO.RefreshRoomsRequested -= RefreshFloor;
        FloorConfigSO.RefreshRoomsRequested += RefreshFloor;

        RunConfigSO.RefreshRoomsRequested -= RefreshRun;
        RunConfigSO.RefreshRoomsRequested += RefreshRun;
    }

    private static void RefreshRun(RunConfigSO run)
    {
        if (!run)
        {
            Debug.LogWarning("[RoomPrefabRefresher] Run config is null.");
            return;
        }

        var paths = new List<string>();
        int floors = 0;

        foreach (var floor in run.Floors)
        {
            if (!floor) continue;
            floors++;
            CollectRoomPaths(floor, paths);
        }

        if (paths.Count == 0)
        {
            Debug.LogWarning($"[RoomPrefabRefresher] '{run.name}' has no room prefabs across its {floors} floor(s).", run);
            return;
        }

        RefreshPaths(paths, $"{run.name} ({floors} floors)", run);
    }

    private static void RefreshFloor(FloorConfigSO config)
    {
        if (!config)
        {
            Debug.LogWarning("[RoomPrefabRefresher] Config is null.");
            return;
        }

        var paths = new List<string>();
        CollectRoomPaths(config, paths);

        if (paths.Count == 0)
        {
            Debug.LogWarning($"[RoomPrefabRefresher] '{config.name}' has no room prefabs assigned.", config);
            return;
        }

        RefreshPaths(paths, config.name, config);
    }

    private static void CollectRoomPaths(FloorConfigSO config, List<string> paths)
    {
        foreach (var room in config.AllRoomPrefabs())
        {
            string path = AssetDatabase.GetAssetPath(room);
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogError($"[RoomPrefabRefresher] '{room.name}' is not a prefab asset — assign the prefab, not a scene object.", room);
                continue;
            }

            if (!paths.Contains(path)) paths.Add(path);
        }
    }

    // Rooms that no FloorConfig references can only be reached this way: select the prefabs
    // (or their folders) in the Project window and run this.
    [MenuItem(SelectionMenu)]
    private static void RefreshSelection()
    {
        var paths = CollectSelectedPrefabPaths();
        if (paths.Count == 0) return;

        if (!EditorUtility.DisplayDialog(
                "Refresh Room Prefabs",
                $"Rebuild walls and corner decorations inside {paths.Count} prefab(s)?\n\n" +
                "Prefabs without a generator are skipped. Existing generated objects are replaced, " +
                "so per-object tweaks are lost.",
                "Refresh", "Cancel"))
            return;

        RefreshPaths(paths, "selection", null);
    }

    [MenuItem(SelectionMenu, true)]
    private static bool RefreshSelectionValidate() => CollectSelectedPrefabPaths().Count > 0;

    private static List<string> CollectSelectedPrefabPaths()
    {
        var paths = new List<string>();
        var folders = new List<string>();

        foreach (var obj in Selection.GetFiltered<Object>(SelectionMode.Assets))
        {
            string path = AssetDatabase.GetAssetPath(obj);
            if (string.IsNullOrEmpty(path)) continue;

            if (AssetDatabase.IsValidFolder(path)) folders.Add(path);
            else if (path.EndsWith(".prefab", System.StringComparison.OrdinalIgnoreCase) && !paths.Contains(path))
                paths.Add(path);
        }

        if (folders.Count > 0)
        {
            foreach (var guid in AssetDatabase.FindAssets("t:Prefab", folders.ToArray()))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!paths.Contains(path)) paths.Add(path);
            }
        }

        return paths;
    }

    private static void RefreshPaths(List<string> paths, string label, Object context)
    {
        int refreshed = 0;
        int noGenerators = 0;
        int locked = 0;
        int failed = 0;

        try
        {
            for (int i = 0; i < paths.Count; i++)
            {
                if (EditorUtility.DisplayCancelableProgressBar(
                        $"Refreshing rooms of '{label}'",
                        paths[i],
                        (float)i / paths.Count))
                    break;

                switch (RefreshRoomPrefab(paths[i], out int lockedInRoom))
                {
                    case Result.Refreshed:
                        refreshed++;
                        locked += lockedInRoom;
                        break;
                    case Result.NoGenerators:
                        noGenerators++;
                        break;
                    case Result.AllLocked:
                        locked += lockedInRoom;
                        break;
                    default:
                        failed++;
                        break;
                }
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }

        AssetDatabase.SaveAssets();

        string tail = (noGenerators > 0 ? $", {noGenerators} without generators" : "") +
                      (locked > 0 ? $", {locked} locked generator(s) left untouched" : "") +
                      (failed > 0 ? $", {failed} failed — see errors above" : "");

        if (failed > 0)
            Debug.LogWarning($"[RoomPrefabRefresher] '{label}': refreshed {refreshed} room prefab(s){tail}.", context);
        else
            Debug.Log($"[RoomPrefabRefresher] '{label}': refreshed {refreshed} room prefab(s){tail}.", context);
    }

    private enum Result
    {
        Refreshed,
        NoGenerators,
        AllLocked,
        Failed
    }

    private static Result RefreshRoomPrefab(string path, out int lockedSkipped)
    {
        lockedSkipped = 0;
        GameObject contents = null;

        try
        {
            contents = PrefabUtility.LoadPrefabContents(path);

            var wallGenerators = contents.GetComponentsInChildren<WallGeneration>(true);
            var innerWallGenerators = contents.GetComponentsInChildren<InnerWallGeneration>(true);
            var cornerGenerators = contents.GetComponentsInChildren<CornerGenerator>(true);

            if (wallGenerators.Length == 0 && innerWallGenerators.Length == 0 && cornerGenerators.Length == 0)
                return Result.NoGenerators;

            int applied = 0;

            foreach (var wallGeneration in wallGenerators)
            {
                if (wallGeneration.IsLocked)
                {
                    lockedSkipped++;
                    continue;
                }

                wallGeneration.CopySpriteShapeSpline();
                wallGeneration.GenerateWalls();
                applied++;
            }

            // Islands inside a room. Trees are deliberately left alone: they are random and get
            // rebuilt per floor at runtime from FloorConfig.Decorations, so baking them into a
            // room prefab that several floors share would be wrong.
            foreach (var innerWallGeneration in innerWallGenerators)
            {
                if (innerWallGeneration.IsLocked)
                {
                    lockedSkipped++;
                    continue;
                }

                innerWallGeneration.CopySpriteShapeSplineToHelper();
                innerWallGeneration.GenerateWalls();
                applied++;
            }

            foreach (var cornerGenerator in cornerGenerators)
            {
                cornerGenerator.Generate();
                applied++;
            }

            if (applied == 0) return Result.AllLocked;

            PrefabUtility.SaveAsPrefabAsset(contents, path, out bool saved);
            if (!saved)
            {
                Debug.LogError($"[RoomPrefabRefresher] Failed to save '{path}'.");
                return Result.Failed;
            }

            return Result.Refreshed;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[RoomPrefabRefresher] '{path}' failed: {e}");
            return Result.Failed;
        }
        finally
        {
            if (contents) PrefabUtility.UnloadPrefabContents(contents);
        }
    }
}
