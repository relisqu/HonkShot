using System.Collections.Generic;
using UnityEngine;

namespace Scripts.LevelSystem.LevelGeneration
{
    public class LevelGenerator : MonoBehaviour
    {
        [SerializeField] private StagePoolSO _stagePoolSO;
        [SerializeField] private int _roomCount = 5;
        [SerializeField] private float _roomHeight = 10f;

        private List<Room> _spawnedRooms = new List<Room>();
        private bool _levelLocked = true;

        private void Awake()
        {
            GenerateLevel();
        }

        private void GenerateLevel()
        {
            Vector3 spawnPosition = Vector3.zero;

            for (int i = 0; i < _roomCount; i++)
            {
                var selectedRoom = GetRandomRoom();
                if (selectedRoom == null) continue;

                Room roomInstance = Instantiate(selectedRoom, spawnPosition, Quaternion.identity, transform);
                if (_spawnedRooms.Count > 0) _spawnedRooms[^1].SetNextRoom(roomInstance);
                _spawnedRooms.Add(roomInstance);
                spawnPosition += Vector3.up * _roomHeight;
            }
            AstarPath.active.Scan();
            foreach (var room in _spawnedRooms)
            {
                room.gameObject.SetActive(false);
            }
        }

        private Room GetRandomRoom()
        {
            float totalChance = 0f;
            foreach (var entry in _stagePoolSO.RoomModels)
            {
                totalChance += entry.SpawnChance;
            }

            float randomPoint = Random.value * totalChance;
            float cumulativeChance = 0f;

            foreach (var entry in _stagePoolSO.RoomModels)
            {
                cumulativeChance += entry.SpawnChance;
                if (randomPoint <= cumulativeChance)
                {
                    return entry.RoomPrefab;
                }
            }

            return null;
        }

        private void UnlockLevel()
        {
            _levelLocked = false;
            Debug.Log("Level unlocked!");
        }

        public Room GetRoom(int i)
        {
            return _spawnedRooms[i];
        }
    }
}