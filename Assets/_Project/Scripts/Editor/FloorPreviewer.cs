using Scripts.LevelSystem.LevelGeneration;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class FloorPreviewer
{
    private const string PreviewParentName = "FloorPreview";
    private const float PreviewLifetime = 20f;
    private const float DefaultRoomHeight = 10f;

    private static double _spawnTime;
    private static bool _isActive;

    static FloorPreviewer()
    {
        EditorApplication.update += OnEditorUpdate;
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
        FloorConfigSO.PreviewRequested -= Show;
        FloorConfigSO.PreviewRequested += Show;
    }

    public static void Show(FloorConfigSO config)
    {
        if (!config)
        {
            Debug.LogWarning("[FloorPreviewer] Config is null.");
            return;
        }

        Cleanup();

        var prefabs = LevelGenerator.SelectFloorPrefabs(config, System.Environment.TickCount);
        if (prefabs == null || prefabs.Count == 0)
        {
            Debug.LogWarning($"[FloorPreviewer] No rooms selected for '{config.name}'.");
            return;
        }

        var parent = new GameObject(PreviewParentName) { hideFlags = HideFlags.DontSaveInEditor };
        float roomHeight = ResolveRoomHeight();
        Vector3 pos = Vector3.zero;

        foreach (var prefab in prefabs)
        {
            if (!prefab) continue;
            var go = PrefabUtility.InstantiatePrefab(prefab.gameObject, parent.transform) as GameObject;
            if (!go) continue;
            go.transform.position = pos;
            go.hideFlags = HideFlags.DontSaveInEditor;
            pos += Vector3.up * roomHeight;
        }

        Selection.activeGameObject = parent;
        if (SceneView.lastActiveSceneView)
            SceneView.lastActiveSceneView.FrameSelected();

        _spawnTime = EditorApplication.timeSinceStartup;
        _isActive = true;
        Debug.Log($"[FloorPreviewer] Spawned {prefabs.Count} rooms from '{config.name}'. Auto-cleanup in {PreviewLifetime:F0}s.");
    }

    public static void Cleanup()
    {
        var existing = GameObject.Find(PreviewParentName);
        if (existing) Object.DestroyImmediate(existing);
        _isActive = false;
    }

    private static void OnEditorUpdate()
    {
        if (!_isActive) return;
        double elapsed = EditorApplication.timeSinceStartup - _spawnTime;
        if (elapsed >= PreviewLifetime) Cleanup();
    }

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode) Cleanup();
    }

    private static float ResolveRoomHeight()
    {
        var generator = Object.FindObjectOfType<LevelGenerator>();
        return generator ? generator.RoomHeight : DefaultRoomHeight;
    }
}
