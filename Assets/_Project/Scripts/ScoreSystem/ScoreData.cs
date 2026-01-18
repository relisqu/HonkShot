using System;

namespace Scripts.ScoreSystem
{
    [Serializable]
    public class ScoreData
    {
        public long TotalScore;
        public long HighestRunScore;
        public int TotalEnemiesKilled;

        public ScoreData()
        {
            TotalScore = 0;
            HighestRunScore = 0;
            TotalEnemiesKilled = 0;
        }
    }
}
