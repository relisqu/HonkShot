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
        [SerializeField] private float _roomHeight = 10f;
        private List<RoomModel> _shuffledRoomModels = new();

        [Space] [Header("Generation Settings")] [SerializeField]
        private int _roomCount = 5;

        private void Awake()
        {
            if (DebugMode.Instance.GeneratingLevels)
            {
            }
        }


        public Floor GenerateFloor()
        {
            return GenerateFloor(_roomCount);
        }


        public Floor GenerateFloor(int roomCount)
        {
            var rooms = new List<Room>();
            ShuffleRoomModels();
            Vector3 spawnPosition = Vector3.zero;
            for (int i = 0; i < roomCount && i < _shuffledRoomModels.Count; i++)
            {
                var model = _shuffledRoomModels[i];
                if (!model.RoomPrefab) continue;
                Room roomInstance = Instantiate(model.RoomPrefab, spawnPosition, Quaternion.identity, transform);
                if (rooms.Count > 0) rooms[^1].SetNextRoom(roomInstance);
                rooms.Add(roomInstance);
            }

            foreach (var room in rooms)
            {
                room.gameObject.SetActive(false);
            }

            return new Floor()
            {
                Rooms = rooms
            };
        }

        private void ShuffleRoomModels()
        {
            if (_shuffledRoomModels.Count == 0)
            {
                _shuffledRoomModels.Clear();
                _shuffledRoomModels.AddRange(_stagePoolSO.RoomModels);
            }

            var rng = new System.Random();
            int n = _shuffledRoomModels.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                (_shuffledRoomModels[k], _shuffledRoomModels[n]) = (_shuffledRoomModels[n], _shuffledRoomModels[k]);
            }
        }
    }
}