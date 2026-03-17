using Scripts.LevelSystem.LevelObjects;
using UnityEngine;

namespace Scripts.Enemies.Swamp
{
    public class SwampArmoredHider : BaseEnemy
    {
        [Header("Components")]
        [SerializeField] private HidingShootingEnemyStateMachine _stateMachine;
        [SerializeField] private EnemyArmor _enemyArmor;
        [SerializeField] private BounceObject _bounceObject;
        [SerializeField] private Collider2D _bounceCollider;

        private void Awake()
        {
            if (!_stateMachine)
                _stateMachine = GetComponent<HidingShootingEnemyStateMachine>();
            if (!_enemyArmor)
                _enemyArmor = GetComponent<EnemyArmor>();
            if (!_bounceObject)
                _bounceObject = GetComponent<BounceObject>();
            if (!_bounceCollider)
                _bounceCollider = GetComponent<Collider2D>();
        }

        private void OnEnable()
        {
            if (_stateMachine)
                _stateMachine.OnStateChanged += StateMachine_OnStateChanged;
        }

        private void OnDisable()
        {
            if (_stateMachine)
                _stateMachine.OnStateChanged -= StateMachine_OnStateChanged;
        }

        private void StateMachine_OnStateChanged(EnemyState prev, EnemyState next)
        {
            if (!_bounceCollider) return;

            _bounceCollider.enabled = next == EnemyState.Active;
        }
    }
}
