#if UNITY_EDITOR
using System;
using System.Collections.Generic;

namespace Scripts.LevelLab
{
    [Serializable]
    public class LevelLabRoomResult
    {
        public string room;
        public string prefabPath;
        public int attempt;
        public bool cleared;
        public bool timeout;
        public string failure;
        public float realTime;
        public float gameTime;
        public int throws;
        public int midFlightThrows;
        public float avgBallSpeed;
        public int sharpDrops;
        public int stuckEvents;
        public float deadTime;
        public float damageTaken;
        public int enemies;
    }

    [Serializable]
    public class LevelLabRunResults
    {
        public string runName;
        public List<LevelLabRoomResult> results = new List<LevelLabRoomResult>();
    }
}
#endif
