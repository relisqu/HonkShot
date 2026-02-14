using System;
using System.Collections.Generic;
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

        public class DialogProgressData
        {
            public List<string> CompletedConditions;

            public void CompleteFirstFloor()
            {
                string completion = "completed_1";
                if (!CompletedConditions.Contains(completion))
                {
                    CompletedConditions.Add(completion);
                }
            }
        }
    }
}