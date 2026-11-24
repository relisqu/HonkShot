using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Scripts.Health;
using Scripts.Other;
using UnityEngine;

namespace Scripts.Enemies.Bullets
{
    public class LaserBaseBullet : BaseBullet
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Collider2D _damageCollider;
        [SerializeField] private LayerMask _obstacleLayer;
        [SerializeField] private Color _prefireColor;
        [SerializeField] private Color _fireColor;
        [SerializeField] private float _maxRange = 5f;
        [SerializeField] private float _prefireDuration = 0.3f;

        [Header("Contact Damage")]
        [Tooltip("Damage ticked while the player stays in the fired laser. The first touch deals the bigger Damage above.")]
        [SerializeField] private float _tickDamage = 4f;
        [Tooltip("Seconds between ticks. Keep it above the player's i-frame time (0.2s), otherwise ticks get absorbed.")]
        [SerializeField] private float _tickInterval = 0.3f;

        private readonly RaycastHit2D[] _rayHits = new RaycastHit2D[1];
        private readonly HashSet<Collider2D> _touchingPlayerColliders = new();
        private PlayerHealth _touchingPlayer;
        private bool _hasDealtContactDamage;
        private float _nextTickTime;
        private Vector2 _startPosition;
        private bool _isFired;

        public float PrefireDuration => _prefireDuration;
        public Vector2 BeamEnd { get; private set; }
        public bool BeamHitsObstacle { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            if (!_damageCollider)
                _damageCollider = GetComponent<Collider2D>();
        }

        protected override void Start()
        {
            _isFired = false;
            _startPosition = transform.position;
            StartCoroutine(PrefireEffect());
        }

        // The laser doesn't travel; it applies contact damage instead.
        protected override void Update()
        {
            ApplyContactDamage();
        }

        private IEnumerator PrefireEffect()
        {
            float length = GetBeamLength(_baseTransform.up);
            var targetScale = new Vector3(_baseTransform.localScale.x, length, _baseTransform.localScale.z);

            _baseTransform.localScale = new Vector3(0, length, _baseTransform.localScale.z);
            _baseTransform.DOScale(targetScale, 0.1f);
            _spriteRenderer.color = _prefireColor;

            yield return new WaitForSeconds(_prefireDuration);
            _spriteRenderer.DOColor(_fireColor, 0.1f);
            _isFired = true;
            OnReady?.Invoke();
            StartCoroutine(DestroyAfterTime());
        }

        // Trigger zones (doors, pickups, holes) don't block the beam; only solid obstacles do.
        private float GetBeamLength(Vector2 direction)
        {
            var filter = new ContactFilter2D { useTriggers = false };
            filter.SetLayerMask(_obstacleLayer);
            int hitCount = Physics2D.Raycast(_startPosition, direction, filter, _rayHits, _maxRange);
            float length = hitCount > 0 ? _rayHits[0].distance : _maxRange;
            BeamHitsObstacle = hitCount > 0;
            BeamEnd = _startPosition + direction.normalized * length;
            return length;
        }

        private IEnumerator DestroyAfterTime()
        {
            yield return new WaitForSeconds(_maxLifetime);
            _isFired = false;
            _baseTransform.DOScale(new Vector3(0, _baseTransform.localScale.y, _baseTransform.localScale.z), 0.1f)
                .OnComplete(() => Destroy(gameObject));
        }

        private void ApplyContactDamage()
        {
            if (!_isFired || !_touchingPlayer) return;

            if (!_hasDealtContactDamage)
            {
                _hasDealtContactDamage = true;
                _nextTickTime = Time.time + _tickInterval;
                DealDamage(_touchingPlayer, _damage);
            }
            else if (Time.time >= _nextTickTime)
            {
                _nextTickTime = Time.time + _tickInterval;
                DealDamage(_touchingPlayer, _tickDamage);
            }
        }

        public override void DamagePlayer(PlayerHealth health)
        {
            DealDamage(health, _tickDamage);
        }

        private void DealDamage(PlayerHealth health, float damage)
        {
            Vector2 hitPoint = _damageCollider
                ? _damageCollider.ClosestPoint(health.transform.position)
                : (Vector2)transform.position;
            health.HealthController.TakeDamage(damage, DamageSource.At(hitPoint));
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.TryGetComponentInBody(out PlayerHealth health)) return;
            _touchingPlayer = health;
            _touchingPlayerColliders.Add(other);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!_touchingPlayerColliders.Remove(other) || _touchingPlayerColliders.Count > 0) return;
            _touchingPlayer = null;
            _hasDealtContactDamage = false;
        }

        private void OnDrawGizmos()
        {
            var up = _baseTransform ? _baseTransform.up : transform.up;
            Debug.DrawRay(_startPosition, up * _maxRange);
        }
    }
}
