using System;
using System.Collections.Generic;
using System.Linq;
using Scripts.LevelSystem.LevelGeneration.Factories;
using Scripts.Other;
using UnityEngine;
using Zenject;

namespace Scripts.LevelSystem.LevelGeneration
{
    public class LevelGenerator : MonoBehaviour
    {
        [Inject] private LevelObjectsFactory _factory;
        [SerializeField] private FloorConfigSO _defaultFloorConfig;
        [SerializeField] private float _roomHeight = 10f;

        public float RoomHeight => _roomHeight;

        private void Awake()
        {
            if (DebugMode.Instance.GeneratingLevels)
            {
            }
        }

        public Floor GenerateFloor() => GenerateFloor(_defaultFloorConfig, Environment.TickCount);

        public Floor GenerateFloor(int seed) => GenerateFloor(_defaultFloorConfig, seed);

        public Floor GenerateFloor(FloorConfigSO floorConfig) => GenerateFloor(floorConfig, Environment.TickCount);

        public Floor GenerateFloor(Room[] rooms)
        {
            return new Floor()
            {
                Rooms = rooms.ToList()
            };
        }

        public Floor GenerateFloor(FloorConfigSO floorConfig, int seed)
        {
            var prefabs = SelectFloorPrefabs(floorConfig, seed);
            var rooms = new List<Room>(prefabs.Count);

            foreach (var prefab in prefabs)
            {
                if (!prefab) continue;
                rooms.Add(_factory.SpawnRoom(prefab, Vector3.zero, transform));
            }

            LinkAndDeactivate(rooms);
            return new Floor() { Rooms = rooms };
        }

        public static List<Room> SelectFloorPrefabs(FloorConfigSO floorConfig, int seed)
        {
            var rng = new System.Random(seed);
            bool useMicropools = floorConfig.UseAdvancedGeneration
                && floorConfig.Micropools != null && floorConfig.Micropools.Count > 0;

            var prefabs = useMicropools
                ? SelectMicropoolPrefabs(floorConfig, rng)
                : SelectFlatPrefabs(floorConfig, rng);

            AppendBossPrefab(floorConfig, prefabs, rng);
            return prefabs;
        }

        private static List<Room> SelectFlatPrefabs(FloorConfigSO floorConfig, System.Random rng)
        {
            var prefabs = new List<Room>();
            var shuffled = ShuffleRoomModels(floorConfig.RoomModels, rng);

            int roomCount = floorConfig.RoomCount;
            for (int i = 0; i < roomCount && i < shuffled.Count; i++)
            {
                var model = shuffled[i];
                if (!model.RoomPrefab) continue;
                prefabs.Add(model.RoomPrefab);
            }

            return prefabs;
        }

        private static List<Room> SelectMicropoolPrefabs(FloorConfigSO floorConfig, System.Random rng)
        {
            var prefabs = new List<Room>();
            var picked = new HashSet<Room>();
            var pools = floorConfig.Micropools;
            int remaining = floorConfig.RoomCount;
            float borrowChance = floorConfig.BorrowFromNeighborChance;
            int threshold = floorConfig.DifficultyThreshold;

            for (int poolIdx = 0; poolIdx < pools.Count && remaining > 0; poolIdx++)
            {
                var pool = pools[poolIdx];
                if (pool == null || pool.RoomModels == null || pool.RoomModels.Count == 0) continue;

                var prevPool = poolIdx > 0 ? pools[poolIdx - 1] : null;
                var nextPool = poolIdx < pools.Count - 1 ? pools[poolIdx + 1] : null;
                float poolAvg = (float)pool.RoomModels.Average(r => r != null ? r.DifficultyValue : 0);
                int slotsToFill = Mathf.Min(pool.Count, remaining);

                for (int slotIdx = 0; slotIdx < slotsToFill; slotIdx++)
                {
                    bool isFirstSlot = slotIdx == 0;
                    bool isLastSlot = slotIdx == slotsToFill - 1;

                    MicroPool sourcePool = pool;
                    bool applyDiffFilter = false;

                    if (isFirstSlot && prevPool != null && rng.NextDouble() < borrowChance
                        && HasEligibleCandidate(prevPool, poolAvg, threshold, picked))
                    {
                        sourcePool = prevPool;
                        applyDiffFilter = true;
                    }
                    else if (isLastSlot && nextPool != null && rng.NextDouble() < borrowChance
                        && HasEligibleCandidate(nextPool, poolAvg, threshold, picked))
                    {
                        sourcePool = nextPool;
                        applyDiffFilter = true;
                    }

                    var pickedModel = PickRoom(sourcePool, poolAvg, threshold, picked, applyDiffFilter, rng);

                    if (pickedModel == null && sourcePool != pool)
                        pickedModel = PickRoom(pool, poolAvg, threshold, picked, false, rng);

                    if (pickedModel == null)
                    {
                        Debug.LogWarning($"[LevelGenerator] Pool {poolIdx} exhausted at slot {slotIdx}; skipping slot.");
                        continue;
                    }

                    picked.Add(pickedModel.RoomPrefab);
                    prefabs.Add(pickedModel.RoomPrefab);
                    remaining--;

                    if (remaining == 0) break;
                }
            }

            if (prefabs.Count < floorConfig.RoomCount)
                Debug.LogWarning($"[LevelGenerator] Generated {prefabs.Count} rooms, requested {floorConfig.RoomCount}. Pools may need more rooms.");

            return prefabs;
        }

        private static bool HasEligibleCandidate(MicroPool source, float referenceAvg, int threshold, HashSet<Room> picked)
        {
            if (source?.RoomModels == null) return false;
            foreach (var room in source.RoomModels)
            {
                if (room == null || !room.RoomPrefab) continue;
                if (picked.Contains(room.RoomPrefab)) continue;
                if (Mathf.Abs(room.DifficultyValue - referenceAvg) <= threshold) return true;
            }
            return false;
        }

        private static RoomModel PickRoom(MicroPool source, float referenceAvg, int threshold, HashSet<Room> picked, bool applyDiffFilter, System.Random rng)
        {
            if (source?.RoomModels == null || source.RoomModels.Count == 0) return null;

            var candidates = new List<RoomModel>();
            float totalWeight = 0f;
            foreach (var room in source.RoomModels)
            {
                if (room == null || !room.RoomPrefab) continue;
                if (picked.Contains(room.RoomPrefab)) continue;
                if (applyDiffFilter && Mathf.Abs(room.DifficultyValue - referenceAvg) > threshold) continue;
                candidates.Add(room);
                totalWeight += Mathf.Max(0f, room.SpawnChance);
            }

            if (candidates.Count == 0) return null;
            if (totalWeight <= 0f) return candidates[rng.Next(0, candidates.Count)];

            float roll = (float)rng.NextDouble() * totalWeight;
            float cumulative = 0f;
            foreach (var room in candidates)
            {
                cumulative += Mathf.Max(0f, room.SpawnChance);
                if (roll <= cumulative) return room;
            }
            return candidates[^1];
        }

        private static void AppendBossPrefab(FloorConfigSO floorConfig, List<Room> prefabs, System.Random rng)
        {
            if (floorConfig.BossRoomPrefabs == null || floorConfig.BossRoomPrefabs.Count == 0) return;
            var bossPrefab = floorConfig.BossRoomPrefabs[rng.Next(0, floorConfig.BossRoomPrefabs.Count)];
            if (!bossPrefab) return;
            prefabs.Add(bossPrefab);
        }

        private static void LinkAndDeactivate(List<Room> rooms)
        {
            for (int i = 1; i < rooms.Count; i++)
                rooms[i - 1].SetNextRoom(rooms[i]);

            foreach (var room in rooms)
                room.gameObject.SetActive(false);
        }

        private static List<RoomModel> ShuffleRoomModels(List<RoomModel> source, System.Random rng)
        {
            var shuffled = new List<RoomModel>(source);
            int n = shuffled.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                (shuffled[k], shuffled[n]) = (shuffled[n], shuffled[k]);
            }
            return shuffled;
        }
    }
}
