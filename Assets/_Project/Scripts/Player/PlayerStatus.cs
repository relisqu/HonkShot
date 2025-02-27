using System;
using Scripts.Player.States;
using UnityEngine;

namespace Scripts.Player
{
    public class PlayerStatus : MonoBehaviour
    {
        private PlayerState _playerState;

        public PlayerState PlayerState => _playerState;
        public Action OnSwapToBall;
        public Action OnSwapToIdle;
        private AnimationStateMachine _animationStateMachine;
        public AnimationStateMachine AnimationStateMachine => _animationStateMachine;

        private void Awake()
        {
            SetDefaultState();
            _animationStateMachine = new AnimationStateMachine();
        }

        public void SetDefaultState()
        {
            SetPlayerState(PlayerState.Ball);
            _animationStateMachine.SetDefaultState(_playerState);
        }


        public void SetPlayerState(PlayerState playerState)
        {
            if (_playerState == playerState) return;

            _playerState = playerState;

            switch (_playerState)
            {
                case PlayerState.Ball:
                    OnSwapToBall?.Invoke();
                    break;
                case PlayerState.Swapping:
                    OnSwapToIdle?.Invoke();
                    break;
                default:
                    break;
            }
        }
    }
}