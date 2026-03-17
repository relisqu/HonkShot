using System;
using UnityEngine;

namespace Scripts.Enemies.Swamp
{
    public enum EnemyState
    {
        Active,
        Hiding,
        Hidden,
        Showing
    }

    public class HidingShootingEnemyStateMachine : MonoBehaviour
    {
        [SerializeField] private EnemyState _initialState = EnemyState.Active;

        private EnemyState _currentState;

        public EnemyState CurrentState => _currentState;
        public bool IsVisible => _currentState == EnemyState.Active || _currentState == EnemyState.Showing;
        public bool IsHidden => _currentState == EnemyState.Hidden || _currentState == EnemyState.Hiding;
        public bool IsTransitioning => _currentState == EnemyState.Hiding || _currentState == EnemyState.Showing;

        public event Action<EnemyState, EnemyState> OnStateChanged;

        private void Awake()
        {
            _currentState = _initialState;
        }

        public bool TrySetState(EnemyState newState)
        {
            if (_currentState == newState) return false;

            if (!IsTransitionValid(_currentState, newState)) return false;

            var prev = _currentState;
            _currentState = newState;
            OnStateChanged?.Invoke(prev, newState);
            return true;
        }

        private bool IsTransitionValid(EnemyState from, EnemyState to)
        {
            return (from, to) switch
            {
                (EnemyState.Active, EnemyState.Hiding) => true,
                (EnemyState.Hiding, EnemyState.Hidden) => true,
                (EnemyState.Hidden, EnemyState.Showing) => true,
                (EnemyState.Showing, EnemyState.Active) => true,
                // Allow forced reset to Active
                (_, EnemyState.Active) => true,
                _ => false,
            };
        }
    }
}
