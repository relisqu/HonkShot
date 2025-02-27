using System;
using UnityEngine;

namespace Scripts.Player.States
{
    public class AnimationStateMachine
    {
        private static readonly int IdleToSwapping = Animator.StringToHash("IdleToSwapping");
        private static readonly int BallToSwapping = Animator.StringToHash("BallToSwapping");
        private static readonly int SwappingToIdle = Animator.StringToHash("SwappingToIdle");
        private static readonly int SwappingToBall = Animator.StringToHash("SwappingToBall");
        private PlayerState _currentState;
        private Animator _animator;

        public AnimationStateMachine(Animator animator, PlayerState initialState = PlayerState.Idle)
        {
            _animator = animator;
            _currentState = initialState;
        }

        public AnimationStateMachine()
        {
        }

        public void SetAnimator(Animator animator)
        {
            _animator = animator;
        }

        public void SetDefaultState(PlayerState initialState = PlayerState.Idle)
        {
            _currentState = initialState;
        }

        public void ChangeState(PlayerState newState)
        {
            if (_currentState == newState) return;

            switch (_currentState)
            {
                case PlayerState.Idle:
                    if (newState == PlayerState.Swapping)
                    {
                        _animator.SetTrigger(IdleToSwapping);
                    }

                    break;

                case PlayerState.Ball:
                    if (newState == PlayerState.Swapping)
                    {
                        _animator.SetTrigger(BallToSwapping);
                    }

                    break;

                case PlayerState.Swapping:
                    if (newState == PlayerState.Idle)
                    {
                        _animator.SetTrigger(SwappingToIdle);
                    }
                    else if (newState == PlayerState.Ball)
                    {
                        _animator.SetTrigger(SwappingToBall);
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // Update the current state
            _currentState = newState;
        }

        // Get the current state
        public PlayerState GetCurrentState()
        {
            return _currentState;
        }
    }
}