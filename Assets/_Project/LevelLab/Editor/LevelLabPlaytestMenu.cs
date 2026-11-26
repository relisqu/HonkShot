#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Scripts.LevelLab
{
    [InitializeOnLoad]
    public static class LevelLabPlaytestMenu
    {
        private const string GameplayScenePath = "Assets/_Project/Scenes/GameplayScene.unity";

        static LevelLabPlaytestMenu()
        {
            EditorApplication.playModeStateChanged += EditorApplication_PlayModeStateChanged;
            EditorApplication.update += EditorApplication_Update;
        }

        // An unfocused editor can stop ticking Play Mode; keep unattended runs moving.
        private static void EditorApplication_Update()
        {
            if (EditorApplication.isPlaying && SessionState.GetBool(LevelLabSession.ActiveKey, false))
                EditorApplication.QueuePlayerLoopUpdate();
        }

        private static void EditorApplication_PlayModeStateChanged(PlayModeStateChange change)
        {
            // A run stopped by hand must not hijack the next normal Play.
            if (change == PlayModeStateChange.EnteredEditMode)
                SessionState.SetBool(LevelLabSession.ActiveKey, false);
        }

        [MenuItem("Tools/LevelLab/Playtest Floor 1 Baseline")]
        public static void PlaytestBaseline() => Start(LevelLabValidator.Floor1RoomPaths(), "baseline", 2);

        [MenuItem("Tools/LevelLab/Playtest LevelLab Rooms")]
        public static void PlaytestLevelLab() => Start(LevelLabRoomPaths(), "levellab", 2);

        [MenuItem("Tools/LevelLab/Playtest Selected Rooms")]
        public static void PlaytestSelected()
        {
            var paths = Selection.objects.Select(AssetDatabase.GetAssetPath).Where(p => p.EndsWith(".prefab")).ToList();
            Start(paths, "selected", 1);
        }

        public static List<string> LevelLabRoomPaths()
        {
            return AssetDatabase.FindAssets("t:Prefab", new[] { LevelLabRoomBuilder.OutputFolder })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(p => !Path.GetFileName(p).StartsWith("LL_00"))
                .OrderBy(p => p).ToList();
        }

        public static bool Start(List<string> prefabPaths, string runName, int repeats)
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogError("[LevelLab] Stop Play Mode before starting a playtest run");
                return false;
            }

            if (prefabPaths == null || prefabPaths.Count == 0)
            {
                Debug.LogError("[LevelLab] No rooms to playtest");
                return false;
            }

            var active = EditorSceneManager.GetActiveScene();
            if (active.path != GameplayScenePath)
            {
                if (active.isDirty)
                {
                    Debug.LogError($"[LevelLab] '{active.name}' has unsaved changes; save it or open GameplayScene first");
                    return false;
                }

                EditorSceneManager.OpenScene(GameplayScenePath);
            }

            SessionState.SetString(LevelLabSession.QueueKey, string.Join("|", prefabPaths));
            SessionState.SetString(LevelLabSession.RunNameKey, runName);
            SessionState.SetInt(LevelLabSession.RepeatsKey, repeats);
            SessionState.SetBool(LevelLabSession.ActiveKey, true);
            Debug.Log($"[LevelLab] Starting run '{runName}' with {prefabPaths.Count} rooms x {repeats}");
            EditorApplication.EnterPlaymode();
            return true;
        }
    }
}
#endif
