using System;
using UnityEngine;

namespace Scripts.LevelSystem.LevelGeneration
{
    [Serializable]
    public class RoomModel
    {
        public int RoomId;
        public float SpawnChance;
        public bool isEmptyRoom;
        public Room RoomPrefab;
    }
}