using Scripts.Health;
using UnityEngine;

namespace Scripts.Enemies.Swamp
{
    public class SwampEnemyDamagePunchEffect : MonoBehaviour
    {
        [SerializeField] private HealthController _healthController;
        [SerializeField] private EnemyTweenController _tweenController;
        [SerializeField] private HidingShootingEnemyStateMachine _stateMachine;
        [SerializeField] private Vector3 _punchStrength = 0.4f * Vector3.one;
        [SerializeField] private float _punchDuration = 0.2f;

        private void Awake()
        {
            if (!_healthController)
                _healthController = GetComponent<HealthController>();
            if (!_tweenController)
                _tweenController = GetComponent<EnemyTweenController>();
            if (!_stateMachine)
                _stateMachine = GetComponent<HidingShootingEnemyStateMachine>();
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
            if (_stateMachine && !_stateMachine.IsVisible) return;

            if (_tweenController)
                _tweenController.RequestPunchScale(_punchStrength, _punchDuration, TweenPriority.Normal);
        }
    }
}
