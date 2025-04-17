using System;
using Scripts.Health;
using Scripts.Player.States;
using UnityEngine;

namespace Scripts.Player
{
    public class PlayerStatus : MonoBehaviour
    {
        private PlayerState _playerState;
        private PlayerState _preSwapState;
        private PlayerState _preHiddenState;
        private AnimationStateMachine _animationStateMachine;

        [SerializeField] private GameObject _idlePlayerGameObject;
        [SerializeField] private GameObject _ballPlayerGameObject;
        [SerializeField] private HealthController _healthController;

        private bool _isHidden;

        public PlayerState PlayerState => _playerState;
        public Action OnSwapToBall;
        public Action OnSwapToIdle;

        public GameObject CurrentGameObject =>
            _idlePlayerGameObject.activeSelf ? _idlePlayerGameObject : _ballPlayerGameObject;

        public HealthController GetHealthController()
        {
            return _healthController;
        }

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
            if (playerState == PlayerState.Swapping)
            {
                _preSwapState = _playerState;
            }

            if (_playerState == playerState) return;

            _playerState = playerState;
            AnimationStateMachine().ChangeState(_playerState);

            switch (_playerState)
            {
                case PlayerState.Ball:
                    OnSwapToBall?.Invoke();
                    break;
                case PlayerState.Swapping:
                    OnSwapToIdle?.Invoke();
                    break;
            }
        }

        public void ResetSwapState()
        {
            SetPlayerState(_preSwapState);
        }

        public void HidePlayer()
        {
            if (_isHidden) return;

            _isHidden = true;
            _preHiddenState = _playerState;

            CurrentGameObject.SetActive(false);
        }

        public void RevealPlayer()
        {
            if (!_isHidden) return;

            _isHidden = false;

            // Reactivate idle or ball object depending on stored state
            if (_preHiddenState == PlayerState.Ball)
            {
                _ballPlayerGameObject.SetActive(true);
                _idlePlayerGameObject.SetActive(false);
            }
            else
            {
                _idlePlayerGameObject.SetActive(true);
                _ballPlayerGameObject.SetActive(false);
            }

            SetPlayerState(_preHiddenState);
        }
    }
}