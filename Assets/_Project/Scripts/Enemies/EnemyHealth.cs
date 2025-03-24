using Scripts.Health;
using UnityEngine;

namespace Scripts.Enemies
{
    [RequireComponent(typeof(HealthController))]
    public class EnemyHealth : MonoBehaviour
    {
        [SerializeField] private HealthController _healthController;
        public HealthController HealthController => _healthController;

        public bool IsAlive()
        {
            return _healthController.IsAlive;
        }
    }
}