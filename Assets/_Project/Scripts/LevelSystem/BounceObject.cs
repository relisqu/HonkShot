using System;
using Scripts.Audio;
using Scripts.Player;
using UnityEngine;

namespace Scripts.LevelSystem
{
    public class BounceObject : MonoBehaviour
    {
        [SerializeField] private float _bounciness;
        public float Bounciness => _bounciness;

        [SerializeField] private bool _hasBounceAnimation;
        [SerializeField] private PunchObjectAnimation _bounceAnimation;

        public void OnCollisionEnter2D(Collision2D other)
        {
            if (!_hasBounceAnimation) return;

            if (other.gameObject.TryGetComponent(out BouncingObject _))
            {
                if (_hasBounceAnimation)
                {
                    _bounceAnimation.ShowPunchAnimation();
                }
            }
        }
    }
}