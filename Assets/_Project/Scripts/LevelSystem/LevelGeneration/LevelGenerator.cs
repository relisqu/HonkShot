using System;
using System.Collections.Generic;
using System.Linq;
using Scripts.LevelSystem.LevelGeneration.Factories;
using Scripts.Other;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

namespace Scripts.LevelSystem.LevelGeneration
{
    public class LevelGenerator : MonoBehaviour
    {
        [Inject] private LevelObjectsFactory _factory;
        [SerializeField] private FloorConfigSO _defaultFloorConfig;
        [SerializeField] private float _roomHeight = 10f;
        private List<RoomModel> _shuffledRoomModels = new();

        private void Awake()
        {
            if (DebugMode.Instance.GeneratingLevels)
            {
            }
        }


        public Floor GenerateFloor()
        {
            return GenerateFloor(_defaultFloorConfig);
        }


        public Floor GenerateFloor(Room[] rooms)
        {
            return new Floor()
            {
                Rooms = rooms.ToList()
            };
        }

        public Floor GenerateFloor(FloorConfigSO floorConfig)
        {
            var rooms = new List<Room>();
            ShuffleRoomModels(floorConfig);
            Vector3 spawnPosition = Vector3.zero;

            int roomCount = floorConfig.RoomCount;
            for (int i = 0; i < roomCount && i < _shuffledRoomModels.Count; i++)
            {
                var model = _shuffledRoomModels[i];
                if (!model.RoomPrefab) continue;
                Room roomInstance = _factory.SpawnRoom(model.RoomPrefab, spawnPosition, transform);
                if (rooms.Count > 0) rooms[^1].SetNextRoom(roomInstance);
                rooms.Add(roomInstance);
            }

            if (floorConfig.BossRoomPrefabs != null && floorConfig.BossRoomPrefabs.Count > 0)
            {
                var bossPrefab = floorConfig.BossRoomPrefabs[Random.Range(0, floorConfig.BossRoomPrefabs.Count)];
                if (bossPrefab)
                {
                    Room bossInstance = _factory.SpawnRoom(bossPrefab, spawnPosition, transform);
                    if (rooms.Count > 0) rooms[^1].SetNextRoom(bossInstance);
                    rooms.Add(bossInstance);
                }
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

        private void ShuffleRoomModels(FloorConfigSO floorConfig)
        {
            _shuffledRoomModels.Clear();
            _shuffledRoomModels.AddRange(floorConfig.RoomModels);

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
