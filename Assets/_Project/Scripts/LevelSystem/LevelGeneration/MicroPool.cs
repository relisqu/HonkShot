using System;
using System.Collections.Generic;
using UnityEngine;

namespace Scripts.LevelSystem.LevelGeneration
{
    [Serializable]
    public class MicroPool
    {
        public List<RoomModel> RoomModels;
        [Min(0)] public int Count;
    }
}
