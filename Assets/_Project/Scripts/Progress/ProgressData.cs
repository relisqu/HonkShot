using System;
using Scripts.ScoreSystem;

namespace Scripts.Progress
{
    public class ProgressData
    {
        public TutorialStatus TutorialCompletedStatus;
        public ScoreData ScoreData = new ScoreData();

        public enum TutorialStatus
        {
            NotStarted,
            InProcess,
            Completed
        }
    }
}