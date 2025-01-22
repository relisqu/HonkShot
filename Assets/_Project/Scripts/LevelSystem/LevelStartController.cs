using System;
using Scripts.Audio;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Scripts.LevelSystem
{
    public class LevelStartController : MonoBehaviour
    {
        private void Start()
        {
            AudioManager.Instance.PlaySmooth(SoundChanelType.Music, "levelMusic", true);
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