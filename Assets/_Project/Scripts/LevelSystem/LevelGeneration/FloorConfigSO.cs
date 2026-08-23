using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Scripts.LevelSystem.LevelGeneration
{
    [CreateAssetMenu(fileName = "NewFloorConfig", menuName = "Game/Floor Config")]
    public class FloorConfigSO : ScriptableObject
    {
        [Header("Default Rooms")] public List<RoomModel> RoomModels;
        [Space] 
        
        [SerializeField] private bool _useAdvancedGeneration;
        
        [Space] [SerializeField] private List<Room> _bossRoomPrefabs;

        public int RoomCount => _roomCount;

        [HideIf(nameof(_useAdvancedGeneration))] [SerializeField]
        private int _roomCount = 5;


        [SerializeField, ShowIf(nameof(_useAdvancedGeneration))]
        [InfoBox("$" + nameof(ValidationWarnings), InfoMessageType.Warning, VisibleIf = nameof(HasValidationWarnings))]
        private List<MicroPool> _micropools;

        [SerializeField, Range(0f, 1f), ShowIf(nameof(_useAdvancedGeneration))]
        private float _borrowFromNeighborChance;

        [SerializeField, ShowIf(nameof(_useAdvancedGeneration))]
        private int _difficultyThreshold;

        [SerializeField] private Color _backgroundColor = new Color(0.129f, 0.086f, 0.251f, 1f);
        [SerializeField] private Color _vignetteColor = new Color(0.063f, 0.012f, 0.188f, 1f);
        [SerializeField] private FloorDecorationsSO _decorations;

        public List<Room> BossRoomPrefabs => _bossRoomPrefabs;
        public bool UseAdvancedGeneration => _useAdvancedGeneration;
        public List<MicroPool> Micropools => _micropools;
        public float BorrowFromNeighborChance => _borrowFromNeighborChance;
        public int DifficultyThreshold => _difficultyThreshold;
        public Color BackgroundColor => _backgroundColor;
        public Color VignetteColor => _vignetteColor;
        public FloorDecorationsSO Decorations => _decorations;

        [ShowInInspector, ReadOnly, ShowIf(nameof(_useAdvancedGeneration))]
        public int MicropoolTotalSlots => _micropools?.Sum(p => p?.Count ?? 0) ?? 0;

        // Handled by RoomPrefabRefresher in the Editor assembly, which is the only place allowed to
        // write into prefab assets. Nothing subscribes at runtime.
        public static System.Action<FloorConfigSO> RefreshRoomsRequested;

        public IEnumerable<Room> AllRoomPrefabs()
        {
            var seen = new HashSet<Room>();

            if (RoomModels != null)
            {
                foreach (var model in RoomModels)
                {
                    var prefab = model?.RoomPrefab;
                    if (prefab && seen.Add(prefab)) yield return prefab;
                }
            }

            if (_micropools != null)
            {
                foreach (var pool in _micropools)
                {
                    if (pool?.RoomModels == null) continue;
                    foreach (var model in pool.RoomModels)
                    {
                        var prefab = model?.RoomPrefab;
                        if (prefab && seen.Add(prefab)) yield return prefab;
                    }
                }
            }

            if (_bossRoomPrefabs != null)
            {
                foreach (var prefab in _bossRoomPrefabs)
                {
                    if (prefab && seen.Add(prefab)) yield return prefab;
                }
            }
        }

        [Button(ButtonSizes.Large), PropertyOrder(-1)]
        [InfoBox("Rebuilds walls and corner decorations inside every room prefab of this floor and saves them. " +
                 "Run this after changing the Wall prefab or a room's spline.")]
        public void RefreshRooms()
        {
            if (Application.isPlaying)
            {
                Debug.LogWarning("[FloorConfigSO] RefreshRooms only works in Edit Mode — prefab assets cannot be written during Play Mode.");
                return;
            }

            if (RefreshRoomsRequested == null)
            {
                Debug.LogError("[FloorConfigSO] RoomPrefabRefresher is not loaded — is Assets/_Project/Scripts/Editor/RoomPrefabRefresher.cs compiling?");
                return;
            }

            RefreshRoomsRequested.Invoke(this);
        }

        public static System.Action<FloorConfigSO> PreviewRequested;

        [Button("Preview Floor")]
        public void PreviewFloor()
        {
            PreviewRequested?.Invoke(this);
        }

        private bool HasValidationWarnings => !string.IsNullOrEmpty(ValidationWarnings);

        private string ValidationWarnings
        {
            get
            {
                if (_micropools == null || _micropools.Count == 0)
                    return "No micropools configured — advanced generation is on but the list is empty.";

                var sb = new StringBuilder();
                for (int i = 0; i < _micropools.Count; i++)
                {
                    var pool = _micropools[i];
                    if (pool == null || pool.RoomModels == null || pool.RoomModels.Count == 0)
                        sb.AppendLine($"Micropool {i}: no RoomModels.");
                    if (pool != null && pool.Count <= 0)
                        sb.AppendLine($"Micropool {i}: Count = {pool.Count}.");

                    if (pool?.RoomModels == null) continue;
                    foreach (var room in pool.RoomModels)
                    {
                        if (room == null) continue;
                        if (room.DifficultyValue < -5 || room.DifficultyValue > 10)
                            sb.AppendLine($"Micropool {i}: room '{(room.RoomPrefab ? room.RoomPrefab.name : "null")}' has DifficultyValue {room.DifficultyValue} outside [-5, 10].");
                    }
                }

                if (_borrowFromNeighborChance > 0f && _micropools.Count < 2)
                    sb.AppendLine($"BorrowFromNeighborChance > 0 but only {_micropools.Count} pool(s) — no swaps will happen.");

                return sb.ToString().TrimEnd();
            }
        }
    }
}
