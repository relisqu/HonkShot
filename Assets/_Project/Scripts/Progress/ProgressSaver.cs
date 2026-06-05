using Scripts.ScoreSystem;
using Zenject;

namespace Scripts.Progress
{
    public class ProgressSaver : IInitializable
    {
        private ProgressData _progressData;
        public ProgressData ProgressData => _progressData;
        public ScoreData ScoreData => _progressData.ScoreData;

        public void SaveProgress()
        {
            ES3.Save("saveFile", _progressData);
        }

        public void LoadProgress()
        {
            _progressData = ES3.Load<ProgressData>("saveFile", new ProgressData());
        }

        public void Initialize()
        {
            LoadProgress();
        }

        public void StartTutorial()
        {
            _progressData.TutorialCompletedStatus = ProgressData.TutorialStatus.InProcess;
        }

        public void CompleteTutorial()
        {
            _progressData.TutorialCompletedStatus = ProgressData.TutorialStatus.Completed;
            SaveProgress();
        }

        public void ResetTutorial()
        {
            _progressData.TutorialCompletedStatus = ProgressData.TutorialStatus.NotStarted;
            SaveProgress();
        }

        public void AddRunScore(long runScore)
        {
            _progressData.ScoreData.TotalScore += runScore;
            if (runScore > _progressData.ScoreData.HighestRunScore)
            {
                _progressData.ScoreData.HighestRunScore = runScore;
            }
            SaveProgress();
        }

        public void AddEnemyKilled()
        {
            _progressData.ScoreData.TotalEnemiesKilled++;
        }

        public bool SpendScore(long amount)
        {
            if (_progressData.ScoreData.TotalScore >= amount)
            {
                _progressData.ScoreData.TotalScore -= amount;
                SaveProgress();
                return true;
            }
            return false;
        }

        public long GetTotalScore()
        {
            return _progressData.ScoreData.TotalScore;
        }
    }
}