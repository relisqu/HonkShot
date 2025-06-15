using System;
using UnityEngine;

namespace Scripts.Health
{
    public class HoleDamageable : MonoBehaviour
    {
        [SerializeField] private HealthController _healthController;
        public HealthController HealthController => _healthController;
        [SerializeField] private float _holeContactDamage;
        [SerializeField] private bool _diesOnHoleContact;

        public Rigidbody2D Rigidbody2D { get; private set; }

        private void Awake()
        {
            Rigidbody2D = GetComponent<Rigidbody2D>();
        }

        public void TakeDamage()
        {
            if (_diesOnHoleContact)
            {
                _healthController.TakeDamage(_holeContactDamage*99999f);
            }
            else
            {
                _healthController.TakeDamage(_holeContactDamage);
            }
        }
    }
}