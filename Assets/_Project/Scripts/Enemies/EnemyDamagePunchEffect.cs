using DG.Tweening;
using Scripts.Health;
using UnityEngine;

namespace Scripts.Enemies
{
    public class EnemyDamagePunchEffect : MonoBehaviour
    {
        [SerializeField] private HealthController _healthController;
        [SerializeField] private Vector3 _punchStrength = 0.4f * Vector3.one;
        [SerializeField] private float _punchDuration = 0.2f;

        private Tweener _punchTween;

        private void Awake()
        {
            if (!_healthController)
                _healthController = GetComponent<HealthController>();
        }

        private void Start()
        {
            _healthController.OnNonLethalDamageReceived += HealthController_OnNonLethalDamageReceived;
        }

        private void OnDestroy()
        {
            _healthController.OnNonLethalDamageReceived -= HealthController_OnNonLethalDamageReceived;
        }

        private void HealthController_OnNonLethalDamageReceived(float damage)
        {
            if (_punchTween != null) return;

            _punchTween = transform.DOPunchScale(_punchStrength, _punchDuration)
                .OnComplete(() => { _punchTween = null; });
        }
    }
}
