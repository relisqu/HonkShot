using System;
using Scripts.Audio;
using Scripts.LevelSystem.LevelGeneration;
using Scripts.Other;
using Scripts.Player.InputHandling;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace Scripts.LevelSystem
{
    public class LevelStartController : MonoBehaviour
    {
        private LevelManager _levelManager;

        [Inject]
        private void Inject(LevelManager levelManager)
        {
            _levelManager = levelManager;
        }

        private void Start()
        {
            
            AudioManager.Instance.PlaySmooth(SoundChanelType.Music, "levelMusic", true);
            InputHandler.Instance.SetInputEnabled(true);
            _levelManager.EnterFloor();
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