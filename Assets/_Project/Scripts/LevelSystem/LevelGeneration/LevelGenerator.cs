using System;
using System.Collections.Generic;
using Scripts.Other;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Scripts.LevelSystem.LevelGeneration
{
    public class LevelGenerator : MonoBehaviour
    {
        [SerializeField] private StagePoolSO _tutorialPoolSO;
        [SerializeField] private StagePoolSO _stagePoolSO;
        [SerializeField] private int _roomCount = 5;
        [SerializeField] private float _roomHeight = 10f;

        private List<Room> _spawnedRooms = new List<Room>();
        private List<RoomModel> _shuffledRoomModels = new List<RoomModel>();
        private bool _levelLocked = true;
        public IReadOnlyList<Room> Rooms => _spawnedRooms;

        private void Awake()
        {
            if (DebugMode.Instance.GeneratingLevels)
            {
                GenerateLevel();
            }
        }

        public void GenerateLevel()
        {
            ClearRooms();
            ShuffleRoomModels();
            Vector3 spawnPosition = Vector3.zero;
            for (int i = 0; i < _roomCount && i < _shuffledRoomModels.Count; i++)
            {
                var model = _shuffledRoomModels[i];
                if (!model.RoomPrefab) continue;
                Room roomInstance = Instantiate(model.RoomPrefab, spawnPosition, Quaternion.identity, transform);
                if (_spawnedRooms.Count > 0) _spawnedRooms[^1].SetNextRoom(roomInstance);
                _spawnedRooms.Add(roomInstance);
            }
            foreach (var room in _spawnedRooms)
            {
                room.gameObject.SetActive(false);
            }
        }

        private void ShuffleRoomModels()
        {
            _shuffledRoomModels.Clear();
            _shuffledRoomModels.AddRange(_stagePoolSO.RoomModels);
            var rng = new System.Random();
            int n = _shuffledRoomModels.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                (_shuffledRoomModels[k], _shuffledRoomModels[n]) = (_shuffledRoomModels[n], _shuffledRoomModels[k]);
            }
        }

        public void ClearRooms()
        {
            foreach (var room in _spawnedRooms)
            {
                if (room != null)
                    Destroy(room.gameObject);
            }
            _spawnedRooms.Clear();
        }

        private void UnlockLevel()
        {
            _levelLocked = false;
            Debug.Log("Level unlocked!");
        }

        public Room GetRoom(int i)
        {
            if (_spawnedRooms.Count > i)
                return _spawnedRooms[i];
            return null;
        }
    }
}