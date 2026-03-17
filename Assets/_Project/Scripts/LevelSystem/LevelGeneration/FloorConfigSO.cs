using System.Collections.Generic;
using Scripts.LevelSystem.LevelObjects;
using Scripts.LevelSystem.TechnicalScripts;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Scripts.LevelSystem.LevelGeneration
{
    [CreateAssetMenu(fileName = "NewFloorConfig", menuName = "Game/Floor Config")]
    public class FloorConfigSO : ScriptableObject
    {
        public List<RoomModel> RoomModels;
        [SerializeField] private List<Room> _bossRoomPrefabs;
        [SerializeField] private int _roomCount = 5;
        [SerializeField] private Color _backgroundColor = new Color(0.129f, 0.086f, 0.251f, 1f);
        [SerializeField] private Color _vignetteColor = new Color(0.063f, 0.012f, 0.188f, 1f);

        public List<Room> BossRoomPrefabs => _bossRoomPrefabs;
        public int RoomCount => _roomCount;
        public Color BackgroundColor => _backgroundColor;
        public Color VignetteColor => _vignetteColor;

        [Button]
        public void RefreshRooms()
        {
            foreach (var roomModel in RoomModels)
            {
                roomModel.RoomPrefab.GetComponent<WallGeneration>().CopySpriteShapeSpline();
                roomModel.RoomPrefab.GetComponent<CornerGenerator>().Generate();
            }
        }
    }
}
