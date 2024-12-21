using System;
using UnityEngine;

namespace Scripts.Player
{
    public class PlayerStatus : MonoBehaviour
    {
        private PlayerState _playerState;

        public PlayerState PlayerState => _playerState;
        public Action OnSwapToBall;
        public Action OnSwapToShooter;

        public void SetDefaultState()
        {
            SetPlayerState(PlayerState.Shooter);
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
                case PlayerState.Shooter:
                    OnSwapToShooter?.Invoke();
                    break;
                case PlayerState.Swapping:
                    break;
                default:
                    break;
            }
        }

        private void Start()
        {
            SetDefaultState();
        }
    }
}