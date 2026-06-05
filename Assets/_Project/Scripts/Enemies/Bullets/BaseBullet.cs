using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Scripts.Enemies.Bullets
{
    using UnityEngine;

    public class BaseBullet : MonoBehaviour
    {
        [SerializeField] protected float _speed = 5f;
        [SerializeField] protected float _maxLifetime = 3f;
        [SerializeField] protected float _damage = 3f;
        [SerializeField] protected Transform _baseTransform;
        public Action OnReady;

        protected virtual void Awake()
        {
            if (_baseTransform == null)
                _baseTransform = transform;
        }

        protected virtual void Start()
        {
            Destroy(gameObject, _maxLifetime);
        }

        protected virtual void Update()
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