using System.Collections.Generic;
using System.Linq;
using System.Text;
using Scripts.LevelSystem.LevelObjects;
using Scripts.LevelSystem.TechnicalScripts;
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

        [Button]
        public void RefreshRooms()
        {
            foreach (var roomModel in RoomModels)
            {
                roomModel.RoomPrefab.GetComponent<WallGeneration>().CopySpriteShapeSpline();
                roomModel.RoomPrefab.GetComponent<CornerGenerator>().Generate();
            }
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