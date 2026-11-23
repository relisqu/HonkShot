using System;
using System.Collections;
using Scripts.Health;
using UnityEngine;

namespace Scripts.Enemies
{
    // A non-lethal hit during an attack wind-up cancels the attack and stuns the enemy briefly.
    // The enemy itself (IAttackInterruptible on the same GameObject) decides how to cancel and resume.
    [RequireComponent(typeof(HealthController))]
    public class EnemyHitStun : MonoBehaviour
    {
        [SerializeField] private HealthController _healthController;

        [Tooltip("How long the enemy stands still after a hit cancels its attack wind-up. " +
                 "The tremble look is tuned in HitFeedbackConfig.")]
        [SerializeField] private float _stunDuration = 0.6f;

        private IAttackInterruptible _enemy;
        private Behaviour _enemyBehaviour;
        private Coroutine _stunRoutine;

        public Action<float> OnStunned;

        public bool IsStunned => _stunRoutine != null;

        private void Awake()
        {
            if (!_healthController)
                _healthController = GetComponent<HealthController>();

            if (TryGetComponent(out _enemy))
                _enemyBehaviour = _enemy as Behaviour;
            if (!_enemyBehaviour)
                Debug.LogWarning($"{name}: EnemyHitStun needs an IAttackInterruptible MonoBehaviour on the same GameObject.", this);
        }

        private void Start()
        {
            _healthController.OnNonLethalDamageReceived += HealthController_NonLethalDamageReceived;
        }

        private void OnDestroy()
        {
            if (_healthController)
                _healthController.OnNonLethalDamageReceived -= HealthController_NonLethalDamageReceived;
        }

        private void OnDisable()
        {
            if (_stunRoutine == null) return;
            StopCoroutine(_stunRoutine);
            _stunRoutine = null;
        }

        private void HealthController_NonLethalDamageReceived(float damage)
        {
            if (IsStunned || !_enemyBehaviour || !_enemyBehaviour.isActiveAndEnabled || !_enemy.IsWindingUp) return;

            _enemy.InterruptAttack();
            OnStunned?.Invoke(_stunDuration);
            _stunRoutine = StartCoroutine(StunRoutine());
        }

        private IEnumerator StunRoutine()
        {
            yield return new WaitForSeconds(_stunDuration);
            _stunRoutine = null;

            if (_healthController.IsAlive && _enemyBehaviour && _enemyBehaviour.isActiveAndEnabled)
                _enemy.ResumeAfterStun();
        }
    }
}
