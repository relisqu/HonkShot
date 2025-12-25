using System;

namespace Scripts.Progress
{
    public class ProgressData
    {
        public TutorialStatus TutorialCompletedStatus;

        public enum TutorialStatus
        {
            NotStarted,
            InProcess, 
            Completed
        }
    }
}