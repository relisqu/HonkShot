using System;
using UnityEngine;

namespace Scripts.Health
{
    public class HoleDamageable : MonoBehaviour
    {
        [SerializeField] private HealthController _healthController;
        public HealthController HealthController => _healthController;
        [SerializeField] private float _holeContactDamage;

        public Rigidbody2D Rigidbody2D { get; private set; }

        private void Awake()
        {
            Rigidbody2D = GetComponent<Rigidbody2D>();
        }

        public void TakeDamage()
        {
            _healthController.TakeDamage(_holeContactDamage);
        }
    }
}