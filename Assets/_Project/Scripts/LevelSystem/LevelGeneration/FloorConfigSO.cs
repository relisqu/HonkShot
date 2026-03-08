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

        public List<Room> BossRoomPrefabs => _bossRoomPrefabs;
        public int RoomCount => _roomCount;

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
