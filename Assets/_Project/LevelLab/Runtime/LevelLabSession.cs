#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Scripts.LevelSystem.LevelGeneration;
using Scripts.Other;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Scripts.LevelLab
{
    // Lives across scene reloads during a LevelLab playtest run. Each room gets a fresh GameplayScene whose
    // LevelManager is pointed at an in-memory one-room floor, so rooms are measured independently.
    public class LevelLabSession : MonoBehaviour
    {
        public const string ActiveKey = "LevelLab.Active";
        public const string QueueKey = "LevelLab.Queue";
        public const string RunNameKey = "LevelLab.RunName";
        public const string RepeatsKey = "LevelLab.Repeats";
        public const string ReportsFolder = "Assets/_Project/LevelLab/Reports";
        private const string TemplateFloorPath = "Assets/_Project/Resources/Data/Floors/FloorConfig_01.asset";

        public float RoomEnterTimeout = 20f;
        public float DelayBeforeReload = 0.4f;

        private readonly List<(string path, int attempt)> _queue = new();
        private int _index;
        private string _runName;
        private readonly LevelLabRunResults _results = new();
        private bool _roomRunning;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Boot()
        {
            if (!SessionState.GetBool(ActiveKey, false)) return;
            var go = new GameObject("LevelLabSession");
            DontDestroyOnLoad(go);
            go.AddComponent<LevelLabSession>();
        }

        private void Awake()
        {
            _runName = SessionState.GetString(RunNameKey, "run");
            int repeats = Mathf.Max(1, SessionState.GetInt(RepeatsKey, 1));
            var paths = SessionState.GetString(QueueKey, "").Split('|').Where(p => !string.IsNullOrEmpty(p)).ToList();
            foreach (var path in paths)
                for (int i = 0; i < repeats; i++)
                    _queue.Add((path, i));

            _results.runName = _runName;
            Debug.Log($"[LevelLab] Session '{_runName}' starting: {paths.Count} rooms x {repeats}");
            SceneManager.sceneLoaded += SceneManager_SceneLoaded;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= SceneManager_SceneLoaded;
        }

        private void SceneManager_SceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (_roomRunning) return;
            if (_index >= _queue.Count)
            {
                Finish();
                return;
            }

            var levelManager = FindObjectOfType<LevelManager>();
            if (!levelManager)
            {
                Debug.LogError("[LevelLab] No LevelManager in the loaded scene; open GameplayScene before starting a run");
                Finish();
                return;
            }

            var (path, attempt) = _queue[_index];
            var roomPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            var room = roomPrefab ? roomPrefab.GetComponent<Room>() : null;
            if (!room)
            {
                Record(new LevelLabRoomResult { room = Path.GetFileNameWithoutExtension(path), prefabPath = path, attempt = attempt, failure = "prefab missing" });
                _index++;
                Reload();
                return;
            }

            if (DebugMode.Instance) DebugMode.Instance.GeneratingLevels = true;
            SetField(levelManager, "_runConfig", BuildRunConfig(room));
            StartCoroutine(RunRoom(room.name, path, attempt));
        }

        private RunConfigSO BuildRunConfig(Room roomPrefab)
        {
            var template = AssetDatabase.LoadAssetAtPath<FloorConfigSO>(TemplateFloorPath);
            var floor = Instantiate(template);
            floor.RoomModels = new List<RoomModel> { new RoomModel { RoomPrefab = roomPrefab, SpawnChance = 1, DifficultyValue = 1 } };
            SetField(floor, "_useAdvancedGeneration", false);
            SetField(floor, "_roomCount", 1);
            SetField(floor, "_bossRoomPrefabs", new List<Room>());

            var run = ScriptableObject.CreateInstance<RunConfigSO>();
            SetField(run, "_floors", new List<FloorConfigSO> { floor });
            return run;
        }

        private IEnumerator RunRoom(string roomName, string path, int attempt)
        {
            _roomRunning = true;
            var result = new LevelLabRoomResult { room = roomName, prefabPath = path, attempt = attempt };

            float waitStart = Time.realtimeSinceStartup;
            Room current = null;
            while (Time.realtimeSinceStartup - waitStart < RoomEnterTimeout)
            {
                current = LevelManager.Instance ? SafeCurrentRoom() : null;
                if (current) break;
                yield return null;
            }

            if (!current)
            {
                result.failure = "room never entered (tutorial not completed?)";
            }
            else
            {
                var bot = gameObject.AddComponent<LevelLabBot>();
                bot.Begin(current, result);
                while (!bot.Finished) yield return null;
                Destroy(bot);
            }

            Record(result);
            Debug.Log($"[LevelLab] {roomName}#{attempt}: cleared={result.cleared} time={result.realTime:0.0}s throws={result.throws} " +
                      $"mid={result.midFlightThrows} avgSpeed={result.avgBallSpeed:0.0} drops={result.sharpDrops} stuck={result.stuckEvents} " +
                      $"dead={result.deadTime:0.0}s dmg={result.damageTaken:0} {result.failure}");

            _index++;
            yield return new WaitForSecondsRealtime(DelayBeforeReload);
            _roomRunning = false;
            Reload();
        }

        private static Room SafeCurrentRoom()
        {
            try
            {
                return LevelManager.Instance.CurrentRoom;
            }
            catch
            {
                return null;
            }
        }

        private void Reload()
        {
            if (TimeManager.Instance) TimeManager.Instance.ResumeGame();
            if (_index >= _queue.Count)
            {
                Finish();
                return;
            }

            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private void Record(LevelLabRoomResult result)
        {
            _results.results.Add(result);
            Directory.CreateDirectory(ReportsFolder);
            File.WriteAllText(Path.Combine(ReportsFolder, _runName + ".json"), JsonUtility.ToJson(_results, true));
        }

        private void Finish()
        {
            Debug.Log($"[LevelLab] Session '{_runName}' finished: {_results.results.Count} results -> {ReportsFolder}/{_runName}.json");
            SessionState.SetBool(ActiveKey, false);
            EditorApplication.isPlaying = false;
        }

        private static void SetField(object target, string field, object value)
        {
            var info = target.GetType().GetField(field, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            if (info == null)
            {
                Debug.LogError($"[LevelLab] Field {field} not found on {target.GetType().Name}");
                return;
            }

            info.SetValue(target, value);
        }
    }
}
#endif
