using Scripts.LevelSystem.LevelObjects;
using Scripts.Player;
using UnityEngine;

namespace Scripts.Audio
{
    /// <summary>
    /// Plays the impact sound when this entity bounces off something. It lives on the bouncing entity
    /// rather than on the thing being hit, so every entity carries its own sound bank and tuning: the
    /// goose and a walking enemy hitting the same wall sound different.
    /// </summary>
    public class BounceSoundEmitter : MonoBehaviour
    {
        [Header("Sounds")]
        [SerializeField] private SoundChanelType _channel = SoundChanelType.LevelObjects;
        [SerializeField] private string _obstacleBounceSound = "enemyBounceObstacle";
        [SerializeField] private string _enemyBounceSound = "enemyBounceEnemy";

        [Header("Loudness")]
        [Tooltip("Impacts at or below this speed are silent.")]
        [SerializeField] private float _minImpactSpeed = 1.5f;

        [Tooltip("Impact speed that maps to the loudest bounce.")]
        [SerializeField] private float _maxImpactSpeed = 20f;

        [Tooltip("Volume scale handed to the AudioManager at zero and full intensity.")]
        [SerializeField] private Vector2 _volumeRange = new Vector2(0.25f, 1f);

        [Tooltip("Shapes how intensity maps onto that volume range.")]
        [SerializeField] private AnimationCurve _response = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        [Tooltip("Ignore further bounces for this long, so one messy contact doesn't machine-gun the sound.")]
        [SerializeField] private float _retriggerCooldown = 0.05f;

        private float _lastPlayTime = float.NegativeInfinity;
        private int _ghostLayer;

        protected virtual void Awake()
        {
            _ghostLayer = LayerMask.NameToLayer("SimulationGhost");
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!AudioManager.Instance) return;
            if (Time.time - _lastPlayTime < _retriggerCooldown) return;

            // Throw prediction replays these collisions on disposable ghost bodies every frame the
            // player is aiming. They have to stay silent.
            if (gameObject.layer == _ghostLayer || collision.gameObject.layer == _ghostLayer) return;
            if (collision.gameObject.GetComponentInParent<PlayerGhostProjectile>()) return;

            float impactSpeed = collision.relativeVelocity.magnitude;
            if (impactSpeed <= _minImpactSpeed) return;

            var surface = ResolveSurface(collision);
            float intensity = Mathf.Clamp01(ResolveIntensity(collision, surface, impactSpeed));

            string soundName = surface == BounceSurface.Enemy ? _enemyBounceSound : _obstacleBounceSound;
            if (string.IsNullOrEmpty(soundName)) return;

            float volumeScale = Mathf.Lerp(_volumeRange.x, _volumeRange.y, _response.Evaluate(intensity));

            _lastPlayTime = Time.time;
            AudioManager.Instance.PlayOneShot(_channel, soundName, volumeScale);
        }

        /// <summary>
        /// How hard this bounce felt, in 0..1. The base emitter has no notion of damage, so it reads
        /// the impact physically. <see cref="PlayerBounceSoundEmitter"/> overrides this to follow the
        /// damage that landed wherever the thing being hit can actually take some.
        /// </summary>
        protected virtual float ResolveIntensity(Collision2D collision, BounceSurface surface, float impactSpeed)
        {
            return NormalizeSpeed(impactSpeed);
        }

        protected float NormalizeSpeed(float impactSpeed)
        {
            if (_maxImpactSpeed <= _minImpactSpeed) return 1f;
            return Mathf.InverseLerp(_minImpactSpeed, _maxImpactSpeed, impactSpeed);
        }

        private static BounceSurface ResolveSurface(Collision2D collision)
        {
            // Walls carry both a BounceObject and a LevelSolidObject; plain solid geometry carrying
            // neither still counts as an obstacle, so nothing we hit ends up silent.
            var bounceObject = collision.gameObject.GetComponentInParent<BounceObject>();
            return bounceObject && bounceObject.Surface == BounceSurface.Enemy
                ? BounceSurface.Enemy
                : BounceSurface.Obstacle;
        }
    }
}
