using System;
using Scripts.Audio;
using UnityEngine;

namespace Scripts.LevelSystem
{
    public class LevelStartController : MonoBehaviour
    {
        private void Start()
        {
            AudioManager.Instance.PlaySmooth(SoundChanelType.Music,"levelMusic", true);
        }
    }
}