using System;
using System.Collections;
using UnityEngine;

namespace Scripts.Enemies.Bullets
{
    public class LaserBaseBullet : BaseBullet
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private LayerMask _obstacleLayer;
        [SerializeField] private Color _prefireColor;

        [SerializeField] private float _maxRange = 5f;

        [SerializeField] private float _prefireDuration = 0.3f;

        private Vector2 _startPosition;
        private bool _isFired = false;

        private void Start()
        {
            _isFired = false;
            _startPosition = transform.position;
            StartCoroutine(PrefireEffect());
        }

        private IEnumerator PrefireEffect()
        {
            Vector2 direction = _baseTransform.up;
            RaycastHit2D hit = Physics2D.Raycast(_startPosition, direction, _maxRange, _obstacleLayer);

            Vector3 position = Vector3.zero;
            if (hit.collider != null)
            {
                Debug.Log(hit.collider.gameObject.name);
                _baseTransform.localScale = new Vector3( _baseTransform.localScale.x, hit.distance, _baseTransform.localScale.z);
            }
            else
            {
                _baseTransform.localScale = new Vector3( _baseTransform.localScale.x, _maxRange, _baseTransform.localScale.z);
            }


            _spriteRenderer.color = _prefireColor;
            yield return new WaitForSeconds(_prefireDuration);
            _spriteRenderer.color = Color.red;
            _isFired = true;
            StartCoroutine(DestroyAfterTime());
        }

        private void OnDrawGizmos()
        {
            Debug.DrawRay(_startPosition, _baseTransform.up * _maxRange);
        }

        private IEnumerator DestroyAfterTime()
        {
            yield return new WaitForSeconds(_maxLifetime);
            Destroy(gameObject);
        }

        public override void DamagePlayer(PlayerHealth health)
        {
            Debug.Log("DamagePlayer"+ _isFired);
            if (_isFired)
            {
                Debug.Log("Damage!!!Player");
                health.HealthController.TakeDamage(_damage);
            }
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (other.gameObject.TryGetComponent(out PlayerHealth health))
            {
                DamagePlayer(health);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.TryGetComponent(out PlayerHealth health))
            {
                DamagePlayer(health);
            }
        }
    }
}