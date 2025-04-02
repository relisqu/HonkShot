using System;
using Scripts.Audio;
using Scripts.Player;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Scripts.LevelSystem.LevelObjects
{
    public class BounceObject : MonoBehaviour
    {
        [SerializeField] private float _bounciness;
        public float Bounciness => _bounciness;

        [SerializeField] private bool _hasBounceAnimation;
        [SerializeField] private PunchObjectAnimation _bounceAnimation;

        public Action OnBounce;

        public void OnCollisionEnter2D(Collision2D other)
        {
            if(gameObject.scene != SceneManager.GetActiveScene()){ return;}
            if (other.gameObject.TryGetComponent(out PlayerGhostProjectile _))
            {
                return;
            }
            else
            {
                OnBounce?.Invoke();

                if (other.gameObject.TryGetComponent(out BouncingObject bO))
                {
                    Debug.Log(other.gameObject.name+" "+gameObject.name+" Bouncing");
                    AudioManager.Instance.PlayOneShot(SoundChanelType.LevelObjects, "gooseCollision");
                    if (_hasBounceAnimation)
                    {
                        _bounceAnimation.ShowPunchAnimation();
                    }
                }
            }
        }
    }
}