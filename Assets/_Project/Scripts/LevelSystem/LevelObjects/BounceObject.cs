using System;
using Scripts.Enemies;
using Scripts.Player;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Scripts.LevelSystem.LevelObjects
{
    public class BounceObject : MonoBehaviour
    {
        [SerializeField] private float _bounciness;
        public float Bounciness => _bounciness;

        [Tooltip("Which bounce sound an entity plays when it hits this. Auto treats anything with an " +
                 "EnemyHealth as Enemy and everything else as Obstacle.")]
        [SerializeField] private BounceSurface _surface = BounceSurface.Auto;

        [SerializeField] private bool _hasBounceAnimation;
        [SerializeField] private PunchObjectAnimation _bounceAnimation;

        public Action OnBounce;

        private BounceSurface _resolvedSurface = BounceSurface.Obstacle;

        public BounceSurface Surface => _resolvedSurface;

        protected virtual void Awake()
        {
            _resolvedSurface = _surface != BounceSurface.Auto
                ? _surface
                : GetComponentInParent<EnemyHealth>(true)
                    ? BounceSurface.Enemy
                    : BounceSurface.Obstacle;
        }

        public virtual void OnCollisionEnter2D(Collision2D other)
        {
            if (!other.enabled) return;

            if (gameObject.scene != SceneManager.GetActiveScene())
            {
                return;
            }


            if (other.gameObject.TryGetComponent(out PlayerGhostProjectile _)) return;

            OnBounce?.Invoke();

            if (!other.gameObject.TryGetComponent(out BouncingObject _)) return;

            // The impact sound is played by the BounceSoundEmitter on whatever bounced off us, not
            // from here: only the bouncing entity knows how hard it hit and which sound bank it owns.
            if (_hasBounceAnimation)
            {
                _bounceAnimation.ShowPunchAnimation();
            }
        }
    }
}
