using System;
using Scripts.Audio;
using UnityEngine;

namespace Scripts.LevelSystem.LevelObjects
{
    public class BouncerLevelObject : MonoBehaviour
    {
        [SerializeField] private BounceObject _bounceObject;

        private void Start()
        {
            _bounceObject.OnBounce += BounceObject_Bounce;
        }

        private void OnDestroy()
        {
            if(!_bounceObject) return;
            _bounceObject.OnBounce -= BounceObject_Bounce;
        }

        private void BounceObject_Bounce()
        {
            AudioManager.Instance.Play(SoundChanelType.LevelObjects,"bouncer");
        }
    }
}