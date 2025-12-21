namespace Scripts.Progress
{
    public class ProgressSaver
    {
        private ProgressData _progressData;
        public ProgressData ProgressData => _progressData;

        public void SaveProgress()
        {
            ES3.Save("saveFile", _progressData);
        }

        public void LoadProgress()
        {
            _progressData = ES3.Load<ProgressData>("saveFile", new ProgressData());
        }


        public void Start()
        {
            LoadProgress();
        }


        public void CompleteTutorial()
        {
            _progressData.IsTutorialCompleted = true;
            SaveProgress();
        }
    }
}