using System;
using UnityEngine.Serialization;

namespace Scripts.Enemies.Bullets
{
    using UnityEngine;

    public class BaseBullet : MonoBehaviour
    {
        [SerializeField] private float _speed = 5f;
        [SerializeField] protected float _maxLifetime = 3f;
        [SerializeField] protected float _damage = 3f;
        [SerializeField] protected Transform _baseTransform;
        public Action OnReady;

        private void Awake()
        {
            if (_baseTransform == null)
                _baseTransform = transform;
        }

        private void Start()
        {
            Destroy(gameObject, _maxLifetime);
        }

        private void Update()
        {
            _baseTransform.position += _baseTransform.up * (_speed * Time.deltaTime);
        }

        public virtual void DamagePlayer(PlayerHealth health)
        {
            health.HealthController.TakeDamage(_damage);
            Destroy(gameObject);
        }

        public void SetParameters(float bulletSpeed, float bulletDamage)
        {
            _speed = bulletSpeed;
            _damage = bulletDamage;
        }
    }
}