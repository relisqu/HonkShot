using Scripts.Player;
using Scripts.Services.Pooling;
using UnityEngine;

namespace Scripts.VFX
{
    /// <summary>
    /// Spawns an impact effect where this entity bounces off something. Like
    /// <see cref="Scripts.Audio.BounceSoundEmitter"/> it lives on the bouncing entity, so each entity can
    /// carry its own effect, and it reads how hard the hit was the same way: from the impact speed.
    /// Soft hits get a smaller effect, very soft ones none at all. The effect art is drawn with the
    /// surface on its right and the splash flying left, so it is turned to make the splash follow the
    /// contact normal.
    /// </summary>
    public class BounceVFXEmitter : MonoBehaviour
    {
        [SerializeField] private AnimatedEffect _effectPrefab;
        [SerializeField] private int _poolSize = 4;

        [Header("Size")]
        [Tooltip("Impacts at or below this speed spawn nothing.")]
        [SerializeField] private float _minImpactSpeed = 3f;

        [Tooltip("Impact speed that maps to the full-size effect.")]
        [SerializeField] private float _maxImpactSpeed = 15f;

        [Tooltip("Effect scale at the weakest shown impact and at full intensity.")]
        [SerializeField] private Vector2 _scaleRange = new Vector2(0.4f, 1f);

        [Tooltip("Shapes how intensity maps onto that scale range.")]
        [SerializeField] private AnimationCurve _response = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        [Tooltip("Ignore further bounces for this long, so one messy contact doesn't stack effects.")]
        [SerializeField] private float _retriggerCooldown = 0.05f;

        private ComponentPool<AnimatedEffect> _pool;
        private float _lastSpawnTime = float.NegativeInfinity;
        private int _ghostLayer;

        private void Awake()
        {
            _ghostLayer = LayerMask.NameToLayer("SimulationGhost");

            if (_effectPrefab)
                _pool = new ComponentPool<AnimatedEffect>(_effectPrefab, _poolSize);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (_pool == null) return;
            if (Time.time - _lastSpawnTime < _retriggerCooldown) return;

            // Throw prediction replays these collisions on disposable ghost bodies every frame the
            // player is aiming. They must not leave effects behind.
            if (gameObject.layer == _ghostLayer || collision.gameObject.layer == _ghostLayer) return;
            if (collision.gameObject.GetComponentInParent<PlayerGhostProjectile>()) return;
            if (collision.contactCount == 0) return;

            float impactSpeed = collision.relativeVelocity.magnitude;
            if (impactSpeed <= _minImpactSpeed) return;

            var point = Vector2.zero;
            var normal = Vector2.zero;
            for (int i = 0; i < collision.contactCount; i++)
            {
                var contact = collision.GetContact(i);
                point += contact.point;
                normal += contact.normal;
            }

            point /= collision.contactCount;
            if (normal.sqrMagnitude < 0.0001f)
                normal = -collision.relativeVelocity;

            // Normal points from the surface towards us; the art splashes along -X.
            float angle = Mathf.Atan2(normal.y, normal.x) * Mathf.Rad2Deg + 180f;

            var effect = _pool.Get(point, Quaternion.Euler(0f, 0f, angle));
            if (!effect) return;

            _lastSpawnTime = Time.time;
            effect.transform.localScale = Vector3.one * ResolveScale(impactSpeed);
        }

        private float ResolveScale(float impactSpeed)
        {
            float intensity = _maxImpactSpeed > _minImpactSpeed
                ? Mathf.InverseLerp(_minImpactSpeed, _maxImpactSpeed, impactSpeed)
                : 1f;

            return Mathf.Lerp(_scaleRange.x, _scaleRange.y, _response.Evaluate(intensity));
        }
    }
}
