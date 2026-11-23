using UnityEngine.SceneManagement;

namespace Scripts.Other
{
    public static class SceneRestarter
    {
        public static void RestartCurrentScene()
        {
            // TimeManager.Awake doesn't reset Time.timeScale, so the reloaded scene would stay frozen.
            if (TimeManager.Instance)
            {
                TimeManager.Instance.SetMenuPaused(false);
                TimeManager.Instance.ResumeGame();
            }

            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
