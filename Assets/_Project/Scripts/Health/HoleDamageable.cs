using UnityEngine;

namespace Scripts.Health
{
    public class HoleDamageable : MonoBehaviour
    {
        [SerializeField] private HealthController _healthController;
        public HealthController HealthController => _healthController;
        [SerializeField] private float _holeContactDamage;

        public void TakeDamage()
        {
            _healthController.TakeDamage(_holeContactDamage);
        }
    }
}