using System;
using Scripts.Audio;
using Scripts.LevelSystem.LevelGeneration;
using Scripts.Managers.Tutorial;
using Scripts.Other;
using Scripts.Player.InputHandling;
using Scripts.Progress;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace Scripts.LevelSystem
{
    public class LevelStartController : MonoBehaviour
    {
        private LevelManager _levelManager;
        private InputHandler _inputHandler;
        private ProgressSaver _progressSaver;
        private TutorialController _tutorialController;

        [Inject]
        private void Inject(LevelManager levelManager, InputHandler inputHandler, ProgressSaver progressSaver,
            TutorialController tutorialController)
        {
            _inputHandler = inputHandler;
            _progressSaver = progressSaver;
            _levelManager = levelManager;
            _progressSaver = progressSaver;
            _tutorialController = tutorialController;
        }

        private void Start()
        {
            AudioManager.Instance.PlaySmooth(SoundChanelType.Music, "levelMusic", true);
            if (_progressSaver.ProgressData.IsTutorialCompleted)
            {
                _tutorialController.CreateTutorial();
                _tutorialController.StartTutorial();
            }
            else
            {
                _levelManager.EnterFloor();
            }

            _inputHandler.SetInputEnabled(InputLayer.GameStart, true);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }
    }
}