using System;
using Scripts.Player.States;
using UnityEngine;

namespace Scripts.Player
{
    public class PlayerStatus : MonoBehaviour
    {
        private PlayerState _playerState;
        private AnimationStateMachine _animationStateMachine;
        [SerializeField] private GameObject _idlePlayerGameObject;
        [SerializeField] private GameObject _ballPlayerGameObject;

        public PlayerState PlayerState => _playerState;
        public Action OnSwapToBall;
        public Action OnSwapToIdle;

        public GameObject CurrentGameObject =>
            _idlePlayerGameObject.activeSelf ? _idlePlayerGameObject : _ballPlayerGameObject;

        public AnimationStateMachine AnimationStateMachine()
        {
            if (_animationStateMachine == null)
            {
                _animationStateMachine = new AnimationStateMachine();
            }

            return _animationStateMachine;
        }

        private void Awake()
        {
            SetDefaultState();
        }

        public void SetDefaultState()
        {
            SetPlayerState(PlayerState.Idle);
            AnimationStateMachine().SetDefaultState(_playerState);
        }


        public void SetPlayerState(PlayerState playerState)
        {
            if (_playerState == playerState) return;

            _playerState = playerState;
            _animationStateMachine.ChangeState(_playerState);

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