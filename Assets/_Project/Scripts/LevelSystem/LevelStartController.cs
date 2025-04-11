using System;
using Scripts.Audio;
using Scripts.LevelSystem.LevelGeneration;
using Scripts.Other;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Scripts.LevelSystem
{
    public class LevelStartController : MonoBehaviour
    {
        [SerializeField] private LevelGenerator _levelGenerator;
        [SerializeField] private LevelManager _levelManager;

        private void Start()
        {
            AudioManager.Instance.PlaySmooth(SoundChanelType.Music, "levelMusic", true);
            _levelManager.EnterRoom(_levelGenerator.GetRoom(0));
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