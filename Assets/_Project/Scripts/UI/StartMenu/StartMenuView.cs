using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Scripts.UI.StartMenu
{
    public class StartMenuView : MonoBehaviour
    {
        [SerializeField] private Button _startButton;

        private void StartButton_Click()
        {
            StartCoroutine(LoadAsyncScene());
        }

        IEnumerator LoadAsyncScene()
        {
            // The Application loads the Scene in the background as the current Scene runs.
            // This is particularly good for creating loading screens.
            // You could also load the Scene by using sceneBuildIndex. In this case Scene2 has
            // a sceneBuildIndex of 1 as shown in Build Settings.

            AsyncOperation asyncLoad =
                SceneManager.LoadSceneAsync("_Project/Scenes/GameplayScene");

            // Wait until the asynchronous scene fully loads
            while (!asyncLoad.isDone)
            {
                yield return null;
            }
        }

        private void Start()
        {
            _startButton.onClick.AddListener(StartButton_Click);
        }


        private void OnDestroy()
        {
            _startButton.onClick.RemoveListener(StartButton_Click);
        }
    }
}